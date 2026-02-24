using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds a Vector3Variable directly to a transform's position.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Vector3 Position Binder")]
    public class Vector3PositionBinder : VariableBinder<Vector3Variable>
    {
        [SerializeField] private Vector3Variable variable;

        [Header("Target")]
        [Tooltip("The transform to move. Uses this object's transform if not set.")]
        [SerializeField] private Transform target;

        [SerializeField] private bool useLocalPosition = false;

        [Header("Interpolation")]
        [SerializeField] private bool smooth = false;
        [SerializeField] private float speed = 10f;
        [SerializeField] private InterpolationMode interpolationMode = InterpolationMode.Lerp;

        private Vector3 _targetPosition;

        protected override Vector3Variable Variable => variable;

        protected override void Bind()
        {
            if (target == null) target = transform;
            _targetPosition = variable.Value;
            ApplyPosition(_targetPosition);
        }

        protected override void OnVariableChanged()
        {
            _targetPosition = variable.Value;
            if (!smooth) ApplyPosition(_targetPosition);
        }

        private void Update()
        {
            if (!smooth) return;

            Vector3 current = GetCurrentPosition();

            Vector3 next = interpolationMode switch
            {
                InterpolationMode.Lerp => Vector3.Lerp(current, _targetPosition, speed * Time.deltaTime),
                InterpolationMode.MoveTowards => Vector3.MoveTowards(current, _targetPosition, speed * Time.deltaTime),
                _ => _targetPosition
            };

            ApplyPosition(next);
        }

        private void ApplyPosition(Vector3 position)
        {
            if (useLocalPosition)
                target.localPosition = position;
            else
                target.position = position;
        }

        private Vector3 GetCurrentPosition()
        {
            return useLocalPosition ? target.localPosition : target.position;
        }

        /// <summary>Snaps to the current target position immediately.</summary>
        public void SnapToTarget()
        {
            ApplyPosition(_targetPosition);
        }

        /// <summary>Defines how position changes are interpolated.</summary>
        public enum InterpolationMode
        {
            Lerp,
            MoveTowards
        }
    }
}
