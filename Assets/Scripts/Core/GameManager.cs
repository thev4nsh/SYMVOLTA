using System;
using UnityEngine;
using SYMVOLTA.FirebaseNS;
using SYMVOLTA.Networking;
using SYMVOLTA.Profile;
using SYMVOLTA.Security;
using SYMVOLTA.Audio;
using SYMVOLTA.Effects;

namespace SYMVOLTA.Core
{
    public enum GameState
    {
        Booting,
        InitializingFirebase,
        CheckingNetwork,
        CheckingVersion,
        CheckingMaintenance,
        LoadingProfile,
        Ready,
        MainMenu,
        Playing,
        Offline,
        Blocked
    }

    public class GameManager : Singleton<GameManager>
    {
        [Header("Configuration")]
        [SerializeField] private GameConfig _gameConfig = new GameConfig();

        private GameState _currentState = GameState.Booting;
        private bool _isOfflineMode = false;
        private bool _isInitialized = false;
        private bool _isEnteringOfflineMode = false;
        private bool _initializationInProgress = false;

        public GameState CurrentState => _currentState;
        public GameConfig Config => _gameConfig;
        public bool IsOfflineMode => _isOfflineMode;
        public bool IsInitialized => _isInitialized;

        public event Action<GameState> OnStateChanged;
        public event Action OnGameReady;

        protected override void Awake()
        {
            base.Awake();
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Input.multiTouchEnabled = false; // Single finger drawing only
            Screen.orientation = ScreenOrientation.Portrait;
            Application.runInBackground = false;
        }

        private async void Start()
        {
            NetworkManager.Instance.OnInternetRestored += HandleInternetRestored;
            NetworkManager.Instance.OnInternetLost += HandleInternetLost;
            await InitializeGame();
        }

        private async System.Threading.Tasks.Task InitializeGame()
        {
            if (_initializationInProgress) return;
            _initializationInProgress = true;

            SetState(GameState.Booting);

            SettingsManager.Instance.Initialize();
            AudioManager.Instance.Initialize();
            EffectsManager.Instance.Initialize();
            OfflineSyncManager.Instance.Initialize();
            SecurityManager.Instance.Initialize();

            SetState(GameState.InitializingFirebase);

            // Step 2: Initialize Firebase
            bool firebaseReady = await FirebaseManager.Instance.InitializeFirebase();

            if (!firebaseReady)
            {
                HandleFirebaseInitializationFailed();
                _initializationInProgress = false;
                return;
            }

            // Step 3: Check network
            SetState(GameState.CheckingNetwork);
            bool hasInternet = NetworkManager.Instance.CheckConnectivity();

            if (!hasInternet)
            {
                HandleNoInternet();
                _initializationInProgress = false;
                return;
            }

            // Step 4: Fetch Remote Config rules
            SetState(GameState.CheckingVersion);
            await FirebaseManager.Instance.FetchRemoteConfig();

            _gameConfig.ApplyRemoteConfig(
                FirebaseManager.Instance.GetRemoteConfigString(Constants.RemoteConfig.MIN_VERSION, Constants.GAME_VERSION),
                FirebaseManager.Instance.GetRemoteConfigString(Constants.RemoteConfig.LATEST_VERSION, Constants.GAME_VERSION),
                FirebaseManager.Instance.GetRemoteConfigString(Constants.RemoteConfig.UPDATE_URL, $"https://play.google.com/store/apps/details?id={Constants.BUNDLE_ID}"),
                FirebaseManager.Instance.GetRemoteConfigString(Constants.RemoteConfig.UPDATE_NOTES, "Performance improvements and competitive stability updates."),
                FirebaseManager.Instance.GetRemoteConfigString(Constants.RemoteConfig.TUTORIAL_URL, "https://www.youtube.com/results?search_query=SYMVOLTA+tutorial"),
                FirebaseManager.Instance.GetRemoteConfigString(Constants.RemoteConfig.SUPPORT_URL, "mailto:support@symvolta.game"),
                FirebaseManager.Instance.GetRemoteConfigBool(Constants.RemoteConfig.MAINTENANCE_MODE, false),
                (int)FirebaseManager.Instance.GetRemoteConfigLong(Constants.RemoteConfig.GAME_TIMER_SECONDS, Constants.DEFAULT_TIMER_SECONDS),
                FirebaseManager.Instance.GetRemoteConfigBool(Constants.RemoteConfig.ANNOUNCEMENT_ACTIVE, false),
                FirebaseManager.Instance.GetRemoteConfigString(Constants.RemoteConfig.ANNOUNCEMENT_ID, ""),
                FirebaseManager.Instance.GetRemoteConfigString(Constants.RemoteConfig.ANNOUNCEMENT_TITLE, ""),
                FirebaseManager.Instance.GetRemoteConfigString(Constants.RemoteConfig.ANNOUNCEMENT_BODY, ""),
                (float)FirebaseManager.Instance.GetRemoteConfigDouble(Constants.RemoteConfig.ACCURACY_THRESHOLD, 0.0)
            );
            AnnouncementService.Instance.InitializeFromConfig(_gameConfig);

            // Step 5: Force Update Check
            if (VersionChecker.IsUpdateRequired(_gameConfig.minVersion))
            {
                SetState(GameState.Blocked);
                Debug.Log("[GameManager] Update Required. Game Blocked.");
                _initializationInProgress = false;
                return;
            }

            // Step 6: Maintenance Check
            SetState(GameState.CheckingMaintenance);
            if (_gameConfig.maintenanceMode)
            {
                SetState(GameState.Blocked);
                Debug.Log("[GameManager] Maintenance Mode Active. Game Blocked.");
                _initializationInProgress = false;
                return;
            }

            // Step 7: Authenticate Player
            bool authSuccess = await FirebaseManager.Instance.SignInAnonymously();
            if (!authSuccess)
            {
                _isOfflineMode = true;
            }

            // Step 8: Load Profile
            SetState(GameState.LoadingProfile);
            await ProfileManager.Instance.LoadProfile();
            await NotificationManager.Instance.Initialize();
            await OfflineSyncManager.Instance.SyncPendingScores();

            // Ready!
            SetState(GameState.Ready);
            _isInitialized = true;
            OnGameReady?.Invoke();

            Debug.Log("[GameManager] Initialization Complete. Game Ready!");

            // TRANSITION TO MAIN MENu
            SceneLoader.Instance.LoadScene(Constants.Scenes.MAIN_MENU);
            _initializationInProgress = false;
        
        }

        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;
            _currentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        public void SetOfflineMode(bool offline)
        {
            _isOfflineMode = offline;
        }

