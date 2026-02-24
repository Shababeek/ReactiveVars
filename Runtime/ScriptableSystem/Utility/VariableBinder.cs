using UniRx;
using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Base class for components that bind a ScriptableVariable to a target.
    /// Handles disposable lifecycle, null checks, subscription, and change callbacks.
    /// UniRx usage is isolated here so child classes stay reactive-framework-agnostic.
    /// </summary>
    /// <typeparam name="T">The ScriptableVariable type this binder works with.</typeparam>
    public abstract class VariableBinder<T> : MonoBehaviour where T : ScriptableVariable
    {
        /// <summary>The variable this binder is bound to.</summary>
        protected abstract T Variable { get; }

        /// <summary>Called once after the variable is validated. Use for initial state setup.</summary>
        protected abstract void Bind();

        /// <summary>Called every time the variable raises a change event.</summary>
        protected abstract void OnVariableChanged();

        private CompositeDisposable _disposable;

        protected virtual void OnEnable()
        {
            _disposable = new CompositeDisposable();
            if (Variable == null)
            {
                Debug.LogWarning($"Variable is not assigned on {gameObject.name}", this);
                return;
            }
            Bind();
            Variable.OnRaised
                .Subscribe(_ => OnVariableChanged())
                .AddTo(_disposable);
        }

        protected virtual void OnDisable()
        {
            _disposable?.Dispose();
        }
    }

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
