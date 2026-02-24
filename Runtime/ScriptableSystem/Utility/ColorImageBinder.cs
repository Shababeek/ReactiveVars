using UnityEngine;
using UnityEngine.UI;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds a ColorVariable to a UI Image's color for live updates.
    /// </summary>
    [AddComponentMenu("Shababeek/ReactiveVars/Binders/Color Image Binder")]
    [RequireComponent(typeof(Image))]
    public class ColorImageBinder : VariableBinder<ColorVariable>
    {
        [Tooltip("The ColorVariable to bind to the image's color.")]
        [SerializeField] private ColorVariable colorVariable;

        [Header("Transition Settings")]
        [Tooltip("Whether to smoothly interpolate color changes.")]
        [SerializeField] private bool smoothTransition = false;

        [Tooltip("Speed of color interpolation (higher = faster transition).")]
        [SerializeField] private float transitionSpeed = 5f;

        [Header("Alpha Settings")]
        [Tooltip("Whether to also update the alpha channel from the variable.")]
        [SerializeField] private bool includeAlpha = true;

        private Image _image;
        private Color _targetColor;

        private void Awake()
        {
            _image = GetComponent<Image>();
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

            Color currentColor = _image.color;
            Color newColor = Color.Lerp(currentColor, _targetColor, transitionSpeed * Time.deltaTime);

            if (!includeAlpha)
            {
                newColor.a = currentColor.a;
            }

            _image.color = newColor;
        }

        private void ApplyColor(Color color)
        {
            if (!includeAlpha)
            {
                color.a = _image.color.a;
            }
            _image.color = color;
        }

        /// <summary>Sets the color immediately without interpolation.</summary>
        public void SetColorImmediate(Color color)
        {
            _targetColor = color;
            ApplyColor(color);
        }

        /// <summary>Gets the current actual color of the image.</summary>
        public Color CurrentColor => _image != null ? _image.color : Color.white;

        /// <summary>Gets the target color being interpolated towards.</summary>
        public Color TargetColor => _targetColor;
    }
}
