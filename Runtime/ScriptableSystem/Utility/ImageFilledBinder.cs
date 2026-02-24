using UnityEngine;
using UnityEngine.UI;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds an IntVariable to an Image's fill amount for live updates.
    /// Perfect for health bars, progress bars, stamina bars, etc.
    /// </summary>
    [AddComponentMenu("Shababeek/ReactiveVars/Binders/Image Filled Binder")]
    [RequireComponent(typeof(Image))]
    public class ImageFilledBinder : VariableBinder<IntVariable>
    {
        [Tooltip("The IntVariable to bind to the image fill.")]
        [SerializeField] private IntVariable intVariable;

        [Header("Fill Settings")]
        [Tooltip("The minimum value from the IntVariable that maps to 0 fill.")]
        [SerializeField] private int minValue = 0;

        [Tooltip("The maximum value from the IntVariable that maps to 1 (full) fill.")]
        [SerializeField] private int maxValue = 100;

        [Tooltip("Invert the fill direction (useful for overlays that hide content as value increases).")]
        [SerializeField] private bool invertFill = false;

        [Tooltip("Whether to smoothly interpolate fill changes.")]
        [SerializeField] private bool smoothFill = false;

        [Tooltip("Fill speed for smooth interpolation (0-1 per second).")]
        [SerializeField] private float fillSpeed = 1f;

        private Image _image;
        private float _targetFillAmount;
        private float _currentFillAmount;

        protected override IntVariable Variable => intVariable;

        protected override void Bind()
        {
            _image = GetComponent<Image>();

            if (_image == null)
            {
                Debug.LogWarning($"Image component not found on {gameObject.name}", this);
                return;
            }

            UpdateFill(intVariable.Value);
        }

        protected override void OnVariableChanged()
        {
            UpdateFill(intVariable.Value);
        }

        private void Update()
        {
            if (smoothFill)
            {
                _currentFillAmount = Mathf.MoveTowards(_currentFillAmount, _targetFillAmount, fillSpeed * Time.deltaTime);
                _image.fillAmount = _currentFillAmount;
            }
        }

        private void UpdateFill(int value)
        {
            int clampedValue = Mathf.Clamp(value, minValue, maxValue);

            float t = maxValue != minValue ? (float)(clampedValue - minValue) / (maxValue - minValue) : 0f;
            _targetFillAmount = Mathf.Clamp01(t);

            if (invertFill)
            {
                _targetFillAmount = 1f - _targetFillAmount;
            }

            if (!smoothFill)
            {
                _image.fillAmount = _targetFillAmount;
                _currentFillAmount = _targetFillAmount;
            }
            else
            {
                if (_currentFillAmount == 0f && _targetFillAmount != 0f)
                {
                    _currentFillAmount = _image.fillAmount;
                }
            }
        }
    }
}
