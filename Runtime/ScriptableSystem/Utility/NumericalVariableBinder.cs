using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Base class for binders that work with any numerical ScriptableVariable.
    /// Handles the INumericalVariable cast and null-check so children get direct typed access.
    /// </summary>
    public abstract class NumericalVariableBinder : VariableBinder<ScriptableVariable>
    {
        /// <summary>The variable cast as INumericalVariable. Null if the variable isn't numerical.</summary>
        protected INumericalVariable NumericalVariable { get; private set; }

        protected sealed override void Bind()
        {
            NumericalVariable = Variable as INumericalVariable;
            if (NumericalVariable == null)
            {
                Debug.LogWarning($"Variable on {gameObject.name} is not a numerical variable", this);
                return;
            }
            BindNumerical();
        }

        protected sealed override void OnVariableChanged()
        {
            if (NumericalVariable != null)
                OnNumericalValueChanged();
        }

        /// <summary>Called once after the numerical variable is validated. Use for initial state setup.</summary>
        protected abstract void BindNumerical();

        /// <summary>Called every time the numerical variable value changes.</summary>
        protected abstract void OnNumericalValueChanged();
    }
}