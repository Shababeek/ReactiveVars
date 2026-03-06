using UniRx;
using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Unified binder for transform properties: position, rotation, and scale.
    /// </summary>
    [AddComponentMenu("Shababeek/ReactiveVars/Binders/Transform Binder")]
    public class TransformBinder : MonoBehaviour
    {
        [Header("Position")]
        [Tooltip("Enable position binding from the variable.")]
        [SerializeField] private bool bindPosition;

        [Tooltip("The Vector3Variable to bind to the position.")]
        [SerializeField] private Vector3Variable positionVariable;

        [Tooltip("If true, use localPosition; if false, use world position.")]
        [SerializeField] private bool useLocalPosition;

        [Tooltip("Offset added to the position variable value.")]
        [SerializeField] private Vector3 positionOffset;

        [Header("Rotation")]
        [Tooltip("Enable rotation binding from the variable.")]
        [SerializeField] private bool bindRotation;

        [Tooltip("Mode for rotation binding (Euler, Quaternion, or Direction2D).")]
        [SerializeField] private RotationMode rotationMode = RotationMode.Euler;

        [Tooltip("The Vector3Variable for Euler angle rotation.")]
        [SerializeField] private Vector3Variable eulerVariable;

        [Tooltip("The QuaternionVariable for quaternion rotation.")]
        [SerializeField] private QuaternionVariable quaternionVariable;

        [Tooltip("The Vector2Variable for 2D direction-based rotation.")]
        [SerializeField] private Vector2Variable directionVariable;

        [Tooltip("The plane for 2D direction rotation (XY, XZ, or YZ).")]
        [SerializeField] private RotationPlane directionPlane = RotationPlane.XY;

        [Tooltip("Angle offset applied to direction-based rotation (degrees).")]
        [SerializeField] private float angleOffset;

        [Tooltip("If true, use localRotation; if false, use world rotation.")]
        [SerializeField] private bool useLocalRotation = true;

        [Header("Scale")]
        [Tooltip("Enable scale binding from the variable.")]
        [SerializeField] private bool bindScale;

        [Tooltip("Scale mode (Vector3 for per-axis, Uniform for single multiplier).")]
        [SerializeField] private ScaleMode scaleMode = ScaleMode.Uniform;

        [Tooltip("The Vector3Variable for per-axis scale.")]
        [SerializeField] private Vector3Variable scaleVector;

        [Tooltip("The ScriptableVariable for uniform scale multiplier.")]
        [SerializeField] private ScriptableVariable uniformScale;

        [Tooltip("Base scale applied when using uniform scale mode.")]
        [SerializeField] private Vector3 baseScale = Vector3.one;

        [Tooltip("Minimum allowed scale value.")]
        [SerializeField] private float minScale = 0.001f;

        [Header("Interpolation")]
        [Tooltip("Enable smooth interpolation of position, rotation, and scale.")]
        [SerializeField] private bool smooth;

        [Tooltip("Interpolation speed (higher = faster).")]
        [SerializeField] private float speed = 5f;

        private CompositeDisposable _disposable;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private Vector3 _targetScale;
        private INumericalVariable _uniformNumVar;

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            if (bindPosition) SetupPosition();
            if (bindRotation) SetupRotation();
            if (bindScale) SetupScale();
        }

        private void OnDisable() => _disposable?.Dispose();

        private void Update()
        {
            if (!smooth) return;

            if (bindPosition)
            {
                var pos = useLocalPosition ? transform.localPosition : transform.position;
                pos = Vector3.Lerp(pos, _targetPosition, speed * Time.deltaTime);
                if (useLocalPosition) transform.localPosition = pos;
                else transform.position = pos;
            }

            if (bindRotation)
            {
                var rot = useLocalRotation ? transform.localRotation : transform.rotation;
                rot = Quaternion.Slerp(rot, _targetRotation, speed * Time.deltaTime);
                if (useLocalRotation) transform.localRotation = rot;
                else transform.rotation = rot;
            }

            if (bindScale)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, speed * Time.deltaTime);
            }
        }

        #region Position

        private void SetupPosition()
        {
            if (positionVariable == null) return;
            UpdatePosition(positionVariable.Value);
            positionVariable.OnValueChanged.Subscribe(UpdatePosition).AddTo(_disposable);
        }

        private void UpdatePosition(Vector3 pos)
        {
            _targetPosition = pos + positionOffset;
            if (!smooth) ApplyPosition();
        }

        private void ApplyPosition()
        {
            if (useLocalPosition) transform.localPosition = _targetPosition;
            else transform.position = _targetPosition;
        }

        #endregion

        #region Rotation

        private void SetupRotation()
        {
            switch (rotationMode)
            {
                case RotationMode.Euler when eulerVariable != null:
                    UpdateEuler(eulerVariable.Value);
                    eulerVariable.OnValueChanged.Subscribe(UpdateEuler).AddTo(_disposable);
                    break;
                case RotationMode.Quaternion when quaternionVariable != null:
                    UpdateQuaternion(quaternionVariable.Value);
                    quaternionVariable.OnValueChanged.Subscribe(UpdateQuaternion).AddTo(_disposable);
                    break;
                case RotationMode.Direction2D when directionVariable != null:
                    UpdateDirection(directionVariable.Value);
                    directionVariable.OnValueChanged.Subscribe(UpdateDirection).AddTo(_disposable);
                    break;
            }
        }

        private void UpdateEuler(Vector3 euler)
        {
            _targetRotation = Quaternion.Euler(euler);
            if (!smooth) ApplyRotation();
        }

        private void UpdateQuaternion(Quaternion quat)
        {
            _targetRotation = quat;
            if (!smooth) ApplyRotation();
        }

        private void UpdateDirection(Vector2 dir)
        {
            if (dir.sqrMagnitude < 0.0001f) return;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + angleOffset;

            Vector3 euler = useLocalRotation ? transform.localEulerAngles : transform.eulerAngles;
            switch (directionPlane)
            {
                case RotationPlane.XY: euler.z = angle; break;
                case RotationPlane.XZ: euler.y = angle; break;
                case RotationPlane.YZ: euler.x = angle; break;
            }
            _targetRotation = Quaternion.Euler(euler);
            if (!smooth) ApplyRotation();
        }

        private void ApplyRotation()
        {
            if (useLocalRotation) transform.localRotation = _targetRotation;
            else transform.rotation = _targetRotation;
        }

        #endregion

        #region Scale

        private void SetupScale()
        {
            if (scaleMode == ScaleMode.Vector3 && scaleVector != null)
            {
                UpdateScale(scaleVector.Value);
                scaleVector.OnValueChanged.Subscribe(UpdateScale).AddTo(_disposable);
            }
            else if (scaleMode == ScaleMode.Uniform && uniformScale != null)
            {
                _uniformNumVar = uniformScale as INumericalVariable;
                if (_uniformNumVar == null) return;
                UpdateUniformScale(_uniformNumVar.AsFloat);
                uniformScale.OnRaised.Subscribe(_ => UpdateUniformScale(_uniformNumVar.AsFloat)).AddTo(_disposable);
            }
        }

        private void UpdateScale(Vector3 scale)
        {
            _targetScale = new Vector3(
                Mathf.Max(scale.x, minScale),
                Mathf.Max(scale.y, minScale),
                Mathf.Max(scale.z, minScale));
            if (!smooth) transform.localScale = _targetScale;
        }

        private void UpdateUniformScale(float value)
        {
            var scale = baseScale * Mathf.Max(value, minScale);
            _targetScale = scale;
            if (!smooth) transform.localScale = _targetScale;
        }

        #endregion

        public enum RotationMode { Euler, Quaternion, Direction2D }
        public enum RotationPlane { XY, XZ, YZ }
        public enum ScaleMode { Vector3, Uniform }
    }
}
