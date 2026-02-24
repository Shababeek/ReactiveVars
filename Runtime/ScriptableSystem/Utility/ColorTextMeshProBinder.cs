using TMPro;
using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds a ColorVariable to TextMeshPro text color.
    /// </summary>
    [AddComponentMenu("Shababeek/ReactiveVars/Binders/Color TextMeshPro Binder")]
    public class ColorTextMeshProBinder : VariableBinder<ColorVariable>
    {
        [Tooltip("The ColorVariable to bind.")]
        [SerializeField] private ColorVariable colorVariable;

        [Tooltip("The TMP_Text component to update. Uses this object's component if not set.")]
        [SerializeField] private TMP_Text textComponent;

        [Header("Settings")]
        [Tooltip("Whether to smoothly interpolate color changes.")]
        [SerializeField] private bool smooth;

        [Tooltip("Color interpolation speed.")]
        [SerializeField] private float speed = 5f;

        [Tooltip("Whether to include alpha channel in color changes.")]
        [SerializeField] private bool includeAlpha = true;

        private Color _targetColor;

        protected override ColorVariable Variable => colorVariable;

        protected override void Bind()
        {
            if (textComponent == null) textComponent = GetComponent<TMP_Text>();
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

            var current = textComponent.color;
            var next = Color.Lerp(current, _targetColor, speed * Time.deltaTime);
            if (!includeAlpha) next.a = current.a;
            textComponent.color = next;
        }

        private void ApplyColor(Color color)
        {
            if (!includeAlpha) color.a = textComponent.color.a;
            textComponent.color = color;
        }
    }
}
