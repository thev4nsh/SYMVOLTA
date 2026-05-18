using System.Collections.Generic;
using UnityEngine;
using SYMVOLTA.Utilities;

namespace SYMVOLTA.Shapes
{
    public static class ShapeDetector
    {
        public static float CalculateAccuracy(List<Vector2> drawnPoints, SYMVOLTA.Shapes.ShapeType targetShape)
        {
            if (drawnPoints == null || drawnPoints.Count < 10) return 0f;

            List<Vector2> smoothed = MathUtils.SmoothPoints(drawnPoints, 3);
            List<Vector2> normalized = MathUtils.NormalizePoints(smoothed);

            float accuracy = targetShape switch
            {
                SYMVOLTA.Shapes.ShapeType.Circle => CircleDetector.CalculateAccuracy(normalized),
                SYMVOLTA.Shapes.ShapeType.Triangle => TriangleDetector.CalculateAccuracy(normalized),
                SYMVOLTA.Shapes.ShapeType.Square => SquareDetector.CalculateAccuracy(normalized),
                SYMVOLTA.Shapes.ShapeType.Star => StarDetector.CalculateAccuracy(normalized),
                SYMVOLTA.Shapes.ShapeType.Heart => HeartDetector.CalculateAccuracy(normalized),
                SYMVOLTA.Shapes.ShapeType.Infinity => InfinityDetector.CalculateAccuracy(normalized),
                _ => 0f
            };

            return Mathf.Clamp(accuracy, 0f, 100f);
        }

        public static float CalculateRealtimeAccuracy(List<Vector2> drawnPoints, SYMVOLTA.Shapes.ShapeType targetShape, float progress)
        {
            if (drawnPoints == null || drawnPoints.Count < 5) return 0f;

            List<Vector2> smoothed = MathUtils.SmoothPoints(drawnPoints, 2);
            List<Vector2> normalized = MathUtils.NormalizePoints(smoothed);

            float accuracy = targetShape switch
            {
                SYMVOLTA.Shapes.ShapeType.Circle => CircleDetector.CalculateRealtimeAccuracy(normalized, progress),
                SYMVOLTA.Shapes.ShapeType.Triangle => TriangleDetector.CalculateRealtimeAccuracy(normalized, progress),
                SYMVOLTA.Shapes.ShapeType.Square => SquareDetector.CalculateRealtimeAccuracy(normalized, progress),
                SYMVOLTA.Shapes.ShapeType.Star => StarDetector.CalculateRealtimeAccuracy(normalized, progress),
                SYMVOLTA.Shapes.ShapeType.Heart => HeartDetector.CalculateRealtimeAccuracy(normalized, progress),
                SYMVOLTA.Shapes.ShapeType.Infinity => InfinityDetector.CalculateRealtimeAccuracy(normalized, progress),
                _ => 0f
            };

            return Mathf.Clamp(accuracy, 0f, 100f);
        }
    }
}
