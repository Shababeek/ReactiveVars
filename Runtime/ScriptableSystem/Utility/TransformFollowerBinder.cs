using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Makes this object follow the transform stored in a TransformVariable.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Binders/Transform Follower Binder")]
    public class TransformFollowerBinder : VariableBinder<TransformVariable>
    {
        [SerializeField] private TransformVariable variable;

        [Header("Follow Options")]
        [SerializeField] private bool followPosition = true;
        [SerializeField] private bool followRotation = false;
        [SerializeField] private bool followScale = false;
        [SerializeField] private Vector3 positionOffset = Vector3.zero;

        [Tooltip("Whether the offset is in local space of the target.")]
        [SerializeField] private bool localOffset = true;

        [Header("Interpolation")]
        [SerializeField] private bool smooth = false;
        [SerializeField] private float positionSpeed = 10f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float scaleSpeed = 10f;

        private Transform _target;

        protected override TransformVariable Variable => variable;

        protected override void Bind()
        {
            _target = variable.Value;
            if (_target != null && !smooth)
                SnapToTarget();
        }

        protected override void OnVariableChanged()
        {
            _target = variable.Value;
            if (_target != null && !smooth)
                SnapToTarget();
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            if (followPosition)
            {
                Vector3 targetPos = GetTargetPosition();
                if (smooth)
                    transform.position = Vector3.Lerp(transform.position, targetPos, positionSpeed * Time.deltaTime);
                else
                    transform.position = targetPos;
            }

            if (followRotation)
            {
                if (smooth)
                    transform.rotation = Quaternion.Slerp(transform.rotation, _target.rotation, rotationSpeed * Time.deltaTime);
                else
                    transform.rotation = _target.rotation;
            }

            if (followScale)
            {
                if (smooth)
                    transform.localScale = Vector3.Lerp(transform.localScale, _target.localScale, scaleSpeed * Time.deltaTime);
                else
                    transform.localScale = _target.localScale;
            }
        }

        private Vector3 GetTargetPosition()
        {
            if (positionOffset == Vector3.zero) return _target.position;

            if (localOffset)
                return _target.TransformPoint(positionOffset);
            else
                return _target.position + positionOffset;
        }

        /// <summary>Immediately snaps to the target's transform.</summary>
        public void SnapToTarget()
        {
            if (_target == null) return;

            if (followPosition) transform.position = GetTargetPosition();
            if (followRotation) transform.rotation = _target.rotation;
            if (followScale) transform.localScale = _target.localScale;
        }

        /// <summary>Gets the current follow target.</summary>
        public Transform Target => _target;
    }
}
