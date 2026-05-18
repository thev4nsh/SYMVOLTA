#if UNITY_EDITOR
using SYMVOLTA.Audio;
using SYMVOLTA.Core;
using SYMVOLTA.Drawing;
using SYMVOLTA.Effects;
using SYMVOLTA.FirebaseNS;
using SYMVOLTA.Gameplay;
using SYMVOLTA.Leaderboard;
using SYMVOLTA.Networking;
using SYMVOLTA.Profile;
using SYMVOLTA.Security;
using SYMVOLTA.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// SYMVOLTA Master Scene Builder.
/// Run via Tools/SYMVOLTA menu in Unity Editor.
/// </summary>
public static class MasterBuilder
{
    [MenuItem("Tools/SYMVOLTA/Build ALL Core Scenes")]
    public static void BuildAllScenes()
    {
        BuildAndSave("BootScene", BuildBootScene);
        BuildAndSave("MainMenu", BuildMainMenuScene);
        BuildAndSave("ShapeSelect", BuildShapeSelectScene);
        BuildAndSave("Gameplay", BuildGameplayScene);
        BuildAndSave("Leaderboard", BuildLeaderboardScene);
        BuildAndSave("Profile", BuildProfileScene);
        BuildAndSave("OfflineMode", BuildOfflineScene);
        BuildAndSave("Loading", BuildLoadingScene);
        ConfigureAndroidBuildSettings();
        AssetDatabase.SaveAssets();
        Debug.Log("<color=cyan>[SYMVOLTA] ALL 8 SCENES BUILT SUCCESSFULLY!</color>");
    }

    private static void BuildAndSave(string sceneName, System.Action build)
    {
        string scenePath = $"Assets/Scenes/{sceneName}.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        build.Invoke();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), scenePath);
    }

    // ====================================================================
    // SCENE 1: BOOT SCENE
    // ====================================================================
    [MenuItem("Tools/SYMVOLTA/Build Boot Scene")]
    public static void BuildBootScene()
    {
        if (!EnsureScene("BootScene")) return;
        ClearScene();
        CreateMainCamera();

        new GameObject("GameManager", typeof(GameManager));
        new GameObject("SceneLoader", typeof(SceneLoader));
        new GameObject("ThemeManager", typeof(ThemeManager));
        new GameObject("AudioManager", typeof(AudioManager), typeof(AudioListener));
        new GameObject("EffectsManager", typeof(EffectsManager));
        new GameObject("FirebaseManager", typeof(FirebaseManager));
        new GameObject("NetworkManager", typeof(NetworkManager));
        new GameObject("ProfileManager", typeof(ProfileManager));
        new GameObject("LeaderboardManager", typeof(LeaderboardManager));
        new GameObject("SecurityManager", typeof(SecurityManager));
        new GameObject("SettingsManager", typeof(SettingsManager));
        new GameObject("NotificationManager", typeof(NotificationManager));
        new GameObject("AnnouncementService", typeof(AnnouncementService));
        new GameObject("OfflineSyncManager", typeof(OfflineSyncManager));
        new GameObject("GameSessionManager", typeof(GameSessionManager));

        GameObject uiMgrObj = new GameObject("UIManager", typeof(UIManager));
        uiMgrObj.AddComponent<RuntimeEventSystem>();
        UIManager uiManager = uiMgrObj.GetComponent<UIManager>();

        GameObject canvas = CreateCanvas("SystemUICanvas", 9999);
        GameObject bg = CreatePanel("PopupBackground", canvas.transform, new Color(0, 0, 0, 0));
        bg.AddComponent<SafeArea>();

        GameObject loadingTxt = CreateText("LoadingText", bg.transform, "INITIALIZING...", 40, Constants.COLOR_TEXT_DIM);
        SetRectTransform(loadingTxt.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, -50), new Vector2(800, 60));

        // Force Update Popup
        GameObject forceUpdate = CreateGlassPanel("ForceUpdatePopup", bg.transform);
        SetupPopup(forceUpdate, false);
        SetRectTransform(forceUpdate.GetComponent<RectTransform>(), AnchorType.MiddleCenter, Vector2.zero, new Vector2(860, 640));
        uiManager.forceUpdatePopup = forceUpdate.AddComponent<UIPanel>();

