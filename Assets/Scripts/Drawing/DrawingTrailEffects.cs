using UnityEngine;
using SYMVOLTA.Audio;
using SYMVOLTA.Effects;

namespace SYMVOLTA.Drawing
{
    [RequireComponent(typeof(DrawingCanvas))]
    public class DrawingTrailEffects : MonoBehaviour
    {
        [SerializeField] private GameObject pointParticlePrefab;
        [SerializeField] private AudioClip drawingTick;
        [SerializeField] private int particleEveryNPoints = 5;

        private DrawingCanvas _canvas;
        private int _pointCounter;

        private void Awake()
        {
            _canvas = GetComponent<DrawingCanvas>();
        }

        private void OnEnable()
        {
            _canvas.OnPointAdded += HandlePointAdded;
        }

        private void OnDisable()
        {
            _canvas.OnPointAdded -= HandlePointAdded;
        }

        private void HandlePointAdded(Vector2 point)
        {
            _pointCounter++;

            if (drawingTick != null && _pointCounter % 12 == 0)
                AudioManager.Instance?.PlaySFX(drawingTick, 0.04f);

            if (pointParticlePrefab != null && _pointCounter % particleEveryNPoints == 0)
                EffectsManager.Instance?.PlayParticle(pointParticlePrefab, point);
        }
    }
}
