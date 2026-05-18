using UnityEngine;
using SYMVOLTA.Core;

namespace SYMVOLTA.UI
{
    public class ThemeManager : Singleton<ThemeManager>
    {
        [Header("Rank Colors")]
        public Color beginnerColor = new Color(0.6f, 0.6f, 0.7f);   // Dim Grey
        public Color rookieColor = new Color(0.8f, 0.8f, 0.8f);     // White
        public Color skilledColor = new Color(0f, 1f, 1f);           // Cyan
        public Color proColor = new Color(0f, 0.8f, 1f);            // Soft Blue
        public Color eliteColor = new Color(0.2f, 0.6f, 1f);        // Bright Blue
        public Color masterColor = new Color(0.6f, 0.2f, 1f);       // Purple Neon
        public Color legendColor = new Color(1f, 0.2f, 0.2f);       // Red Neon
        public Color godlikeColor = new Color(1f, 0.8f, 0f);        // Gold
        public Color perfectBeingColor = new Color(1f, 1f, 1f);     // Pure White Neon

        /// <summary>
        /// Returns the corresponding neon color for a player's rank title.
        /// Used by UI elements to dynamically glow the correct color.
        /// </summary>
        public Color GetRankColor(string rankTitle)
        {
            return rankTitle switch
            {
                "Rookie" => rookieColor,
                "Skilled" => skilledColor,
                "Pro" => proColor,
                "Elite" => eliteColor,
                "Master" => masterColor,
                "Legend" => legendColor,
                "Godlike" => godlikeColor,
                "Perfect Being" => perfectBeingColor,
                _ => beginnerColor // Default fallback
            };
        }
    }
}