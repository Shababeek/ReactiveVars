using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Activates or deactivates a target GameObject based on a BoolVariable.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Binders/GameObject Active Binder")]
    public class GameObjectActiveBinder : VariableBinder<BoolVariable>
    {
        [Tooltip("The BoolVariable that controls the active state.")]
        [SerializeField] private BoolVariable variable;

        [Tooltip("The target GameObject to activate/deactivate. Defaults to this GameObject if not set.")]
        [SerializeField] private GameObject targetObject;

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
            if (Variable == null) return;
            var target = targetObject != null ? targetObject : gameObject;
            bool value = invert ? !Variable.Value : Variable.Value;
            target.SetActive(value);
        }
    }
}
