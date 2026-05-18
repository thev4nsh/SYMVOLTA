using System.Collections.Generic;
using UnityEngine;
using SYMVOLTA.Utilities;

namespace SYMVOLTA.Shapes
{
    public static class StarDetector
    {
        public static float CalculateAccuracy(List<Vector2> points)
        {
            if (points.Count < 15) return 0f;
            float radialConsistency = CalcRadialConsistency(points);
            float pointScore = CalcPointScore(points);
            float accuracy = (radialConsistency * 0.5f) + (pointScore * 0.5f);
            return Mathf.Clamp01(accuracy) * 100f;
        }

        public static float CalculateRealtimeAccuracy(List<Vector2> points, float progress)
        {
            if (points.Count < 5) return 0f;
            return CalcRadialConsistency(points) * 100f * Mathf.Lerp(0.2f, 1f, progress);
        }

        private static float CalcRadialConsistency(List<Vector2> points)
        {
            Vector2 center = MathUtils.GetCentroid(points);
            float maxDist = 0f, minDist = float.MaxValue;
            foreach (var p in points) { maxDist = Mathf.Max(maxDist, Vector2.Distance(p, center)); minDist = Mathf.Min(minDist, Vector2.Distance(p, center)); }
            if (maxDist < 0.01f) return 0f;

            float radialContrast = (maxDist - minDist) / maxDist;
            return Mathf.InverseLerp(0.25f, 0.65f, radialContrast);
        }

        private static float CalcPointScore(List<Vector2> points)
        {
            Vector2 center = MathUtils.GetCentroid(points);
            List<Vector2> peaks = new List<Vector2>();
            for (int i = 1; i < points.Count - 1; i++)
            {
                float distPrev = Vector2.Distance(points[i - 1], center);
                float distCurr = Vector2.Distance(points[i], center);
                float distNext = Vector2.Distance(points[i + 1], center);
                if (distCurr > distPrev && distCurr > distNext && distCurr > 0.5f) peaks.Add(points[i]);
            }
            int peakCount = peaks.Count;
            if (peakCount == 5) return 1f;
            return Mathf.Max(0f, 1f - Mathf.Abs(peakCount - 5) * 0.2f);
        }
    }
}
