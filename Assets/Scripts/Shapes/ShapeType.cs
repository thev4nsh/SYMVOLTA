namespace SYMVOLTA.Shapes
{
    /// <summary>
    /// Defines all available shape modes in the game.
    /// </summary>
    public enum ShapeType
    {
        Circle = 0,
        Triangle = 1,
        Square = 2,
        Star = 3,
        Heart = 4,
        Infinity = 5
    }

    /// <summary>
    /// Extension methods for ShapeType to get display names and leaderboard IDs.
    /// </summary>
    public static class ShapeTypeExtensions
    {
        public static string DisplayName(this ShapeType type)
        {
            return type switch
            {
                ShapeType.Circle => "CIRCLE",
                ShapeType.Triangle => "TRIANGLE",
                ShapeType.Square => "SQUARE",
                ShapeType.Star => "STAR",
                ShapeType.Heart => "HEART",
                ShapeType.Infinity => "INFINITY",
                _ => "UNKNOWN"
            };
        }

        public static string LeaderboardId(this ShapeType type)
        {
            return type switch
            {
                ShapeType.Circle => "lb_circle",
                ShapeType.Triangle => "lb_triangle",
                ShapeType.Square => "lb_square",
                ShapeType.Star => "lb_star",
                ShapeType.Heart => "lb_heart",
                ShapeType.Infinity => "lb_infinity",
                _ => "lb_unknown"
            };
        }
    }
}