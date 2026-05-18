using System;

namespace SYMVOLTA.Core
{
    /// <summary>
    /// Handles semantic version comparison to enforce mandatory game updates.
    /// </summary>
    public static class VersionChecker
    {
        /// <summary>
        /// Returns true if the current game version is LOWER than the minimum required version.
        /// </summary>
        public static bool IsUpdateRequired(string minVersion)
        {
            return CompareVersions(Constants.GAME_VERSION, minVersion) < 0;
        }

        /// <summary>
        /// Compares two version strings.
        /// Returns: -1 if a < b, 0 if equal, 1 if a > b
        /// </summary>
        public static int CompareVersions(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0;

            string[] partsA = a.Split('.');
            string[] partsB = b.Split('.');

            int maxParts = Math.Max(partsA.Length, partsB.Length);

            for (int i = 0; i < maxParts; i++)
            {
                int valA = i < partsA.Length && int.TryParse(partsA[i], out int pa) ? pa : 0;
                int valB = i < partsB.Length && int.TryParse(partsB[i], out int pb) ? pb : 0;

                if (valA < valB) return -1;
                if (valA > valB) return 1;
            }

            return 0;
        }

        public static string GetCurrentVersion()
        {
            return Constants.GAME_VERSION;
        }
    }
}