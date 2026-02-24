using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Controls how a binder reads variable changes.
    /// </summary>
    public enum BinderUpdateMode
    {
        [Tooltip("Reacts to variable change events via subscription.")]
        Subscribe,
        [Tooltip("Polls the variable value every frame in Update.")]
        Poll
    }

    /// <summary>
    /// Base class for components that bind a ScriptableVariable to a target.
    /// Handles disposable lifecycle, null checks, subscription, and change callbacks.
    /// UniRx usage is isolated here so child classes stay reactive-framework-agnostic.
    /// </summary>
    /// <typeparam name="T">The ScriptableVariable type this binder works with.</typeparam>
    public abstract class VariableBinder<T> : MonoBehaviour where T : ScriptableVariable
    {
        [Tooltip("Subscribe reacts to change events. Poll reads the value every frame.")]
        [SerializeField] private BinderUpdateMode updateMode = BinderUpdateMode.Subscribe;

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

            if (updateMode == BinderUpdateMode.Subscribe)
            {
                Variable.OnRaised
                    .Subscribe(_ => OnVariableChanged())
                    .AddTo(_disposable);
            }
        }

        protected virtual void OnDisable()
        {
            _disposable?.Dispose();
        }

        private void Update()
        {
            if (updateMode == BinderUpdateMode.Poll && Variable != null)
                OnVariableChanged();
        }
    }

    /// <summary>
    /// Base class for two-way binders that sync between a ScriptableVariable and a UI element.
    /// Handles disposable lifecycle, recursion guards, and bidirectional subscription.
    /// </summary>
    /// <typeparam name="T">The ScriptableVariable type this binder works with.</typeparam>
    public abstract class TwoWayVariableBinder<T> : MonoBehaviour where T : ScriptableVariable
    {
        [Tooltip("Subscribe reacts to change events. Poll reads the value every frame.")]
        [SerializeField] private BinderUpdateMode updateMode = BinderUpdateMode.Subscribe;

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

            if (updateMode == BinderUpdateMode.Subscribe)
            {
                Variable.OnRaised
                    .Subscribe(_ => GuardedUpdate(UpdateUIFromVariable))
                    .AddTo(_disposable);
            }

            SubscribeToUI();
        }

        protected virtual void OnDisable()
        {
            _disposable?.Dispose();
        }

        private void Update()
        {
            if (updateMode == BinderUpdateMode.Poll && Variable != null)
                GuardedUpdate(UpdateUIFromVariable);
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
        [Tooltip("Subscribe reacts to change events. Poll reads the value every frame.")]
        [SerializeField] private BinderUpdateMode updateMode = BinderUpdateMode.Subscribe;

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

            if (updateMode == BinderUpdateMode.Subscribe)
            {
                if (Variable1 != null)
                    Variable1.OnRaised.Subscribe(_ => OnVariable1Changed()).AddTo(_disposable);

                if (Variable2 != null)
                    Variable2.OnRaised.Subscribe(_ => OnVariable2Changed()).AddTo(_disposable);
            }
        }

        protected virtual void OnDisable()
        {
            _disposable?.Dispose();
        }

        private void Update()
        {
            if (updateMode != BinderUpdateMode.Poll) return;
            if (Variable1 != null) OnVariable1Changed();
            if (Variable2 != null) OnVariable2Changed();
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
        [Tooltip("Subscribe reacts to change events. Poll reads the value every frame.")]
        [SerializeField] private BinderUpdateMode updateMode = BinderUpdateMode.Subscribe;

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

            if (updateMode == BinderUpdateMode.Subscribe)
            {
                if (Variable1 != null)
                    Variable1.OnRaised.Subscribe(_ => OnVariable1Changed()).AddTo(_disposable);

                if (Variable2 != null)
                    Variable2.OnRaised.Subscribe(_ => OnVariable2Changed()).AddTo(_disposable);

                if (Variable3 != null)
                    Variable3.OnRaised.Subscribe(_ => OnVariable3Changed()).AddTo(_disposable);
            }
        }

        protected virtual void OnDisable()
        {
            _disposable?.Dispose();
        }

        private void Update()
        {
            if (updateMode != BinderUpdateMode.Poll) return;
            if (Variable1 != null) OnVariable1Changed();
            if (Variable2 != null) OnVariable2Changed();
            if (Variable3 != null) OnVariable3Changed();
        }
    }
}
