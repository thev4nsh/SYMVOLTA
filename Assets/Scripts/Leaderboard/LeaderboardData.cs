using System;
using System.Collections.Generic;

namespace SYMVOLTA.Leaderboard
{
    [Serializable]
    public class LeaderboardEntry
    {
        public string uid;
        public string username;
        public float score;
        public int rank;
        public string rankTitle;
        public long timestamp;
        public bool isPlayerEntry;

        public LeaderboardEntry() { }

        public LeaderboardEntry(string uid, string username, float score, string rankTitle)
        {
            this.uid = uid;
            this.username = username;
            this.score = score;
            this.rankTitle = rankTitle;
            this.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { "uid", uid },
                { "username", username },
                { "score", score },
                { "rankTitle", rankTitle },
                { "timestamp", timestamp }
            };
        }

        public static LeaderboardEntry FromDictionary(Dictionary<string, object> dict)
        {
            LeaderboardEntry entry = new LeaderboardEntry
            {
                uid = dict.ContainsKey("uid") ? dict["uid"]?.ToString() : "",
                username = dict.ContainsKey("username") ? dict["username"]?.ToString() : "Unknown",
                score = dict.ContainsKey("score") ? Convert.ToSingle(dict["score"]) : 0f,
                rankTitle = dict.ContainsKey("rankTitle") ? dict["rankTitle"]?.ToString() : "Beginner",
                timestamp = dict.ContainsKey("timestamp") ? Convert.ToInt64(dict["timestamp"]) : 0
            };
            return entry;
        }
    }

    [Serializable]
    public class LeaderboardBoard
    {
        public string boardId; // e.g., "lb_circle" or "overall"
        public List<LeaderboardEntry> topEntries; // Top 100
        public LeaderboardEntry playerEntry;

        public LeaderboardBoard()
        {
            topEntries = new List<LeaderboardEntry>();
        }

        public void SortAndRank()
        {
            // Sort descending by score
            topEntries.Sort((a, b) => b.score.CompareTo(a.score));

            // Assign ranks
            for (int i = 0; i < topEntries.Count; i++)
            {
                topEntries[i].rank = i + 1;
            }
        }

        public void TrimToSize(int maxSize)
        {
            if (topEntries.Count > maxSize)
            {
                topEntries.RemoveRange(maxSize, topEntries.Count - maxSize);
            }
        }
    }
}
