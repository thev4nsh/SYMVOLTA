using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SYMVOLTA.Core;
using SYMVOLTA.Gameplay;
using SYMVOLTA.Profile;
using SYMVOLTA.Shapes;
using SYMVOLTA.Leaderboard;

namespace SYMVOLTA.UI
{
    public class SceneBootstrap : MonoBehaviour
    {
        [Tooltip("Exact scene name: MainMenu | ShapeSelect | Gameplay | Leaderboard | Profile | OfflineMode | BootScene")]
        public string SceneName;

        private void Start()
        {
            if (string.IsNullOrEmpty(SceneName))
            {
                Debug.LogError($"[SceneBootstrap] SceneName is empty on '{gameObject.name}'!");
                return;
            }
            Debug.Log($"[SceneBootstrap] Wiring scene: {SceneName}");
            UIWiring.WireScene(SceneName, transform);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(SceneName))
                Debug.LogWarning($"[SceneBootstrap] SceneName is empty on '{gameObject.name}'.", this);
        }
#endif
    }

    public static class UIWiring
    {
        private static Transform _gameplayRoot;
        private static bool _gameplayWired = false;
        public static bool IsGameplayWired => _gameplayWired;

        public static void WireScene(string sceneName, Transform root)
        {
            Debug.Log($"[UIWiring] WireScene('{sceneName}', root='{root?.name}')");

            switch (sceneName)
            {
                case "MainMenu": WireMainMenu(root); break;
                case "ShapeSelect": WireShapeSelect(root); break;
                case "Gameplay": WireGameplay(root); break;
                case "Leaderboard": WireLeaderboard(root); break;
                case "Profile": WireProfile(root); break;
                case "OfflineMode": WireOffline(root); break;
                case "Boot":
                case "BootScene": WireBoot(root); break;
                default:
                    Debug.LogWarning($"[UIWiring] No wiring defined for: {sceneName}");
                    break;
            }
        }

        // ======================================================================
        // MAIN MENU
        // ======================================================================
        private static void WireMainMenu(Transform root)
        {
            Listen(root, "EnterGameButton",
                () => SceneLoader.Instance.LoadScene(Constants.Scenes.SHAPE_SELECT));

            Listen(root, "LeaderboardButton",
                () => SceneLoader.Instance.LoadScene(Constants.Scenes.LEADERBOARD));

            Listen(root, "ProfileButton",
                () => SceneLoader.Instance.LoadScene(Constants.Scenes.PROFILE));

            GameObject settingsPanel = Find(root, "SettingsPanel")?.gameObject;
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
                EnsureCanvasGroup(settingsPanel, true);
            }

            Listen(root, "SettingsButton", () =>
            {
                if (settingsPanel != null)
                {
                    settingsPanel.SetActive(true);
                    SetGroupInteractable(settingsPanel, true);
                }
            });

            Listen(root, "CloseSettingsButton", () =>
            {
                if (settingsPanel != null)
                {
                    SetGroupInteractable(settingsPanel, false);
                    settingsPanel.SetActive(false);
                }
            });

            Listen(root, "ExitGameButton", () => GameManager.Instance?.ExitGame());

            WireSlider(root, "MusicSlider", v => Audio.AudioManager.Instance?.SetMusicVolume(v));
            WireSlider(root, "SFXSlider", v => Audio.AudioManager.Instance?.SetSFXVolume(v));
            WireToggle(root, "MusicToggle", v => Audio.AudioManager.Instance?.SetMusicMuted(!v));
            WireToggle(root, "SFXToggle", v => Audio.AudioManager.Instance?.SetSfxMuted(!v));
            WireToggle(root, "NotificationToggle", v => UIManager.Instance?.OnClick_SetNotifications(v));

            RefreshMainMenuData(root);
            if (root.GetComponent<UsernamePromptUI>() == null)
                root.gameObject.AddComponent<UsernamePromptUI>();

            if (ProfileManager.Instance != null)
                ProfileManager.Instance.OnProfileUpdated += _ => RefreshMainMenuData(root);

            Debug.Log("<color=green>[UIWiring] MainMenu wired.</color>");
        }

        private static void RefreshMainMenuData(Transform root)
        {
            PlayerProfile p = ProfileManager.Instance?.CurrentProfile;
            if (p == null) return;

            SetText(root, "RankTitleText", p.rankTitle.ToUpper());
            SetText(root, "GlobalScoreText", $"GLOBAL SCORE: {p.globalScore:F1}");
            SetText(root, "WorldRankText",
                $"WORLD RANK: {(p.worldRank > 0 ? "#" + p.worldRank : "#---")}");

            var rankLabel = Find(root, "RankTitleText")?.GetComponent<TextMeshProUGUI>();
            if (rankLabel != null && ThemeManager.Instance != null)
                rankLabel.color = ThemeManager.Instance.GetRankColor(p.rankTitle);
        }

        // ======================================================================
        // SHAPE SELECT
        // ======================================================================
        private static void WireShapeSelect(Transform root)
        {
            Listen(root, "BackButton",
                () => SceneLoader.Instance.LoadScene(Constants.Scenes.MAIN_MENU));

            string[] shapes = { "Circle", "Triangle", "Square", "Star", "Heart", "Infinity" };
            for (int i = 0; i < shapes.Length; i++)
            {
                int index = i;
                Listen(root, $"{shapes[i]}Button", () =>
                {
                    PlayerPrefs.SetInt("SelectedShape", index);
                    PlayerPrefs.Save();
                    SceneLoader.Instance.LoadScene(Constants.Scenes.GAMEPLAY);
                });
            }

            Debug.Log("<color=green>[UIWiring] ShapeSelect wired.</color>");
        }

        // ======================================================================
        // GAMEPLAY - CRITICAL WIRING
        // ======================================================================
        private static void WireGameplay(Transform root)
        {
            Debug.Log($"[UIWiring] WireGameplay called. root='{root?.name}'");

            _gameplayRoot = root;

            // Hide game over panel immediately
            GameObject gameOverPanel = Find(root, "GameOverPanel")?.gameObject;
            if (gameOverPanel != null)
            {
                EnsureCanvasGroup(gameOverPanel, false);
                SetGroupInteractable(gameOverPanel, false);
            }

            SetActive(root, "WarningOverlay", false);
            EnsureGameplaySettingsUi(root);

            GameObject settingsPanel = Find(root, "SettingsPanel")?.gameObject;
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
                EnsureCanvasGroup(settingsPanel, false);
            }

            // Buttons
            Listen(root, "QuitButton",
                () => GameSessionManager.Instance?.QuitSession());

            Listen(root, "SettingsButton", () =>
            {
                if (settingsPanel == null) return;
                settingsPanel.SetActive(true);
                SetGroupInteractable(settingsPanel, true);
            });

            Listen(root, "CloseSettingsButton", () =>
            {
                if (settingsPanel == null) return;
                SetGroupInteractable(settingsPanel, false);
                settingsPanel.SetActive(false);
            });

            Listen(root, "ExitGameButton", () => GameManager.Instance?.ExitGame());
            WireSlider(root, "MusicSlider", v => Audio.AudioManager.Instance?.SetMusicVolume(v));
            WireSlider(root, "SFXSlider", v => Audio.AudioManager.Instance?.SetSFXVolume(v));
            WireToggle(root, "MusicToggle", v => Audio.AudioManager.Instance?.SetMusicMuted(!v));
            WireToggle(root, "SFXToggle", v => Audio.AudioManager.Instance?.SetSfxMuted(!v));
            WireToggle(root, "NotificationToggle", v => UIManager.Instance?.OnClick_SetNotifications(v));

            Listen(root, "NewGameButton", () =>
            {
                if (gameOverPanel != null) SetGroupInteractable(gameOverPanel, false);
                GameSessionManager.Instance?.StartSession(
                    GameSessionManager.Instance.CurrentShape);
            });

            Listen(root, "ExitToMenuButton",
                () => SceneLoader.Instance.LoadScene(Constants.Scenes.MAIN_MENU));

            // SUBSCRIBE TO EVENTS
            if (GameSessionManager.Instance != null)
            {
                int id = GameSessionManager.Instance.GetInstanceID();
                Debug.Log($"[UIWiring] GameSessionManager found. InstanceID={id}. Subscribing to events...");
                UnsubscribeGameplayEvents();

                GameSessionManager.Instance.OnSessionStarted += Gameplay_OnSessionStarted;
                GameSessionManager.Instance.OnTimerTick += Gameplay_OnTimerTick;
                GameSessionManager.Instance.OnAccuracyUpdated += Gameplay_OnAccuracyUpdated;
                GameSessionManager.Instance.OnWarningPhaseStarted += Gameplay_OnWarningPhase;
                GameSessionManager.Instance.OnSessionEnded += Gameplay_OnSessionEnded;

                _gameplayWired = true;
                Debug.Log($"<color=green>[UIWiring] Gameplay wired. Events subscribed to InstanceID={id}.</color>");
            }
            else
            {
                Debug.LogError("[UIWiring] GameSessionManager.Instance is NULL! Cannot subscribe!");
            }
        }

        public static void UnsubscribeGameplayEvents()
        {
            if (GameSessionManager.Instance == null) return;

            int id = GameSessionManager.Instance.GetInstanceID();
            Debug.LogWarning($"<color=orange>[UIWiring] UnsubscribeGameplayEvents called! InstanceID={id}. Stack trace:\n{UnityEngine.StackTraceUtility.ExtractStackTrace()}</color>");

            GameSessionManager.Instance.OnSessionStarted -= Gameplay_OnSessionStarted;
            GameSessionManager.Instance.OnTimerTick -= Gameplay_OnTimerTick;
            GameSessionManager.Instance.OnAccuracyUpdated -= Gameplay_OnAccuracyUpdated;
            GameSessionManager.Instance.OnWarningPhaseStarted -= Gameplay_OnWarningPhase;
            GameSessionManager.Instance.OnSessionEnded -= Gameplay_OnSessionEnded;

            _gameplayWired = false;
            _gameplayRoot = null;
        }

        // Named Gameplay event handlers

        private static void Gameplay_OnSessionStarted()
        {
            if (_gameplayRoot == null) { UnsubscribeGameplayEvents(); return; }
            if (GameSessionManager.Instance == null) return;

            Debug.Log("[UIWiring] OnSessionStarted fired.");

            SetText(_gameplayRoot, "ShapeNameText",
                GameSessionManager.Instance.CurrentShape.DisplayName());

            var timerTxt = Find(_gameplayRoot, "TimerText")?.GetComponent<TextMeshProUGUI>();
            if (timerTxt != null) timerTxt.color = Constants.COLOR_NEON_WHITE;

            SetActive(_gameplayRoot, "WarningOverlay", false);
            SetText(_gameplayRoot, "AccuracyText", "0.0%");

            GameObject gameOverPanel = Find(_gameplayRoot, "GameOverPanel")?.gameObject;
            if (gameOverPanel != null) SetGroupInteractable(gameOverPanel, false);
        }

        private static void Gameplay_OnTimerTick(float timeRemaining)
        {
            if (_gameplayRoot == null) { UnsubscribeGameplayEvents(); return; }

            int m = Mathf.FloorToInt(timeRemaining / 60f);
            int s = Mathf.FloorToInt(timeRemaining % 60f);
            SetText(_gameplayRoot, "TimerText", $"{m:00}:{s:00}");
        }

        private static void Gameplay_OnAccuracyUpdated(float accuracy)
        {
            if (_gameplayRoot == null) { UnsubscribeGameplayEvents(); return; }
            SetText(_gameplayRoot, "AccuracyText", $"{accuracy:F1}%");
        }

        private static void Gameplay_OnWarningPhase()
        {
            if (_gameplayRoot == null) { UnsubscribeGameplayEvents(); return; }

            SetActive(_gameplayRoot, "WarningOverlay", true);
            var timerTxt = Find(_gameplayRoot, "TimerText")?.GetComponent<TextMeshProUGUI>();
            if (timerTxt != null) timerTxt.color = Constants.COLOR_WARNING_RED;
        }

        private static void Gameplay_OnSessionEnded(float finalAccuracy)
        {
            Debug.Log($"<color=yellow>[UIWiring] OnSessionEnded FIRED! accuracy={finalAccuracy:F1}%, root='{_gameplayRoot?.name}'</color>");

            if (_gameplayRoot == null)
            {
                Debug.LogError("[UIWiring] _gameplayRoot is NULL in OnSessionEnded! Unsubscribing.");
                UnsubscribeGameplayEvents();
                return;
            }

            // Populate game over panel
            SetText(_gameplayRoot, "FinalAccuracyText", $"{finalAccuracy:F1}%");

            if (GameSessionManager.Instance != null)
                SetText(_gameplayRoot, "ShapeNameText",
                    GameSessionManager.Instance.CurrentShape.DisplayName());

            PlayerProfile profile = ProfileManager.Instance?.CurrentProfile;
            if (profile != null)
            {
                SetText(_gameplayRoot, "RankTitleText", profile.rankTitle.ToUpper());

                var rankLbl = Find(_gameplayRoot, "RankTitleText")?.GetComponent<TextMeshProUGUI>();
                if (rankLbl != null && ThemeManager.Instance != null)
                    rankLbl.color = ThemeManager.Instance.GetRankColor(profile.rankTitle);

                if (GameSessionManager.Instance != null)
                {
                    float best = profile.shapeScores
                        .GetScore(GameSessionManager.Instance.CurrentShape).bestAccuracy;
                    SetText(_gameplayRoot, "PersonalBestText", $"BEST: {best:F1}%");
                }
            }

            string msg = finalAccuracy >= 95f ? "LEGENDARY!"
                       : finalAccuracy >= 80f ? "EXCELLENT!"
                       : finalAccuracy >= 60f ? "GOOD JOB!"
                       : finalAccuracy >= 40f ? "KEEP TRYING"
                       : "PRACTICE MORE";
            SetText(_gameplayRoot, "ResultMessageText", msg);
            PopulateTopThree(_gameplayRoot);

            // SHOW THE GAME OVER PANEL
            GameObject gameOverPanel = Find(_gameplayRoot, "GameOverPanel")?.gameObject;
            if (gameOverPanel != null)
            {
                Debug.Log("[UIWiring] GameOverPanel found. Setting VISIBLE and INTERACTABLE.");
                SetGroupInteractable(gameOverPanel, true);
            }
            else
            {
                Debug.LogError("[UIWiring] GameOverPanel NOT FOUND! Cannot show results.");
            }
        }

        // ======================================================================
        // LEADERBOARD
        // ======================================================================
        private static void WireLeaderboard(Transform root)
        {
            Listen(root, "BackButton",
                () => SceneLoader.Instance.LoadScene(Constants.Scenes.MAIN_MENU));

            Listen(root, "OverallTab", () => LoadLeaderboardTab(root, "overall"));
            Listen(root, "CircleTab", () => LoadLeaderboardTab(root, "lb_circle"));
            Listen(root, "TriangleTab", () => LoadLeaderboardTab(root, "lb_triangle"));
            Listen(root, "SquareTab", () => LoadLeaderboardTab(root, "lb_square"));
            Listen(root, "StarTab", () => LoadLeaderboardTab(root, "lb_star"));
            Listen(root, "HeartTab", () => LoadLeaderboardTab(root, "lb_heart"));
            Listen(root, "InfinityTab", () => LoadLeaderboardTab(root, "lb_infinity"));

            LoadLeaderboardTab(root, "overall");

            Debug.Log("<color=green>[UIWiring] Leaderboard wired.</color>");
        }

        private static async void LoadLeaderboardTab(Transform root, string boardId)
        {
            Transform content = Find(root, "Content");
            if (content == null) { Debug.LogError("[UIWiring] Leaderboard 'Content' not found."); return; }

            for (int i = content.childCount - 1; i >= 0; i--)
                Object.Destroy(content.GetChild(i).gameObject);

            if (GameManager.Instance != null && GameManager.Instance.IsOfflineMode)
            {
                LeaderboardRowFactory.CreateInfoRow(content, "OFFLINE - leaderboard unavailable");
                return;
            }

            LeaderboardBoard board = boardId == "overall"
                ? await LeaderboardManager.Instance.FetchOverallLeaderboard()
                : await LeaderboardManager.Instance.FetchLeaderboard(BoardIdToShape(boardId));

            if (board == null || board.topEntries == null || board.topEntries.Count == 0)
            {
                LeaderboardRowFactory.CreateInfoRow(content, "No entries yet - be the first!");
                return;
            }

            string myUid = ProfileManager.Instance?.CurrentProfile?.uid;
            bool myShown = false;

            for (int i = 0; i < Mathf.Min(board.topEntries.Count, 100); i++)
            {
                var entry = board.topEntries[i];
                bool isMe = entry.uid == myUid;
                if (isMe) myShown = true;
                LeaderboardRowFactory.CreateEntryRow(content, entry, isMe);
            }

            if (!myShown && myUid != null)
            {
                var mine = board.playerEntry ?? board.topEntries.Find(e => e.uid == myUid);
                if (mine != null)
                {
                    LeaderboardRowFactory.CreateInfoRow(content, "Your rank");
                    LeaderboardRowFactory.CreateEntryRow(content, mine, true);
                }
                else
                {
                    LeaderboardRowFactory.CreateInfoRow(content, "You are not ranked yet");
                }
            }
        }

        private static ShapeType BoardIdToShape(string id) => id switch
        {
            "lb_circle" => ShapeType.Circle,
            "lb_triangle" => ShapeType.Triangle,
            "lb_square" => ShapeType.Square,
            "lb_star" => ShapeType.Star,
            "lb_heart" => ShapeType.Heart,
            "lb_infinity" => ShapeType.Infinity,
            _ => ShapeType.Circle
        };

        // ======================================================================
        // PROFILE
        // ======================================================================
        private static void WireProfile(Transform root)
        {
            Listen(root, "BackButton",
                () => SceneLoader.Instance.LoadScene(Constants.Scenes.MAIN_MENU));

            GameObject editPanel = Find(root, "EditNamePanel")?.gameObject;
            if (editPanel != null)
            {
                editPanel.SetActive(false);
                EnsureCanvasGroup(editPanel, true);
                SetGroupInteractable(editPanel, false);
            }

            Listen(root, "EditNameButton", () =>
            {
                if (editPanel != null)
                {
                    editPanel.SetActive(true);
                    SetGroupInteractable(editPanel, true);
                    var input = Find(root, "NameInputField")?.GetComponent<TMP_InputField>();
                    if (input != null && ProfileManager.Instance?.CurrentProfile != null)
                        input.text = ProfileManager.Instance.CurrentProfile.username;
                }
            });

            Listen(root, "ConfirmNameButton", () =>
            {
                var input = Find(root, "NameInputField")?.GetComponent<TMP_InputField>();
                if (input == null) return;
                string name = input.text.Trim();
                if (name.Length >= 3 && name.Length <= 20)
                {
                    ConfirmProfileName(root, editPanel, name);
                }
            });

            Listen(root, "CancelNameButton", () =>
            {
                SetGroupInteractable(editPanel, false);
                if (editPanel != null) editPanel.SetActive(false);
            });

            RefreshProfileData(root);

            if (ProfileManager.Instance != null)
                ProfileManager.Instance.OnProfileUpdated += _ => RefreshProfileData(root);

            Debug.Log("<color=green>[UIWiring] Profile wired.</color>");
        }

        private static void RefreshProfileData(Transform root)
        {
            PlayerProfile p = ProfileManager.Instance?.CurrentProfile;
            if (p == null) return;

            SetText(root, "UsernameText", p.username.ToUpper());
            SetText(root, "UIDText", $"UID: {p.uid.Substring(0, Mathf.Min(8, p.uid.Length))}...");
            SetText(root, "RankTitleText", p.rankTitle.ToUpper());

            var rankLbl = Find(root, "RankTitleText")?.GetComponent<TextMeshProUGUI>();
            if (rankLbl != null && ThemeManager.Instance != null)
                rankLbl.color = ThemeManager.Instance.GetRankColor(p.rankTitle);

            SetText(root, "CircleScoreText",
                $"CIRCLE: {p.shapeScores.GetScore(ShapeType.Circle).bestAccuracy:F1}%");
            SetText(root, "TriangleScoreText",
                $"TRIANGLE: {p.shapeScores.GetScore(ShapeType.Triangle).bestAccuracy:F1}%");
            SetText(root, "SquareScoreText",
                $"SQUARE: {p.shapeScores.GetScore(ShapeType.Square).bestAccuracy:F1}%");
            SetText(root, "StarScoreText",
                $"STAR: {p.shapeScores.GetScore(ShapeType.Star).bestAccuracy:F1}%");
            SetText(root, "HeartScoreText",
                $"HEART: {p.shapeScores.GetScore(ShapeType.Heart).bestAccuracy:F1}%");
            SetText(root, "InfinityScoreText",
                $"INFINITY: {p.shapeScores.GetScore(ShapeType.Infinity).bestAccuracy:F1}%");
        }

        // ======================================================================
        // OFFLINE
        // ======================================================================
        private static void WireOffline(Transform root)
        {
            Listen(root, "RetryConnectionBtn", () => GameManager.Instance?.RetryConnection());
            Listen(root, "EnterGameBtn", () => GameManager.Instance?.EnterOfflineMode());
            Debug.Log("<color=green>[UIWiring] Offline wired.</color>");
        }

        // ======================================================================
        // BOOT
        // ======================================================================
        private static void WireBoot(Transform root)
        {
            Listen(root, "UpdateButton", () => UIManager.Instance?.OnClick_OpenStoreToUpdate());
            Listen(root, "TutorialButton", () => UIManager.Instance?.OnClick_OpenTutorial());
            Listen(root, "PlayOfflineBtn", () => UIManager.Instance?.OnClick_EnterOfflineMode());
            Listen(root, "RetryBtn", () => UIManager.Instance?.OnClick_RetryConnection());
            Listen(root, "ExitBtn", () => UIManager.Instance?.OnClick_ExitGame());
            Listen(root, "MaintenanceExitBtn", () => UIManager.Instance?.OnClick_ExitGame());
            Listen(root, "SupportBtn", () => UIManager.Instance?.OnClick_OpenSupport());
            Listen(root, "CloseAnnouncementButton", () => UIManager.Instance?.OnClick_CloseAnnouncement());
            Debug.Log("<color=green>[UIWiring] Boot wired.</color>");
        }

        private static async void ConfirmProfileName(Transform root, GameObject editPanel, string name)
        {
            bool accepted = await ProfileManager.Instance.TryUpdateUsernameAsync(name);
            if (!accepted)
            {
                SetText(root, "NameStatusText", "Name unavailable");
                return;
            }

            SetGroupInteractable(editPanel, false);
            if (editPanel != null) editPanel.SetActive(false);
            RefreshProfileData(root);
        }

        private static async void PopulateTopThree(Transform root)
        {
            if (GameManager.Instance == null || GameManager.Instance.IsOfflineMode) return;
            if (LeaderboardManager.Instance == null || GameSessionManager.Instance == null) return;

            LeaderboardBoard board = await LeaderboardManager.Instance.FetchLeaderboard(GameSessionManager.Instance.CurrentShape);
            for (int i = 0; i < 3; i++)
            {
                string text = i < board.topEntries.Count
                    ? $"{i + 1}. {board.topEntries[i].username} - {board.topEntries[i].score:F1}%"
                    : $"{i + 1}. ---";
                SetText(root, $"Top{i + 1}Text", text);
            }
        }

        private static void EnsureGameplaySettingsUi(Transform root)
        {
            if (root == null) return;

            if (Find(root, "SettingsButton") == null)
            {
                GameObject settingsButton = RuntimeButton("SettingsButton", root, "SET");
                SetRuntimeRect(settingsButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-80f, -80f), new Vector2(120f, 80f));
            }

            if (Find(root, "SettingsPanel") != null) return;

            GameObject panel = new GameObject("SettingsPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(GlassPanel));
            panel.transform.SetParent(root, false);
            SetRuntimeRect(panel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820f, 940f));
            panel.GetComponent<Image>().color = new Color(0.05f, 0.1f, 0.15f, 0.92f);
            EnsureCanvasGroup(panel, false);

            RuntimeLabel("SettingsTitle", panel.transform, "SETTINGS", 56, Constants.COLOR_NEON_WHITE, new Vector2(0f, 350f), new Vector2(700f, 80f));
            RuntimeLabel("MusicLabel", panel.transform, "MUSIC", 30, Constants.COLOR_CYAN, new Vector2(0f, 250f), new Vector2(620f, 48f));
            GameObject musicSlider = RuntimeSlider("MusicSlider", panel.transform, Audio.AudioManager.Instance != null ? Audio.AudioManager.Instance.MusicVolume : 0.5f);
            SetRuntimeRect(musicSlider.GetComponent<RectTransform>(), Center, Center, Center, new Vector2(0f, 185f), new Vector2(620f, 56f));

            RuntimeLabel("SFXLabel", panel.transform, "SFX", 30, Constants.COLOR_CYAN, new Vector2(0f, 95f), new Vector2(620f, 48f));
            GameObject sfxSlider = RuntimeSlider("SFXSlider", panel.transform, Audio.AudioManager.Instance != null ? Audio.AudioManager.Instance.SfxVolume : 0.8f);
            SetRuntimeRect(sfxSlider.GetComponent<RectTransform>(), Center, Center, Center, new Vector2(0f, 30f), new Vector2(620f, 56f));

            GameObject musicToggle = RuntimeToggle("MusicToggle", panel.transform, "MUSIC ON", Audio.AudioManager.Instance == null || !Audio.AudioManager.Instance.IsMusicMuted);
            SetRuntimeRect(musicToggle.GetComponent<RectTransform>(), Center, Center, Center, new Vector2(0f, -90f), new Vector2(520f, 62f));

            GameObject sfxToggle = RuntimeToggle("SFXToggle", panel.transform, "SFX ON", Audio.AudioManager.Instance == null || !Audio.AudioManager.Instance.IsSfxMuted);
            SetRuntimeRect(sfxToggle.GetComponent<RectTransform>(), Center, Center, Center, new Vector2(0f, -170f), new Vector2(520f, 62f));

            bool notifications = FirebaseNS.NotificationManager.Instance == null || FirebaseNS.NotificationManager.Instance.NotificationsEnabled;
            GameObject notificationToggle = RuntimeToggle("NotificationToggle", panel.transform, "NOTIFICATIONS", notifications);
            SetRuntimeRect(notificationToggle.GetComponent<RectTransform>(), Center, Center, Center, new Vector2(0f, -250f), new Vector2(520f, 62f));

            GameObject exitButton = RuntimeButton("ExitGameButton", panel.transform, "EXIT GAME");
            SetRuntimeRect(exitButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-150f, 70f), new Vector2(270f, 80f));

            GameObject closeButton = RuntimeButton("CloseSettingsButton", panel.transform, "CLOSE");
            SetRuntimeRect(closeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(150f, 70f), new Vector2(270f, 80f));
        }

        // ======================================================================
        // LOW-LEVEL HELPERS
        // ======================================================================

        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

        private static GameObject RuntimeButton(string name, Transform parent, string text)
        {
            GameObject button = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(NeonButton));
            button.transform.SetParent(parent, false);
            Image image = button.GetComponent<Image>();
            image.color = new Color(0.04f, 0.1f, 0.14f, 0.95f);
            image.raycastTarget = true;
            RuntimeLabel("Text", button.transform, text, 30, Constants.COLOR_CYAN, Vector2.zero, new Vector2(300f, 80f));
            StretchRuntime(button.transform.GetChild(0).GetComponent<RectTransform>());
            return button;
        }

        private static TextMeshProUGUI RuntimeLabel(string name, Transform parent, string text, int fontSize, Color color, Vector2 position, Vector2 size)
        {
            GameObject label = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            label.transform.SetParent(parent, false);
            RectTransform rt = label.GetComponent<RectTransform>();
            SetRuntimeRect(rt, Center, Center, Center, position, size);
            TextMeshProUGUI tmp = label.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static GameObject RuntimeSlider(string name, Transform parent, float value)
        {
            GameObject sliderObj = new GameObject(name, typeof(RectTransform), typeof(Slider));
            sliderObj.transform.SetParent(parent, false);
            Slider slider = sliderObj.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = Mathf.Clamp01(value);

            GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(sliderObj.transform, false);
            StretchRuntime(bg.GetComponent<RectTransform>());
            bg.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.18f, 1f);

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObj.transform, false);
            StretchRuntime(fillArea.GetComponent<RectTransform>());
            fillArea.GetComponent<RectTransform>().offsetMin = new Vector2(8f, 0f);
            fillArea.GetComponent<RectTransform>().offsetMax = new Vector2(-8f, 0f);

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            StretchRuntime(fill.GetComponent<RectTransform>());
            fill.GetComponent<Image>().color = Constants.COLOR_CYAN;
            slider.fillRect = fill.GetComponent<RectTransform>();

            GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderObj.transform, false);
            StretchRuntime(handleArea.GetComponent<RectTransform>());

            GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            handle.GetComponent<RectTransform>().sizeDelta = new Vector2(42f, 42f);
            handle.GetComponent<Image>().color = Constants.COLOR_NEON_WHITE;
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();
            return sliderObj;
        }

        private static GameObject RuntimeToggle(string name, Transform parent, string text, bool isOn)
        {
            GameObject toggleObj = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            toggleObj.transform.SetParent(parent, false);
            Toggle toggle = toggleObj.GetComponent<Toggle>();
            toggle.isOn = isOn;

            GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(toggleObj.transform, false);
            SetRuntimeRect(bg.GetComponent<RectTransform>(), Center, Center, Center, new Vector2(-220f, 0f), new Vector2(50f, 50f));
            Image bgImage = bg.GetComponent<Image>();
            bgImage.color = new Color(0.16f, 0.18f, 0.22f, 1f);
            toggle.targetGraphic = bgImage;

            GameObject check = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            check.transform.SetParent(bg.transform, false);
            StretchRuntime(check.GetComponent<RectTransform>());
            check.GetComponent<RectTransform>().offsetMin = new Vector2(7f, 7f);
            check.GetComponent<RectTransform>().offsetMax = new Vector2(-7f, -7f);
            check.GetComponent<Image>().color = Constants.COLOR_CYAN;
            toggle.graphic = check.GetComponent<Image>();

            TextMeshProUGUI label = RuntimeLabel("Label", toggleObj.transform, text, 30, Constants.COLOR_NEON_WHITE, new Vector2(40f, 0f), new Vector2(420f, 54f));
            label.alignment = TextAlignmentOptions.MidlineLeft;
            return toggleObj;
        }

        private static void SetRuntimeRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = size;
        }

        private static void StretchRuntime(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = Center;
        }

        private static void Listen(Transform root, string name, UnityEngine.Events.UnityAction cb)
        {
            var t = DeepFind(root, name);
            if (t == null)
            {
                Debug.LogWarning($"[UIWiring] Button NOT FOUND: '{name}' under root '{root?.name}'");
                return;
            }
            var btn = t.GetComponent<Button>();
            if (btn == null)
            {
                Debug.LogWarning($"[UIWiring] No Button component on: '{name}'");
                return;
            }
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(cb);
        }

        private static void WireSlider(Transform root, string name, UnityEngine.Events.UnityAction<float> cb)
        {
            var t = DeepFind(root, name);
            if (t == null) return;
            var s = t.GetComponent<Slider>();
            if (s == null) return;
            s.onValueChanged.RemoveAllListeners();
            s.onValueChanged.AddListener(cb);
        }

        private static void WireToggle(Transform root, string name, UnityEngine.Events.UnityAction<bool> cb)
        {
            var t = DeepFind(root, name);
            if (t == null) return;
            var tog = t.GetComponent<Toggle>();
            if (tog == null) return;
            tog.onValueChanged.RemoveAllListeners();
            tog.onValueChanged.AddListener(cb);
        }

        private static void SetText(Transform root, string name, string text)
        {
            var t = DeepFind(root, name);
            if (t == null)
            {
                // Only warn once per element to avoid spam
                return;
            }
            var tmp = t.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.text = text;
        }

        private static void SetActive(Transform root, string name, bool active)
        {
            var t = DeepFind(root, name);
            if (t != null) t.gameObject.SetActive(active);
        }

        private static void SetGroupInteractable(GameObject obj, bool visible)
        {
            if (obj == null) return;
            CanvasGroup cg = obj.GetComponent<CanvasGroup>();
            if (cg == null) cg = obj.AddComponent<CanvasGroup>();
            cg.alpha = visible ? 1f : 0f;
            cg.blocksRaycasts = visible;
            cg.interactable = visible;
        }

        private static void EnsureCanvasGroup(GameObject obj, bool startVisible)
        {
            if (obj == null) return;
            CanvasGroup cg = obj.GetComponent<CanvasGroup>();
            if (cg == null) cg = obj.AddComponent<CanvasGroup>();
            cg.alpha = startVisible ? 1f : 0f;
            cg.blocksRaycasts = startVisible;
            cg.interactable = startVisible;
        }

        private static Transform Find(Transform root, string name) => DeepFind(root, name);

        private static Transform DeepFind(Transform parent, string name)
        {
            if (parent == null) return null;
            if (parent.name == name) return parent;
            foreach (Transform child in parent)
            {
                var result = DeepFind(child, name);
                if (result != null) return result;
            }
            return null;
        }
    }

    // ==========================================================================
    // LEADERBOARD ROW FACTORY
    // ==========================================================================
    public static class LeaderboardRowFactory
    {
        public static void CreateEntryRow(Transform parent, LeaderboardEntry entry, bool isMe)
        {
            var row = new GameObject("Row", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 80);

            row.AddComponent<Image>().color = isMe
                ? new Color(0f, 0.8f, 1f, 0.15f)
                : new Color(0.05f, 0.1f, 0.15f, 0.7f);

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(20, 20, 10, 10);
            hlg.spacing = 20;
            hlg.childForceExpandWidth = true;
            hlg.childControlWidth = true;

            Color dim = Constants.COLOR_TEXT_DIM;
            Color bright = isMe ? Constants.COLOR_CYAN : Constants.COLOR_NEON_WHITE;
            Color accent = isMe ? Constants.COLOR_CYAN : dim;

            AddCell(row.transform, $"#{entry.rank}", 34, accent, 80f);
            AddCell(row.transform, entry.username, 30, bright, 0f, flexible: true);
            AddCell(row.transform, entry.rankTitle, 24, dim, 160f);
            AddCell(row.transform, $"{entry.score:F1}%", 34, bright, 130f);
        }

        public static void CreateInfoRow(Transform parent, string message)
        {
            var row = new GameObject("InfoRow", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 60);
            var tmp = row.AddComponent<TextMeshProUGUI>();
            tmp.text = message;
            tmp.fontSize = 28;
            tmp.color = Constants.COLOR_TEXT_DIM;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
        }

        private static void AddCell(Transform parent, string text, int size, Color color, float width, bool flexible = false)
        {
            var cell = new GameObject("Cell", typeof(RectTransform));
            cell.transform.SetParent(parent, false);
            var tmp = cell.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.raycastTarget = false;
            var le = cell.AddComponent<LayoutElement>();
            if (width > 0f) le.preferredWidth = width;
            if (flexible) le.flexibleWidth = 1f;
        }
    }
}
