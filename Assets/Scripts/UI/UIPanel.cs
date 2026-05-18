using System.Collections;
using UnityEngine;

namespace SYMVOLTA.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPanel : MonoBehaviour
    {
        [Header("Panel Settings")]
        [SerializeField] protected float fadeInDuration = 0.3f;
        [SerializeField] protected float fadeOutDuration = 0.2f;

        private CanvasGroup _canvasGroup;
        private Coroutine _fadeCoroutine;

        public bool IsVisible => _canvasGroup != null && _canvasGroup.alpha > 0.9f;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// Shows the panel with a smooth fade-in.
        /// </summary>
        public virtual void Show()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeRoutine(1f, fadeInDuration));
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
        }

        /// <summary>
        /// Hides the panel with a smooth fade-out.
        /// </summary>
        public virtual void Hide()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeRoutine(0f, fadeOutDuration));
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

        /// <summary>
        /// Instantly sets the panel visible or hidden without animation.
        /// </summary>
        public void SetVisibleInstant(bool visible)
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.blocksRaycasts = visible;
            _canvasGroup.interactable = visible;
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration)
        {
            float startAlpha = _canvasGroup.alpha;
            float time = 0f;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
            _fadeCoroutine = null;
        }
    }
}