        private void HandleFirebaseInitializationFailed()
        {
            HandleNoInternet();
        }

        private void HandleNoInternet()
        {
            _isOfflineMode = true;
            SetState(GameState.Offline);
            Debug.Log("[GameManager] No Internet. Entering Offline Mode.");
        }

        public async void EnterOfflineMode()
        {
            if (_isEnteringOfflineMode) return;
            _isEnteringOfflineMode = true;

            try
            {
                _isOfflineMode = true;
                SetState(GameState.LoadingProfile);

                if (ProfileManager.Instance != null)
                {
                    await ProfileManager.Instance.LoadProfile();
                }

                _isInitialized = true;
                SetState(GameState.Ready);
                OnGameReady?.Invoke();

                if (SceneLoader.Instance != null)
                {
                    SceneLoader.Instance.LoadScene(Constants.Scenes.MAIN_MENU);
                }
            }
            finally
            {
                _isEnteringOfflineMode = false;
            }
        }

        public async void RetryConnection()
        {
            _isOfflineMode = false;
            await InitializeGame();
        }

        private async void HandleInternetRestored()
        {
            if (!_isOfflineMode) return;

            _isOfflineMode = false;
            bool firebaseReady = await FirebaseManager.Instance.InitializeFirebase();
            if (firebaseReady)
            {
                await FirebaseManager.Instance.SignInAnonymously();
                await ProfileManager.Instance.LoadProfile();
                await OfflineSyncManager.Instance.SyncPendingScores();
            }
        }

        private void HandleInternetLost()
        {
            _isOfflineMode = true;
            if (_currentState == GameState.Playing) return;
            SetState(GameState.Offline);
        }

        public void ExitGame()
        {
            Application.Quit();
        }

        protected override void OnDestroy()
        {
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.OnInternetRestored -= HandleInternetRestored;
                NetworkManager.Instance.OnInternetLost -= HandleInternetLost;
            }

            base.OnDestroy();
        }
    }
}
