using UnityEngine;
using UnityEngine.UI;

namespace SYMVOLTA.Effects
{
    public class NeonPulse : MonoBehaviour
    {
        [SerializeField] private float speed = 2f;
        [SerializeField] private float intensity = 0.25f;

        private Graphic _graphic;
        private Color _baseColor;
        private Vector3 _baseScale;

        private void Awake()
        {
            _graphic = GetComponent<Graphic>();
            if (_graphic != null) _baseColor = _graphic.color;
            _baseScale = transform.localScale;
        }

        private void Update()
        {
            float pulse = (Mathf.Sin(Time.unscaledTime * speed) + 1f) * 0.5f;
            transform.localScale = _baseScale * (1f + pulse * intensity * 0.05f);

            if (_graphic != null)
            {
                Color c = _baseColor;
                c.a = Mathf.Clamp01(_baseColor.a + pulse * intensity);
                _graphic.color = c;
            }
        }
    }
}
