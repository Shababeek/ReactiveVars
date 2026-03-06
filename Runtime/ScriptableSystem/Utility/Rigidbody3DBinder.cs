using UniRx;
using UnityEngine;

namespace Shababeek.ReactiveVars
{
    public enum VelocityInputMode
    {
        Vector3,
        Vector2XY,
        Vector2XZ,
        Vector2YZ,
        Vector2PlusFloat,
        FloatDirection
    }

    public enum VelocityApplicationMode
    {
        SetVelocity,
        AddForce,
        AddAcceleration
    }

    /// <summary>
    /// Binds variables to Rigidbody 3D velocity/forces with multiple input modes.
    /// </summary>
    [AddComponentMenu("Shababeek/ReactiveVars/Binders/Rigidbody 3D Binder")]
    public class Rigidbody3DBinder : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("The Rigidbody to apply velocity/forces to.")]
        [SerializeField] private Rigidbody rb;

        [Header("Input Mode")]
        [Tooltip("Input mode for velocity (Vector3, Vector2XY, Vector2XZ, Vector2YZ, Vector2PlusFloat, FloatDirection).")]
        [SerializeField] private VelocityInputMode inputMode = VelocityInputMode.Vector3;

        [Header("Vector3 Input")]
        [Tooltip("The Vector3Variable for velocity input.")]
        [SerializeField] private Vector3Variable vector3Input;

        [Header("Vector2 Input")]
        [Tooltip("The Vector2Variable for velocity input.")]
        [SerializeField] private Vector2Variable vector2Input;

        [Header("Float Input (for Vector2+Float or Direction modes)")]
        [Tooltip("The FloatVariable for magnitude or single axis input.")]
        [SerializeField] private FloatVariable floatInput;

        [Header("Direction (for FloatDirection mode)")]
        [Tooltip("Direction vector for FloatDirection input mode.")]
        [SerializeField] private Vector3 direction = Vector3.forward;

        [Header("Application")]
        [Tooltip("How to apply velocity (SetVelocity, AddForce, or AddAcceleration).")]
        [SerializeField] private VelocityApplicationMode applicationMode = VelocityApplicationMode.SetVelocity;

        [Tooltip("If true, apply in local space; if false, use world space.")]
        [SerializeField] private bool useLocalSpace = false;

        [Tooltip("Multiplier for the velocity value.")]
        [SerializeField] private float multiplier = 1f;

        [Tooltip("If true, continuously apply velocity; if false, apply only once per change.")]
        [SerializeField] private bool continuous = true;

        private CompositeDisposable _disposable;
        private Vector3 _currentVelocity;

        private void OnEnable()
        {
            if (!rb) rb = GetComponent<Rigidbody>();
            if (!rb)
            {
                Debug.LogError($"Rigidbody3DBinder on {gameObject.name}: No Rigidbody found.", this);
                return;
            }
            _disposable = new CompositeDisposable();
            SubscribeToInputs();
        }

        private void OnDisable() => _disposable?.Dispose();

        private void SubscribeToInputs()
        {
            switch (inputMode)
            {
                case VelocityInputMode.Vector3:
                    if (vector3Input != null)
                    {
                        vector3Input.OnValueChanged
                            .Subscribe(v => _currentVelocity = v)
                            .AddTo(_disposable);
                        _currentVelocity = vector3Input.Value;
                    }
                    break;

                case VelocityInputMode.Vector2XY:
                case VelocityInputMode.Vector2XZ:
                case VelocityInputMode.Vector2YZ:
                    if (vector2Input != null)
                    {
                        vector2Input.OnValueChanged
                            .Subscribe(v => UpdateFromVector2(v))
                            .AddTo(_disposable);
                        UpdateFromVector2(vector2Input.Value);
                    }
                    break;

                case VelocityInputMode.Vector2PlusFloat:
                    if (vector2Input != null)
                    {
                        vector2Input.OnValueChanged
                            .Subscribe(_ => UpdateFromVector2PlusFloat())
                            .AddTo(_disposable);
                    }
                    if (floatInput != null)
                    {
                        floatInput.OnValueChanged
                            .Subscribe(_ => UpdateFromVector2PlusFloat())
                            .AddTo(_disposable);
                    }
                    UpdateFromVector2PlusFloat();
                    break;

                case VelocityInputMode.FloatDirection:
                    if (floatInput != null)
                    {
                        floatInput.OnValueChanged
                            .Subscribe(f => _currentVelocity = direction.normalized * f)
                            .AddTo(_disposable);
                        _currentVelocity = direction.normalized * floatInput.Value;
                    }
                    break;
            }
        }

        private void UpdateFromVector2(Vector2 v)
        {
            _currentVelocity = inputMode switch
            {
                VelocityInputMode.Vector2XY => new Vector3(v.x, v.y, 0),
                VelocityInputMode.Vector2XZ => new Vector3(v.x, 0, v.y),
                VelocityInputMode.Vector2YZ => new Vector3(0, v.x, v.y),
                _ => Vector3.zero
            };
        }

        private void UpdateFromVector2PlusFloat()
        {
            Vector2 v2 = vector2Input != null ? vector2Input.Value : Vector2.zero;
            float f = floatInput != null ? floatInput.Value : 0f;
            _currentVelocity = new Vector3(v2.x, f, v2.y); // XZ plane + Y height
        }

        private void FixedUpdate()
        {
            if (!continuous || rb == null) return;
            Apply();
        }

        public void Apply()
        {
            if (rb == null) return;

            Vector3 velocity = _currentVelocity * multiplier;
            if (useLocalSpace)
            {
                velocity = transform.TransformDirection(velocity);
            }

            switch (applicationMode)
            {
                case VelocityApplicationMode.SetVelocity:
                    rb.linearVelocity = velocity;
                    break;
                case VelocityApplicationMode.AddForce:
                    rb.AddForce(velocity, ForceMode.Force);
                    break;
                case VelocityApplicationMode.AddAcceleration:
                    rb.AddForce(velocity, ForceMode.Acceleration);
                    break;
            }
        }

        public void SetVelocity(Vector3 velocity)
        {
            _currentVelocity = velocity;
            if (!continuous) Apply();
        }
    }
}
