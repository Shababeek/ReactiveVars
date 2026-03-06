using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Moves object between two positions based on a numerical variable (0-1 or custom range).
    /// </summary>
    [AddComponentMenu("Shababeek/ReactiveVars/Binders/Numerical Position Binder")]
    public class NumericalPositionBinder : NumericalVariableBinder
    {
        [Tooltip("The numerical ScriptableVariable to drive the position.")]
        [SerializeField] private ScriptableVariable variable;

        [Header("Positions")]
        [Tooltip("Starting position (0-value in the range).")]
        [SerializeField] private Vector3 startPosition;

        [Tooltip("Ending position (max-value in the range).")]
        [SerializeField] private Vector3 endPosition;

        [Tooltip("If true, use localPosition; if false, use world position.")]
        [SerializeField] private bool useLocalPosition = true;

        [Header("Value Range")]
        [Tooltip("Minimum value in the range (maps to start position).")]
        [SerializeField] private float minValue = 0f;

        [Tooltip("Maximum value in the range (maps to end position).")]
        [SerializeField] private float maxValue = 1f;

        [Header("Interpolation")]
        [Tooltip("Enable smooth position interpolation.")]
        [SerializeField] private bool smooth;

        [Tooltip("Speed of interpolation (higher = faster).")]
        [SerializeField] private float speed = 5f;

        [Tooltip("Animation curve for non-linear interpolation.")]
        [SerializeField] private AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

        private Vector3 _targetPosition;

        protected override ScriptableVariable Variable => variable;

        protected override void BindNumerical()
        {
            _targetPosition = useLocalPosition ? transform.localPosition : transform.position;
            UpdatePosition(NumericalVariable.AsFloat);
        }

        protected override void OnNumericalValueChanged()
        {
            UpdatePosition(NumericalVariable.AsFloat);
        }

        private void Update()
        {
            if (!smooth) return;

            var current = useLocalPosition ? transform.localPosition : transform.position;
            var next = Vector3.Lerp(current, _targetPosition, speed * Time.deltaTime);

            if (useLocalPosition) transform.localPosition = next;
            else transform.position = next;
        }

        private void UpdatePosition(float value)
        {
            float t = Mathf.InverseLerp(minValue, maxValue, value);
            t = curve.Evaluate(t);
            _targetPosition = Vector3.Lerp(startPosition, endPosition, t);

            if (!smooth) ApplyPosition();
        }

        private void ApplyPosition()
        {
            if (useLocalPosition) transform.localPosition = _targetPosition;
            else transform.position = _targetPosition;
        }

        /// <summary>Sets start position to current transform position.</summary>
        [ContextMenu("Set Start Position")]
        public void SetStartPosition()
        {
            startPosition = useLocalPosition ? transform.localPosition : transform.position;
        }

        /// <summary>Sets end position to current transform position.</summary>
        [ContextMenu("Set End Position")]
        public void SetEndPosition()
        {
            endPosition = useLocalPosition ? transform.localPosition : transform.position;
        }

        /// <summary>Preview start position in editor.</summary>
        [ContextMenu("Preview Start")]
        public void PreviewStart()
        {
            if (useLocalPosition) transform.localPosition = startPosition;
            else transform.position = startPosition;
        }

        /// <summary>Preview end position in editor.</summary>
        [ContextMenu("Preview End")]
        public void PreviewEnd()
        {
            if (useLocalPosition) transform.localPosition = endPosition;
            else transform.position = endPosition;
        }
    }
}
