using UniRx;
using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds numeric variables to Rigidbody physics properties (mass, drag, angular drag, gravity).
    /// </summary>
    [AddComponentMenu("Shababeek/ReactiveVars/Binders/Rigidbody Property Binder")]
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyPropertyBinder : MonoBehaviour
    {
        [Header("Mass")]
        [Tooltip("Float variable to control the rigidbody's mass.")]
        [SerializeField] private FloatVariable massVariable;

        [Header("Drag")]
        [Tooltip("Float variable to control linear drag.")]
        [SerializeField] private FloatVariable dragVariable;

        [Tooltip("Float variable to control angular drag.")]
        [SerializeField] private FloatVariable angularDragVariable;

        [Header("Gravity")]
        [Tooltip("Bool variable to control whether gravity affects this rigidbody.")]
        [SerializeField] private BoolVariable useGravityVariable;

        [Header("Kinematic")]
        [Tooltip("Bool variable to control whether the rigidbody is kinematic.")]
        [SerializeField] private BoolVariable isKinematicVariable;

        [Header("Constraints")]
        [Tooltip("Bool variable to freeze all position axes.")]
        [SerializeField] private BoolVariable freezePositionVariable;

        [Tooltip("Bool variable to freeze all rotation axes.")]
        [SerializeField] private BoolVariable freezeRotationVariable;

        [Header("Interpolation")]
        [Tooltip("Bool variable to enable/disable interpolation.")]
        [SerializeField] private BoolVariable interpolateVariable;

        private Rigidbody _rb;
        private CompositeDisposable _disposable;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            if (massVariable != null)
            {
                UpdateMass(massVariable.Value);
                massVariable.OnValueChanged
                    .Subscribe(UpdateMass)
                    .AddTo(_disposable);
            }

            if (dragVariable != null)
            {
                UpdateDrag(dragVariable.Value);
                dragVariable.OnValueChanged
                    .Subscribe(UpdateDrag)
                    .AddTo(_disposable);
            }

            if (angularDragVariable != null)
            {
                UpdateAngularDrag(angularDragVariable.Value);
                angularDragVariable.OnValueChanged
                    .Subscribe(UpdateAngularDrag)
                    .AddTo(_disposable);
            }

            if (useGravityVariable != null)
            {
                UpdateGravity(useGravityVariable.Value);
                useGravityVariable.OnValueChanged
                    .Subscribe(UpdateGravity)
                    .AddTo(_disposable);
            }

            if (isKinematicVariable != null)
            {
                UpdateKinematic(isKinematicVariable.Value);
                isKinematicVariable.OnValueChanged
                    .Subscribe(UpdateKinematic)
                    .AddTo(_disposable);
            }

            if (freezePositionVariable != null)
            {
                UpdateFreezePosition(freezePositionVariable.Value);
                freezePositionVariable.OnValueChanged
                    .Subscribe(UpdateFreezePosition)
                    .AddTo(_disposable);
            }

            if (freezeRotationVariable != null)
            {
                UpdateFreezeRotation(freezeRotationVariable.Value);
                freezeRotationVariable.OnValueChanged
                    .Subscribe(UpdateFreezeRotation)
                    .AddTo(_disposable);
            }

            if (interpolateVariable != null)
            {
                UpdateInterpolation(interpolateVariable.Value);
                interpolateVariable.OnValueChanged
                    .Subscribe(UpdateInterpolation)
                    .AddTo(_disposable);
            }
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }

        private void UpdateMass(float value)
        {
            _rb.mass = Mathf.Max(0.0001f, value);
        }

        private void UpdateDrag(float value)
        {
            _rb.linearDamping = Mathf.Max(0f, value);
        }

        private void UpdateAngularDrag(float value)
        {
            _rb.angularDamping = Mathf.Max(0f, value);
        }

        private void UpdateGravity(bool value)
        {
            _rb.useGravity = value;
        }

        private void UpdateKinematic(bool value)
        {
            _rb.isKinematic = value;
        }

        private void UpdateFreezePosition(bool freeze)
        {
            if (freeze)
                _rb.constraints |= RigidbodyConstraints.FreezePosition;
            else
                _rb.constraints &= ~RigidbodyConstraints.FreezePosition;
        }

        private void UpdateFreezeRotation(bool freeze)
        {
            if (freeze)
                _rb.constraints |= RigidbodyConstraints.FreezeRotation;
            else
                _rb.constraints &= ~RigidbodyConstraints.FreezeRotation;
        }

        private void UpdateInterpolation(bool value)
        {
            _rb.interpolation = value ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;
        }
    }
}
