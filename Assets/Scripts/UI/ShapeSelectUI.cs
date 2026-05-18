using UnityEngine;
using UnityEngine.UI;
using SYMVOLTA.Core;
using SYMVOLTA.Gameplay;

namespace SYMVOLTA.UI
{
    public class ShapeSelectUI : MonoBehaviour
    {
        private void Start()
        {
            WireButtons();
        }

        private void WireButtons()
        {
            FindAndListen("BackButton", OnBackClicked);

            string[] shapeNames = { "Circle", "Triangle", "Square", "Star", "Heart", "Infinity" };
            for (int i = 0; i < shapeNames.Length; i++)
            {
                int index = i;
                FindAndListen($"{shapeNames[i]}Button", () => OnShapeSelected(index));
            }

            Debug.Log("<color=green>[ShapeSelectUI] Buttons wired successfully!</color>");
        }

        private void FindAndListen(string buttonName, UnityEngine.Events.UnityAction callback)
        {
            Transform t = FindDeepChild(transform, buttonName);
            if (t != null)
            {
                Button btn = t.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(callback);
                }
            }
            else
            {
                Debug.LogError($"[ShapeSelectUI] Could not find button: {buttonName}");
            }
        }

        private Transform FindDeepChild(Transform parent, string name)
        {
            Transform found = parent.Find(name);
            if (found != null) return found;

            foreach (Transform child in parent)
            {
                found = FindDeepChild(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private void OnBackClicked()
        {
            Debug.Log("<color=cyan>[ShapeSelectUI] BACK CLICKED!</color>");
            SceneLoader.Instance.LoadScene(Constants.Scenes.MAIN_MENU);
        }

        private void OnShapeSelected(int shapeIndex)
        {
            Debug.Log($"<color=cyan>[ShapeSelectUI] Shape selected: {shapeIndex}</color>");
            ShapeSelectManager manager = FindFirstObjectByType<ShapeSelectManager>();
            if (manager != null)
            {
                manager.OnShapeSelected(shapeIndex);
            }
        }
    }
}