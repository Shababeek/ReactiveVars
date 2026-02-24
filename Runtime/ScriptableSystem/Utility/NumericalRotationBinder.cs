using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds any numeric variable (IntVariable or FloatVariable) to an object's rotation.
    /// </summary>
    /// <remarks>
    /// This binder works with any variable that implements INumericalVariable, making it
    /// more flexible than the IntVariable-specific IntVariableRotationBinder. It maps a
    /// numeric value range to a rotation angle range.
    ///
    /// Common use cases include:
    /// - Gauge needles (speedometers, fuel gauges)
    /// - Dial indicators
    /// - Analog clock hands
    /// - Compass needles
    /// </remarks>
    /// <example>
    /// <code>
    /// // Speedometer: FloatVariable speed (0-200) maps to rotation (0-270 degrees)
    /// // Clock hand: IntVariable hours (0-12) maps to rotation (0-360 degrees)
    /// </code>
    /// </example>
    [AddComponentMenu("Shababeek/Scriptable System/Numerical Rotation Binder")]
    public class NumericalRotationBinder : NumericalVariableBinder
    {
        [Tooltip("The numeric variable to bind (IntVariable or FloatVariable).")]
        [SerializeField] private ScriptableVariable variable;

        [Header("Value Mapping")]
        [Tooltip("The minimum value from the variable that maps to minAngle.")]
        [SerializeField] private float minValue = 0f;

        [Tooltip("The maximum value from the variable that maps to maxAngle.")]
        [SerializeField] private float maxValue = 100f;

        [Header("Rotation Settings")]
        [Tooltip("The axis to rotate around.")]
        [SerializeField] private RotationAxis rotationAxis = RotationAxis.Z;

        [Tooltip("The rotation angle in degrees when value equals minValue.")]
        [SerializeField] private float minAngle = 0f;

        [Tooltip("The rotation angle in degrees when value equals maxAngle.")]
        [SerializeField] private float maxAngle = 360f;

        [Tooltip("Whether to use local rotation instead of world rotation.")]
        [SerializeField] private bool useLocalRotation = true;

        [Header("Interpolation")]
        [Tooltip("Whether to smoothly interpolate rotation changes.")]
        [SerializeField] private bool smoothRotation = false;

        [Tooltip("Rotation speed for smooth interpolation (degrees per second).")]
        [SerializeField] private float rotationSpeed = 90f;

        [Tooltip("Use shortest path for angle interpolation (recommended for dials).")]
        [SerializeField] private bool useShortestPath = true;

        private float _targetAngle;
        private float _currentAngle;

        protected override ScriptableVariable Variable => variable;

        protected override void BindNumerical()
        {
            _currentAngle = GetCurrentRotation();
            UpdateRotation(NumericalVariable.AsFloat);
        }

        protected override void OnNumericalValueChanged()
        {
            UpdateRotation(NumericalVariable.AsFloat);
        }

        private void Update()
        {
            if (!smoothRotation) return;

            if (useShortestPath)
            {
                _currentAngle = Mathf.MoveTowardsAngle(_currentAngle, _targetAngle, rotationSpeed * Time.deltaTime);
            }
            else
            {
                _currentAngle = Mathf.MoveTowards(_currentAngle, _targetAngle, rotationSpeed * Time.deltaTime);
            }

            ApplyRotation(_currentAngle);
        }

        private void UpdateRotation(float value)
        {
            float clampedValue = Mathf.Clamp(value, minValue, maxValue);

            float t = Mathf.Approximately(maxValue, minValue)
                ? 0f
                : (clampedValue - minValue) / (maxValue - minValue);

            _targetAngle = Mathf.Lerp(minAngle, maxAngle, t);

            if (!smoothRotation)
            {
                ApplyRotation(_targetAngle);
                _currentAngle = _targetAngle;
            }
        }

        private void ApplyRotation(float angle)
        {
            Vector3 eulerAngles = GetCurrentRotationEuler();

            switch (rotationAxis)
            {
                case RotationAxis.X:
                    eulerAngles.x = angle;
                    break;
                case RotationAxis.Y:
                    eulerAngles.y = angle;
                    break;
                case RotationAxis.Z:
                    eulerAngles.z = angle;
                    break;
            }

            if (useLocalRotation)
            {
                transform.localRotation = Quaternion.Euler(eulerAngles);
            }
            else
            {
                transform.rotation = Quaternion.Euler(eulerAngles);
            }
        }

        private float GetCurrentRotation()
        {
            Vector3 eulerAngles = GetCurrentRotationEuler();

            return rotationAxis switch
            {
                RotationAxis.X => eulerAngles.x,
                RotationAxis.Y => eulerAngles.y,
                RotationAxis.Z => eulerAngles.z,
                _ => 0f
            };
        }

        private Vector3 GetCurrentRotationEuler()
        {
            return useLocalRotation ? transform.localEulerAngles : transform.eulerAngles;
        }

        /// <summary>Sets the rotation immediately without interpolation.</summary>
        public void SetRotationImmediate(float angle)
        {
            _targetAngle = angle;
            _currentAngle = angle;
            ApplyRotation(angle);
        }

        /// <summary>Gets the current rotation angle.</summary>
        public float CurrentAngle => _currentAngle;

        /// <summary>Gets the target rotation angle.</summary>
        public float TargetAngle => _targetAngle;

        /// <summary>Recalculates the rotation based on the current variable value.</summary>
        public void Refresh()
        {
            if (NumericalVariable != null)
            {
                UpdateRotation(NumericalVariable.AsFloat);
            }
        }

        /// <summary>Defines which axis to rotate around.</summary>
        public enum RotationAxis
        {
            X,
            Y,
            Z
        }
    }
}
