using UnityEngine;
using SYMVOLTA.Core;
using SYMVOLTA.Shapes;

namespace SYMVOLTA.Gameplay
{
    public class ShapeSelectManager : MonoBehaviour
    {
        public void OnShapeSelected(int shapeIndex)
        {
            SYMVOLTA.Shapes.ShapeType selectedShape = (SYMVOLTA.Shapes.ShapeType)shapeIndex;
            Debug.Log($"[ShapeSelectManager] Selected Shape: {selectedShape.DisplayName()}");

            PlayerPrefs.SetInt("SelectedShape", shapeIndex);
            PlayerPrefs.Save();

            SceneLoader.Instance.LoadScene(Constants.Scenes.GAMEPLAY);
        }
    }
}