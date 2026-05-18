using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using SYMVOLTA.Core;
using SYMVOLTA.Leaderboard;
using SYMVOLTA.Security;
using SYMVOLTA.Shapes;
using SYMVOLTA.Utilities;

namespace SYMVOLTA.Networking
{
    [Serializable]
    public class PendingScore
    {
        public string uid;
        public string username;
        public int shape;
        public float score;
        public float globalScore;
        public string rankTitle;
        public long timestamp;
        public float matchDuration;
        public string hash;

        public ShapeType ShapeType => (ShapeType)shape;
    }

    [Serializable]
    public class PendingScoreQueue
    {
        public List<PendingScore> scores = new List<PendingScore>();
    }

    public class OfflineSyncManager : Singleton<OfflineSyncManager>
    {
        private PendingScoreQueue _queue = new PendingScoreQueue();
        private bool _isSyncing;

        public int PendingCount => _queue?.scores?.Count ?? 0;
        public event Action<int> OnQueueChanged;

        public void Initialize()
        {
            _queue = SaveSystem.LoadData<PendingScoreQueue>(Constants.SaveKeys.OFFLINE_QUEUE);
            if (_queue == null || _queue.scores == null)
                _queue = new PendingScoreQueue();
        }

        public void Enqueue(PendingScore score)
        {
            if (score == null) return;
            _queue.scores.Add(score);
            Save();
        }

        public async Task SyncPendingScores()
        {
            if (_isSyncing || PendingCount == 0) return;
            if (GameManager.Instance == null || GameManager.Instance.IsOfflineMode) return;
            if (LeaderboardManager.Instance == null) return;

            _isSyncing = true;
            try
            {
                List<PendingScore> accepted = new List<PendingScore>();

                foreach (PendingScore pending in _queue.scores)
                {
                    if (!SecurityManager.Instance.ValidatePendingScore(pending))
                        continue;

                    bool submitted = await LeaderboardManager.Instance.SubmitScoreRecord(pending);
                    if (submitted)
                        accepted.Add(pending);
                }

                for (int i = 0; i < accepted.Count; i++)
                    _queue.scores.Remove(accepted[i]);

                Save();
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private void Save()
        {
            SaveSystem.SaveData(Constants.SaveKeys.OFFLINE_QUEUE, _queue);
            OnQueueChanged?.Invoke(PendingCount);
        }
    }
}
