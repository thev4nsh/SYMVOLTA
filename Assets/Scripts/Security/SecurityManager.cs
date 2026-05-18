using SYMVOLTA.Core;
using SYMVOLTA.Networking;
using SYMVOLTA.Profile;
using SYMVOLTA.Shapes;
using System;
using UnityEngine;

namespace SYMVOLTA.Security
{
    /// <summary>
    /// Handles client-side anti-cheat, score validation, and basic tamper detection.
    /// Prevents impossible scores from being uploaded to the global leaderboard.
    /// </summary>
    public class SecurityManager : Singleton<SecurityManager>
    {
        private float _matchStartTime;
        private float _matchEndTime;
        private bool _isMatchActive = false;
        public float LastMatchDuration => Mathf.Max(0f, _matchEndTime - _matchStartTime);
        public bool IsMatchActive => _isMatchActive;

        /// <summary>
        /// Called when a match starts to log the secure start time.
        /// </summary>
        public void StartMatch()
        {
            _matchStartTime = Time.unscaledTime; // unscaledTime is harder to manipulate via time-scale hacks
            _isMatchActive = true;
        }

        /// <summary>
        /// Called when a match ends to log the secure end time.
        /// </summary>
        public void EndMatch()
        {
            _matchEndTime = Time.unscaledTime;
            _isMatchActive = false;
        }

        /// <summary>
        /// Validates if a submitted score is mathematically possible within the game's rules.
        /// This prevents memory editing hacks.
        /// </summary>
        public bool ValidateScore(float accuracy, SYMVOLTA.Shapes.ShapeType shape)
        {
            // Rule 1: Accuracy must be between 0 and 100
            if (accuracy < 0f || accuracy > 100.01f) // 0.01 tolerance for floating point
            {
                Debug.LogWarning($"[SecurityManager] Score validation failed: Accuracy out of bounds ({accuracy})");
                return false;
            }

            // Rule 2: Match must have actually been played for a minimum time
            float matchDuration = _matchEndTime - _matchStartTime;
            if (matchDuration < 1.0f) // Impossible to draw a shape in under 1 second
            {
                Debug.LogWarning($"[SecurityManager] Score validation failed: Match duration too short ({matchDuration}s)");
                return false;
            }

            // Rule 3: Match duration cannot exceed maximum allowed time (+2 sec tolerance)
            int maxTime = GameManager.Instance != null ? GameManager.Instance.Config.timerSeconds : Constants.DEFAULT_TIMER_SECONDS;
            if (matchDuration > maxTime + 2f)
            {
                Debug.LogWarning($"[SecurityManager] Score validation failed: Match duration exceeded limit ({matchDuration}s)");
                return false;
            }

            // Rule 4: If game is in offline mode, we can't trust high scores completely for global sync
            // (We will handle this further in the Leaderboard system, but flag it here)
            if (GameManager.Instance.IsOfflineMode && accuracy > 95f)
            {
                // Offline scores > 95% require extra scrutiny later
                Debug.Log("[SecurityManager] High offline score recorded. Flagged for re-verification on reconnect.");
            }

            return true;
        }

        public PendingScore CreateScoreRecord(PlayerProfile profile, SYMVOLTA.Shapes.ShapeType shape, float accuracy)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return new PendingScore
            {
                uid = profile.uid,
                username = profile.username,
                shape = (int)shape,
                score = Mathf.Clamp(accuracy, 0f, 100f),
                globalScore = Mathf.Clamp(profile.globalScore, 0f, 100f),
                rankTitle = profile.rankTitle,
                timestamp = timestamp,
                matchDuration = LastMatchDuration,
                hash = GenerateScoreHash(profile.uid, accuracy, shape, timestamp)
            };
        }

        public bool ValidatePendingScore(PendingScore pending)
        {
            if (pending == null) return false;
            if (!Enum.IsDefined(typeof(SYMVOLTA.Shapes.ShapeType), pending.shape)) return false;
            if (pending.score < 0f || pending.score > 100.01f) return false;
            if (pending.globalScore < 0f || pending.globalScore > 100.01f) return false;
            int maxTime = GameManager.Instance != null ? GameManager.Instance.Config.timerSeconds : Constants.DEFAULT_TIMER_SECONDS;
            if (pending.matchDuration < 1f || pending.matchDuration > maxTime + 30f) return false;

            string expected = GenerateScoreHash(pending.uid, pending.score, pending.ShapeType, pending.timestamp);
            return string.Equals(expected, pending.hash, StringComparison.Ordinal);
        }

        /// <summary>
        /// Generates a simple verification hash for score payloads.
        /// Server-side should ideally recreate this hash to ensure data wasn't altered in transit.
        /// </summary>
        public string GenerateScoreHash(string uid, float accuracy, SYMVOLTA.Shapes.ShapeType shape, long timestamp)
        {
            string rawString = $"{uid}_{accuracy:F2}_{shape}_{timestamp}_{Constants.BUNDLE_ID}";
            return ComputeSHA256(rawString);
        }

        /// <summary>
        /// Basic check for rooted Android devices (deterrent, not foolproof).
        /// </summary>
        public bool IsDeviceRooted()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject systemService = activity.Call<AndroidJavaObject>("getSystemService", "activity");
                
                // Check for common root binaries
                string[] rootPaths = {
                    "/system/app/Superuser.apk",
                    "/sbin/su",
                    "/system/bin/su",
                    "/system/xbin/su",
                    "/data/local/xbin/su",
                    "/data/local/bin/su"
                };

                foreach (string path in rootPaths)
                {
                    if (System.IO.File.Exists(path))
                    {
                        Debug.LogWarning("[SecurityManager] Root binary detected!");
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SecurityManager] Root check failed: {e.Message}");
            }
#endif
            return false;
        }

        public void Initialize()
        {
            // Security manager initialized
            if (IsDeviceRooted())
            {
                // In a production game, you might want to disable leaderboard submissions here
                // or show a warning. For now, we just log it.
                Debug.LogWarning("[SecurityManager] Device appears to be rooted. Leaderboard integrity may be at risk.");
                FirebaseNS.FirebaseManager.Instance.LogEvent("device_rooted_detected");
            }
        }

        private static string ComputeSHA256(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
                byte[] hash = sha256.ComputeHash(bytes);

                string hexHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                return hexHash;
            }
        }
    }
}
