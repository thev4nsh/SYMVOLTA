using UnityEngine;
using SYMVOLTA.Core;

namespace SYMVOLTA.UI
{
    public class UIManager : Singleton<UIManager>
    {
        [Header("System Popups (Drag here in Inspector)")]
        [SerializeField] public UIPanel forceUpdatePopup;
        [SerializeField] public UIPanel offlinePopup;
        [SerializeField] public UIPanel maintenancePopup;
        [SerializeField] public UIPanel announcementPopup;

        [Header("System References")]
        [SerializeField] public TMPro.TextMeshProUGUI forceUpdateCurrentVersionText;
        [SerializeField] public TMPro.TextMeshProUGUI forceUpdateLatestVersionText;
        [SerializeField] public TMPro.TextMeshProUGUI forceUpdateNotesText;
        [SerializeField] public TMPro.TextMeshProUGUI announcementTitleText;
        [SerializeField] public TMPro.TextMeshProUGUI announcementBodyText;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            // Listen to GameManager state changes
            GameManager.Instance.OnStateChanged += HandleGameStateChange;
            FirebaseNS.AnnouncementService.Instance.OnAnnouncementReady += HandleAnnouncementReady;

            // DOUBLE CHECK: In case GameManager fired the event before we subscribed
            if (GameManager.Instance != null)
            {
                HandleGameStateChange(GameManager.Instance.CurrentState);
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null) return;

            bool hardBlocked = GameManager.Instance.CurrentState == GameState.Blocked;
            if (hardBlocked && Input.GetKeyDown(KeyCode.Escape))
            {
                // Mandatory update and maintenance screens intentionally consume Back.
            }
        }

        private void HandleGameStateChange(GameState newState)
        {
            // Hide all system popups first
            HideAllSystemPopups();

            switch (newState)
            {
                case GameState.Offline:
                    ShowOfflinePopup();
                    break;

                case GameState.Blocked:
                    // Blocked can mean either Update Required or Maintenance Mode
                    if (VersionChecker.IsUpdateRequired(GameManager.Instance.Config.minVersion))
                    {
                        ShowForceUpdatePopup();
                    }
                    else if (GameManager.Instance.Config.maintenanceMode)
                    {
                        ShowMaintenancePopup();
                    }
                    break;
            }
        }

        private void HideAllSystemPopups()
        {
            if (forceUpdatePopup != null) forceUpdatePopup.Hide();
            if (offlinePopup != null) offlinePopup.Hide();
            if (maintenancePopup != null) maintenancePopup.Hide();
            if (announcementPopup != null) announcementPopup.Hide();
        }

        public void ShowForceUpdatePopup()
        {
            if (forceUpdatePopup == null) return;

            if (forceUpdateCurrentVersionText != null)
                forceUpdateCurrentVersionText.text = $"Current: {Constants.GAME_VERSION}";

            if (forceUpdateLatestVersionText != null)
                forceUpdateLatestVersionText.text = $"Latest: {GameManager.Instance.Config.latestVersion}";

            if (forceUpdateNotesText != null)
                forceUpdateNotesText.text = GameManager.Instance.Config.updateNotes;

            forceUpdatePopup.Show();
        }

        public void ShowOfflinePopup()
        {
            if (offlinePopup != null) offlinePopup.Show();
        }

        public void ShowMaintenancePopup()
        {
            if (maintenancePopup != null) maintenancePopup.Show();
        }

        public void ShowAnnouncementPopup()
        {
            if (announcementPopup != null) announcementPopup.Show();
        }

        private void HandleAnnouncementReady(FirebaseNS.Announcement announcement)
        {
            if (announcementTitleText != null) announcementTitleText.text = announcement.title;
            if (announcementBodyText != null) announcementBodyText.text = announcement.body;
            ShowAnnouncementPopup();
        }

        #region UI Button Callbacks

        public void OnClick_RetryConnection()
        {
            GameManager.Instance.RetryConnection();
        }

        public void OnClick_EnterOfflineMode()
        {
            GameManager.Instance.EnterOfflineMode();
        }

        public void OnClick_ExitGame()
        {
            GameManager.Instance.ExitGame();
        }

        public void OnClick_OpenStoreToUpdate()
        {
            string url = GameManager.Instance.Config.updateUrl;

            if (!string.IsNullOrEmpty(url))
            {
                Application.OpenURL(url);
            }
            else
            {
                // Fallback just in case it's empty in Firebase
                Application.OpenURL($"https://play.google.com/store/apps/details?id={Constants.BUNDLE_ID}");
            }
        }
        public void OnClick_OpenTutorial()
        {
            string tutorialUrl = GameManager.Instance.Config.tutorialUrl;

            Application.OpenURL(tutorialUrl);
        }

        public void OnClick_OpenSupport()
        {
            Application.OpenURL(GameManager.Instance.Config.supportUrl);
        }

        public void OnClick_CloseAnnouncement()
        {
            FirebaseNS.AnnouncementService.Instance.MarkSeen(FirebaseNS.AnnouncementService.Instance.Current);
            if (announcementPopup != null) announcementPopup.Hide();
        }

        public void OnClick_SetNotifications(bool enabled)
        {
            FirebaseNS.NotificationManager.Instance.SetNotificationsEnabled(enabled);
        }
        #endregion

        protected override void OnDestroy()
        {
            if (FirebaseNS.AnnouncementService.Instance != null)
                FirebaseNS.AnnouncementService.Instance.OnAnnouncementReady -= HandleAnnouncementReady;

            base.OnDestroy();
        }
    }
}
