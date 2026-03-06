using UniRx;
using UnityEngine;

namespace Shababeek.ReactiveVars
{
    public enum AngularInputMode
    {
        Vector3,
        Vector2XY,
        Vector2XZ,
        FloatSingleAxis
    }

    /// <summary>
    /// Binds variables to Rigidbody angular velocity or applies as torque.
    /// </summary>
    [AddComponentMenu("Shababeek/ReactiveVars/Binders/Angular Velocity Binder")]
    public class AngularVelocityBinder : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("The 3D Rigidbody to apply angular velocity to.")]
        [SerializeField] private Rigidbody rb3D;

        [Tooltip("The 2D Rigidbody to apply angular velocity to.")]
        [SerializeField] private Rigidbody2D rb2D;

        [Header("Input Mode")]
        [Tooltip("Input mode for angular velocity (Vector3, Vector2XY, Vector2XZ, FloatSingleAxis).")]
        [SerializeField] private AngularInputMode inputMode = AngularInputMode.Vector3;

        [Header("Vector3 Input")]
        [Tooltip("The Vector3Variable for angular velocity input.")]
        [SerializeField] private Vector3Variable vector3Input;

        [Header("Vector2 Input")]
        [Tooltip("The Vector2Variable for angular velocity input.")]
        [SerializeField] private Vector2Variable vector2Input;

        [Header("Float Input")]
        [Tooltip("The FloatVariable for single-axis angular velocity.")]
        [SerializeField] private FloatVariable floatInput;

        [Tooltip("The axis to rotate around in FloatSingleAxis mode.")]
        [SerializeField] private Vector3 rotationAxis = Vector3.up;

        [Header("Application")]
        [Tooltip("If true, set angular velocity; if false, apply as torque.")]
        [SerializeField] private bool setVelocity = true;

        [Tooltip("If true, apply in local space; if false, use world space.")]
        [SerializeField] private bool useLocalSpace = false;

        [Tooltip("Multiplier for the angular velocity value.")]
        [SerializeField] private float multiplier = 1f;

        [Tooltip("If true, continuously apply angular velocity; if false, apply only once per change.")]
        [SerializeField] private bool continuous = true;

        private CompositeDisposable _disposable;
        private Vector3 _currentAngularVelocity;

        private void OnEnable()
        {
            if (rb3D == null) rb3D = GetComponent<Rigidbody>();
            if (rb2D == null) rb2D = GetComponent<Rigidbody2D>();
            _disposable = new CompositeDisposable();
            SubscribeToInputs();
        }

        private void OnDisable() => _disposable?.Dispose();

        private void SubscribeToInputs()
        {
            switch (inputMode)
            {
                case AngularInputMode.Vector3:
                    if (vector3Input != null)
                    {
                        vector3Input.OnValueChanged
                            .Subscribe(v => _currentAngularVelocity = v)
                            .AddTo(_disposable);
                        _currentAngularVelocity = vector3Input.Value;
                    }
                    break;

                case AngularInputMode.Vector2XY:
                    if (vector2Input != null)
                    {
                        vector2Input.OnValueChanged
                            .Subscribe(v => _currentAngularVelocity = new Vector3(v.x, v.y, 0))
                            .AddTo(_disposable);
                        var val = vector2Input.Value;
                        _currentAngularVelocity = new Vector3(val.x, val.y, 0);
                    }
                    break;

                case AngularInputMode.Vector2XZ:
                    if (vector2Input != null)
                    {
                        vector2Input.OnValueChanged
                            .Subscribe(v => _currentAngularVelocity = new Vector3(v.x, 0, v.y))
                            .AddTo(_disposable);
                        var v2 = vector2Input.Value;
                        _currentAngularVelocity = new Vector3(v2.x, 0, v2.y);
                    }
                    break;

                case AngularInputMode.FloatSingleAxis:
                    if (floatInput != null)
                    {
                        floatInput.OnValueChanged
                            .Subscribe(f => _currentAngularVelocity = rotationAxis.normalized * f)
                            .AddTo(_disposable);
                        _currentAngularVelocity = rotationAxis.normalized * floatInput.Value;
                    }
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (!continuous) return;
            Apply();
        }

        public void Apply()
        {
            Vector3 angularVel = _currentAngularVelocity * multiplier;
            if (useLocalSpace && rb3D != null)
            {
                angularVel = transform.TransformDirection(angularVel);
            }

            if (rb3D != null)
            {
                if (setVelocity)
                    rb3D.angularVelocity = angularVel;
                else
                    rb3D.AddTorque(angularVel, ForceMode.Force);
            }
            else if (rb2D != null)
            {
                // 2D only uses Z axis rotation
                float z = inputMode == AngularInputMode.FloatSingleAxis
                    ? _currentAngularVelocity.magnitude * Mathf.Sign(rotationAxis.z)
                    : _currentAngularVelocity.z;

                if (setVelocity)
                    rb2D.angularVelocity = z * multiplier;
                else
                    rb2D.AddTorque(z * multiplier, ForceMode2D.Force);
            }
        }
    }
}
