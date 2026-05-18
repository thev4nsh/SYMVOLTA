using System.Collections.Generic;
using UnityEngine;
using SYMVOLTA.Utilities;

namespace SYMVOLTA.Shapes
{
    public static class CircleDetector
    {
        public static float CalculateAccuracy(List<Vector2> points)
        {
            if (points.Count < 10) return 0f;
            float radiusConsistency = CalcRadiusConsistency(points);
            float closureScore = CalcClosureScore(points);
            float circularity = CalcCircularity(points);

            float accuracy = (radiusConsistency * 0.4f) + (closureScore * 0.2f) + (circularity * 0.4f);
            return Mathf.Clamp01(accuracy) * 100f;
        }

        public static float CalculateRealtimeAccuracy(List<Vector2> points, float progress)
        {
            if (points.Count < 5) return 0f;
            float radiusConsistency = CalcRadiusConsistency(points);
            float progressMultiplier = Mathf.Lerp(0.4f, 1f, progress);
            return Mathf.Clamp01(radiusConsistency) * 100f * progressMultiplier;
        }

        private static float CalcRadiusConsistency(List<Vector2> points)
        {
            Vector2 centroid = MathUtils.GetCentroid(points);
            float totalRadius = 0f;
            foreach (var p in points) totalRadius += Vector2.Distance(p, centroid);
            float avgRadius = totalRadius / points.Count;
            if (avgRadius < 0.01f) return 0f;

            float variance = 0f;
            foreach (var p in points) variance += Mathf.Pow(Vector2.Distance(p, centroid) - avgRadius, 2);
            float stdDev = Mathf.Sqrt(variance / points.Count);
            return Mathf.Clamp01(1f - (stdDev / avgRadius) * 2f);
        }

        private static float CalcClosureScore(List<Vector2> points)
        {
            float dist = Vector2.Distance(points[0], points[points.Count - 1]);
            float perimeter = MathUtils.CalculatePerimeter(points);
            if (perimeter < 0.01f) return 0f;
            return Mathf.Clamp01(1f - (dist / perimeter) * 10f);
        }

        private static float CalcCircularity(List<Vector2> points)
        {
            float perimeter = MathUtils.CalculatePerimeter(points);
            float area = MathUtils.CalculateArea(points);
            if (perimeter <= 0f) return 0f;
            return Mathf.Clamp01((4f * Mathf.PI * area) / (perimeter * perimeter));
        }
    }
}