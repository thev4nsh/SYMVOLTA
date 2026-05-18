using System;
using System.Collections.Generic;
using UnityEngine;
using SYMVOLTA.Shapes;

namespace SYMVOLTA.Future
{
    [Serializable]
    public struct StrokeFrame
    {
        public float time;
        public Vector2 position;
        public float pressure;
    }

    [Serializable]
    public class ReplayData
    {
        public string replayId;
        public string uid;
        public ShapeType shape;
        public float score;
        public List<StrokeFrame> frames = new List<StrokeFrame>();
    }

    public interface IReplayRecorder
    {
        void Begin(ShapeType shape);
        void Record(Vector2 worldPosition, float pressure);
        ReplayData End(float finalScore);
    }

    public interface IGhostDrawingProvider
    {
        ReplayData GetBestGhost(ShapeType shape);
    }

    public interface IDailyChallengeProvider
    {
        string CurrentChallengeId { get; }
        ShapeType CurrentShape { get; }
    }

    public interface ISeasonProvider
    {
        string CurrentSeasonId { get; }
        DateTimeOffset EndsAt { get; }
    }
}
