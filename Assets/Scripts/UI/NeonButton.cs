using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SYMVOLTA.UI
{
    [RequireComponent(typeof(Button))]
    public class NeonButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Glow Settings")]
        [SerializeField] private Color glowColor = new Color(0f, 0.8f, 1f, 0.8f);

        private Vector3 _originalScale;
        private Button _button;
        private bool _pressed;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _originalScale = transform.localScale;

            Outline outline = gameObject.AddComponent<Outline>();
            outline.effectColor = glowColor;
            outline.effectDistance = new Vector2(1.5f, 1.5f);
        }

        private void Update()
        {
            Vector3 target = _pressed ? _originalScale * 0.96f : _originalScale;
            transform.localScale = Vector3.Lerp(transform.localScale, target, Time.unscaledDeltaTime * 18f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            _pressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pressed = false;
        }

        private void OnDisable()
        {
            _pressed = false;
            transform.localScale = _originalScale;
        }
    }
}
