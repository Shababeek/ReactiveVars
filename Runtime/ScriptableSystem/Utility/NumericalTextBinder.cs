using TMPro;
using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Displays a numeric variable as formatted text on a TextMeshPro component.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Binders/Numerical Text Binder")]
    [RequireComponent(typeof(TMP_Text))]
    public class NumericalTextBinder : NumericalVariableBinder
    {
        [SerializeField] private ScriptableVariable variable;

        [Header("Format")]
        [Tooltip("Format string using {0} as placeholder (e.g. 'HP: {0:F0}/100', '{0}%', '${0:F2}').")]
        [SerializeField] private string format = "{0}";

        [SerializeField] private bool displayAsInt = false;

        [Header("Value Mapping")]
        [Tooltip("Optional multiplier applied before display (e.g. 100 to show 0-1 as percentage).")]
        [SerializeField] private float multiplier = 1f;

        [Tooltip("Optional offset added after multiplier.")]
        [SerializeField] private float offset = 0f;

        private TMP_Text _text;

        protected override ScriptableVariable Variable => variable;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        protected override void BindNumerical()
        {
            UpdateText(NumericalVariable.AsFloat);
        }

        protected override void OnNumericalValueChanged()
        {
            UpdateText(NumericalVariable.AsFloat);
        }

        private void UpdateText(float value)
        {
            float displayValue = value * multiplier + offset;

            if (displayAsInt)
                _text.text = string.Format(format, Mathf.RoundToInt(displayValue));
            else
                _text.text = string.Format(format, displayValue);
        }

        /// <summary>Forces a refresh from the current variable value.</summary>
        public void Refresh()
        {
            if (NumericalVariable != null)
                UpdateText(NumericalVariable.AsFloat);
        }
    }
}
