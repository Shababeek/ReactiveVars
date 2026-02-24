using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Moves object between two positions based on a numerical variable (0-1 or custom range).
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Numerical Position Binder")]
    public class NumericalPositionBinder : NumericalVariableBinder
    {
        [SerializeField] private ScriptableVariable variable;

        [Header("Positions")]
        [SerializeField] private Vector3 startPosition;
        [SerializeField] private Vector3 endPosition;
        [SerializeField] private bool useLocalPosition = true;

        [Header("Value Range")]
        [SerializeField] private float minValue = 0f;
        [SerializeField] private float maxValue = 1f;

        [Header("Interpolation")]
        [SerializeField] private bool smooth;
        [SerializeField] private float speed = 5f;
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
