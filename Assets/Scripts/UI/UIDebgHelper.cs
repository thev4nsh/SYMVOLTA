using UnityEngine;
using UnityEngine.EventSystems;

namespace SYMVOLTA.UI
{
    /// <summary>
    /// Add this to the EventSystem to log EVERY UI click. Helps debug touch issues.
    /// </summary>
    public class UIDebugHelper : MonoBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            GameObject hit = eventData.pointerCurrentRaycast.gameObject;
            if (hit != null)
                Debug.Log($"<color=yellow>[UI DEBUG] Click detected on: {hit.name}</color>");
        }
    }
}
