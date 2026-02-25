using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Enables or disables a target Behaviour based on a BoolVariable.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Binders/Enable Component Binder")]
    public class EnableComponentBinder : VariableBinder<BoolVariable>
    {
        [Tooltip("The BoolVariable that controls the enabled state.")]
        [SerializeField] private BoolVariable variable;

        [Tooltip("The target Behaviour to enable/disable.")]
        [SerializeField] private Behaviour targetComponent;

        [Tooltip("Invert the boolean value before applying.")]
        [SerializeField] private bool invert;

        protected override BoolVariable Variable => variable;

        protected override void Bind()
        {
            Apply();
        }

        protected override void OnVariableChanged()
        {
            Apply();
        }

        private void Apply()
        {
            if (Variable == null || targetComponent == null) return;
            targetComponent.enabled = invert ? !Variable.Value : Variable.Value;
        }
    }
}
