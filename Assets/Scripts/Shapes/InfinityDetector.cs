using System.Collections.Generic;
using UnityEngine;
using SYMVOLTA.Utilities;

namespace SYMVOLTA.Shapes
{
    public static class InfinityDetector
    {
        public static float CalculateAccuracy(List<Vector2> points)
        {
            if (points.Count < 15) return 0f;
            float loopScore = CalcLoopScore(points);
            float crossoverScore = CalcCrossoverScore(points);
            float accuracy = (loopScore * 0.6f) + (crossoverScore * 0.4f);
            return Mathf.Clamp01(accuracy) * 100f;
        }

        public static float CalculateRealtimeAccuracy(List<Vector2> points, float progress)
        {
            if (points.Count < 5) return 0f;
            return CalcLoopScore(points) * 100f * Mathf.Lerp(0.2f, 1f, progress);
        }

        private static float CalcLoopScore(List<Vector2> points)
        {
            // Check if the path crosses itself by looking at line segment intersections
            int intersections = 0;
            for (int i = 1; i < points.Count - 2; i += 3) // Skip some points for performance
            {
                for (int j = i + 2; j < points.Count - 1; j += 3)
                {
                    if (LineIntersection(points[i - 1], points[i], points[j - 1], points[j])) intersections++;
                }
            }
            if (intersections >= 1) return 1f;
            return 0.1f;
        }

        private static float CalcCrossoverScore(List<Vector2> points)
        {
            // Infinity should have a crossover near the horizontal center
            Vector2 centroid = MathUtils.GetCentroid(points);
            int nearCenter = 0;
            foreach (var p in points)
            {
                if (Vector2.Distance(p, centroid) < 0.1f) nearCenter++;
            }
            return Mathf.Clamp01(nearCenter / 10f); // At least a few points should cross the center
        }

        private static bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            float d1 = Direction(p3, p4, p1);
            float d2 = Direction(p3, p4, p2);
            float d3 = Direction(p1, p2, p3);
            float d4 = Direction(p1, p2, p4);
            if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) && ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0))) return true;
            return false;
        }

        private static float Direction(Vector2 a, Vector2 b, Vector2 c)
        {
            return (c.x - a.x) * (b.y - a.y) - (b.x - a.x) * (c.y - a.y);
        }
    }
}