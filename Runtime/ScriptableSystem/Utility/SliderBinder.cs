using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds a numeric variable to a UI Slider with configurable binding direction.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Slider Binder")]
    [RequireComponent(typeof(Slider))]
    public class SliderBinder : MonoBehaviour
    {
        [SerializeField] private ScriptableVariable variable;

        [Header("Binding Mode")]
        [SerializeField] private BindingMode bindingMode = BindingMode.TwoWay;

        [Header("Value Mapping")]
        [Tooltip("Map variable value to slider range. If false, uses variable value directly.")]
        [SerializeField] private bool useValueMapping = false;

        [Tooltip("Variable value that maps to slider min.")]
        [SerializeField] private float minVariableValue = 0f;

        [Tooltip("Variable value that maps to slider max.")]
        [SerializeField] private float maxVariableValue = 100f;

        [Header("Options")]
        [SerializeField] private bool roundToInt = false;

        private Slider _slider;
        private CompositeDisposable _disposable;
        private INumericalVariable _numericalVariable;
        private bool _isUpdating;

        /// <summary>Defines how the slider and variable stay in sync.</summary>
        public enum BindingMode
        {
            /// <summary>Variable changes update slider only.</summary>
            OneWayToSlider,
            /// <summary>Slider changes update variable only.</summary>
            OneWayToVariable,
            /// <summary>Both directions sync.</summary>
            TwoWay
        }

        private void Awake()
        {
            _slider = GetComponent<Slider>();
        }

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();

            if (variable == null)
            {
                Debug.LogWarning($"Variable is not assigned on {gameObject.name}", this);
                return;
            }

            _numericalVariable = variable as INumericalVariable;
            if (_numericalVariable == null)
            {
                Debug.LogWarning($"Variable on {gameObject.name} is not a numerical variable", this);
                return;
            }

            if (bindingMode != BindingMode.OneWayToVariable)
            {
                UpdateSliderFromVariable();
                variable.OnRaised
                    .Subscribe(_ => UpdateSliderFromVariable())
                    .AddTo(_disposable);
            }

            if (bindingMode != BindingMode.OneWayToSlider)
            {
                _slider.onValueChanged.AsObservable()
                    .Subscribe(OnSliderValueChanged)
                    .AddTo(_disposable);
            }
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }

        private void UpdateSliderFromVariable()
        {
            if (_isUpdating) return;
            _isUpdating = true;
            try
            {
                float value = _numericalVariable.AsFloat;
                _slider.value = useValueMapping
                    ? Mathf.Lerp(_slider.minValue, _slider.maxValue,
                        Mathf.InverseLerp(minVariableValue, maxVariableValue, value))
                    : value;
            }
            finally { _isUpdating = false; }
        }

        private void OnSliderValueChanged(float sliderValue)
        {
            if (_isUpdating) return;
            _isUpdating = true;
            try
            {
                float value = useValueMapping
                    ? Mathf.Lerp(minVariableValue, maxVariableValue,
                        Mathf.InverseLerp(_slider.minValue, _slider.maxValue, sliderValue))
                    : sliderValue;

                if (roundToInt) value = Mathf.Round(value);
                _numericalVariable.SetFromFloat(value);
            }
            finally { _isUpdating = false; }
        }

        /// <summary>Manually syncs the slider to the current variable value.</summary>
        public void SyncSliderToVariable()
        {
            if (_numericalVariable != null)
                UpdateSliderFromVariable();
        }

        /// <summary>Manually syncs the variable to the current slider value.</summary>
        public void SyncVariableToSlider()
        {
            if (_slider != null && _numericalVariable != null)
                OnSliderValueChanged(_slider.value);
        }

        /// <summary>Copies value mapping range to slider min/max and disables mapping.</summary>
        [ContextMenu("Setup Slider Range From Mapping")]
        public void SetupSliderRangeFromMapping()
        {
            if (_slider == null) _slider = GetComponent<Slider>();
            _slider.minValue = minVariableValue;
            _slider.maxValue = maxVariableValue;
            useValueMapping = false;
        }
    }
}
