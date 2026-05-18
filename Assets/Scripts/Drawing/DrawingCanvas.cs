using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using SYMVOLTA.Core;
using SYMVOLTA.Gameplay;
using SYMVOLTA.Shapes;

namespace SYMVOLTA.Drawing
{
    [RequireComponent(typeof(LineRenderer))]
    public class DrawingCanvas : MonoBehaviour
    {
        [Header("Drawing Settings")]
        [SerializeField] private float minPointDistance = 0.045f;
        [SerializeField] private float maxInterpolatedSegment = 0.12f;
        [SerializeField] private float lineWidth = 0.15f;
        [SerializeField, Range(0.05f, 0.95f)] private float smoothing = 0.38f;

        private readonly List<Vector2> _drawnPoints = new List<Vector2>(Constants.MAX_DRAWING_POINTS);
        private readonly Vector3[] _linePositions = new Vector3[Constants.MAX_DRAWING_POINTS];

        private LineRenderer _lineRenderer;
        private Camera _mainCamera;
        private ShapeType _targetShape;
        private bool _isDrawing;
        private bool _hasFinished;
        private int _currentFrame;
        private float _currentAccuracy;
        private float _drawingProgress;
        private Vector2 _filteredPoint;
        private bool _hasFilteredPoint;
        private bool _inputStartedOverUi;

        public bool IsDrawing => _isDrawing;
        public float CurrentAccuracy => _currentAccuracy;
        public List<Vector2> DrawnPoints => _drawnPoints;

        public event Action<float> OnAccuracyUpdated;
        public event Action<Vector2> OnPointAdded;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            SetupLineRenderer();
        }

        private void Start()
        {
            _mainCamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
        }

        public void StartNewDrawing(ShapeType shapeType)
        {
            _targetShape = shapeType;
            ResetCanvas();
            _isDrawing = true;
        }

        public void FinishDrawing()
        {
            if (_hasFinished) return;

            _hasFinished = true;
            _isDrawing = false;

            _currentAccuracy = _drawnPoints.Count >= 10
                ? ShapeDetector.CalculateAccuracy(_drawnPoints, _targetShape)
                : 0f;

            OnAccuracyUpdated?.Invoke(_currentAccuracy);
            GameSessionManager.Instance?.PlayerFinishedDrawing();
        }

        public void ResetCanvas()
        {
            _drawnPoints.Clear();
            _lineRenderer.positionCount = 0;
            _currentAccuracy = 0f;
            _drawingProgress = 0f;
            _hasFinished = false;
            _isDrawing = false;
            _currentFrame = 0;
            _hasFilteredPoint = false;
            _inputStartedOverUi = false;
        }

        private void Update()
        {
            if (!_isDrawing) return;

            HandleInput();

            _currentFrame++;
            if (_currentFrame >= Constants.ACCURACY_UPDATE_INTERVAL_FRAMES && _drawnPoints.Count > 5)
            {
                _currentFrame = 0;
                _drawingProgress = Mathf.Clamp01(_drawnPoints.Count / 180f);
                _currentAccuracy = ShapeDetector.CalculateRealtimeAccuracy(_drawnPoints, _targetShape, _drawingProgress);
                OnAccuracyUpdated?.Invoke(_currentAccuracy);
            }
        }

        private void HandleInput()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.fingerId != 0 && _drawnPoints.Count == 0) return;

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        _inputStartedOverUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId);
                        if (_inputStartedOverUi) return;
                        AddScreenPoint(touch.position);
                        break;
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (_inputStartedOverUi) return;
                        AddScreenPoint(touch.position);
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (_inputStartedOverUi)
                        {
                            _inputStartedOverUi = false;
                            return;
                        }
                        FinishDrawing();
                        break;
                }
                return;
            }

#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                _inputStartedOverUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
                if (!_inputStartedOverUi)
                    AddScreenPoint(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0))
            {
                if (_inputStartedOverUi) return;
                AddScreenPoint(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (_inputStartedOverUi)
                {
                    _inputStartedOverUi = false;
                    return;
                }
                _inputStartedOverUi = false;
                FinishDrawing();
            }
#endif
        }

        private void AddScreenPoint(Vector2 screenPosition)
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
            if (_mainCamera == null) return;

            Vector3 world = _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -_mainCamera.transform.position.z));
            AddWorldPoint(new Vector2(world.x, world.y));
        }

        private void AddWorldPoint(Vector2 point)
        {
            Vector2 filtered = !_hasFilteredPoint
                ? point
                : Vector2.Lerp(_filteredPoint, point, smoothing);

            _filteredPoint = filtered;
            _hasFilteredPoint = true;

            if (_drawnPoints.Count == 0)
            {
                AppendPoint(filtered);
                return;
            }

            Vector2 last = _drawnPoints[_drawnPoints.Count - 1];
            float distance = Vector2.Distance(last, filtered);
            if (distance < minPointDistance) return;

            int segments = Mathf.Clamp(Mathf.CeilToInt(distance / maxInterpolatedSegment), 1, 8);
            for (int i = 1; i <= segments; i++)
            {
                Vector2 interpolated = Vector2.Lerp(last, filtered, i / (float)segments);
                if (_drawnPoints.Count == 0 || Vector2.Distance(_drawnPoints[_drawnPoints.Count - 1], interpolated) >= minPointDistance)
                    AppendPoint(interpolated);
            }
        }

        private void AppendPoint(Vector2 point)
        {
            if (_drawnPoints.Count >= Constants.MAX_DRAWING_POINTS)
            {
                FinishDrawing();
                return;
            }

            int index = _drawnPoints.Count;
            _drawnPoints.Add(point);
            _linePositions[index] = new Vector3(point.x, point.y, 0f);
            _lineRenderer.positionCount = index + 1;
            _lineRenderer.SetPosition(index, _linePositions[index]);
            OnPointAdded?.Invoke(point);

            if (index >= 5 && index % 3 == 0)
            {
                _drawingProgress = Mathf.Clamp01(_drawnPoints.Count / 180f);
                _currentAccuracy = ShapeDetector.CalculateRealtimeAccuracy(_drawnPoints, _targetShape, _drawingProgress);
                OnAccuracyUpdated?.Invoke(_currentAccuracy);
            }
        }

        private void SetupLineRenderer()
        {
            _lineRenderer.positionCount = 0;
            _lineRenderer.startWidth = lineWidth;
            _lineRenderer.endWidth = lineWidth;
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.numCornerVertices = 8;
            _lineRenderer.numCapVertices = 8;
            _lineRenderer.sortingOrder = 10;
            _lineRenderer.textureMode = LineTextureMode.Stretch;

            Shader shader = Shader.Find("Sprites/Default")
                         ?? Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Unlit/Color");

            if (shader != null)
            {
                Material mat = new Material(shader);
                mat.color = Constants.COLOR_NEON_WHITE;
                _lineRenderer.material = mat;
            }

            _lineRenderer.startColor = Constants.COLOR_NEON_WHITE;
            _lineRenderer.endColor = Constants.COLOR_NEON_WHITE;
        }
    }
}
