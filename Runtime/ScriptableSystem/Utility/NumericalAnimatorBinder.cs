using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds a numeric variable to an Animator float or int parameter.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Binders/Numerical Animator Binder")]
    [RequireComponent(typeof(Animator))]
    public class NumericalAnimatorBinder : NumericalVariableBinder
    {
        [SerializeField] private ScriptableVariable variable;

        [Header("Animator Parameter")]
        [SerializeField] private string parameterName;
        [SerializeField] private bool setAsInteger = false;

        [Header("Damping")]
        [Tooltip("Whether to use Animator's built-in damping for smooth transitions.")]
        [SerializeField] private bool useDamping = false;

        [Tooltip("Damping time for smooth transitions (only when useDamping is true).")]
        [SerializeField] private float dampTime = 0.1f;

        private Animator _animator;
        private int _parameterHash;

        protected override ScriptableVariable Variable => variable;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        protected override void BindNumerical()
        {
            _parameterHash = Animator.StringToHash(parameterName);
            UpdateParameter(NumericalVariable.AsFloat);
        }

        protected override void OnNumericalValueChanged()
        {
            UpdateParameter(NumericalVariable.AsFloat);
        }

        private void UpdateParameter(float value)
        {
            if (_animator == null || !_animator.isActiveAndEnabled) return;

            if (setAsInteger)
            {
                _animator.SetInteger(_parameterHash, Mathf.RoundToInt(value));
            }
            else if (useDamping)
            {
                _animator.SetFloat(_parameterHash, value, dampTime, Time.deltaTime);
            }
            else
            {
                _animator.SetFloat(_parameterHash, value);
            }
        }

        /// <summary>Forces a refresh from the current variable value.</summary>
        public void Refresh()
        {
            if (NumericalVariable != null)
                UpdateParameter(NumericalVariable.AsFloat);
        }
    }
}
