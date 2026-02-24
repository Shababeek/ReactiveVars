using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds a BoolVariable to start/stop ParticleSystem emission.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Bool Particle Binder")]
    [RequireComponent(typeof(ParticleSystem))]
    public class BoolParticleBinder : VariableBinder<BoolVariable>
    {
        [SerializeField] private BoolVariable variable;

        [Header("Behavior")]
        [Tooltip("Invert the boolean (true = stop, false = play).")]
        [SerializeField] private bool invert = false;

        [SerializeField] private bool clearOnStop = false;
        [SerializeField] private bool includeChildren = true;

        private ParticleSystem _particleSystem;

        protected override BoolVariable Variable => variable;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        protected override void Bind()
        {
            ApplyState(GetEffectiveValue());
        }

        protected override void OnVariableChanged()
        {
            ApplyState(GetEffectiveValue());
        }

        private bool GetEffectiveValue()
        {
            return invert ? !variable.Value : variable.Value;
        }

        private void ApplyState(bool shouldPlay)
        {
            if (shouldPlay)
            {
                if (!_particleSystem.isPlaying)
                    _particleSystem.Play(includeChildren);
            }
            else
            {
                if (_particleSystem.isPlaying)
                {
                    _particleSystem.Stop(includeChildren,
                        clearOnStop ? ParticleSystemStopBehavior.StopEmittingAndClear
                                    : ParticleSystemStopBehavior.StopEmitting);
                }
            }
        }
    }
}
