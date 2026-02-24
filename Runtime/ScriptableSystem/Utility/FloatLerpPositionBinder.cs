using UnityEngine;

namespace Shababeek.ReactiveVars
{
    public enum FloatLerpMode
    {
        Direct,
        Velocity,
        SmoothDamp
    }

    /// <summary>
    /// Moves an object between two positions based on a float value (0-1).
    /// Supports direct, velocity-based, and smooth interpolation modes.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Float Lerp Position Binder")]
    public class FloatLerpPositionBinder : VariableBinder<FloatVariable>
    {
        [Header("Input")]
        [Tooltip("The FloatVariable to bind (0-1 range).")]
        [SerializeField] private FloatVariable floatInput;

        [Header("Target")]
        [Tooltip("The transform to move. Uses this object's transform if not set.")]
        [SerializeField] private Transform target;

        [Tooltip("Whether to use local space instead of world space.")]
        [SerializeField] private bool useLocalSpace = true;

        [Header("Positions")]
        [Tooltip("The position when value is 0.")]
        [SerializeField] private Vector3 startPosition;

        [Tooltip("The position when value is 1.")]
        [SerializeField] private Vector3 endPosition = Vector3.forward;

        [Header("Movement Mode")]
        [Tooltip("How to interpolate between positions.")]
        [SerializeField] private FloatLerpMode mode = FloatLerpMode.Direct;

        [Tooltip("Movement speed for Velocity mode.")]
        [SerializeField] private float velocitySpeed = 2f;

        [Tooltip("Smooth time for SmoothDamp mode.")]
        [SerializeField] private float smoothTime = 0.1f;

        [Tooltip("Easing curve applied to the position interpolation.")]
        [SerializeField] private AnimationCurve easingCurve = AnimationCurve.Linear(0, 0, 1, 1);

        private float _currentT;
        private float _targetT;
        private float _velocity;

        protected override FloatVariable Variable => floatInput;

        protected override void Bind()
        {
            if (target == null) target = transform;
            _currentT = Mathf.Clamp01(floatInput.Value);
            _targetT = _currentT;

            if (mode == FloatLerpMode.Direct)
            {
                ApplyPosition(_currentT);
            }
        }

        protected override void OnVariableChanged()
        {
            _targetT = Mathf.Clamp01(floatInput.Value);

            if (mode == FloatLerpMode.Direct)
            {
                _currentT = _targetT;
                ApplyPosition(_currentT);
            }
        }

        private void Update()
        {
            if (mode == FloatLerpMode.Direct) return;

            if (mode == FloatLerpMode.Velocity)
            {
                _currentT = Mathf.MoveTowards(_currentT, _targetT, velocitySpeed * Time.deltaTime);
            }
            else if (mode == FloatLerpMode.SmoothDamp)
            {
                _currentT = Mathf.SmoothDamp(_currentT, _targetT, ref _velocity, smoothTime);
            }

            ApplyPosition(_currentT);
        }

        private void ApplyPosition(float t)
        {
            float easedT = easingCurve.Evaluate(t);
            Vector3 position = Vector3.Lerp(startPosition, endPosition, easedT);

            if (useLocalSpace)
                target.localPosition = position;
            else
                target.position = position;
        }

        /// <summary>Sets the start position.</summary>
        public void SetStartPosition(Vector3 pos)
        {
            startPosition = pos;
            ApplyPosition(_currentT);
        }

        /// <summary>Sets the end position.</summary>
        public void SetEndPosition(Vector3 pos)
        {
            endPosition = pos;
            ApplyPosition(_currentT);
        }

        /// <summary>Sets both start and end positions.</summary>
        public void SetPositions(Vector3 start, Vector3 end)
        {
            startPosition = start;
            endPosition = end;
            ApplyPosition(_currentT);
        }

        /// <summary>Sets the target T value directly.</summary>
        public void SetValue(float value)
        {
            _targetT = Mathf.Clamp01(value);
            if (mode == FloatLerpMode.Direct)
            {
                _currentT = _targetT;
                ApplyPosition(_currentT);
            }
        }

        /// <summary>Immediately snaps to the current target position.</summary>
        public void SnapToTarget()
        {
            _currentT = _targetT;
            _velocity = 0f;
            ApplyPosition(_currentT);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 start = startPosition;
            Vector3 end = endPosition;

            if (target != null && useLocalSpace && target.parent != null)
            {
                start = target.parent.TransformPoint(startPosition);
                end = target.parent.TransformPoint(endPosition);
            }

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(start, 0.02f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(end, 0.02f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(start, end);

            if (Application.isPlaying)
            {
                Vector3 current = Vector3.Lerp(start, end, _currentT);
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(current, 0.03f);
            }
        }
    }
}
