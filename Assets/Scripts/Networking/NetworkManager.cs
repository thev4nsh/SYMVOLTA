using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using SYMVOLTA.Core;

namespace SYMVOLTA.Networking
{
    /// <summary>
    /// Monitors internet connectivity using real ping tests.
    /// Unity's built-in reachability only checks if a network exists, 
    /// this verifies if we can actually reach the outside world.
    /// </summary>
    public class NetworkManager : Singleton<NetworkManager>
    {
        private bool _isConnected = true;
        private float _checkInterval = 10f; // Check every 10 seconds in background
        private Coroutine _backgroundCheckRoutine;

        public bool IsConnected => _isConnected;

        public event Action OnInternetLost;
        public event Action OnInternetRestored;

        protected override void Awake()
        {
            base.Awake();
            _isConnected = Application.internetReachability != NetworkReachability.NotReachable;
        }

        private void Start()
        {
            StartBackgroundCheck();
        }

        /// <summary>
        /// Instantly checks Unity's basic reachability without a ping.
        /// Good for quick UI checks.
        /// </summary>
        public bool CheckConnectivity()
        {
            bool reachable = Application.internetReachability != NetworkReachability.NotReachable;
            UpdateConnectionState(reachable);
            return reachable;
        }

        /// <summary>
        /// Performs an actual web request to Google/Firebase to verify true internet access.
        /// </summary>
        public void CheckInternetWithPing(Action<bool> onCheckComplete)
        {
            StartCoroutine(PingRoutine(onCheckComplete));
        }

        private IEnumerator PingRoutine(Action<bool> onCheckComplete)
        {
            // Quick check first
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                UpdateConnectionState(false);
                onCheckComplete?.Invoke(false);
                yield break;
            }

            // Real ping check using UnityWebRequest
            using (UnityWebRequest request = UnityWebRequest.Head("https://firebase.google.com"))
            {
                request.timeout = 5; // 5 second timeout
                yield return request.SendWebRequest();

                bool success = request.result == UnityWebRequest.Result.Success;
                UpdateConnectionState(success);
                onCheckComplete?.Invoke(success);
            }
        }

        private void StartBackgroundCheck()
        {
            if (_backgroundCheckRoutine != null)
            {
                StopCoroutine(_backgroundCheckRoutine);
            }
            _backgroundCheckRoutine = StartCoroutine(BackgroundCheckRoutine());
        }

        private IEnumerator BackgroundCheckRoutine()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(_checkInterval);

                // Don't do heavy ping if game is paused, just quick check
                if (Application.isFocused)
                {
                    CheckInternetWithPing(null);
                }
                else
                {
                    CheckConnectivity();
                }
            }
        }

        private void UpdateConnectionState(bool currentlyConnected)
        {
            if (_isConnected && !currentlyConnected)
            {
                _isConnected = false;
                Debug.Log("[NetworkManager] Internet Connection LOST.");
                OnInternetLost?.Invoke();
            }
            else if (!_isConnected && currentlyConnected)
            {
                _isConnected = true;
                Debug.Log("[NetworkManager] Internet Connection RESTORED.");
                OnInternetRestored?.Invoke();
            }
        }

        public void SetCheckInterval(float seconds)
        {
            _checkInterval = Mathf.Max(5f, seconds); // Minimum 5 seconds to prevent spam
        }
    }
}