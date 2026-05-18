using System.Collections.Generic;
using UnityEngine;

namespace SYMVOLTA.Utilities
{
    /// <summary>
    /// Mathematical utilities for 2D geometry, point processing, and shape analysis.
    /// Critical for smoothing touch input and calculating drawing accuracy.
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// Applies a moving average filter to smooth out touch jitter.
        /// </summary>
        public static List<Vector2> SmoothPoints(List<Vector2> points, int iterations = 1)
        {
            if (points == null) return new List<Vector2>();
            if (points.Count < 3) return new List<Vector2>(points);

            List<Vector2> currentPoints = new List<Vector2>(points);

            for (int iter = 0; iter < iterations; iter++)
            {
                List<Vector2> smoothed = new List<Vector2> { currentPoints[0] }; // Keep first point

                for (int i = 1; i < currentPoints.Count - 1; i++)
                {
                    Vector2 prev = currentPoints[i - 1];
                    Vector2 curr = currentPoints[i];
                    Vector2 next = currentPoints[i + 1];

                    // Weighted average (center point has highest weight)
                    Vector2 smoothPoint = (prev + curr * 2f + next) / 4f;
                    smoothed.Add(smoothPoint);
                }

                smoothed.Add(currentPoints[currentPoints.Count - 1]); // Keep last point
                currentPoints = smoothed;
            }

            return currentPoints;
        }

        /// <summary>
        /// Normalizes points to a 0-1 bounding box. 
        /// This allows us to compare shapes regardless of how big or small the player drew them.
        /// </summary>
        public static List<Vector2> NormalizePoints(List<Vector2> points)
        {
            if (points == null) return new List<Vector2>();
            if (points.Count < 2) return new List<Vector2>(points);

            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;

            foreach (var p in points)
            {
                minX = Mathf.Min(minX, p.x);
                maxX = Mathf.Max(maxX, p.x);
                minY = Mathf.Min(minY, p.y);
                maxY = Mathf.Max(maxY, p.y);
            }

            float width = maxX - minX;
            float height = maxY - minY;
            float scale = Mathf.Max(width, height);

            if (scale < 0.001f) return new List<Vector2>(points);

            List<Vector2> normalized = new List<Vector2>(points.Count);
            foreach (var p in points)
            {
                normalized.Add(new Vector2(
                    (p.x - minX) / scale,
                    (p.y - minY) / scale
                ));
            }

            return normalized;
        }

        /// <summary>
        /// Finds the geometric center of a set of points.
        /// </summary>
        public static Vector2 GetCentroid(List<Vector2> points)
        {
            if (points == null || points.Count == 0) return Vector2.zero;

            float totalX = 0f;
            float totalY = 0f;

            for (int i = 0; i < points.Count; i++)
            {
                totalX += points[i].x;
                totalY += points[i].y;
            }

            return new Vector2(totalX / points.Count, totalY / points.Count);
        }

        /// <summary>
        /// Calculates the total length of the line connecting all points.
        /// </summary>
        public static float CalculatePerimeter(List<Vector2> points)
        {
            if (points == null || points.Count < 2) return 0f;

            float perimeter = 0f;
            for (int i = 1; i < points.Count; i++)
            {
                perimeter += Vector2.Distance(points[i - 1], points[i]);
            }

            return perimeter;
        }

        /// <summary>
        /// Calculates the area of a polygon using the Shoelace formula.
        /// </summary>
        public static float CalculateArea(List<Vector2> points)
        {
            if (points == null || points.Count < 3) return 0f;

            float area = 0f;
            int n = points.Count;

            for (int i = 0; i < n; i++)
            {
                Vector2 current = points[i];
                Vector2 next = points[(i + 1) % n];
                area += (current.x * next.y) - (next.x * current.y);
            }

            return Mathf.Abs(area / 2f);
        }

        /// <summary>
        /// Calculates the shortest distance from a point to an infinite line defined by two points.
        /// Used heavily in detecting straight edges (Triangles, Squares).
        /// </summary>
        public static float PointToLineDistance(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            float dx = lineEnd.x - lineStart.x;
            float dy = lineEnd.y - lineStart.y;

            // If line is actually just a single point
            if (dx == 0f && dy == 0f)
            {
                return Vector2.Distance(point, lineStart);
            }

            // Calculate the distance using the 2D cross product formula
            float numerator = Mathf.Abs(dy * point.x - dx * point.y + lineEnd.x * lineStart.y - lineEnd.y * lineStart.x);
            float denominator = Mathf.Sqrt(dx * dx + dy * dy);

            return numerator / denominator;
        }

        /// <summary>
        /// Reduces the number of points in a line while preserving its shape (Ramer-Douglas-Peucker algorithm).
        /// Used to optimize LineRenderer performance and find sharp corners.
        /// </summary>
        public static List<Vector2> SimplifyPoints(List<Vector2> points, float tolerance = 0.01f)
        {
            if (points == null) return new List<Vector2>();
            if (points.Count < 3) return new List<Vector2>(points);

            float maxDistance = 0f;
            int maxIndex = 0;

            Vector2 start = points[0];
            Vector2 end = points[points.Count - 1];

            // Find the point with the maximum distance from the line between start and end
            for (int i = 1; i < points.Count - 1; i++)
            {
                float dist = PointToLineDistance(points[i], start, end);
                if (dist > maxDistance)
                {
                    maxDistance = dist;
                    maxIndex = i;
                }
            }

            // If max distance is greater than tolerance, recursively simplify
            if (maxDistance > tolerance)
            {
                List<Vector2> left = SimplifyPoints(points.GetRange(0, maxIndex + 1), tolerance);
                List<Vector2> right = SimplifyPoints(points.GetRange(maxIndex, points.Count - maxIndex), tolerance);

                List<Vector2> result = new List<Vector2>(left);
                result.AddRange(right.GetRange(1, right.Count - 1)); // Skip duplicate point at junction
                return result;
            }
            else
            {
                // All points in between are within tolerance, discard them
                return new List<Vector2> { start, end };
            }
        }
    }
}
