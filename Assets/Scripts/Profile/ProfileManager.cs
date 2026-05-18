using SYMVOLTA.Core;
using SYMVOLTA.FirebaseNS;
using SYMVOLTA.Shapes;
using SYMVOLTA.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Firestore;
using SYMVOLTA.Networking;

namespace SYMVOLTA.Profile
{
    /// <summary>
    /// Manages the player's local and cloud profile data.
    /// </summary>
    public class ProfileManager : Singleton<ProfileManager>
    {
        private PlayerProfile _currentProfile;
        public PlayerProfile CurrentProfile => _currentProfile;

        public event Action<PlayerProfile> OnProfileLoaded;
        public event Action<PlayerProfile> OnProfileUpdated;

        /// <summary>
        /// Loads the profile from local save. If none exists, creates a new one.
        /// Called by GameManager during boot.
        /// </summary>
        public async Task LoadProfile()
        {
            // Running on main thread to prevent Unity API threading exceptions.
            // Local JSON parsing is fast enough that we don't need Task.Run here.

            if (SaveSystem.SaveExists(Constants.SaveKeys.PROFILE))
            {
                _currentProfile = SaveSystem.LoadData<PlayerProfile>(Constants.SaveKeys.PROFILE);

                if (_currentProfile == null)
                {
                    Debug.LogWarning("[ProfileManager] Save file was corrupt. Creating new profile.");
                    _currentProfile = CreateNewProfile();
                }
                else
                {
                    Debug.Log($"[ProfileManager] Profile loaded: {_currentProfile.username}");
                }
            }
            else
            {
                Debug.Log("[ProfileManager] No save found. Creating new profile.");
                _currentProfile = CreateNewProfile();
            }

            // Link Firebase UID if available
            if (SYMVOLTA.FirebaseNS.FirebaseManager.Instance.IsFirebaseReady &&
                SYMVOLTA.FirebaseNS.FirebaseManager.Instance.Auth.CurrentUser != null &&
                _currentProfile.uid != SYMVOLTA.FirebaseNS.FirebaseManager.Instance.Auth.CurrentUser.UserId)
            {
                _currentProfile.uid = SYMVOLTA.FirebaseNS.FirebaseManager.Instance.Auth.CurrentUser.UserId;
                SaveProfileLocally();
            }

            OnProfileLoaded?.Invoke(_currentProfile);

            if (!GameManager.Instance.IsOfflineMode && FirebaseManager.Instance.IsFirebaseReady)
            {
                await LoadCloudProfile();
                await EnsureUsernameReservation();
                await SaveProfileToCloud();
            }

            await Task.CompletedTask; // Keep the async signature so GameManager can await it
        }
        private PlayerProfile CreateNewProfile()
        {
            PlayerProfile newProfile = new PlayerProfile
            {
                uid = System.Guid.NewGuid().ToString(), // Temporary local UID
                username = $"Player_{UnityEngine.Random.Range(1000, 9999)}",
                isFirstLaunch = true
            };

            _currentProfile = newProfile;
            SaveProfileLocally();
            return newProfile;
        }

        public void SaveProfileLocally()
        {
            if (_currentProfile == null) return;
            SaveSystem.SaveData(Constants.SaveKeys.PROFILE, _currentProfile);
        }

        /// <summary>
        /// Updates the profile with a new shape accuracy score.
        /// Validates the score and applies weighted calculations.
        /// </summary>
        public bool SubmitScore(SYMVOLTA.Shapes.ShapeType shape, float accuracy)
        {
            if (_currentProfile == null) return false;

            // Validate score
            if (!Security.SecurityManager.Instance.ValidateScore(accuracy, shape))
            {
                Debug.LogWarning("[ProfileManager] Score rejected by Security Manager.");
                return false;
            }

            // Update score (this automatically recalculates global score and rank title)
            ShapeWeights weights = GameManager.Instance.Config.shapeWeights;
            _currentProfile.UpdateShapeScore(shape, accuracy, weights);

            SaveProfileLocally();
            OnProfileUpdated?.Invoke(_currentProfile);

            PendingScore pendingScore = Security.SecurityManager.Instance.CreateScoreRecord(_currentProfile, shape, accuracy);

            if (GameManager.Instance != null && GameManager.Instance.IsOfflineMode)
            {
                OfflineSyncManager.Instance.Enqueue(pendingScore);
            }
            else
            {
                _ = SaveProfileToCloud();
            }

            Debug.Log($"[ProfileManager] Score submitted! Shape: {shape}, Accuracy: {accuracy:F1}%, Global Score: {_currentProfile.globalScore:F1}%, Rank: {_currentProfile.rankTitle}");
            return true;
        }