        CreateTitleText("UpdateTitle", forceUpdate.transform, "NEW UPDATE REQUIRED");
        uiManager.forceUpdateCurrentVersionText = CreateText("CurrentVersionText", forceUpdate.transform, "Current: 1.0.0", 36, Color.white).GetComponent<TextMeshProUGUI>();
        SetRectTransform(uiManager.forceUpdateCurrentVersionText.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, 60), new Vector2(700, 50));
        uiManager.forceUpdateLatestVersionText = CreateText("LatestVersionText", forceUpdate.transform, "Latest: 1.0.0", 36, Color.white).GetComponent<TextMeshProUGUI>();
        SetRectTransform(uiManager.forceUpdateLatestVersionText.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, 0), new Vector2(700, 50));
        uiManager.forceUpdateNotesText = CreateText("UpdateNotesText", forceUpdate.transform, "Update notes", 28, Constants.COLOR_TEXT_DIM).GetComponent<TextMeshProUGUI>();
        SetRectTransform(uiManager.forceUpdateNotesText.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, -90), new Vector2(720, 120));

        GameObject updateBtn = CreateButton("UpdateButton", forceUpdate.transform, "UPDATE NOW");
        SetRectTransform(updateBtn.GetComponent<RectTransform>(), AnchorType.BottomCenter, new Vector2(-160, 80), new Vector2(280, 80));

        GameObject tutBtn = CreateButton("TutorialButton", forceUpdate.transform, "TUTORIAL");
        SetRectTransform(tutBtn.GetComponent<RectTransform>(), AnchorType.BottomCenter, new Vector2(160, 80), new Vector2(280, 80));

        // Offline Popup
        GameObject offline = CreateGlassPanel("OfflinePopup", bg.transform);
        SetupPopup(offline, false);
        SetRectTransform(offline.GetComponent<RectTransform>(), AnchorType.MiddleCenter, Vector2.zero, new Vector2(860, 520));
        uiManager.offlinePopup = offline.AddComponent<UIPanel>();
        CreateTitleText("OfflineTitle", offline.transform, "NO INTERNET");
        SetRectTransform(FindChild(offline.transform, "OfflineTitle").GetComponent<RectTransform>(), AnchorType.TopCenter, new Vector2(0, -80), new Vector2(700, 100));

        GameObject playOfflineBtn = CreateButton("PlayOfflineBtn", offline.transform, "PLAY OFFLINE");
        SetRectTransform(playOfflineBtn.GetComponent<RectTransform>(), AnchorType.BottomCenter, new Vector2(-260, 80), new Vector2(240, 80));

        GameObject retryBtn = CreateButton("RetryBtn", offline.transform, "RETRY");
        SetRectTransform(retryBtn.GetComponent<RectTransform>(), AnchorType.BottomCenter, new Vector2(0, 80), new Vector2(220, 80));

        GameObject offlineExitBtn = CreateButton("ExitBtn", offline.transform, "EXIT");
        SetRectTransform(offlineExitBtn.GetComponent<RectTransform>(), AnchorType.BottomCenter, new Vector2(250, 80), new Vector2(220, 80));

        // Maintenance Popup
        GameObject maintenance = CreateGlassPanel("MaintenancePopup", bg.transform);
        SetupPopup(maintenance, false);
        SetRectTransform(maintenance.GetComponent<RectTransform>(), AnchorType.MiddleCenter, Vector2.zero, new Vector2(860, 420));
        uiManager.maintenancePopup = maintenance.AddComponent<UIPanel>();
        CreateTitleText("MaintTitle", maintenance.transform, "MAINTENANCE");
        SetRectTransform(FindChild(maintenance.transform, "MaintTitle").GetComponent<RectTransform>(), AnchorType.TopCenter, new Vector2(0, -80), new Vector2(700, 100));

        GameObject exitBtn = CreateButton("MaintenanceExitBtn", maintenance.transform, "EXIT");
        SetRectTransform(exitBtn.GetComponent<RectTransform>(), AnchorType.BottomCenter, new Vector2(-150, 80), new Vector2(260, 80));

        GameObject supportBtn = CreateButton("SupportBtn", maintenance.transform, "SUPPORT");
        SetRectTransform(supportBtn.GetComponent<RectTransform>(), AnchorType.BottomCenter, new Vector2(150, 80), new Vector2(260, 80));

        GameObject announcement = CreateGlassPanel("AnnouncementPopup", bg.transform);
        SetupPopup(announcement, false);
        SetRectTransform(announcement.GetComponent<RectTransform>(), AnchorType.MiddleCenter, Vector2.zero, new Vector2(860, 520));
        uiManager.announcementPopup = announcement.AddComponent<UIPanel>();
        uiManager.announcementTitleText = CreateText("AnnouncementTitleText", announcement.transform, "ANNOUNCEMENT", 50, Constants.COLOR_NEON_WHITE).GetComponent<TextMeshProUGUI>();
        SetRectTransform(uiManager.announcementTitleText.GetComponent<RectTransform>(), AnchorType.TopCenter, new Vector2(0, -80), new Vector2(740, 80));
        uiManager.announcementBodyText = CreateText("AnnouncementBodyText", announcement.transform, "", 30, Constants.COLOR_TEXT_DIM).GetComponent<TextMeshProUGUI>();
        SetRectTransform(uiManager.announcementBodyText.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, 20), new Vector2(740, 180));
        GameObject closeAnnouncement = CreateButton("CloseAnnouncementButton", announcement.transform, "OK");
        SetRectTransform(closeAnnouncement.GetComponent<RectTransform>(), AnchorType.BottomCenter, new Vector2(0, 70), new Vector2(260, 80));

        SceneBootstrap bootBootstrap = canvas.AddComponent<SceneBootstrap>();
        bootBootstrap.SceneName = "BootScene";

        Debug.Log("[SYMVOLTA] Boot Scene Built.");
    }

    // ====================================================================
    // SCENE 2: MAIN MENU
    // ====================================================================
    [MenuItem("Tools/SYMVOLTA/Build Main Menu Scene")]
    public static void BuildMainMenuScene()
    {
        if (!EnsureScene("MainMenu")) return;
        ClearScene();
        CreateMainCamera();

        GameObject canvas = CreateCanvas("MainMenuCanvas", 100);
        GameObject bg = CreatePanel("Background", canvas.transform, Constants.COLOR_BG_BLACK);
        bg.AddComponent<SafeArea>();

        GameObject profileBtn = CreateButton("ProfileButton", bg.transform, "PROFILE");
        SetRectTransform(profileBtn.GetComponent<RectTransform>(), AnchorType.TopLeft, new Vector2(150, -100), new Vector2(250, 80));

        GameObject settingsBtn = CreateButton("SettingsButton", bg.transform, "SETTINGS");
        SetRectTransform(settingsBtn.GetComponent<RectTransform>(), AnchorType.TopRight, new Vector2(-150, -100), new Vector2(250, 80));

        GameObject rankTitleText = CreateText("RankTitleText", bg.transform, "BEGINNER", 50, Constants.COLOR_CYAN);
        SetRectTransform(rankTitleText.GetComponent<RectTransform>(), AnchorType.TopCenter, new Vector2(0, -110), new Vector2(600, 80));

        GameObject gameTitle = CreateTitleText("GameTitle", bg.transform, "SYMVOLTA");
        SetRectTransform(gameTitle.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, 300), new Vector2(800, 150));

        GameObject enterBtn = CreateButton("EnterGameButton", bg.transform, "ENTER GAME");
        SetRectTransform(enterBtn.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, 50), new Vector2(600, 120));

        GameObject lbBtn = CreateButton("LeaderboardButton", bg.transform, "LEADERBOARD");
        SetRectTransform(lbBtn.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, -120), new Vector2(600, 120));

        GameObject scoreText = CreateText("GlobalScoreText", bg.transform, "GLOBAL SCORE: 0", 40, Constants.COLOR_TEXT_DIM);
        SetRectTransform(scoreText.GetComponent<RectTransform>(), AnchorType.BottomCenter, new Vector2(0, 180), new Vector2(800, 60));

        GameObject rankText = CreateText("WorldRankText", bg.transform, "WORLD RANK: #---", 40, Constants.COLOR_TEXT_DIM);
        SetRectTransform(rankText.GetComponent<RectTransform>(), AnchorType.BottomCenter, new Vector2(0, 110), new Vector2(800, 60));

        // Settings Panel: CanvasGroup keeps it from blocking clicks when hidden.
        GameObject settingsPanel = CreateGlassPanel("SettingsPanel", bg.transform);
        SetRectTransform(settingsPanel.GetComponent<RectTransform>(), AnchorType.MiddleCenter, Vector2.zero, new Vector2(800, 1000));
        CanvasGroup settingsCG = settingsPanel.AddComponent<CanvasGroup>();
        settingsCG.alpha = 0f;
        settingsCG.blocksRaycasts = false;
        settingsCG.interactable = false;

        CreateTitleText("SettingsTitle", settingsPanel.transform, "SETTINGS");
        SetRectTransform(FindChild(settingsPanel.transform, "SettingsTitle").GetComponent<RectTransform>(),
            AnchorType.TopCenter, new Vector2(0, -80), new Vector2(700, 100));

        GameObject musicLabel = CreateText("MusicLabel", settingsPanel.transform, "MUSIC", 36, Constants.COLOR_CYAN);
        SetRectTransform(musicLabel.GetComponent<RectTransform>(), AnchorType.TopCenter, new Vector2(0, -220), new Vector2(600, 50));
        GameObject musicSlider = CreateSlider("MusicSlider", settingsPanel.transform, 0.5f);
        SetRectTransform(musicSlider.GetComponent<RectTransform>(), AnchorType.TopCenter, new Vector2(0, -290), new Vector2(600, 60));

        GameObject sfxLabel = CreateText("SFXLabel", settingsPanel.transform, "SFX", 36, Constants.COLOR_CYAN);
        SetRectTransform(sfxLabel.GetComponent<RectTransform>(), AnchorType.TopCenter, new Vector2(0, -380), new Vector2(600, 50));
        GameObject sfxSlider = CreateSlider("SFXSlider", settingsPanel.transform, 0.8f);
        SetRectTransform(sfxSlider.GetComponent<RectTransform>(), AnchorType.TopCenter, new Vector2(0, -450), new Vector2(600, 60));

        GameObject musicToggle = CreateToggle("MusicToggle", settingsPanel.transform, "MUSIC ON");
        SetRectTransform(musicToggle.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, 20), new Vector2(500, 60));

        GameObject sfxToggle = CreateToggle("SFXToggle", settingsPanel.transform, "SFX ON");
        SetRectTransform(sfxToggle.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, -60), new Vector2(500, 60));

        GameObject notificationToggle = CreateToggle("NotificationToggle", settingsPanel.transform, "NOTIFICATIONS");
        SetRectTransform(notificationToggle.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, -140), new Vector2(500, 60));

        GameObject settingsExitBtn = CreateButton("ExitGameButton", settingsPanel.transform, "EXIT GAME");
        SetRectTransform(settingsExitBtn.GetComponent<RectTransform>(), AnchorType.BottomCenter, new Vector2(-160, 80), new Vector2(280, 80));

        GameObject closeSettingsBtn = CreateButton("CloseSettingsButton", settingsPanel.transform, "CLOSE");
        SetRectTransform(closeSettingsBtn.GetComponent<RectTransform>(), AnchorType.BottomCenter, new Vector2(160, 80), new Vector2(280, 80));

        SceneBootstrap mainMenuBootstrap = canvas.AddComponent<SceneBootstrap>();
        mainMenuBootstrap.SceneName = "MainMenu";
        canvas.AddComponent<UsernamePromptUI>();

        Debug.Log("[SYMVOLTA] Main Menu Scene Built.");
    }

    // ====================================================================
    // SCENE 3: SHAPE SELECT
    // ====================================================================
    [MenuItem("Tools/SYMVOLTA/Build Shape Select Scene")]
    public static void BuildShapeSelectScene()
    {
        if (!EnsureScene("ShapeSelect")) return;
        ClearScene();
        CreateMainCamera();

        new GameObject("ShapeSelectManager", typeof(ShapeSelectManager));

        GameObject canvas = CreateCanvas("ShapeSelectCanvas", 100);
        GameObject bg = CreatePanel("Background", canvas.transform, Constants.COLOR_BG_BLACK);
        bg.AddComponent<SafeArea>();

        GameObject backBtn = CreateButton("BackButton", bg.transform, "BACK");
        SetRectTransform(backBtn.GetComponent<RectTransform>(), AnchorType.TopLeft, new Vector2(150, -100), new Vector2(200, 80));

        GameObject title = CreateTitleText("TitleText", bg.transform, "SELECT SHAPE");
        SetRectTransform(title.GetComponent<RectTransform>(), AnchorType.TopCenter, new Vector2(0, -120), new Vector2(800, 100));

        GameObject gridObj = CreateGlassPanel("ShapeGrid", bg.transform);
        SetRectTransform(gridObj.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, -60), new Vector2(920, 1300));

        GridLayoutGroup grid = gridObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(390, 390);
        grid.spacing = new Vector2(40, 40);
        grid.padding = new RectOffset(20, 20, 30, 30);
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;

        string[] shapeNames = { "Circle", "Triangle", "Square", "Star", "Heart", "Infinity" };
        for (int i = 0; i < shapeNames.Length; i++)
        {
            GameObject btn = CreateButton($"{shapeNames[i]}Button", gridObj.transform, shapeNames[i].ToUpper());
            TextMeshProUGUI lbl = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (lbl != null) { lbl.fontSize = 44; lbl.lineSpacing = -10; }
        }

        canvas.AddComponent<ShapeSelectUI>();

        Debug.Log("[SYMVOLTA] Shape Select Scene Built.");
    }

    // ====================================================================
    // SCENE 4: GAMEPLAY - NO AudioListener (AudioManager from Boot has one)
    // ====================================================================
    [MenuItem("Tools/SYMVOLTA/Build Gameplay Scene")]
    public static void BuildGameplayScene()
    {
        if (!EnsureScene("Gameplay")) return;
        ClearScene();

        // Camera: NO AudioListener. AudioManager from Boot scene is persistent.
        GameObject camObj = new GameObject("MainCamera", typeof(Camera));
        Camera cam = camObj.GetComponent<Camera>();
        cam.backgroundColor = Constants.COLOR_BG_BLACK;
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.nearClipPlane = -10f;
        cam.farClipPlane = 10f;
        cam.depth = -1;
        camObj.tag = "MainCamera";

        new GameObject("GameplayInitializer", typeof(GameplayInitializer));

        // Drawing Canvas
        GameObject drawingObj = new GameObject("DrawingCanvas", typeof(DrawingCanvas), typeof(DrawingTrailEffects));
        drawingObj.transform.position = Vector3.zero;
        LineRenderer lr = drawingObj.GetComponent<LineRenderer>();
        lr.startWidth = 0.15f;
        lr.endWidth = 0.15f;
        lr.numCornerVertices = 5;
        lr.numCapVertices = 5;
        lr.useWorldSpace = true;
        lr.positionCount = 0;
        lr.sortingOrder = 10;
        Shader urpUnlit = Shader.Find("Sprites/Default")
                       ?? Shader.Find("Universal Render Pipeline/Unlit")
                       ?? Shader.Find("Unlit/Color");
        if (urpUnlit != null)
        {
            Material neonMat = new Material(urpUnlit);
            neonMat.color = Constants.COLOR_NEON_WHITE;
            lr.material = neonMat;
        }
        lr.startColor = Constants.COLOR_NEON_WHITE;
        lr.endColor = Constants.COLOR_NEON_WHITE;

        // UI Canvas
        GameObject canvas = CreateCanvas("GameplayUICanvas", 100);
        GameObject hudPanel = CreatePanel("HUDPanel", canvas.transform, new Color(0, 0, 0, 0));
        hudPanel.AddComponent<SafeArea>();

        // Quit button
        GameObject quitBtn = CreateButton("QuitButton", hudPanel.transform, "BACK");
        SetRectTransform(quitBtn.GetComponent<RectTransform>(), AnchorType.TopLeft, new Vector2(80, -80), new Vector2(100, 100));

        GameObject gameplaySettingsBtn = CreateButton("SettingsButton", hudPanel.transform, "SET");
        SetRectTransform(gameplaySettingsBtn.GetComponent<RectTransform>(), AnchorType.TopRight, new Vector2(-80, -80), new Vector2(120, 80));

        // Timer
        GameObject timerTxt = CreateText("TimerText", hudPanel.transform, "02:00", 80, Constants.COLOR_NEON_WHITE);
        SetRectTransform(timerTxt.GetComponent<RectTransform>(), AnchorType.TopCenter, new Vector2(0, -100), new Vector2(400, 100));

        // Shape name
        GameObject shapeNameTxt = CreateText("ShapeNameText", hudPanel.transform, "CIRCLE", 50, Constants.COLOR_TEXT_DIM);
        SetRectTransform(shapeNameTxt.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, 250), new Vector2(400, 80));

        // Accuracy
        GameObject accuracyTxt = CreateText("AccuracyText", hudPanel.transform, "0.0%", 140, Constants.COLOR_CYAN);
        SetRectTransform(accuracyTxt.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, 100), new Vector2(600, 200));

        // Warning overlay
        GameObject warningOverlay = CreatePanel("WarningOverlay", hudPanel.transform, new Color(1f, 0.2f, 0.2f, 0.08f));
        warningOverlay.SetActive(false);

        GameObject settingsPanel = CreateGlassPanel("SettingsPanel", hudPanel.transform);
        SetRectTransform(settingsPanel.GetComponent<RectTransform>(), AnchorType.MiddleCenter, Vector2.zero, new Vector2(820, 940));
        if (settingsPanel.TryGetComponent(out Image settingsImage)) settingsImage.raycastTarget = true;
        CanvasGroup settingsCG = settingsPanel.AddComponent<CanvasGroup>();
        settingsCG.alpha = 0f;
        settingsCG.blocksRaycasts = false;
        settingsCG.interactable = false;

        CreateTitleText("SettingsTitle", settingsPanel.transform, "SETTINGS");
        SetRectTransform(FindChild(settingsPanel.transform, "SettingsTitle").GetComponent<RectTransform>(),
            AnchorType.TopCenter, new Vector2(0, -80), new Vector2(700, 100));

        GameObject gameplayMusicLabel = CreateText("MusicLabel", settingsPanel.transform, "MUSIC", 36, Constants.COLOR_CYAN);
        SetRectTransform(gameplayMusicLabel.GetComponent<RectTransform>(), AnchorType.TopCenter, new Vector2(0, -220), new Vector2(600, 50));
        GameObject gameplayMusicSlider = CreateSlider("MusicSlider", settingsPanel.transform, 0.5f);
        SetRectTransform(gameplayMusicSlider.GetComponent<RectTransform>(), AnchorType.TopCenter, new Vector2(0, -290), new Vector2(600, 60));

        GameObject gameplaySfxLabel = CreateText("SFXLabel", settingsPanel.transform, "SFX", 36, Constants.COLOR_CYAN);
        SetRectTransform(gameplaySfxLabel.GetComponent<RectTransform>(), AnchorType.TopCenter, new Vector2(0, -380), new Vector2(600, 50));
        GameObject gameplaySfxSlider = CreateSlider("SFXSlider", settingsPanel.transform, 0.8f);
        SetRectTransform(gameplaySfxSlider.GetComponent<RectTransform>(), AnchorType.TopCenter, new Vector2(0, -450), new Vector2(600, 60));

        GameObject gameplayMusicToggle = CreateToggle("MusicToggle", settingsPanel.transform, "MUSIC ON");
        SetRectTransform(gameplayMusicToggle.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, 20), new Vector2(500, 60));
        GameObject gameplaySfxToggle = CreateToggle("SFXToggle", settingsPanel.transform, "SFX ON");
        SetRectTransform(gameplaySfxToggle.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, -60), new Vector2(500, 60));
        GameObject gameplayNotificationToggle = CreateToggle("NotificationToggle", settingsPanel.transform, "NOTIFICATIONS");
        SetRectTransform(gameplayNotificationToggle.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, -140), new Vector2(500, 60));

        GameObject gameplayExitBtn = CreateButton("ExitGameButton", settingsPanel.transform, "EXIT GAME");
        SetRectTransform(gameplayExitBtn.GetComponent<RectTransform>(), AnchorType.BottomCenter, new Vector2(-160, 80), new Vector2(280, 80));
        GameObject gameplayCloseSettingsBtn = CreateButton("CloseSettingsButton", settingsPanel.transform, "CLOSE");
        SetRectTransform(gameplayCloseSettingsBtn.GetComponent<RectTransform>(), AnchorType.BottomCenter, new Vector2(160, 80), new Vector2(280, 80));

        // Game Over Panel: CanvasGroup managed by UIWiring.
        GameObject gameOverPanel = CreateGlassPanel("GameOverPanel", hudPanel.transform);
        SetRectTransform(gameOverPanel.GetComponent<RectTransform>(), AnchorType.MiddleCenter, Vector2.zero, new Vector2(860, 1100));
        CanvasGroup goCG = gameOverPanel.AddComponent<CanvasGroup>();
        goCG.alpha = 0f;
        goCG.blocksRaycasts = false;
        goCG.interactable = false;

        CreateTitleText("ResultTitle", gameOverPanel.transform, "RESULT");
        SetRectTransform(FindChild(gameOverPanel.transform, "ResultTitle").GetComponent<RectTransform>(),
            AnchorType.TopCenter, new Vector2(0, -80), new Vector2(700, 100));

        CreateText("ResultMessageText", gameOverPanel.transform, "EXCELLENT!", 60, Constants.COLOR_CYAN);
        SetRectTransform(FindChild(gameOverPanel.transform, "ResultMessageText").GetComponent<RectTransform>(),
            AnchorType.TopCenter, new Vector2(0, -210), new Vector2(700, 80));

        CreateText("ShapeNameText", gameOverPanel.transform, "CIRCLE", 40, Constants.COLOR_TEXT_DIM);
        SetRectTransform(FindChild(gameOverPanel.transform, "ShapeNameText").GetComponent<RectTransform>(),
            AnchorType.TopCenter, new Vector2(0, -310), new Vector2(700, 60));

        CreateText("FinalAccuracyText", gameOverPanel.transform, "0.0%", 120, Constants.COLOR_NEON_WHITE);
        SetRectTransform(FindChild(gameOverPanel.transform, "FinalAccuracyText").GetComponent<RectTransform>(),
            AnchorType.MiddleCenter, new Vector2(0, 80), new Vector2(600, 160));

        CreateText("PersonalBestText", gameOverPanel.transform, "BEST: 0.0%", 40, Constants.COLOR_TEXT_DIM);
        SetRectTransform(FindChild(gameOverPanel.transform, "PersonalBestText").GetComponent<RectTransform>(),
            AnchorType.MiddleCenter, new Vector2(0, -60), new Vector2(600, 60));

        CreateText("RankTitleText", gameOverPanel.transform, "BEGINNER", 50, Constants.COLOR_CYAN);
        SetRectTransform(FindChild(gameOverPanel.transform, "RankTitleText").GetComponent<RectTransform>(),
            AnchorType.MiddleCenter, new Vector2(0, -150), new Vector2(600, 70));

        CreateText("Top1Text", gameOverPanel.transform, "1. ---", 28, Constants.COLOR_NEON_WHITE);
        SetRectTransform(FindChild(gameOverPanel.transform, "Top1Text").GetComponent<RectTransform>(),
            AnchorType.MiddleCenter, new Vector2(0, -245), new Vector2(680, 42));
        CreateText("Top2Text", gameOverPanel.transform, "2. ---", 28, Constants.COLOR_TEXT_DIM);
        SetRectTransform(FindChild(gameOverPanel.transform, "Top2Text").GetComponent<RectTransform>(),
            AnchorType.MiddleCenter, new Vector2(0, -295), new Vector2(680, 42));
        CreateText("Top3Text", gameOverPanel.transform, "3. ---", 28, Constants.COLOR_TEXT_DIM);
        SetRectTransform(FindChild(gameOverPanel.transform, "Top3Text").GetComponent<RectTransform>(),
            AnchorType.MiddleCenter, new Vector2(0, -345), new Vector2(680, 42));

        GameObject newGameBtn = CreateButton("NewGameButton", gameOverPanel.transform, "PLAY AGAIN");
        SetRectTransform(newGameBtn.GetComponent<RectTransform>(), AnchorType.BottomCenter, new Vector2(-160, 80), new Vector2(280, 80));

        GameObject exitMenuBtn = CreateButton("ExitToMenuButton", gameOverPanel.transform, "MAIN MENU");
        SetRectTransform(exitMenuBtn.GetComponent<RectTransform>(), AnchorType.BottomCenter, new Vector2(160, 80), new Vector2(280, 80));

        SceneBootstrap gameplayBootstrap = canvas.AddComponent<SceneBootstrap>();
        gameplayBootstrap.SceneName = "Gameplay";

        Debug.Log("[SYMVOLTA] Gameplay Scene Built.");
    }

    // ====================================================================
    // SCENE 5: LEADERBOARD
    // ====================================================================
    [MenuItem("Tools/SYMVOLTA/Build Leaderboard Scene")]
    public static void BuildLeaderboardScene()
    {
        if (!EnsureScene("Leaderboard")) return;
        ClearScene();
        CreateMainCamera();

        new GameObject("LeaderboardManager", typeof(LeaderboardManager));

        GameObject canvas = CreateCanvas("LeaderboardCanvas", 100);
        GameObject bg = CreatePanel("Background", canvas.transform, Constants.COLOR_BG_BLACK);
        bg.AddComponent<SafeArea>();

        GameObject backBtn = CreateButton("BackButton", bg.transform, "BACK");
        SetRectTransform(backBtn.GetComponent<RectTransform>(), AnchorType.TopLeft, new Vector2(150, -100), new Vector2(200, 80));

        GameObject title = CreateTitleText("TitleText", bg.transform, "LEADERBOARD");
        SetRectTransform(title.GetComponent<RectTransform>(), AnchorType.TopCenter, new Vector2(0, -110), new Vector2(800, 100));

        GameObject tabsObj = CreateUIObject("Tabs", bg.transform);
        SetRectTransform(tabsObj.GetComponent<RectTransform>(), AnchorType.TopCenter, new Vector2(0, -220), new Vector2(1020, 80));
        HorizontalLayoutGroup hLayout = tabsObj.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 5; hLayout.childControlWidth = true; hLayout.childForceExpandWidth = true;

        string[] tabNames = { "Overall", "Circle", "Triangle", "Square", "Star", "Heart", "Infinity" };
        foreach (string tName in tabNames)
        {
            GameObject tab = CreateButton($"{tName}Tab", tabsObj.transform, tName.ToUpper());
            TextMeshProUGUI lbl = tab.GetComponentInChildren<TextMeshProUGUI>();
            if (lbl != null) lbl.fontSize = 24;
        }

        GameObject scrollObj = CreateUIObject("ScrollView", bg.transform);
        SetRectTransform(scrollObj.GetComponent<RectTransform>(), AnchorType.Stretch, Vector2.zero, Vector2.zero);
        scrollObj.GetComponent<RectTransform>().offsetMin = new Vector2(30, 60);
        scrollObj.GetComponent<RectTransform>().offsetMax = new Vector2(-30, -320);
        scrollObj.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        ScrollRect scrollRect = scrollObj.AddComponent<ScrollRect>();

        GameObject viewport = CreateUIObject("Viewport", scrollObj.transform);
        StretchToParent(viewport.GetComponent<RectTransform>());
        viewport.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        GameObject content = CreateUIObject("Content", viewport.transform);
        RectTransform contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.offsetMin = Vector2.zero;
        contentRT.offsetMax = Vector2.zero;

        VerticalLayoutGroup vLayout = content.AddComponent<VerticalLayoutGroup>();
        vLayout.spacing = 12;
        vLayout.childControlHeight = true;
        vLayout.childForceExpandHeight = false;
        vLayout.padding = new RectOffset(10, 10, 10, 10);

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = content.GetComponent<RectTransform>();
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 30f;

        SceneBootstrap leaderboardBootstrap = canvas.AddComponent<SceneBootstrap>();
        leaderboardBootstrap.SceneName = "Leaderboard";

        Debug.Log("[SYMVOLTA] Leaderboard Scene Built.");
    }

    // ====================================================================
    // SCENE 6: PROFILE
    // ====================================================================
    [MenuItem("Tools/SYMVOLTA/Build Profile Scene")]
    public static void BuildProfileScene()
    {
        if (!EnsureScene("Profile")) return;
        ClearScene();
        CreateMainCamera();

        GameObject canvas = CreateCanvas("ProfileCanvas", 100);
        GameObject bg = CreatePanel("Background", canvas.transform, Constants.COLOR_BG_BLACK);
        bg.AddComponent<SafeArea>();

        GameObject backBtn = CreateButton("BackButton", bg.transform, "BACK");
        SetRectTransform(backBtn.GetComponent<RectTransform>(), AnchorType.TopLeft, new Vector2(150, -100), new Vector2(200, 80));

        CreateTitleText("TitleText", bg.transform, "PROFILE");
        SetRectTransform(FindChild(bg.transform, "TitleText").GetComponent<RectTransform>(),
            AnchorType.TopCenter, new Vector2(0, -110), new Vector2(800, 100));

        CreateGlassPanel("AvatarFrame", bg.transform);
        SetRectTransform(FindChild(bg.transform, "AvatarFrame").GetComponent<RectTransform>(),
            AnchorType.TopCenter, new Vector2(0, -350), new Vector2(300, 300));

        CreateText("UsernameText", bg.transform, "PLAYER_0000", 60, Constants.COLOR_NEON_WHITE);
        SetRectTransform(FindChild(bg.transform, "UsernameText").GetComponent<RectTransform>(),
            AnchorType.TopCenter, new Vector2(0, -700), new Vector2(800, 80));

        CreateText("UIDText", bg.transform, "UID: --------...", 28, Constants.COLOR_TEXT_DIM);
        SetRectTransform(FindChild(bg.transform, "UIDText").GetComponent<RectTransform>(),
            AnchorType.TopCenter, new Vector2(0, -780), new Vector2(800, 50));

        CreateText("RankTitleText", bg.transform, "BEGINNER", 50, Constants.COLOR_CYAN);
        SetRectTransform(FindChild(bg.transform, "RankTitleText").GetComponent<RectTransform>(),
            AnchorType.TopCenter, new Vector2(0, -850), new Vector2(600, 70));

        GameObject statsObj = CreateGlassPanel("StatsGrid", bg.transform);
        SetRectTransform(statsObj.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, -80), new Vector2(920, 760));

        GridLayoutGroup statsGrid = statsObj.AddComponent<GridLayoutGroup>();
        statsGrid.cellSize = new Vector2(400, 110);
        statsGrid.spacing = new Vector2(20, 20);
        statsGrid.padding = new RectOffset(20, 20, 20, 20);
        statsGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        statsGrid.constraintCount = 2;

        string[] shapeNames = { "Circle", "Triangle", "Square", "Star", "Heart", "Infinity" };
        foreach (string shape in shapeNames)
        {
            CreateText($"{shape}ScoreText", statsObj.transform, $"{shape.ToUpper()}: 0.0%", 34, Constants.COLOR_CYAN);
        }

        GameObject editBtn = CreateButton("EditNameButton", bg.transform, "EDIT NAME");
        SetRectTransform(editBtn.GetComponent<RectTransform>(), AnchorType.BottomCenter, new Vector2(0, 100), new Vector2(400, 80));

        // Edit Name Panel: CanvasGroup keeps it from blocking clicks when hidden.
        GameObject editPanel = CreateGlassPanel("EditNamePanel", bg.transform);
        SetRectTransform(editPanel.GetComponent<RectTransform>(), AnchorType.MiddleCenter, Vector2.zero, new Vector2(760, 400));
        CanvasGroup editCG = editPanel.AddComponent<CanvasGroup>();
        editCG.alpha = 0f;
        editCG.blocksRaycasts = false;
        editCG.interactable = false;

        CreateText("EditNameTitle", editPanel.transform, "ENTER NEW NAME", 40, Constants.COLOR_CYAN);
        SetRectTransform(FindChild(editPanel.transform, "EditNameTitle").GetComponent<RectTransform>(),
            AnchorType.TopCenter, new Vector2(0, -60), new Vector2(700, 60));

        GameObject inputObj = CreateUIObject("NameInputField", editPanel.transform);
        SetRectTransform(inputObj.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, 20), new Vector2(660, 80));
        inputObj.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 1f);
        TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
        GameObject inputText = CreateText("Text", inputObj.transform, "", 36, Constants.COLOR_NEON_WHITE);
        StretchToParent(inputText.GetComponent<RectTransform>());
        inputField.textComponent = inputText.GetComponent<TextMeshProUGUI>();
        inputField.characterLimit = 20;

        GameObject confirmBtn = CreateButton("ConfirmNameButton", editPanel.transform, "CONFIRM");
        SetRectTransform(confirmBtn.GetComponent<RectTransform>(), AnchorType.BottomCenter, new Vector2(-160, 60), new Vector2(260, 80));

        GameObject cancelBtn = CreateButton("CancelNameButton", editPanel.transform, "CANCEL");
        SetRectTransform(cancelBtn.GetComponent<RectTransform>(), AnchorType.BottomCenter, new Vector2(160, 60), new Vector2(260, 80));

        SceneBootstrap profileBootstrap = canvas.AddComponent<SceneBootstrap>();
        profileBootstrap.SceneName = "Profile";

        Debug.Log("[SYMVOLTA] Profile Scene Built.");
    }

    // ====================================================================
    // SCENE 7: LOADING
    // ====================================================================
    [MenuItem("Tools/SYMVOLTA/Build Loading Scene")]
    public static void BuildLoadingScene()
    {
        if (!EnsureScene("Loading")) return;
        ClearScene();
        CreateMainCamera();

        GameObject canvas = CreateCanvas("LoadingCanvas", 100);
        GameObject bg = CreatePanel("Background", canvas.transform, Constants.COLOR_BG_BLACK);
        bg.AddComponent<SafeArea>();

        GameObject logo = CreateTitleText("LogoText", bg.transform, "SYMVOLTA");
        SetRectTransform(logo.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, 100), new Vector2(800, 150));

        GameObject loadTxt = CreateText("LoadingText", bg.transform, "LOADING...", 40, Constants.COLOR_TEXT_DIM);
        SetRectTransform(loadTxt.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, -50), new Vector2(400, 60));

        GameObject barBg = CreatePanel("ProgressBarBg", bg.transform, new Color(0.1f, 0.1f, 0.1f, 1f));
        SetRectTransform(barBg.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, -150), new Vector2(600, 30));

        GameObject barFill = CreatePanel("ProgressBarFill", barBg.transform, Constants.COLOR_CYAN);
        barFill.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        barFill.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1);
        barFill.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        barFill.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        Debug.Log("[SYMVOLTA] Loading Scene Built.");
    }

    // ====================================================================
    // SCENE 8: OFFLINE MODE
    // ====================================================================
    [MenuItem("Tools/SYMVOLTA/Build Offline Scene")]
    public static void BuildOfflineScene()
    {
        if (!EnsureScene("OfflineMode")) return;
        ClearScene();
        CreateMainCamera();

        GameObject canvas = CreateCanvas("OfflineCanvas", 100);
        GameObject bg = CreatePanel("Background", canvas.transform, Constants.COLOR_BG_BLACK);
        bg.AddComponent<SafeArea>();

        GameObject title = CreateTitleText("TitleText", bg.transform, "OFFLINE MODE");
        SetRectTransform(title.GetComponent<RectTransform>(), AnchorType.TopCenter, new Vector2(0, -300), new Vector2(800, 100));

        GameObject info = CreateText("InfoText", bg.transform, "LEADERBOARDS DISABLED", 40, Constants.COLOR_WARNING_RED);
        SetRectTransform(info.GetComponent<RectTransform>(), AnchorType.TopCenter, new Vector2(0, -440), new Vector2(800, 60));

        GameObject retryBtn = CreateButton("RetryConnectionBtn", bg.transform, "RETRY CONNECTION");
        SetRectTransform(retryBtn.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, 50), new Vector2(600, 100));

        GameObject playBtn = CreateButton("EnterGameBtn", bg.transform, "PLAY OFFLINE");
        SetRectTransform(playBtn.GetComponent<RectTransform>(), AnchorType.MiddleCenter, new Vector2(0, -100), new Vector2(600, 100));

        Debug.Log("[SYMVOLTA] Offline Scene Built.");
    }

    // ====================================================================
    // HELPERS
    // ====================================================================

    private static void ClearScene()
    {
        GameObject[] all = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var go in all)
        {
            if (go != null && go.transform.parent == null)
                Object.DestroyImmediate(go);
        }
    }

    private static bool EnsureScene(string sceneName)
    {
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            Debug.LogWarning($"<color=yellow>Open '{sceneName}' scene first!</color>");
            return false;
        }
        return true;
    }

    /// <summary>Creates a main camera. NO AudioListener; AudioManager from Boot provides it.</summary>
    private static void CreateMainCamera()
    {
        GameObject camObj = new GameObject("MainCamera", typeof(Camera));
        Camera cam = camObj.GetComponent<Camera>();
        cam.backgroundColor = Constants.COLOR_BG_BLACK;
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.nearClipPlane = -10f;
        cam.farClipPlane = 10f;
        cam.depth = -1;
        camObj.tag = "MainCamera";
    }

    private static void SetupPopup(GameObject popupObj, bool startVisible)
    {
        CanvasGroup cg = popupObj.GetComponent<CanvasGroup>();
        if (cg == null) cg = popupObj.AddComponent<CanvasGroup>();
        cg.alpha = startVisible ? 1f : 0f;
        cg.blocksRaycasts = startVisible;
        cg.interactable = startVisible;
    }

    private static GameObject CreateCanvas(string name, int sortOrder)
    {
        GameObject c = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas cv = c.GetComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = sortOrder;
        CanvasScaler s = c.GetComponent<CanvasScaler>();
        s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(1080, 1920);
        s.matchWidthOrHeight = 0.5f;
        return c;
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color, bool blockRaycasts = false)
    {
        GameObject o = CreateUIObject(name, parent);
        Image img = o.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = blockRaycasts;
        StretchToParent(o.GetComponent<RectTransform>());
        return o;
    }

    private static GameObject CreateGlassPanel(string name, Transform parent)
    {
        GameObject o = CreatePanel(name, parent, new Color(0.05f, 0.1f, 0.15f, 0.85f), false);
        o.AddComponent<GlassPanel>();
        return o;
    }

    private static GameObject CreateButton(string name, Transform parent, string text)
    {
        GameObject b = CreateUIObject(name, parent);
        Image img = b.AddComponent<Image>();
        img.color = new Color(0.05f, 0.1f, 0.15f, 0.9f);
        img.raycastTarget = true;
        b.AddComponent<Button>();
        b.AddComponent<NeonButton>();
        GameObject t = CreateText("Text", b.transform, text, 36, Constants.COLOR_CYAN);
        StretchToParent(t.GetComponent<RectTransform>());
        return b;
    }

    private static GameObject CreateSlider(string name, Transform parent, float defaultValue)
    {
        GameObject sliderObj = CreateUIObject(name, parent);
        sliderObj.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = defaultValue;

        GameObject bg = CreateUIObject("Background", sliderObj.transform);
        StretchToParent(bg.GetComponent<RectTransform>());
        bg.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);

        GameObject fillArea = CreateUIObject("Fill Area", sliderObj.transform);
        StretchToParent(fillArea.GetComponent<RectTransform>());
        fillArea.GetComponent<RectTransform>().offsetMax = new Vector2(-10, 0);
        fillArea.GetComponent<RectTransform>().offsetMin = new Vector2(5, 0);

        GameObject fill = CreateUIObject("Fill", fillArea.transform);
        StretchToParent(fill.GetComponent<RectTransform>());
        fill.AddComponent<Image>().color = Constants.COLOR_CYAN;
        slider.fillRect = fill.GetComponent<RectTransform>();

        GameObject handleArea = CreateUIObject("Handle Slide Area", sliderObj.transform);
        StretchToParent(handleArea.GetComponent<RectTransform>());

        GameObject handle = CreateUIObject("Handle", handleArea.transform);
        handle.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
        handle.AddComponent<Image>().color = Constants.COLOR_NEON_WHITE;
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.targetGraphic = handle.GetComponent<Image>();

        return sliderObj;
    }

    private static GameObject CreateToggle(string name, Transform parent, string labelText)
    {
        GameObject togObj = CreateUIObject(name, parent);
        Toggle toggle = togObj.AddComponent<Toggle>();

        GameObject bg = CreateUIObject("Background", togObj.transform);
        bg.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 50);
        bg.GetComponent<RectTransform>().anchoredPosition = new Vector2(-220, 0);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
        toggle.targetGraphic = bgImg;

        GameObject check = CreateUIObject("Checkmark", bg.transform);
        StretchToParent(check.GetComponent<RectTransform>());
        check.GetComponent<RectTransform>().offsetMin = new Vector2(5, 5);
        check.GetComponent<RectTransform>().offsetMax = new Vector2(-5, -5);
        Image checkImg = check.AddComponent<Image>();
        checkImg.color = Constants.COLOR_CYAN;
        toggle.graphic = checkImg;
        toggle.isOn = true;

        GameObject label = CreateText("Label", togObj.transform, labelText, 34, Constants.COLOR_NEON_WHITE);
        label.GetComponent<RectTransform>().anchoredPosition = new Vector2(30, 0);
        label.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 50);
        TextMeshProUGUI tmp = label.GetComponent<TextMeshProUGUI>();
        if (tmp != null) tmp.alignment = TextAlignmentOptions.MidlineLeft;

        return togObj;
    }

    private static GameObject CreateText(string name, Transform parent, string text, int size, Color color)
    {
        GameObject o = CreateUIObject(name, parent);
        TextMeshProUGUI tmp = o.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return o;
    }

    private static GameObject CreateTitleText(string name, Transform parent, string text)
    {
        return CreateText(name, parent, text, 80, Constants.COLOR_NEON_WHITE);
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject o = new GameObject(name, typeof(RectTransform));
        o.transform.SetParent(parent, false);
        return o;
    }

    private static Transform FindChild(Transform parent, string name)
    {
        Transform t = parent.Find(name);
        if (t == null) Debug.LogWarning($"[MasterBuilder] Could not find child '{name}' under '{parent.name}'");
        return t;
    }

    private enum AnchorType
    {
        TopLeft, TopCenter, TopRight,
        MiddleLeft, MiddleCenter, MiddleRight,
        BottomLeft, BottomCenter, BottomRight,
        Stretch
    }

    private static void SetRectTransform(RectTransform rt, AnchorType anchor, Vector2 position, Vector2 size)
    {
        Vector2 anchorMin, anchorMax, pivot;
        pivot = new Vector2(0.5f, 0.5f);

        switch (anchor)
        {
            case AnchorType.TopLeft: anchorMin = anchorMax = new Vector2(0, 1); pivot = new Vector2(0, 1); break;
            case AnchorType.TopCenter: anchorMin = anchorMax = new Vector2(0.5f, 1); pivot = new Vector2(0.5f, 1); break;
            case AnchorType.TopRight: anchorMin = anchorMax = new Vector2(1, 1); pivot = new Vector2(1, 1); break;
            case AnchorType.MiddleLeft: anchorMin = anchorMax = new Vector2(0, 0.5f); break;
            case AnchorType.MiddleCenter: anchorMin = anchorMax = new Vector2(0.5f, 0.5f); break;
            case AnchorType.MiddleRight: anchorMin = anchorMax = new Vector2(1, 0.5f); break;
            case AnchorType.BottomLeft: anchorMin = anchorMax = new Vector2(0, 0); pivot = new Vector2(0, 0); break;
            case AnchorType.BottomCenter: anchorMin = anchorMax = new Vector2(0.5f, 0); pivot = new Vector2(0.5f, 0); break;
            case AnchorType.BottomRight: anchorMin = anchorMax = new Vector2(1, 0); pivot = new Vector2(1, 0); break;
            default: anchorMin = Vector2.zero; anchorMax = Vector2.one; break;
        }

        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }

    private static void StretchToParent(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    [MenuItem("Tools/SYMVOLTA/Configure Android Production Settings")]
    public static void ConfigureAndroidBuildSettings()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        PlayerSettings.companyName = "SYMVOLTA";
        PlayerSettings.productName = Constants.GAME_NAME;
        PlayerSettings.applicationIdentifier = Constants.BUNDLE_ID;
        PlayerSettings.bundleVersion = Constants.GAME_VERSION;
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        Debug.Log("[SYMVOLTA] Android production settings configured.");
    }
}
#endif
