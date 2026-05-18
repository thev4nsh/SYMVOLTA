using System.Collections;
using UnityEngine;
using SYMVOLTA.Core;
using SYMVOLTA.Shapes;

namespace SYMVOLTA.Gameplay
{
    /// <summary>
    /// Reads what shape was selected and starts the Game Session.
    /// Waits 1 frame before starting so UIWiring can subscribe to events first.
    /// </summary>
    public class GameplayInitializer : MonoBehaviour
    {
        private IEnumerator Start()
        {
            // Wait 1 frame so SceneBootstrap.Start() can wire UIWiring first.
            // Without this, GameSessionManager fires OnSessionStarted before
            // UIWiring has subscribed, and the GameOver panel never shows.
            yield return null;

            int shapeIndex = PlayerPrefs.GetInt("SelectedShape", 0);
            SYMVOLTA.Shapes.ShapeType selectedShape = (SYMVOLTA.Shapes.ShapeType)shapeIndex;

            Debug.Log($"[GameplayInitializer] Starting session for shape: {selectedShape.DisplayName()}");

            if (GameSessionManager.Instance != null)
            {
                GameSessionManager.Instance.StartSession(selectedShape);
            }
            else
            {
                Debug.LogError("[GameplayInitializer] GameSessionManager not found!");
            }
        }
    }
}