using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace SYMVOLTA.UI
{
    /// <summary>
    /// Placed on the UIManager. Ensures an EventSystem ALWAYS exists.
    /// Survives scene loads so clicks never die.
    /// </summary>
    public class RuntimeEventSystem : MonoBehaviour
    {
        private GameObject _spawnedEventSystem;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnsureEventSystem();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureEventSystem();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus) EnsureEventSystem();
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null && EventSystem.current.isActiveAndEnabled) return;

            CreateEventSystem();
        }

        private void CreateEventSystem()
        {
            Debug.Log("[RuntimeEventSystem] EventSystem missing! Creating a persistent one...");

            // Destroy our old one if it exists but is broken
            if (_spawnedEventSystem != null)
            {
                Destroy(_spawnedEventSystem);
            }

            _spawnedEventSystem = new GameObject("RuntimeEventSystem");

            // CRITICAL: Parent to UIManager so it survives scene loads!
            _spawnedEventSystem.transform.SetParent(transform, false);

            // Add EventSystem component
            _spawnedEventSystem.AddComponent<EventSystem>();

            // Add the correct Input Module based on what Unity is using
#if ENABLE_INPUT_SYSTEM
            _spawnedEventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#elif ENABLE_LEGACY_INPUT_MANAGER
            _spawnedEventSystem.AddComponent<StandaloneInputModule>();
#else
            _spawnedEventSystem.AddComponent<StandaloneInputModule>();
#endif

            // Add our debug helper
            _spawnedEventSystem.AddComponent<UIDebugHelper>();
        }
    }
}