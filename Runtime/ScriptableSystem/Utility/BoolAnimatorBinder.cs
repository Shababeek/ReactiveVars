using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds a BoolVariable to an Animator bool or trigger parameter.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Binders/Bool Animator Binder")]
    [RequireComponent(typeof(Animator))]
    public class BoolAnimatorBinder : VariableBinder<BoolVariable>
    {
        [SerializeField] private BoolVariable variable;

        [Header("Animator Parameter")]
        [SerializeField] private string parameterName;

        [Tooltip("Use trigger mode instead of bool (fires trigger on true).")]
        [SerializeField] private bool useTriggerMode = false;

        [Header("Options")]
        [SerializeField] private bool invert = false;

        private Animator _animator;
        private int _parameterHash;

        protected override BoolVariable Variable => variable;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        protected override void Bind()
        {
            _parameterHash = Animator.StringToHash(parameterName);
            UpdateParameter(GetEffectiveValue());
        }

        protected override void OnVariableChanged()
        {
            UpdateParameter(GetEffectiveValue());
        }

        private bool GetEffectiveValue()
        {
            return invert ? !variable.Value : variable.Value;
        }

        private void UpdateParameter(bool value)
        {
            if (_animator == null || !_animator.isActiveAndEnabled) return;

            if (useTriggerMode)
            {
                if (value)
                    _animator.SetTrigger(_parameterHash);
                else
                    _animator.ResetTrigger(_parameterHash);
            }
            else
            {
                _animator.SetBool(_parameterHash, value);
            }
        }

        /// <summary>Forces a refresh from the current variable value.</summary>
        public void Refresh()
        {
            if (variable != null)
                UpdateParameter(GetEffectiveValue());
        }
    }
}
