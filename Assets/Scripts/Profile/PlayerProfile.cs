using System;
using System.Collections.Generic;
using SYMVOLTA.Shapes;
using SYMVOLTA.Core;
namespace SYMVOLTA.Profile
{
    [Serializable]
    public class PlayerProfile
    {
        public string uid;
        public string username;
        public int profileIconIndex;
        public float globalScore;
        public int worldRank;
        public string rankTitle;
        public AllShapeScores shapeScores;
        public bool isFirstLaunch;

        public PlayerProfile()
        {
            uid = "";
            username = "";
            profileIconIndex = 0;
            globalScore = 0f;
            worldRank = -1; // -1 means unranked
            rankTitle = "Beginner";
            shapeScores = new AllShapeScores();
            isFirstLaunch = true;
        }

        public void SetUsername(string newName)
        {
            username = newName;
        }

        public void UpdateShapeScore(SYMVOLTA.Shapes.ShapeType type, float accuracy, ShapeWeights weights)
        {
            shapeScores.UpdateScore(type, accuracy, weights);
            RecalculateGlobalScore(weights);
            rankTitle = CalculateRankTitle(globalScore);
        }

        public void RecalculateGlobalScore(ShapeWeights weights)
        {
            globalScore = shapeScores.GetGlobalScore(weights);
        }

        public static string CalculateRankTitle(float score)
        {
            if (score >= 99.5f) return "Perfect Being";
            if (score >= 98f) return "Godlike";
            if (score >= 95f) return "Legend";
            if (score >= 90f) return "Master";
            if (score >= 80f) return "Elite";
            if (score >= 70f) return "Pro";
            if (score >= 60f) return "Skilled";
            if (score >= 50f) return "Rookie";
            return "Beginner";
        }

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { "uid", uid },
                { "username", username },
                { "profileIconIndex", profileIconIndex },
                { "globalScore", globalScore },
                { "worldRank", worldRank },
                { "rankTitle", rankTitle },
                { "shapeScores", shapeScores.ToDictionary() },
                { "updatedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
            };
        }

        public void ApplyDictionary(Dictionary<string, object> data, ShapeWeights weights)
        {
            if (data == null) return;

            if (data.TryGetValue("uid", out object uidObj)) uid = uidObj?.ToString();
            if (data.TryGetValue("username", out object usernameObj)) username = usernameObj?.ToString();
            if (data.TryGetValue("profileIconIndex", out object iconObj)) profileIconIndex = Convert.ToInt32(iconObj);
            if (data.TryGetValue("worldRank", out object worldRankObj)) worldRank = Convert.ToInt32(worldRankObj);

            if (data.TryGetValue("shapeScores", out object scoresObj))
            {
                if (scoresObj is Dictionary<string, object> scoreDict)
                    shapeScores.ApplyDictionary(scoreDict);
                else if (scoresObj is IDictionary<string, object> genericDict)
                    shapeScores.ApplyDictionary(new Dictionary<string, object>(genericDict));
            }

            RecalculateGlobalScore(weights);
            rankTitle = CalculateRankTitle(globalScore);
        }
    }
}
