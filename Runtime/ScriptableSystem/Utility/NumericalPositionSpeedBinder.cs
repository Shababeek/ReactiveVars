using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds any numeric variable to an object's movement speed between two positions.
    /// </summary>
    /// <remarks>
    /// Unlike NumericalPositionBinder which maps values directly to positions,
    /// this binder maps values to movement speed. A value of -1 moves toward start position,
    /// 0 stops movement, and 1 moves toward end position.
    ///
    /// Common use cases include:
    /// - Sliding doors (button held = door moves, released = stops)
    /// - Conveyor belts (speed control)
    /// - Elevator platforms (up/down input)
    /// - Throttle-controlled movement
    /// </remarks>
    [AddComponentMenu("Shababeek/ReactiveVars/Binders/Numerical Position Speed Binder")]
    public class NumericalPositionSpeedBinder : NumericalVariableBinder
    {
        [Tooltip("The numeric variable to bind (IntVariable or FloatVariable).")]
        [SerializeField] private ScriptableVariable variable;

        [Header("Positions")]
        [Tooltip("The starting position (reached when moving at negative speed).")]
        [SerializeField] private Vector3 startPosition;

        [Tooltip("The ending position (reached when moving at positive speed).")]
        [SerializeField] private Vector3 endPosition;

        [Tooltip("Whether to use local position instead of world position.")]
        [SerializeField] private bool useLocalPosition = true;

        [Header("Value Mapping")]
        [Tooltip("The variable value that maps to maximum speed toward start position.")]
        [SerializeField] private float minValue = -1f;

        [Tooltip("The variable value that maps to maximum speed toward end position.")]
        [SerializeField] private float maxValue = 1f;

        [Header("Speed Settings")]
        [Tooltip("Maximum movement speed in units per second.")]
        [SerializeField] private float maxSpeed = 2f;

        [Tooltip("Values within this threshold from center will be treated as zero (no movement).")]
        [SerializeField] private float deadZone = 0.01f;

        [Header("Behavior")]
        [Tooltip("If true, object stops at start/end positions. If false, wraps around.")]
        [SerializeField] private bool clampToEndpoints = true;

        [Tooltip("Smoothly accelerate/decelerate instead of instant speed change.")]
        [SerializeField] private bool smoothAcceleration = false;

        [Tooltip("Acceleration rate when smoothAcceleration is enabled.")]
        [SerializeField] private float accelerationRate = 10f;

        [Tooltip("Avoid Position Control")]
        [SerializeField] private bool _isPaused = false;

        private float _targetSpeed;
        private float _currentSpeed;
        private float _currentT; // 0 = start, 1 = end

        protected override ScriptableVariable Variable => variable;

        /// <summary>Pauses position movement.</summary>
        public void Pause() => _isPaused = true;

        /// <summary>Resumes position movement.</summary>
        public void Resume() => _isPaused = false;

        /// <summary>Sets the paused state.</summary>
        public void SetPaused(bool paused) => _isPaused = paused;

        protected override void BindNumerical()
        {
            _currentT = CalculateCurrentT();
            ApplyPosition();
            UpdateSpeed(NumericalVariable.AsFloat);
            
        }

        protected override void OnNumericalValueChanged()
        {
            UpdateSpeed(NumericalVariable.AsFloat);
        }

        private void Update()
        {
            if (_isPaused) return;

            if (smoothAcceleration)
            {
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, _targetSpeed, accelerationRate * Time.deltaTime);
            }
            else
            {
                _currentSpeed = _targetSpeed;
            }

            if (Mathf.Approximately(_currentSpeed, 0f)) return;

            float totalDistance = Vector3.Distance(startPosition, endPosition);
            if (Mathf.Approximately(totalDistance, 0f)) return;

            float deltaT = (_currentSpeed / totalDistance) * Time.deltaTime;
            float newT = _currentT + deltaT;

            if (clampToEndpoints)
            {
                newT = Mathf.Clamp01(newT);
            }
            else
            {
                newT = Mathf.Repeat(newT, 1f);
            }

            _currentT = newT;
            ApplyPosition();
        }

        private void UpdateSpeed(float value)
        {
            float center = (minValue + maxValue) / 2f;
            float range = (maxValue - minValue) / 2f;

            if (Mathf.Approximately(range, 0f))
            {
                _targetSpeed = 0f;
                return;
            }

            float normalizedValue = (value - center) / range;

            if (Mathf.Abs(normalizedValue) < deadZone)
            {
                _targetSpeed = 0f;
                return;
            }

            normalizedValue = Mathf.Clamp(normalizedValue, -1f, 1f);
            _targetSpeed = normalizedValue * maxSpeed;
        }

        private void ApplyPosition()
        {
            Vector3 position = Vector3.Lerp(startPosition, endPosition, _currentT);

            if (useLocalPosition)
                transform.localPosition = position;
            else
                transform.position = position;
        }

        private float CalculateCurrentT()
        {
            Vector3 currentPos = useLocalPosition ? transform.localPosition : transform.position;
            Vector3 toEnd = endPosition - startPosition;

            if (toEnd.sqrMagnitude < 0.0001f) return 0f;

            Vector3 toCurrent = currentPos - startPosition;
            return Mathf.Clamp01(Vector3.Dot(toCurrent, toEnd.normalized) / toEnd.magnitude);
        }

        #region Public API

        /// <summary>Gets the current movement speed in units per second.</summary>
        public float CurrentSpeed => _currentSpeed;

        /// <summary>Gets the current position as a value between 0 (start) and 1 (end).</summary>
        public float CurrentT => _currentT;

        /// <summary>Gets the current world/local position.</summary>
        public Vector3 CurrentPosition => Vector3.Lerp(startPosition, endPosition, _currentT);

        /// <summary>Immediately sets the position to a specific T value (0-1).</summary>
        public void SetPositionImmediate(float t)
        {
            _currentT = clampToEndpoints ? Mathf.Clamp01(t) : Mathf.Repeat(t, 1f);
            ApplyPosition();
        }

        /// <summary>Moves to start position immediately.</summary>
        public void GoToStart() => SetPositionImmediate(0f);

        /// <summary>Moves to end position immediately.</summary>
        public void GoToEnd() => SetPositionImmediate(1f);

        /// <summary>Moves to center position immediately.</summary>
        public void GoToCenter() => SetPositionImmediate(0.5f);

        #endregion

        #region Editor Helpers

        /// <summary>Sets start position to current transform position.</summary>
        [ContextMenu("Set Start Position")]
        public void SetStartPosition()
        {
            startPosition = useLocalPosition ? transform.localPosition : transform.position;
        }

        /// <summary>Sets end position to current transform position.</summary>
        [ContextMenu("Set End Position")]
        public void SetEndPosition()
        {
            endPosition = useLocalPosition ? transform.localPosition : transform.position;
        }

        /// <summary>Preview start position in editor.</summary>
        [ContextMenu("Preview Start")]
        public void PreviewStart()
        {
            if (useLocalPosition) transform.localPosition = startPosition;
            else transform.position = startPosition;
        }

        /// <summary>Preview end position in editor.</summary>
        [ContextMenu("Preview End")]
        public void PreviewEnd()
        {
            if (useLocalPosition) transform.localPosition = endPosition;
            else transform.position = endPosition;
        }

        #endregion
    }
}
