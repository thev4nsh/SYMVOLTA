using System;
using UnityEngine;
using SYMVOLTA.Core;
using SYMVOLTA.Drawing;
using SYMVOLTA.Shapes;
using SYMVOLTA.Profile;
using SYMVOLTA.Security;

namespace SYMVOLTA.Gameplay
{
    public enum SessionState
    {
        None,
        Playing,
        WarningPhase,
        Finished
    }

    public class GameSessionManager : Singleton<GameSessionManager>
    {
        [Header("Session Data")]
        [SerializeField] private ShapeType _currentShapeType;
        [SerializeField] private SessionState _currentState = SessionState.None;

        [Header("Timer")]
        [SerializeField] private float _timeRemaining;
        private bool _isTimerRunning = false;
        private bool _isInWarningPhase = false;

        private float _realtimeAccuracy = 0f;
        private DrawingCanvas _drawingCanvas;

        public SessionState CurrentState => _currentState;
        public ShapeType CurrentShape => _currentShapeType;
        public float TimeRemaining => _timeRemaining;
        public float RealtimeAccuracy => _realtimeAccuracy;

        public event Action OnSessionStarted;
        public event Action<float> OnTimerTick;
        public event Action<float> OnAccuracyUpdated;
        public event Action OnWarningPhaseStarted;
        public event Action<float> OnSessionEnded;

        protected override void Awake()
        {
            base.Awake();
            Debug.Log($"[GameSessionManager] Awake! InstanceID={GetInstanceID()}");
        }

        public void StartSession(ShapeType shapeType)
        {
            Debug.Log($"<color=cyan>[GameSessionManager] StartSession called! Shape: {shapeType}, InstanceID={GetInstanceID()}</color>");

            _currentShapeType = shapeType;
            _realtimeAccuracy = 0f;
            _isInWarningPhase = false;
            _currentState = SessionState.Playing;

            int maxTime = GameManager.Instance != null
                ? GameManager.Instance.Config.timerSeconds
                : Constants.DEFAULT_TIMER_SECONDS;

            _timeRemaining = maxTime;
            _isTimerRunning = true;
            GameManager.Instance?.SetState(GameState.Playing);

            _drawingCanvas = FindFirstObjectByType<DrawingCanvas>();
            if (_drawingCanvas != null)
            {
                _drawingCanvas.OnAccuracyUpdated -= HandleDrawingAccuracyUpdated;
                _drawingCanvas.ResetCanvas();
                _drawingCanvas.OnAccuracyUpdated += HandleDrawingAccuracyUpdated;
                _drawingCanvas.StartNewDrawing(shapeType);
                Debug.Log("[GameSessionManager] DrawingCanvas found and started.");
            }
            else
            {
                Debug.LogError("[GameSessionManager] DrawingCanvas NOT FOUND!");
            }

            SecurityManager.Instance?.StartMatch();

            Debug.Log($"[GameSessionManager] Firing OnSessionStarted. Subscribers: {OnSessionStarted?.GetInvocationList()?.Length ?? 0}");
            OnSessionStarted?.Invoke();
            OnAccuracyUpdated?.Invoke(0f);
            OnTimerTick?.Invoke(_timeRemaining);
        }

        public void PlayerFinishedDrawing()
        {
            Debug.Log($"<color=yellow>[GameSessionManager] PlayerFinishedDrawing called! State: {_currentState}, InstanceID={GetInstanceID()}</color>");

            if (_currentState == SessionState.Finished)
            {
                Debug.Log("[GameSessionManager] Already finished. Ignoring.");
                return;
            }
            EndSession();
        }

        public void QuitSession()
        {
            Debug.Log($"[GameSessionManager] QuitSession called! InstanceID={GetInstanceID()}");

            _currentState = SessionState.Finished;
            _isTimerRunning = false;
            _drawingCanvas?.ResetCanvas();
            UI.UIWiring.UnsubscribeGameplayEvents();
            GameManager.Instance?.SetState(GameState.MainMenu);
            SceneLoader.Instance.LoadScene(Constants.Scenes.MAIN_MENU);
        }

        private void Update()
        {
            if (_currentState != SessionState.Playing &&
                _currentState != SessionState.WarningPhase) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                QuitSession();
                return;
            }

            if (!_isTimerRunning) return;

            _timeRemaining -= Time.unscaledDeltaTime;

            if (!_isInWarningPhase && _timeRemaining <= 20f)
            {
                _isInWarningPhase = true;
                _currentState = SessionState.WarningPhase;
                OnWarningPhaseStarted?.Invoke();
            }

            if (_timeRemaining <= 0f)
            {
                _timeRemaining = 0f;
                _isTimerRunning = false;
                EndSession();
                return;
            }

            OnTimerTick?.Invoke(_timeRemaining);
        }

        private void EndSession()
        {
            if (_currentState == SessionState.Finished)
            {
                Debug.Log("[GameSessionManager] EndSession: already finished. Skipping.");
                return;
            }

            _currentState = SessionState.Finished;
            _isTimerRunning = false;
            GameManager.Instance?.SetState(GameState.Ready);

            Debug.Log($"<color=yellow>[GameSessionManager] EndSession called! InstanceID={GetInstanceID()}</color>");

            SecurityManager.Instance?.EndMatch();

            if (_drawingCanvas == null)
                _drawingCanvas = FindFirstObjectByType<DrawingCanvas>();

            if (_drawingCanvas != null)
            {
                _drawingCanvas.OnAccuracyUpdated -= HandleDrawingAccuracyUpdated;
                _drawingCanvas.FinishDrawing(); // Safe: guarded by _hasFinished
                _realtimeAccuracy = _drawingCanvas.CurrentAccuracy;
                Debug.Log($"[GameSessionManager] Accuracy from canvas: {_realtimeAccuracy:F1}%");
            }
            else
            {
                Debug.LogError("[GameSessionManager] DrawingCanvas is NULL in EndSession!");
            }

            // Submit score
            if (ProfileManager.Instance != null)
            {
                bool accepted = ProfileManager.Instance.SubmitScore(_currentShapeType, _realtimeAccuracy);
                if (accepted && GameManager.Instance != null && !GameManager.Instance.IsOfflineMode)
                    _ = Leaderboard.LeaderboardManager.Instance?.SubmitScore(_currentShapeType, _realtimeAccuracy);
            }

            Debug.Log($"<color=yellow>[GameSessionManager] Firing OnSessionEnded! Accuracy: {_realtimeAccuracy:F1}%, InstanceID={GetInstanceID()}, Subscribers: {OnSessionEnded?.GetInvocationList()?.Length ?? 0}</color>");
            OnSessionEnded?.Invoke(_realtimeAccuracy);
        }

        private void HandleDrawingAccuracyUpdated(float accuracy)
        {
            _realtimeAccuracy = accuracy;
            OnAccuracyUpdated?.Invoke(accuracy);
        }

        protected override void OnDestroy()
        {
            Debug.Log($"<color=red>[GameSessionManager] OnDestroy called! InstanceID={GetInstanceID()}. Stack trace:\n{StackTraceUtility.ExtractStackTrace()}</color>");
            base.OnDestroy();
        }
    }
}
