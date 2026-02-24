using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds any numeric variable to continuous rotation speed.
    /// </summary>
    [AddComponentMenu("Shababeek/ReactiveVars/Binders/Numerical Rotation Speed Binder")]
    public class NumericalRotationSpeedBinder : NumericalVariableBinder
    {
        [Tooltip("The numeric variable to bind (IntVariable or FloatVariable).")]
        [SerializeField] private ScriptableVariable variable;

        [Header("Value Mapping")]
        [Tooltip("The variable value that maps to maximum speed in the negative direction.")]
        [SerializeField] private float minValue = -1f;

        [Tooltip("The variable value that maps to maximum speed in the positive direction.")]
        [SerializeField] private float maxValue = 1f;

        [Header("Rotation Speed Settings")]
        [Tooltip("The axis to rotate around.")]
        [SerializeField] private RotationAxis rotationAxis = RotationAxis.Z;

        [Tooltip("Maximum rotation speed in degrees per second.")]
        [SerializeField] private float maxRotationSpeed = 180f;

        [Tooltip("Whether to use local rotation instead of world rotation.")]
        [SerializeField] private bool useLocalRotation = true;

        [Header("Angle Limits")]
        [Tooltip("Whether to clamp rotation to angle limits.")]
        [SerializeField] private bool useAngleLimits = false;

        [Tooltip("Minimum angle limit in degrees.")]
        [SerializeField] private float minAngle = -90f;

        [Tooltip("Maximum angle limit in degrees.")]
        [SerializeField] private float maxAngle = 90f;

        [Header("Dead Zone")]
        [Tooltip("Values within this threshold from center will be treated as zero.")]
        [SerializeField] private float deadZone = 0.01f;

        [Header("Debug")]
        [Tooltip("Whether to log rotation debug info.")]
        [SerializeField] private bool enableDebugLogs = false;

        [Tooltip("Avoid Rot Control")]
        [SerializeField] private bool _isPaused = false;

        private Vector3 _trackedEulerAngles;
        private float _currentAngle;
        private float _currentSpeed;

        protected override ScriptableVariable Variable => variable;

        /// <summary>Pauses rotation.</summary>
        public void Pause() => _isPaused = true;

        /// <summary>Resumes rotation.</summary>
        public void Resume() => _isPaused = false;

        /// <summary>Sets the paused state.</summary>
        public void SetPaused(bool paused) => _isPaused = paused;

        protected override void BindNumerical()
        {
            InitializeRotation();
            UpdateSpeed(NumericalVariable.AsFloat);
        }

        protected override void OnNumericalValueChanged()
        {
            UpdateSpeed(NumericalVariable.AsFloat);
        }

        private void Update()
        {
            if (_isPaused || Mathf.Approximately(_currentSpeed, 0f))
                return;

            float deltaAngle = _currentSpeed * Time.deltaTime;
            float newAngle = _currentAngle + deltaAngle;

            if (useAngleLimits)
            {
                if (newAngle >= maxAngle && _currentSpeed > 0f)
                {
                    newAngle = maxAngle;
                    _currentSpeed = 0f;
                }
                else if (newAngle <= minAngle && _currentSpeed < 0f)
                {
                    newAngle = minAngle;
                    _currentSpeed = 0f;
                }
            }

            _currentAngle = newAngle;
            SetAxisAngle(_currentAngle);
            ApplyRotation();

            if (enableDebugLogs)
            {
                Debug.Log($"[ROT] Angle: {_currentAngle:F2}, Speed: {_currentSpeed:F2}");
            }
        }

        private void InitializeRotation()
        {
            Vector3 rawEuler = useLocalRotation
                ? transform.localEulerAngles
                : transform.eulerAngles;

            _trackedEulerAngles = new Vector3(
                NormalizeAngle(rawEuler.x),
                NormalizeAngle(rawEuler.y),
                NormalizeAngle(rawEuler.z)
            );

            _currentAngle = GetAxisAngle();

            if (useAngleLimits)
                _currentAngle = Mathf.Clamp(_currentAngle, minAngle, maxAngle);

            SetAxisAngle(_currentAngle);
            ApplyRotation();
        }

        private void UpdateSpeed(float value)
        {
            float center = (minValue + maxValue) * 0.5f;
            float range = (maxValue - minValue) * 0.5f;

            if (Mathf.Approximately(range, 0f))
            {
                _currentSpeed = 0f;
                return;
            }

            float normalized = (value - center) / range;

            if (Mathf.Abs(normalized) < deadZone)
            {
                _currentSpeed = 0f;
                return;
            }

            normalized = Mathf.Clamp(normalized, -1f, 1f);
            _currentSpeed = normalized * maxRotationSpeed;

            if (useAngleLimits)
            {
                if (_currentAngle >= maxAngle && _currentSpeed > 0f)
                    _currentSpeed = 0f;
                else if (_currentAngle <= minAngle && _currentSpeed < 0f)
                    _currentSpeed = 0f;
            }
        }

        private void ApplyRotation()
        {
            Quaternion rotation = Quaternion.Euler(_trackedEulerAngles);

            if (useLocalRotation)
                transform.localRotation = rotation;
            else
                transform.rotation = rotation;
        }

        private float GetAxisAngle()
        {
            return rotationAxis switch
            {
                RotationAxis.X => _trackedEulerAngles.x,
                RotationAxis.Y => _trackedEulerAngles.y,
                RotationAxis.Z => _trackedEulerAngles.z,
                _ => 0f
            };
        }

        private void SetAxisAngle(float angle)
        {
            switch (rotationAxis)
            {
                case RotationAxis.X:
                    _trackedEulerAngles.x = angle;
                    break;
                case RotationAxis.Y:
                    _trackedEulerAngles.y = angle;
                    break;
                case RotationAxis.Z:
                    _trackedEulerAngles.z = angle;
                    break;
            }
        }

        private float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f) angle -= 360f;
            if (angle < -180f) angle += 360f;
            return angle;
        }

        #region Public API

        /// <summary>Gets the current rotation angle.</summary>
        public float CurrentAngle => _currentAngle;

        /// <summary>Gets the current rotation speed in degrees per second.</summary>
        public float CurrentSpeed => _currentSpeed;

        /// <summary>Sets the angle immediately without speed-based movement.</summary>
        public void SetAngleImmediate(float angle)
        {
            if (useAngleLimits)
                angle = Mathf.Clamp(angle, minAngle, maxAngle);

            _currentAngle = angle;
            SetAxisAngle(angle);
            ApplyRotation();
        }

        /// <summary>Resets rotation to the center of limits or zero.</summary>
        public void ResetRotation()
        {
            SetAngleImmediate(useAngleLimits ? (minAngle + maxAngle) * 0.5f : 0f);
        }

        #endregion

        /// <summary>Defines which axis to rotate around.</summary>
        public enum RotationAxis
        {
            X,
            Y,
            Z
        }
    }
}
