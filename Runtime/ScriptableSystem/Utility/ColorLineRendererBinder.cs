using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds a ColorVariable to a LineRenderer's start and/or end color.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Binders/Color LineRenderer Binder")]
    [RequireComponent(typeof(LineRenderer))]
    public class ColorLineRendererBinder : VariableBinder<ColorVariable>
    {
        [SerializeField] private ColorVariable colorVariable;

        [Header("Target")]
        [SerializeField] private bool setStartColor = true;
        [SerializeField] private bool setEndColor = true;

        [Header("Interpolation")]
        [SerializeField] private bool smooth = false;
        [SerializeField] private float speed = 5f;

        private LineRenderer _lineRenderer;
        private Color _targetColor;

        protected override ColorVariable Variable => colorVariable;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        protected override void Bind()
        {
            _targetColor = colorVariable.Value;
            ApplyColor(_targetColor);
        }

        protected override void OnVariableChanged()
        {
            _targetColor = colorVariable.Value;
            if (!smooth) ApplyColor(_targetColor);
        }

        private void Update()
        {
            if (!smooth) return;

            Color current = _lineRenderer.startColor;
            Color next = Color.Lerp(current, _targetColor, speed * Time.deltaTime);
            ApplyColor(next);
        }

        private void ApplyColor(Color color)
        {
            if (setStartColor) _lineRenderer.startColor = color;
            if (setEndColor) _lineRenderer.endColor = color;
        }
    }
}
