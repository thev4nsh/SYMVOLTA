using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SYMVOLTA.Core
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        private string _currentScene = "";
        private string _targetScene = "";
        private bool _isLoading = false;
        private Action _onLoadComplete;

        public string CurrentScene => _currentScene;
        public bool IsLoading => _isLoading;

        public event Action<string> OnSceneLoadStarted;
        public event Action<string> OnSceneLoadComplete;

        protected override void Awake()
        {
            base.Awake();
            _currentScene = SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// Starts an asynchronous scene load with a small delay for smooth transitions.
        /// </summary>
        /// <param name="sceneName">The exact name of the Unity scene to load.</param>
        /// <param name="onComplete">Optional callback when load finishes.</param>
        public void LoadScene(string sceneName, Action onComplete = null)
        {
            // FAILSAFE: If stuck loading, force reset so buttons don't die forever
            if (_isLoading)
            {
                Debug.LogWarning("[SceneLoader] Was stuck loading. Force resetting to allow new click.");
                _isLoading = false;
            }

            _isLoading = true;
            _targetScene = sceneName;
            _onLoadComplete = onComplete;
            OnSceneLoadStarted?.Invoke(sceneName);
            Debug.Log($"[SceneLoader] Starting to load scene: {sceneName}");

            StartCoroutine(LoadSceneSequence(sceneName));
        }

        private IEnumerator LoadSceneSequence(string sceneName)
        {
            AsyncOperation targetOp = null;

            try
            {
                targetOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SceneLoader] FAILED to start loading scene '{sceneName}': {e.Message}");
                _isLoading = false; // CRITICAL: Reset so buttons don't get locked!
                yield break;
            }

            if (targetOp == null)
            {
                Debug.LogError($"[SceneLoader] LoadSceneAsync returned null for '{sceneName}'. Is the scene added to Build Settings?");
                _isLoading = false; // Reset lock
                yield break;
            }

            targetOp.allowSceneActivation = true;

            while (!targetOp.isDone)
            {
                yield return null;
            }

            _currentScene = sceneName;
            _isLoading = false;
            OnSceneLoadComplete?.Invoke(sceneName);
            _onLoadComplete?.Invoke();
            Debug.Log($"[SceneLoader] Successfully loaded scene: {sceneName}");
        }

        public void ReloadCurrentScene()
        {
            LoadScene(_currentScene);
        }
    }
}