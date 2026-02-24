using UniRx;
using UnityEngine;
using UnityEngine.Events;

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

    /// <summary>
    /// Base class for two-way binders that sync between a ScriptableVariable and a UI element.
    /// Handles disposable lifecycle, recursion guards, and bidirectional subscription.
    /// </summary>
    /// <typeparam name="T">The ScriptableVariable type this binder works with.</typeparam>
    public abstract class TwoWayVariableBinder<T> : MonoBehaviour where T : ScriptableVariable
    {
        /// <summary>The variable this binder syncs with.</summary>
        protected abstract T Variable { get; }

        /// <summary>Pushes the current variable value to the UI element.</summary>
        protected abstract void UpdateUIFromVariable();

        /// <summary>Subscribes to UI element changes. Use SubscribeUIEvent or GuardedUpdate.</summary>
        protected abstract void SubscribeToUI();

        private CompositeDisposable _disposable;
        private bool _isUpdating;

        /// <summary>Executes an action inside a recursion guard. Nested calls are skipped.</summary>
        protected void GuardedUpdate(System.Action action)
        {
            if (_isUpdating) return;
            _isUpdating = true;
            try { action(); }
            finally { _isUpdating = false; }
        }

        /// <summary>Subscribes to a UnityEvent and auto-disposes on disable.</summary>
        protected void SubscribeUIEvent(UnityEvent evt, System.Action handler)
        {
            evt.AsObservable()
                .Subscribe(_ => handler())
                .AddTo(_disposable);
        }

        /// <summary>Subscribes to a typed UnityEvent and auto-disposes on disable.</summary>
        protected void SubscribeUIEvent<TValue>(UnityEvent<TValue> evt, System.Action<TValue> handler)
        {
            evt.AsObservable()
                .Subscribe(handler)
                .AddTo(_disposable);
        }

        protected virtual void OnEnable()
        {
            _disposable = new CompositeDisposable();
            if (Variable == null)
            {
                Debug.LogWarning($"Variable is not assigned on {gameObject.name}", this);
                return;
            }

            GuardedUpdate(UpdateUIFromVariable);

            Variable.OnRaised
                .Subscribe(_ => GuardedUpdate(UpdateUIFromVariable))
                .AddTo(_disposable);

            SubscribeToUI();
        }

        protected virtual void OnDisable()
        {
            _disposable?.Dispose();
        }
    }

    /// <summary>
    /// Base class for binders that observe two ScriptableVariables.
    /// Either variable may be null — only non-null variables are subscribed to.
    /// </summary>
    public abstract class VariableBinder<T1, T2> : MonoBehaviour
        where T1 : ScriptableVariable
        where T2 : ScriptableVariable
    {
        /// <summary>First variable (may be null).</summary>
        protected abstract T1 Variable1 { get; }

        /// <summary>Second variable (may be null).</summary>
        protected abstract T2 Variable2 { get; }

        /// <summary>Called once after subscriptions are established.</summary>
        protected abstract void Bind();

        /// <summary>Called when Variable1 raises a change event.</summary>
        protected abstract void OnVariable1Changed();

        /// <summary>Called when Variable2 raises a change event.</summary>
        protected abstract void OnVariable2Changed();

        private CompositeDisposable _disposable;

        protected virtual void OnEnable()
        {
            _disposable = new CompositeDisposable();

            if (Variable1 == null && Variable2 == null)
            {
                Debug.LogWarning($"No variables assigned on {gameObject.name}", this);
                return;
            }

            Bind();

            if (Variable1 != null)
                Variable1.OnRaised.Subscribe(_ => OnVariable1Changed()).AddTo(_disposable);

            if (Variable2 != null)
                Variable2.OnRaised.Subscribe(_ => OnVariable2Changed()).AddTo(_disposable);
        }

        protected virtual void OnDisable()
        {
            _disposable?.Dispose();
        }
    }

    /// <summary>
    /// Base class for binders that observe three ScriptableVariables.
    /// Any variable may be null — only non-null variables are subscribed to.
    /// </summary>
    public abstract class VariableBinder<T1, T2, T3> : MonoBehaviour
        where T1 : ScriptableVariable
        where T2 : ScriptableVariable
        where T3 : ScriptableVariable
    {
        /// <summary>First variable (may be null).</summary>
        protected abstract T1 Variable1 { get; }

        /// <summary>Second variable (may be null).</summary>
        protected abstract T2 Variable2 { get; }

        /// <summary>Third variable (may be null).</summary>
        protected abstract T3 Variable3 { get; }

        /// <summary>Called once after subscriptions are established.</summary>
        protected abstract void Bind();

        /// <summary>Called when Variable1 raises a change event.</summary>
        protected abstract void OnVariable1Changed();

        /// <summary>Called when Variable2 raises a change event.</summary>
        protected abstract void OnVariable2Changed();

        /// <summary>Called when Variable3 raises a change event.</summary>
        protected abstract void OnVariable3Changed();

        private CompositeDisposable _disposable;

        protected virtual void OnEnable()
        {
            _disposable = new CompositeDisposable();

            if (Variable1 == null && Variable2 == null && Variable3 == null)
            {
                Debug.LogWarning($"No variables assigned on {gameObject.name}", this);
                return;
            }

            Bind();

            if (Variable1 != null)
                Variable1.OnRaised.Subscribe(_ => OnVariable1Changed()).AddTo(_disposable);

            if (Variable2 != null)
                Variable2.OnRaised.Subscribe(_ => OnVariable2Changed()).AddTo(_disposable);

            if (Variable3 != null)
                Variable3.OnRaised.Subscribe(_ => OnVariable3Changed()).AddTo(_disposable);
        }

        protected virtual void OnDisable()
        {
            _disposable?.Dispose();
        }
    }
}
