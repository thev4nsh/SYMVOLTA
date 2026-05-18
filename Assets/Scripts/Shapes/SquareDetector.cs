using System.Collections.Generic;
using UnityEngine;
using SYMVOLTA.Utilities;

namespace SYMVOLTA.Shapes
{
    public static class SquareDetector
    {
        public static float CalculateAccuracy(List<Vector2> points)
        {
            if (points.Count < 10) return 0f;
            int corners = FindCorners(points).Count;
            float cornerScore = corners == 4 ? 1f : Mathf.Max(0f, 1f - Mathf.Abs(corners - 4) * 0.25f);
            float angleScore = CalcAngleScore(points);
            float accuracy = (cornerScore * 0.4f) + (angleScore * 0.6f);
            return Mathf.Clamp01(accuracy) * 100f;
        }

        public static float CalculateRealtimeAccuracy(List<Vector2> points, float progress)
        {
            if (points.Count < 5) return 0f;
            int corners = FindCorners(points).Count;
            float cornerScore = corners >= 2 ? 0.5f : 0.1f;
            return cornerScore * 100f * Mathf.Lerp(0.3f, 1f, progress);
        }

        private static List<Vector2> FindCorners(List<Vector2> points)
        {
            points = MathUtils.SimplifyPoints(points, 0.075f);
            List<Vector2> corners = new List<Vector2>();
            for (int i = 1; i < points.Count - 1; i++)
            {
                Vector2 prev = (points[i] - points[i - 1]).normalized;
                Vector2 next = (points[i + 1] - points[i]).normalized;
                if (Vector2.Angle(prev, next) > 48f) corners.Add(points[i]);
            }
            return corners;
        }

        private static float CalcAngleScore(List<Vector2> points)
        {
            List<Vector2> corners = FindCorners(points);
            if (corners.Count < 3) return 0f;
            float totalDev = 0f;
            int checks = Mathf.Min(corners.Count, 4);
            for (int i = 0; i < checks; i++)
            {
                Vector2 a = corners[i % corners.Count];
                Vector2 b = corners[(i + 1) % corners.Count];
                Vector2 c = corners[(i + 2) % corners.Count];
                float angle = Vector2.Angle((a - b).normalized, (c - b).normalized);
                totalDev += Mathf.Abs(angle - 90f);
            }
            return Mathf.Clamp01(1f - (totalDev / 180f));
        }
    }
}
