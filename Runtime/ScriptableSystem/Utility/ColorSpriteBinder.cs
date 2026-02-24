using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds a ColorVariable to a SpriteRenderer's color for live updates.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Color Sprite Binder")]
    [RequireComponent(typeof(SpriteRenderer))]
    public class ColorSpriteBinder : VariableBinder<ColorVariable>
    {
        [Tooltip("The ColorVariable to bind to the sprite's color.")]
        [SerializeField] private ColorVariable colorVariable;

        [Header("Transition Settings")]
        [Tooltip("Whether to smoothly interpolate color changes.")]
        [SerializeField] private bool smoothTransition = false;

        [Tooltip("Speed of color interpolation (higher = faster transition).")]
        [SerializeField] private float transitionSpeed = 5f;

        [Header("Alpha Settings")]
        [Tooltip("Whether to also update the alpha channel from the variable.")]
        [SerializeField] private bool includeAlpha = true;

        private SpriteRenderer _spriteRenderer;
        private Color _targetColor;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        protected override ColorVariable Variable => colorVariable;

        protected override void Bind()
        {
            _targetColor = colorVariable.Value;
            ApplyColor(_targetColor);
        }

        protected override void OnVariableChanged()
        {
            _targetColor = colorVariable.Value;
            if (!smoothTransition) ApplyColor(_targetColor);
        }

        private void Update()
        {
            if (!smoothTransition) return;

            Color currentColor = _spriteRenderer.color;
            Color newColor = Color.Lerp(currentColor, _targetColor, transitionSpeed * Time.deltaTime);

            if (!includeAlpha)
            {
                newColor.a = currentColor.a;
            }

            _spriteRenderer.color = newColor;
        }

        private void ApplyColor(Color color)
        {
            if (!includeAlpha)
            {
                color.a = _spriteRenderer.color.a;
            }
            _spriteRenderer.color = color;
        }

        /// <summary>Sets the color immediately without interpolation.</summary>
        public void SetColorImmediate(Color color)
        {
            _targetColor = color;
            ApplyColor(color);
        }

        /// <summary>Gets the current actual color of the sprite.</summary>
        public Color CurrentColor => _spriteRenderer != null ? _spriteRenderer.color : Color.white;

        /// <summary>Gets the target color being interpolated towards.</summary>
        public Color TargetColor => _targetColor;
    }
}
