using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds a numeric variable to ParticleSystem properties like emission rate, speed, or size.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Binders/Numerical Particle Binder")]
    [RequireComponent(typeof(ParticleSystem))]
    public class NumericalParticleBinder : NumericalVariableBinder
    {
        [SerializeField] private ScriptableVariable variable;

        [Header("Target Property")]
        [SerializeField] private ParticleProperty targetProperty = ParticleProperty.EmissionRate;

        [Header("Value Mapping")]
        [SerializeField] private float minValue = 0f;
        [SerializeField] private float maxValue = 1f;

        [Tooltip("Particle property value when variable equals minValue.")]
        [SerializeField] private float minOutput = 0f;

        [Tooltip("Particle property value when variable equals maxValue.")]
        [SerializeField] private float maxOutput = 50f;

        private ParticleSystem _particleSystem;

        protected override ScriptableVariable Variable => variable;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        protected override void BindNumerical()
        {
            UpdateParticleProperty(NumericalVariable.AsFloat);
        }

        protected override void OnNumericalValueChanged()
        {
            UpdateParticleProperty(NumericalVariable.AsFloat);
        }

        private void UpdateParticleProperty(float value)
        {
            float t = Mathf.InverseLerp(minValue, maxValue, value);
            float output = Mathf.Lerp(minOutput, maxOutput, t);

            switch (targetProperty)
            {
                case ParticleProperty.EmissionRate:
                    var emission = _particleSystem.emission;
                    emission.rateOverTime = output;
                    break;

                case ParticleProperty.StartSpeed:
                    var main = _particleSystem.main;
                    main.startSpeed = output;
                    break;

                case ParticleProperty.StartSize:
                    var mainSize = _particleSystem.main;
                    mainSize.startSize = output;
                    break;

                case ParticleProperty.StartLifetime:
                    var mainLife = _particleSystem.main;
                    mainLife.startLifetime = Mathf.Max(0.01f, output);
                    break;

                case ParticleProperty.SimulationSpeed:
                    var mainSim = _particleSystem.main;
                    mainSim.simulationSpeed = Mathf.Max(0f, output);
                    break;

                case ParticleProperty.GravityModifier:
                    var mainGrav = _particleSystem.main;
                    mainGrav.gravityModifier = output;
                    break;
            }
        }

        /// <summary>Forces a refresh from the current variable value.</summary>
        public void Refresh()
        {
            if (NumericalVariable != null)
                UpdateParticleProperty(NumericalVariable.AsFloat);
        }

        /// <summary>Defines which ParticleSystem property to control.</summary>
        public enum ParticleProperty
        {
            EmissionRate,
            StartSpeed,
            StartSize,
            StartLifetime,
            SimulationSpeed,
            GravityModifier
        }
    }
}
