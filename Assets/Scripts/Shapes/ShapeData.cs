using System;
using System.Collections.Generic;
using UnityEngine;
using SYMVOLTA.Core;

namespace SYMVOLTA.Shapes
{
    [Serializable]
    public class ShapeScore
    {
        public SYMVOLTA.Shapes.ShapeType type;
        public float bestAccuracy;
        public float weightedScore;
        public int attempts;
        public long timestamp;

        public ShapeScore()
        {
            type = SYMVOLTA.Shapes.ShapeType.Circle;
            bestAccuracy = 0f;
            weightedScore = 0f;
            attempts = 0;
            timestamp = 0;
        }

        public ShapeScore(SYMVOLTA.Shapes.ShapeType shapeType)
        {
            type = shapeType;
            bestAccuracy = 0f;
            weightedScore = 0f;
            attempts = 0;
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public void UpdateScore(float accuracy, float weight)
        {
            attempts++;
            if (accuracy > bestAccuracy)
            {
                bestAccuracy = accuracy;
                weightedScore = accuracy * weight;
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
        }

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { "type", type.ToString() },
                { "bestAccuracy", bestAccuracy },
                { "weightedScore", weightedScore },
                { "attempts", attempts },
                { "timestamp", timestamp }
            };
        }

        public static ShapeScore FromDictionary(Dictionary<string, object> data)
        {
            ShapeScore score = new ShapeScore();
            if (data == null) return score;

            if (data.TryGetValue("type", out object typeObj) &&
                Enum.TryParse(typeObj?.ToString(), out ShapeType parsedType))
                score.type = parsedType;

            if (data.TryGetValue("bestAccuracy", out object bestObj))
                score.bestAccuracy = Convert.ToSingle(bestObj);

            if (data.TryGetValue("weightedScore", out object weightedObj))
                score.weightedScore = Convert.ToSingle(weightedObj);

            if (data.TryGetValue("attempts", out object attemptsObj))
                score.attempts = Convert.ToInt32(attemptsObj);

            if (data.TryGetValue("timestamp", out object timestampObj))
                score.timestamp = Convert.ToInt64(timestampObj);

            return score;
        }
    }

    [Serializable]
    public class AllShapeScores
    {
        public List<ShapeScore> scores = new List<ShapeScore>();

        public AllShapeScores()
        {
            foreach (SYMVOLTA.Shapes.ShapeType type in Enum.GetValues(typeof(SYMVOLTA.Shapes.ShapeType)))
            {
                scores.Add(new ShapeScore(type));
            }
        }

        public ShapeScore GetScore(SYMVOLTA.Shapes.ShapeType type)
        {
            foreach (var s in scores)
            {
                if (s.type == type) return s;
            }

            ShapeScore created = new ShapeScore(type);
            scores.Add(created);
            return created;
        }

        public void UpdateScore(SYMVOLTA.Shapes.ShapeType type, float accuracy, ShapeWeights weights)
        {
            float weight = weights.GetWeight(type);
            ShapeScore score = GetScore(type);
            score.UpdateScore(accuracy, weight);

            // Replace in list
            for (int i = 0; i < scores.Count; i++)
            {
                if (scores[i].type == type)
                {
                    scores[i] = score;
                    break;
                }
            }
        }

        public float GetTotalWeightedScore()
        {
            float total = 0f;
            foreach (var s in scores)
            {
                total += s.weightedScore;
            }
            return total;
        }

        public float GetMaxPossibleWeightedScore(ShapeWeights weights)
        {
            float total = 0f;
            foreach (SYMVOLTA.Shapes.ShapeType type in Enum.GetValues(typeof(SYMVOLTA.Shapes.ShapeType)))
            {
                total += 100f * weights.GetWeight(type);
            }
            return total;
        }

        // The Global Score is a percentage of total weighted points achieved vs maximum possible
        public float GetGlobalScore(ShapeWeights weights)
        {
            float totalWeighted = GetTotalWeightedScore();
            float maxPossible = GetMaxPossibleWeightedScore(weights);
            if (maxPossible <= 0f) return 0f;
            return (totalWeighted / maxPossible) * 100f;
        }

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            foreach (ShapeScore score in scores)
                dict[score.type.ToString()] = score.ToDictionary();
            return dict;
        }

        public void ApplyDictionary(Dictionary<string, object> data)
        {
            if (data == null) return;

            foreach (KeyValuePair<string, object> pair in data)
            {
                if (!Enum.TryParse(pair.Key, out ShapeType type)) continue;

                Dictionary<string, object> scoreDict = pair.Value as Dictionary<string, object>;
                if (scoreDict == null && pair.Value is IDictionary<string, object> genericDict)
                    scoreDict = new Dictionary<string, object>(genericDict);
                if (scoreDict == null) continue;

                ShapeScore incoming = ShapeScore.FromDictionary(scoreDict);
                incoming.type = type;

                for (int i = 0; i < scores.Count; i++)
                {
                    if (scores[i].type == type)
                    {
                        scores[i] = incoming;
                        break;
                    }
                }
            }
        }
    }
}
