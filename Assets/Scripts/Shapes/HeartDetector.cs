using System.Collections.Generic;
using UnityEngine;
using SYMVOLTA.Utilities;

namespace SYMVOLTA.Shapes
{
    public static class HeartDetector
    {
        public static float CalculateAccuracy(List<Vector2> points)
        {
            if (points.Count < 10) return 0f;
            float symmetryScore = CalcSymmetryScore(points);
            float bottomScore = CalcBottomPointScore(points);
            float accuracy = (symmetryScore * 0.6f) + (bottomScore * 0.4f);
            return Mathf.Clamp01(accuracy) * 100f;
        }

        public static float CalculateRealtimeAccuracy(List<Vector2> points, float progress)
        {
            if (points.Count < 5) return 0f;
            return CalcSymmetryScore(points) * 100f * Mathf.Lerp(0.2f, 1f, progress);
        }

        private static float CalcSymmetryScore(List<Vector2> points)
        {
            float minX = float.MaxValue, maxX = float.MinValue;
            foreach (var p in points) { minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x); }
            float centerX = (minX + maxX) / 2f;

            float error = 0f;
            int checkedCount = 0;
            int outerStride = Mathf.Max(1, points.Count / 80);
            int innerStride = Mathf.Max(1, points.Count / 120);

            for (int i = 0; i < points.Count; i += outerStride)
            {
                Vector2 p = points[i];
                Vector2 mirrored = new Vector2(2f * centerX - p.x, p.y);
                float minDist = float.MaxValue;
                for (int j = 0; j < points.Count; j += innerStride)
                {
                    Vector2 o = points[j];
                    minDist = Mathf.Min(minDist, Vector2.Distance(mirrored, o));
                }
                error += minDist;
                checkedCount++;
            }
            return checkedCount == 0 ? 0f : Mathf.Clamp01(1f - (error / checkedCount) * 5f);
        }

        private static float CalcBottomPointScore(List<Vector2> points)
        {
            Vector2 centroid = MathUtils.GetCentroid(points);
            float lowestY = float.MaxValue;
            foreach (var p in points) lowestY = Mathf.Min(lowestY, p.y);

            // Bottom point should be roughly centered horizontally
            Vector2 bottomPoint = Vector2.zero;
            foreach (var p in points) { if (p.y == lowestY) bottomPoint = p; }

            return Mathf.Clamp01(1f - Mathf.Abs(bottomPoint.x - centroid.x) * 5f);
        }
    }
}
