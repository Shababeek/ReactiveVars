using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds a QuaternionVariable to a transform's rotation.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Quaternion Rotation Binder")]
    public class QuaternionRotationBinder : VariableBinder<QuaternionVariable>
    {
        [SerializeField] private QuaternionVariable variable;

        [Header("Target")]
        [Tooltip("The transform to rotate. Uses this object's transform if not set.")]
        [SerializeField] private Transform target;

        [SerializeField] private bool useLocalRotation = false;

        [Header("Interpolation")]
        [SerializeField] private bool smooth = false;
        [SerializeField] private float speed = 10f;

        private Quaternion _targetRotation;

        protected override QuaternionVariable Variable => variable;

        protected override void Bind()
        {
            if (target == null) target = transform;
            _targetRotation = variable.Value;
            ApplyRotation(_targetRotation);
        }

        protected override void OnVariableChanged()
        {
            _targetRotation = variable.Value;
            if (!smooth) ApplyRotation(_targetRotation);
        }

        private void Update()
        {
            if (!smooth) return;

            Quaternion current = useLocalRotation ? target.localRotation : target.rotation;
            Quaternion next = Quaternion.Slerp(current, _targetRotation, speed * Time.deltaTime);
            ApplyRotation(next);
        }

        private void ApplyRotation(Quaternion rotation)
        {
            if (useLocalRotation)
                target.localRotation = rotation;
            else
                target.rotation = rotation;
        }

        /// <summary>Snaps to the current target rotation immediately.</summary>
        public void SnapToTarget()
        {
            ApplyRotation(_targetRotation);
        }
    }
}
