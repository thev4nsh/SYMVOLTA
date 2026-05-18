using System;

namespace SYMVOLTA.Core
{
    [Serializable]
    public class GameConfig
    {
        public string minVersion = "1.0.0";
        public string latestVersion = "1.0.0";
        public string updateUrl = "https://play.google.com/store/apps/details?id=com.symvolta.game";
        public string updateNotes = "Performance improvements and competitive stability updates.";
        public string tutorialUrl = "https://www.youtube.com/results?search_query=SYMVOLTA+tutorial";
        public string supportUrl = "mailto:support@symvolta.game";
        public bool maintenanceMode = false;
        public int timerSeconds = 120;
        public bool announcementActive = false;
        public string announcementId = "";
        public string announcementTitle = "";
        public string announcementBody = "";
        public float accuracyThreshold = 0f;

        public ShapeWeights shapeWeights = new ShapeWeights();

        /// <summary>
        /// Called when Remote Config data is successfully fetched.
        /// </summary>
        public void ApplyRemoteConfig(
            string minVer, string latestVer, string updUrl, string notes, string tutorial,
            string support, bool maint, int timer, bool annActive, string annId,
            string annTitle, string annBody, float accThreshold)
        {
            minVersion = minVer;
            latestVersion = latestVer;
            updateUrl = updUrl;
            updateNotes = notes;
            tutorialUrl = tutorial;
            supportUrl = support;
            maintenanceMode = maint;
            timerSeconds = timer;
            announcementActive = annActive;
            announcementId = annId;
            announcementTitle = annTitle;
            announcementBody = annBody;
            accuracyThreshold = accThreshold;
        }
    }

    [Serializable]
    public class ShapeWeights
    {
        public float circle = 1.0f;
        public float triangle = 1.2f;
        public float square = 1.3f;
        public float heart = 1.5f;
        public float star = 1.7f;
        public float infinity = 2.0f;

        public float GetWeight(SYMVOLTA.Shapes.ShapeType type)
        {
            return type switch
            {
                SYMVOLTA.Shapes.ShapeType.Circle => circle,
                SYMVOLTA.Shapes.ShapeType.Triangle => triangle,
                SYMVOLTA.Shapes.ShapeType.Square => square,
                SYMVOLTA.Shapes.ShapeType.Heart => heart,
                SYMVOLTA.Shapes.ShapeType.Star => star,
                SYMVOLTA.Shapes.ShapeType.Infinity => infinity,
                _ => 1.0f
            };
        }
    }
}
