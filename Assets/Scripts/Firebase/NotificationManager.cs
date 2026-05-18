using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Messaging;
using SYMVOLTA.Core;
using SYMVOLTA.Utilities;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace SYMVOLTA.FirebaseNS
{
    public class NotificationManager : Singleton<NotificationManager>
    {
        private const string PostNotificationsPermission = "android.permission.POST_NOTIFICATIONS";
        private bool _initialized;
        private string _fcmToken = "";

        public string FcmToken => _fcmToken;
        public bool NotificationsEnabled => SaveSystem.LoadBool(Constants.SaveKeys.NOTIFICATION_ENABLED, true);

        public event Action<string> OnTokenUpdated;
        public event Action<FirebaseMessage> OnMessageReceived;

        public async Task Initialize()
        {
            if (_initialized) return;

            FirebaseMessaging.TokenReceived += HandleTokenReceived;
            FirebaseMessaging.MessageReceived += HandleMessageReceived;
            FirebaseMessaging.TokenRegistrationOnInitEnabled = true;
            _initialized = true;

            if (NotificationsEnabled)
            {
                RequestNotificationPermissionIfNeeded();
            }

            try
            {
                _fcmToken = await FirebaseMessaging.GetTokenAsync();
                if (!string.IsNullOrEmpty(_fcmToken))
                    OnTokenUpdated?.Invoke(_fcmToken);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[NotificationManager] FCM token unavailable: {e.Message}");
            }
        }

        public void SetNotificationsEnabled(bool enabled)
        {
            SaveSystem.SaveBool(Constants.SaveKeys.NOTIFICATION_ENABLED, enabled);
            SettingsManager.Instance?.SetNotificationsEnabled(enabled);

            if (enabled)
            {
                RequestNotificationPermissionIfNeeded();
            }
        }

        public void RequestNotificationPermissionIfNeeded()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!Permission.HasUserAuthorizedPermission(PostNotificationsPermission))
            {
                Permission.RequestUserPermission(PostNotificationsPermission);
            }
#endif
        }

        public Task SubscribeToTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic)) return Task.CompletedTask;
            return FirebaseMessaging.SubscribeAsync(topic.Trim());
        }

        public Task UnsubscribeFromTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic)) return Task.CompletedTask;
            return FirebaseMessaging.UnsubscribeAsync(topic.Trim());
        }

        private void HandleTokenReceived(object sender, TokenReceivedEventArgs token)
        {
            _fcmToken = token.Token;
            SaveSystem.SaveString("symvolta_fcm_token", _fcmToken);
            OnTokenUpdated?.Invoke(_fcmToken);
        }

        private void HandleMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            OnMessageReceived?.Invoke(e.Message);
        }

        protected override void OnDestroy()
        {
            FirebaseMessaging.TokenReceived -= HandleTokenReceived;
            FirebaseMessaging.MessageReceived -= HandleMessageReceived;
            base.OnDestroy();
        }
    }
}