        public void UpdateUsername(string newName)
        {
            if (_currentProfile == null) return;
            _currentProfile.SetUsername(newName);
            SaveProfileLocally();
            OnProfileUpdated?.Invoke(_currentProfile);
            _ = SaveProfileToCloud();
        }

        public async Task<bool> TryUpdateUsernameAsync(string newName, bool markFirstLaunchComplete = false)
        {
            if (_currentProfile == null) return false;

            string normalized = NormalizeUsername(newName);
            if (normalized.Length < 3 || normalized.Length > 20) return false;

            if (!GameManager.Instance.IsOfflineMode && FirebaseManager.Instance.IsFirebaseReady)
            {
                bool reserved = await ReserveUsername(normalized);
                if (!reserved) return false;
            }

            _currentProfile.SetUsername(normalized);
            if (markFirstLaunchComplete)
                _currentProfile.isFirstLaunch = false;

            SaveProfileLocally();
            await SaveProfileToCloud();
            OnProfileUpdated?.Invoke(_currentProfile);
            return true;
        }

        public void MarkFirstLaunchComplete()
        {
            if (_currentProfile == null) return;
            _currentProfile.isFirstLaunch = false;
            SaveProfileLocally();
        }

        private async Task LoadCloudProfile()
        {
            try
            {
                if (_currentProfile == null || string.IsNullOrEmpty(_currentProfile.uid)) return;
                DocumentReference docRef = FirebaseManager.Instance.Db.Collection(Constants.Firestore.USERS).Document(_currentProfile.uid);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
                if (!snapshot.Exists) return;

                _currentProfile.ApplyDictionary(snapshot.ToDictionary(), GameManager.Instance.Config.shapeWeights);
                SaveProfileLocally();
                OnProfileUpdated?.Invoke(_currentProfile);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ProfileManager] Cloud profile load failed: {e.Message}");
            }
        }

        public async Task SaveProfileToCloud()
        {
            if (_currentProfile == null) return;
            if (GameManager.Instance != null && GameManager.Instance.IsOfflineMode) return;
            if (!FirebaseManager.Instance.IsFirebaseReady) return;

            try
            {
                DocumentReference docRef = FirebaseManager.Instance.Db.Collection(Constants.Firestore.USERS).Document(_currentProfile.uid);
                await docRef.SetAsync(_currentProfile.ToDictionary(), SetOptions.MergeAll);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ProfileManager] Cloud profile save failed: {e.Message}");
            }
        }

        private async Task EnsureUsernameReservation()
        {
            if (_currentProfile == null || string.IsNullOrWhiteSpace(_currentProfile.username)) return;
            await ReserveUsername(_currentProfile.username);
        }

        private async Task<bool> ReserveUsername(string username)
        {
            string normalized = NormalizeUsername(username);
            string key = normalized.ToLowerInvariant();
            DocumentReference docRef = FirebaseManager.Instance.Db.Collection(Constants.Firestore.USERNAMES).Document(key);

            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                Dictionary<string, object> data = snapshot.ToDictionary();
                string owner = data.TryGetValue("uid", out object ownerObj) ? ownerObj?.ToString() : "";
                if (!string.Equals(owner, _currentProfile.uid, StringComparison.Ordinal))
                    return false;
            }

            Dictionary<string, object> usernameDoc = new Dictionary<string, object>
            {
                { "uid", _currentProfile.uid },
                { "username", normalized },
                { "updatedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
            };
            await docRef.SetAsync(usernameDoc, SetOptions.MergeAll);
            return true;
        }

        private string NormalizeUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return "";
            string trimmed = username.Trim();
            char[] chars = trimmed.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                if (!char.IsLetterOrDigit(c) && c != '_')
                    chars[i] = '_';
            }
            return new string(chars);
        }
    }
}
