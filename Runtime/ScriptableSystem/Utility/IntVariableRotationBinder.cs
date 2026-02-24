using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds an IntVariable to an object's rotation for live updates.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Int Variable Rotation Binder")]
    public class IntVariableRotationBinder : VariableBinder<IntVariable>
    {
        [Tooltip("The IntVariable to bind to the rotation.")]
        [SerializeField] private IntVariable intVariable;

        [Header("Rotation Settings")]
        [Tooltip("The axis to rotate around.")]
        [SerializeField] private RotationAxis rotationAxis = RotationAxis.Z;

        [Tooltip("The minimum rotation angle in degrees.")]
        [SerializeField] private float minAngle = 0f;

        [Tooltip("The maximum rotation angle in degrees.")]
        [SerializeField] private float maxAngle = 360f;

        [Tooltip("The minimum value from the IntVariable that maps to minAngle.")]
        [SerializeField] private int minValue = 0;

        [Tooltip("The maximum value from the IntVariable that maps to maxAngle.")]
        [SerializeField] private int maxValue = 100;

        [Tooltip("Whether to use local rotation instead of world rotation.")]
        [SerializeField] private bool useLocalRotation = true;

        [Tooltip("Whether to smoothly interpolate rotation changes.")]
        [SerializeField] private bool smoothRotation = false;

        [Tooltip("Rotation speed for smooth interpolation (degrees per second).")]
        [SerializeField] private float rotationSpeed = 90f;

        private float _targetAngle;
        private float _currentAngle;

        protected override IntVariable Variable => intVariable;

        protected override void Bind()
        {
            UpdateRotation(intVariable.Value);
        }

        protected override void OnVariableChanged()
        {
            UpdateRotation(intVariable.Value);
        }

        private void Update()
        {
            if (smoothRotation)
            {
                _currentAngle = Mathf.MoveTowardsAngle(_currentAngle, _targetAngle, rotationSpeed * Time.deltaTime);
                ApplyRotation(_currentAngle);
            }
        }

        private void UpdateRotation(int value)
        {
            int clampedValue = Mathf.Clamp(value, minValue, maxValue);

            float t = maxValue != minValue ? (float)(clampedValue - minValue) / (maxValue - minValue) : 0f;
            _targetAngle = Mathf.Lerp(minAngle, maxAngle, t);

            if (!smoothRotation)
            {
                ApplyRotation(_targetAngle);
                _currentAngle = _targetAngle;
            }
            else
            {
                if (_currentAngle == 0f && _targetAngle != 0f)
                {
                    _currentAngle = GetCurrentRotation();
                }
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

        private enum RotationAxis
        {
            X,
            Y,
            Z
        }
    }
}
