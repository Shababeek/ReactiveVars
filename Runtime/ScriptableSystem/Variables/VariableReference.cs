using System;
using UnityEngine;
using UniRx;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// A reference that can point to either a ScriptableVariable or use a constant value.
    /// Provides UniRx integration for reactive programming.
    /// </summary>
    [Serializable]
    public class VariableReference<T>
    {
        [Tooltip("The referenced ScriptableVariable (used when useConstant is false).")]
        [SerializeField] private ScriptableVariable<T> variable;

        [Tooltip("If true, use the constant value; if false, use the variable reference.")]
        [SerializeField] private bool useConstant;

        [Tooltip("The constant value to use when useConstant is true.")]
        [SerializeField] private T constantValue;

        [Tooltip("Display name for the reference.")]
        [SerializeField] private string name;

        private IDisposable _subscription;
        private readonly Subject<T> _onValueChangedSubject = new();

        /// <summary>
        /// Gets or sets the name of the reference (either the constant name or variable name).
        /// </summary>
        public string Name
        {
            get => useConstant ? name : variable.name;
            set
            {
                if (useConstant) name = value;
                else variable.name = value;
            }
        }

        /// <summary>
        /// Gets or sets the current value (either from the variable or constant).
        /// </summary>
        public T Value
        {
            get => useConstant ? constantValue : (variable.Value);
            set
            {
                if (useConstant)
                {
                    constantValue = value;
                    _onValueChangedSubject.OnNext(constantValue);
                }
                else if (variable != null)
                {
                    variable.Value = value;
                }
            }
        }

        /// <summary>
        /// Gets an observable that emits when the value changes.
        /// </summary>
        public IObservable<T> OnValueChanged =>
            useConstant ? _onValueChangedSubject.AsObservable() : variable.OnValueChanged;

        public static implicit operator T(VariableReference<T> reference) => reference.Value;
    }
}