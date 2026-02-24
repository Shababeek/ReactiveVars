using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds a numeric variable to an AudioSource's volume and/or pitch.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Binders/Numerical Audio Binder")]
    [RequireComponent(typeof(AudioSource))]
    public class NumericalAudioBinder : NumericalVariableBinder
    {
        [SerializeField] private ScriptableVariable variable;

        [Header("Target Properties")]
        [SerializeField] private AudioProperty targetProperty = AudioProperty.Volume;

        [Header("Value Mapping")]
        [SerializeField] private float minValue = 0f;
        [SerializeField] private float maxValue = 1f;

        [Tooltip("AudioSource value when variable equals minValue.")]
        [SerializeField] private float minOutput = 0f;

        [Tooltip("AudioSource value when variable equals maxValue.")]
        [SerializeField] private float maxOutput = 1f;

        [Header("Interpolation")]
        [SerializeField] private bool smooth = false;
        [SerializeField] private float smoothSpeed = 10f;

        private AudioSource _audioSource;
        private float _targetValue;
        private float _currentValue;

        protected override ScriptableVariable Variable => variable;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        protected override void BindNumerical()
        {
            _currentValue = GetCurrentPropertyValue();
            UpdateAudioProperty(NumericalVariable.AsFloat);
        }

        protected override void OnNumericalValueChanged()
        {
            UpdateAudioProperty(NumericalVariable.AsFloat);
        }

        private void Update()
        {
            if (!smooth) return;

            _currentValue = Mathf.MoveTowards(_currentValue, _targetValue, smoothSpeed * Time.deltaTime);
            SetPropertyValue(_currentValue);
        }

        private void UpdateAudioProperty(float value)
        {
            float t = Mathf.InverseLerp(minValue, maxValue, value);
            _targetValue = Mathf.Lerp(minOutput, maxOutput, t);

            if (!smooth)
            {
                _currentValue = _targetValue;
                SetPropertyValue(_targetValue);
            }
        }

        private void SetPropertyValue(float value)
        {
            switch (targetProperty)
            {
                case AudioProperty.Volume:
                    _audioSource.volume = Mathf.Clamp01(value);
                    break;
                case AudioProperty.Pitch:
                    _audioSource.pitch = value;
                    break;
                case AudioProperty.SpatialBlend:
                    _audioSource.spatialBlend = Mathf.Clamp01(value);
                    break;
                case AudioProperty.ReverbZoneMix:
                    _audioSource.reverbZoneMix = Mathf.Clamp(value, 0f, 1.1f);
                    break;
            }
        }

        private float GetCurrentPropertyValue()
        {
            return targetProperty switch
            {
                AudioProperty.Volume => _audioSource.volume,
                AudioProperty.Pitch => _audioSource.pitch,
                AudioProperty.SpatialBlend => _audioSource.spatialBlend,
                AudioProperty.ReverbZoneMix => _audioSource.reverbZoneMix,
                _ => 0f
            };
        }

        /// <summary>Defines which AudioSource property to control.</summary>
        public enum AudioProperty
        {
            Volume,
            Pitch,
            SpatialBlend,
            ReverbZoneMix
        }
    }
}
