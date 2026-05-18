using UnityEngine;

namespace SYMVOLTA.Core
{
    /// <summary>
    /// Master reference for all constant values in the game.
    /// Prevents hardcoding strings and magic numbers.
    /// </summary>
    public static class Constants
    {
        // Game Identity
        public const string GAME_NAME = "SYMVOLTA";
        public const string GAME_VERSION = "1.0.0";
        public const string BUNDLE_ID = "com.symvolta.game";

        // Scene Names
        public static class Scenes
        {
            public const string BOOT = "BootScene";
            public const string MAIN_MENU = "MainMenu";
            public const string SHAPE_SELECT = "ShapeSelect";
            public const string GAMEPLAY = "Gameplay";
            public const string LEADERBOARD = "Leaderboard";
            public const string PROFILE = "Profile";
            public const string OFFLINE_MODE = "OfflineMode";
            public const string LOADING = "Loading";
        }

        // Firestore Database Collection Names
        public static class Firestore
        {
            public const string USERS = "users";
            public const string USERNAMES = "usernames";
            public const string LEADERBOARDS = "leaderboards";
            public const string SCORES = "scores";
            public const string ANNOUNCEMENTS = "announcements";
        }

        // Firebase Remote Config Parameter Keys
        public static class RemoteConfig
        {
            public const string MIN_VERSION = "min_version";
            public const string LATEST_VERSION = "latest_version";
            public const string UPDATE_URL = "update_url";
            public const string UPDATE_NOTES = "update_notes";
            public const string TUTORIAL_URL = "tutorial_url";
            public const string SUPPORT_URL = "support_url";
            public const string MAINTENANCE_MODE = "maintenance_mode";
            public const string GAME_TIMER_SECONDS = "game_timer_seconds";
            public const string ANNOUNCEMENT_TITLE = "announcement_title";
            public const string ANNOUNCEMENT_BODY = "announcement_body";
            public const string ANNOUNCEMENT_ACTIVE = "announcement_active";
            public const string ANNOUNCEMENT_ID = "announcement_id";
            public const string ACCURACY_THRESHOLD = "accuracy_threshold";
        }

        // Gameplay Tuning
        public const int DEFAULT_TIMER_SECONDS = 120;
        public const int LEADERBOARD_TOP_COUNT = 10;
        public const int MAX_DRAWING_POINTS = 2000;
        public const float POINT_MIN_DISTANCE = 0.05f;
        public const float SMOOTHING_FACTOR = 0.3f;
        public const int ACCURACY_UPDATE_INTERVAL_FRAMES = 5;

        // UI Colors (Hex converted to Color for code use)
        public static readonly Color COLOR_BG_BLACK = new Color(0f, 0f, 0f, 1f);
        public static readonly Color COLOR_NEON_WHITE = new Color(1f, 1f, 1f, 1f);
        public static readonly Color COLOR_CYAN = new Color(0f, 1f, 1f, 1f);
        public static readonly Color COLOR_CYAN_GLOW = new Color(0f, 0.8f, 1f, 0.6f);
        public static readonly Color COLOR_WARNING_RED = new Color(1f, 0.2f, 0.2f, 1f);
        public static readonly Color COLOR_PANEL_GLASS = new Color(0.1f, 0.1f, 0.15f, 0.7f);
        public static readonly Color COLOR_TEXT_DIM = new Color(0.6f, 0.6f, 0.7f, 1f);

        // Local Save Keys (Used by PlayerPrefs or JSON saves)
        public static class SaveKeys
        {
            public const string PROFILE = "symvolta_profile";
            public const string SETTINGS = "symvolta_settings";
            public const string LOCAL_SCORES = "symvolta_local_scores";
            public const string OFFLINE_QUEUE = "symvolta_offline_queue";
            public const string FIRST_LAUNCH = "symvolta_first_launch";
            public const string NOTIFICATION_ENABLED = "symvolta_notifications";
            public const string LAST_ANNOUNCEMENT = "symvolta_last_announcement";
            public const string INSTALL_ID = "symvolta_install_id";
        }
    }
}
