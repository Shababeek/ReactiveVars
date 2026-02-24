using UnityEngine;
using UnityEngine.UI;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Samples a color from a gradient based on a numeric variable and applies it to a target.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Gradient Sampler Binder")]
    public class GradientSamplerBinder : NumericalVariableBinder
    {
        [SerializeField] private ScriptableVariable variable;

        [Header("Gradient")]
        [Tooltip("The gradient to sample from. Can be set inline or use a GradientVariable.")]
        [SerializeField] private Gradient gradient = new Gradient();

        [Tooltip("Optional GradientVariable (overrides inline gradient if set).")]
        [SerializeField] private GradientVariable gradientVariable;

        [Header("Value Mapping")]
        [Tooltip("The variable value that maps to gradient time 0.")]
        [SerializeField] private float minValue = 0f;

        [Tooltip("The variable value that maps to gradient time 1.")]
        [SerializeField] private float maxValue = 100f;

        [Header("Target")]
        [SerializeField] private ColorTarget colorTarget = ColorTarget.Image;
        [SerializeField] private Image targetImage;
        [SerializeField] private SpriteRenderer targetSprite;
        [SerializeField] private Renderer targetRenderer;

        [Header("Interpolation")]
        [SerializeField] private bool smooth = false;
        [SerializeField] private float smoothSpeed = 5f;

        private Color _targetColor;
        private Color _currentColor;

        protected override ScriptableVariable Variable => variable;

        protected override void BindNumerical()
        {
            AutoDetectTarget();
            float t = GetNormalizedValue(NumericalVariable.AsFloat);
            _targetColor = SampleGradient(t);
            _currentColor = _targetColor;
            ApplyColor(_currentColor);
        }

        protected override void OnNumericalValueChanged()
        {
            float t = GetNormalizedValue(NumericalVariable.AsFloat);
            _targetColor = SampleGradient(t);
            if (!smooth) ApplyColor(_targetColor);
        }

        private void Update()
        {
            if (!smooth) return;

            _currentColor = Color.Lerp(_currentColor, _targetColor, smoothSpeed * Time.deltaTime);
            ApplyColor(_currentColor);
        }

        private float GetNormalizedValue(float value)
        {
            if (Mathf.Approximately(maxValue, minValue)) return 0f;
            return Mathf.Clamp01((value - minValue) / (maxValue - minValue));
        }

        private Color SampleGradient(float t)
        {
            Gradient activeGradient = gradientVariable?.Value ?? gradient;

            return activeGradient.Evaluate(t);
        }

        private void ApplyColor(Color color)
        {
            switch (colorTarget)
            {
                case ColorTarget.Image:
                    if (targetImage != null) targetImage.color = color;
                    break;
                case ColorTarget.SpriteRenderer:
                    if (targetSprite != null) targetSprite.color = color;
                    break;
                case ColorTarget.Material:
                    if (targetRenderer != null) targetRenderer.material.color = color;
                    break;
            }
        }

        private void AutoDetectTarget()
        {
            switch (colorTarget)
            {
                case ColorTarget.Image:
                    if (targetImage == null) targetImage = GetComponent<Image>();
                    break;
                case ColorTarget.SpriteRenderer:
                    if (targetSprite == null) targetSprite = GetComponent<SpriteRenderer>();
                    break;
                case ColorTarget.Material:
                    if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
                    break;
            }
        }

        /// <summary>Forces a refresh from the current variable value.</summary>
        public void Refresh()
        {
            if (NumericalVariable != null)
            {
                float t = GetNormalizedValue(NumericalVariable.AsFloat);
                _targetColor = SampleGradient(t);
                _currentColor = _targetColor;
                ApplyColor(_currentColor);
            }
        }

        /// <summary>Defines which component receives the sampled color.</summary>
        public enum ColorTarget
        {
            Image,
            SpriteRenderer,
            Material
        }
    }
}
