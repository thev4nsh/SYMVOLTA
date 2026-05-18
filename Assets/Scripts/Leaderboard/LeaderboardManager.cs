using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;
using SYMVOLTA.Core;
using SYMVOLTA.FirebaseNS;
using SYMVOLTA.Networking;
using SYMVOLTA.Profile;
using SYMVOLTA.Shapes;

namespace SYMVOLTA.Leaderboard
{
    public class LeaderboardManager : Singleton<LeaderboardManager>
    {
        public event Action<LeaderboardBoard> OnLeaderboardLoaded;

        public Task<LeaderboardBoard> FetchLeaderboard(ShapeType shapeType)
        {
            return FetchBoard(shapeType.LeaderboardId());
        }

        public Task<LeaderboardBoard> FetchOverallLeaderboard()
        {
            return FetchBoard("overall");
        }

        private async Task<LeaderboardBoard> FetchBoard(string boardId)
        {
            LeaderboardBoard board = new LeaderboardBoard { boardId = boardId };

            if (!FirebaseManager.Instance.IsFirebaseReady || GameManager.Instance.IsOfflineMode)
            {
                Debug.LogWarning("[LeaderboardManager] Fetch skipped. Offline or Firebase unavailable.");
                return board;
            }

            try
            {
                CollectionReference scoresRef = BoardScores(boardId);
                QuerySnapshot snapshot = await scoresRef
                    .OrderByDescending("score")
                    .Limit(Constants.LEADERBOARD_TOP_COUNT)
                    .GetSnapshotAsync();

                int rank = 1;
                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    LeaderboardEntry entry = EntryFromSnapshot(doc);
                    entry.rank = rank++;
                    board.topEntries.Add(entry);
                }

                await AttachPlayerEntry(board);
                OnLeaderboardLoaded?.Invoke(board);
            }
            catch (Exception e)
            {
                Debug.LogError($"[LeaderboardManager] Failed to fetch leaderboard {boardId}: {e.Message}");
            }

            return board;
        }

        public async Task SubmitScore(ShapeType shapeType, float accuracy)
        {
            try
            {
                if (!FirebaseManager.Instance.IsFirebaseReady || GameManager.Instance.IsOfflineMode) return;

                PlayerProfile profile = ProfileManager.Instance.CurrentProfile;
                if (profile == null) return;

                PendingScore record = Security.SecurityManager.Instance.CreateScoreRecord(profile, shapeType, accuracy);
                await SubmitScoreRecord(record);
            }
            catch (Exception e)
            {
                Debug.LogError($"[LeaderboardManager] SubmitScore failed: {e.Message}");
            }
        }

        public async Task<bool> SubmitScoreRecord(PendingScore record)
        {
            if (record == null) return false;
            if (!FirebaseManager.Instance.IsFirebaseReady || GameManager.Instance.IsOfflineMode) return false;
            if (!Security.SecurityManager.Instance.ValidatePendingScore(record)) return false;

            try
            {
                await UpdateBoard(record.ShapeType.LeaderboardId(), record, record.score);
                await UpdateBoard("overall", record, record.globalScore);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LeaderboardManager] SubmitScoreRecord failed: {e.Message}");
                return false;
            }
        }

        private async Task UpdateBoard(string boardId, PendingScore record, float score)
        {
            DocumentReference docRef = BoardScores(boardId).Document(record.uid);
            DocumentSnapshot current = await docRef.GetSnapshotAsync();

            if (current.Exists)
            {
                Dictionary<string, object> data = current.ToDictionary();
                float existing = data.TryGetValue("score", out object scoreObj) ? Convert.ToSingle(scoreObj) : 0f;
                if (existing >= score)
                {
                    await docRef.SetAsync(new Dictionary<string, object>
                    {
                        { "username", record.username },
                        { "rankTitle", record.rankTitle },
                        { "lastSeenAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
                    }, SetOptions.MergeAll);
                    return;
                }
            }

            Dictionary<string, object> payload = new Dictionary<string, object>
            {
                { "uid", record.uid },
                { "username", record.username },
                { "score", Mathf.Clamp(score, 0f, 100f) },
                { "rankTitle", record.rankTitle },
                { "shape", record.shape },
                { "timestamp", record.timestamp },
                { "hash", record.hash },
                { "clientVersion", Constants.GAME_VERSION }
            };

            await docRef.SetAsync(payload, SetOptions.MergeAll);
        }

        private async Task AttachPlayerEntry(LeaderboardBoard board)
        {
            string uid = ProfileManager.Instance?.CurrentProfile?.uid;
            if (string.IsNullOrEmpty(uid)) return;

            for (int i = 0; i < board.topEntries.Count; i++)
            {
                if (board.topEntries[i].uid == uid)
                {
                    board.playerEntry = board.topEntries[i];
                    board.playerEntry.isPlayerEntry = true;
                    return;
                }
            }

            DocumentSnapshot ownSnapshot = await BoardScores(board.boardId).Document(uid).GetSnapshotAsync();
            if (!ownSnapshot.Exists) return;

            LeaderboardEntry ownEntry = EntryFromSnapshot(ownSnapshot);
            QuerySnapshot higherScores = await BoardScores(board.boardId)
                .WhereGreaterThan("score", ownEntry.score)
                .GetSnapshotAsync();
            ownEntry.rank = higherScores.Count + 1;
            ownEntry.isPlayerEntry = true;
            board.playerEntry = ownEntry;
        }

        private CollectionReference BoardScores(string boardId)
        {
            return FirebaseManager.Instance.Db
                .Collection(Constants.Firestore.LEADERBOARDS)
                .Document(boardId)
                .Collection(Constants.Firestore.SCORES);
        }

        private LeaderboardEntry EntryFromSnapshot(DocumentSnapshot snapshot)
        {
            Dictionary<string, object> data = snapshot.ToDictionary();
            LeaderboardEntry entry = LeaderboardEntry.FromDictionary(data);
            if (string.IsNullOrEmpty(entry.uid))
                entry.uid = snapshot.Id;
            return entry;
        }

        public int GetPlayerRank(LeaderboardBoard board, string uid)
        {
            if (board == null || string.IsNullOrEmpty(uid)) return -1;
            if (board.playerEntry != null && board.playerEntry.uid == uid) return board.playerEntry.rank;

            foreach (LeaderboardEntry entry in board.topEntries)
            {
                if (entry.uid == uid) return entry.rank;
            }

            return -1;
        }
    }
}
