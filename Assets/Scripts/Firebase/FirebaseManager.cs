using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using SYMVOLTA.Core;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.RemoteConfig;
using Firebase.Analytics;
using Firebase.Messaging;

namespace SYMVOLTA.FirebaseNS
{
    public class FirebaseManager : Singleton<FirebaseManager>
    {
        private FirebaseApp _app;
        private bool _isFirebaseReady;
        private FirebaseAuth _auth;
        private FirebaseFirestore _db;
        private bool _hasFetchedRemoteConfig;

        public bool IsFirebaseReady => _isFirebaseReady;
        public bool IsRemoteConfigFetched => _hasFetchedRemoteConfig;
        public FirebaseAuth Auth => _auth;
        public FirebaseFirestore Db => _db;

        public async Task<bool> InitializeFirebase()
        {
            if (_isFirebaseReady) return true;

            try
            {
                DependencyStatus dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

                if (dependencyStatus != DependencyStatus.Available)
                {
                    Debug.LogError($"[FirebaseManager] Could not resolve Firebase dependencies: {dependencyStatus}");
                    _isFirebaseReady = false;
                    return false;
                }

                _app = FirebaseApp.DefaultInstance;
                _auth = FirebaseAuth.DefaultInstance;
                _db = FirebaseFirestore.DefaultInstance;
                FirebaseMessaging.TokenRegistrationOnInitEnabled = true;

                _isFirebaseReady = true;
                Debug.Log("[FirebaseManager] Firebase initialized.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[FirebaseManager] Initialization failed: {e.Message}");
                _isFirebaseReady = false;
                return false;
            }
        }

        public async Task<bool> SignInAnonymously()
        {
            if (!_isFirebaseReady || _auth == null) return false;

            try
            {
                if (_auth.CurrentUser != null) return true;

                AuthResult result = await _auth.SignInAnonymouslyAsync();
                Debug.Log($"[FirebaseManager] Signed in anonymously. UID: {result.User.UserId}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[FirebaseManager] Anonymous sign-in failed: {e.Message}");
                return false;
            }
        }

        public async Task FetchRemoteConfig()
        {
            if (!_isFirebaseReady) return;

            try
            {
                Dictionary<string, object> defaults = new Dictionary<string, object>
                {
                    { Constants.RemoteConfig.MIN_VERSION, Constants.GAME_VERSION },
                    { Constants.RemoteConfig.LATEST_VERSION, Constants.GAME_VERSION },
                    { Constants.RemoteConfig.UPDATE_URL, $"https://play.google.com/store/apps/details?id={Constants.BUNDLE_ID}" },
                    { Constants.RemoteConfig.UPDATE_NOTES, "Performance improvements and competitive stability updates." },
                    { Constants.RemoteConfig.TUTORIAL_URL, "https://www.youtube.com/results?search_query=SYMVOLTA+tutorial" },
                    { Constants.RemoteConfig.SUPPORT_URL, "mailto:support@symvolta.game" },
                    { Constants.RemoteConfig.MAINTENANCE_MODE, false },
                    { Constants.RemoteConfig.GAME_TIMER_SECONDS, Constants.DEFAULT_TIMER_SECONDS },
                    { Constants.RemoteConfig.ANNOUNCEMENT_ACTIVE, false },
                    { Constants.RemoteConfig.ANNOUNCEMENT_ID, "" },
                    { Constants.RemoteConfig.ANNOUNCEMENT_TITLE, "" },
                    { Constants.RemoteConfig.ANNOUNCEMENT_BODY, "" },
                    { Constants.RemoteConfig.ACCURACY_THRESHOLD, 0.0 }
                };

                FirebaseRemoteConfig config = FirebaseRemoteConfig.DefaultInstance;
                await config.SetDefaultsAsync(defaults);
                await config.FetchAsync(TimeSpan.FromHours(1));
                await config.ActivateAsync();

                _hasFetchedRemoteConfig = true;
                Debug.Log("[FirebaseManager] Remote Config fetched and activated.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[FirebaseManager] Remote Config fetch failed: {e.Message}");
                _hasFetchedRemoteConfig = false;
            }
        }

        public string GetRemoteConfigString(string key, string defaultValue = "")
        {
            if (!_hasFetchedRemoteConfig) return defaultValue;
            string value = FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        public bool GetRemoteConfigBool(string key, bool defaultValue = false)
        {
            if (!_hasFetchedRemoteConfig) return defaultValue;
            return FirebaseRemoteConfig.DefaultInstance.GetValue(key).BooleanValue;
        }

        public long GetRemoteConfigLong(string key, long defaultValue = 0)
        {
            if (!_hasFetchedRemoteConfig) return defaultValue;
            long value = FirebaseRemoteConfig.DefaultInstance.GetValue(key).LongValue;
            return value == 0 ? defaultValue : value;
        }

        public double GetRemoteConfigDouble(string key, double defaultValue = 0.0)
        {
            if (!_hasFetchedRemoteConfig) return defaultValue;
            return FirebaseRemoteConfig.DefaultInstance.GetValue(key).DoubleValue;
        }

        public void LogEvent(string eventName)
        {
            if (!_isFirebaseReady || string.IsNullOrWhiteSpace(eventName)) return;
            FirebaseAnalytics.LogEvent(eventName);
        }

        public void LogEvent(string eventName, string paramName, string paramValue)
        {
            if (!_isFirebaseReady || string.IsNullOrWhiteSpace(eventName)) return;
            FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
        }
    }
}
