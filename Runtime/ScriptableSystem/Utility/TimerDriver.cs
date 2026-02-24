using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Counts up or down each frame, writing the elapsed time to a FloatVariable and firing a GameEvent on completion.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Drivers/Timer Driver")]
    public class TimerDriver : VariableDriver<FloatVariable>
    {
        public enum TimerMode
        {
            CountUp,
            CountDown
        }

        [Tooltip("The FloatVariable to receive the current timer value.")]
        [SerializeField] private FloatVariable variable;

        [Tooltip("Count up from zero or count down from duration.")]
        [SerializeField] private TimerMode mode = TimerMode.CountDown;

        [Tooltip("Duration in seconds. For CountUp, this is the target time. For CountDown, this is the starting time.")]
        [SerializeField] private float duration = 1f;

        [Tooltip("Whether the timer loops after completing.")]
        [SerializeField] private bool loop;

        [Tooltip("Start the timer automatically on enable.")]
        [SerializeField] private bool autoStart = true;

        [Tooltip("Use unscaled time (ignores Time.timeScale).")]
        [SerializeField] private bool useUnscaledTime;

        [Tooltip("Optional GameEvent to fire when the timer completes.")]
        [SerializeField] private GameEvent onCompleted;

        [Tooltip("Optional BoolVariable set to true while the timer is running.")]
        [SerializeField] private BoolVariable isRunningVariable;

        protected override FloatVariable Variable => variable;

        private float _currentTime;
        private bool _isRunning;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (autoStart) StartTimer();
        }

        /// <summary>Starts or restarts the timer.</summary>
        public void StartTimer()
        {
            _currentTime = mode == TimerMode.CountDown ? duration : 0f;
            _isRunning = true;
            UpdateRunningState(true);
            WriteValue(_currentTime);
        }

        /// <summary>Stops the timer without resetting.</summary>
        public void StopTimer()
        {
            _isRunning = false;
            UpdateRunningState(false);
        }

        /// <summary>Resumes the timer from where it stopped.</summary>
        public void ResumeTimer()
        {
            _isRunning = true;
            UpdateRunningState(true);
        }

        /// <summary>Resets the timer to its initial value without starting it.</summary>
        public void ResetTimer()
        {
            _currentTime = mode == TimerMode.CountDown ? duration : 0f;
            _isRunning = false;
            UpdateRunningState(false);
            WriteValue(_currentTime);
        }

        private void Update()
        {
            if (!_isRunning || Variable == null) return;

            float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if (mode == TimerMode.CountDown)
            {
                _currentTime -= delta;
                if (_currentTime <= 0f)
                {
                    _currentTime = 0f;
                    HandleCompleted();
                }
            }
            else
            {
                _currentTime += delta;
                if (_currentTime >= duration)
                {
                    _currentTime = duration;
                    HandleCompleted();
                }
            }

            WriteValue(_currentTime);
        }

        private void HandleCompleted()
        {
            if (onCompleted != null)
                onCompleted.Raise();

            if (loop)
            {
                _currentTime = mode == TimerMode.CountDown ? duration : 0f;
            }
            else
            {
                _isRunning = false;
                UpdateRunningState(false);
            }
        }

        private void WriteValue(float value)
        {
            if (SilentUpdates)
                Variable.SetValueWithoutNotify(value);
            else
                Variable.Value = value;
        }

        private void UpdateRunningState(bool running)
        {
            if (isRunningVariable == null) return;
            if (SilentUpdates)
                isRunningVariable.SetValueWithoutNotify(running);
            else
                isRunningVariable.Value = running;
        }
    }
}
