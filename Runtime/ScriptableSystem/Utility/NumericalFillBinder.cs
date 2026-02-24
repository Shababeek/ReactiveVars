using UnityEngine;
using UnityEngine.UI;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds any numeric variable (IntVariable or FloatVariable) to a UI Image's fill amount.
    /// </summary>
    [AddComponentMenu("Shababeek/ReactiveVars/Binders/Numerical Fill Binder")]
    [RequireComponent(typeof(Image))]
    public class NumericalFillBinder : NumericalVariableBinder
    {
        [Tooltip("The numeric variable to bind (IntVariable or FloatVariable).")]
        [SerializeField] private ScriptableVariable variable;

        [Header("Fill Settings")]
        [Tooltip("The minimum value from the variable that maps to 0 fill.")]
        [SerializeField] private float minValue = 0f;

        [Tooltip("The maximum value from the variable that maps to 1 (full) fill.")]
        [SerializeField] private float maxValue = 100f;

        [Tooltip("Whether to invert the fill (max value = 0 fill, min value = full fill).")]
        [SerializeField] private bool invertFill = false;

        [Header("Interpolation")]
        [Tooltip("Whether to smoothly interpolate fill changes.")]
        [SerializeField] private bool smoothFill = false;

        [Tooltip("Fill speed for smooth interpolation (0-1 per second).")]
        [SerializeField] private float fillSpeed = 2f;

        private Image _image;
        private float _targetFillAmount;
        private float _currentFillAmount;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        protected override ScriptableVariable Variable => variable;

        protected override void BindNumerical()
        {
            _currentFillAmount = _image.fillAmount;
            UpdateFill(NumericalVariable.AsFloat);
        }

        protected override void OnNumericalValueChanged()
        {
            UpdateFill(NumericalVariable.AsFloat);
        }

        private void Update()
        {
            if (!smoothFill) return;

            _currentFillAmount = Mathf.MoveTowards(_currentFillAmount, _targetFillAmount, fillSpeed * Time.deltaTime);
            _image.fillAmount = _currentFillAmount;
        }

        private void UpdateFill(float value)
        {
            float normalized;
            if (Mathf.Approximately(maxValue, minValue))
            {
                normalized = 0f;
            }
            else
            {
                normalized = Mathf.Clamp01((value - minValue) / (maxValue - minValue));
            }

            if (invertFill)
            {
                normalized = 1f - normalized;
            }

            _targetFillAmount = normalized;

            if (!smoothFill)
            {
                _image.fillAmount = _targetFillAmount;
                _currentFillAmount = _targetFillAmount;
            }
        }

        /// <summary>Sets the fill amount immediately without interpolation.</summary>
        public void SetFillImmediate(float fillAmount)
        {
            _targetFillAmount = Mathf.Clamp01(fillAmount);
            _currentFillAmount = _targetFillAmount;
            _image.fillAmount = _targetFillAmount;
        }

        /// <summary>Gets the current fill amount.</summary>
        public float CurrentFillAmount => _currentFillAmount;

        /// <summary>Gets the target fill amount.</summary>
        public float TargetFillAmount => _targetFillAmount;

        /// <summary>Recalculates the fill based on the current variable value.</summary>
        public void Refresh()
        {
            if (NumericalVariable != null)
            {
                UpdateFill(NumericalVariable.AsFloat);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_image == null) _image = GetComponent<Image>();
            if (_image != null && _image.type != Image.Type.Filled)
            {
                Debug.LogWarning($"Image on {gameObject.name} is not set to Filled type. NumericalFillBinder requires a Filled image.", this);
            }
        }
#endif
    }
}
