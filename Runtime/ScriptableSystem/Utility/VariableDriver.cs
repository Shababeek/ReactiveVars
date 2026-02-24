using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Base class for components that write values into a ScriptableVariable from an external source.
    /// Drivers are the write-side counterpart to Binders (which read).
    /// </summary>
    /// <typeparam name="T">The ScriptableVariable type this driver writes to.</typeparam>
    public abstract class VariableDriver<T> : MonoBehaviour where T : ScriptableVariable
    {
        [Tooltip("When true, updates the variable without raising change events. Pair with Poll mode binders.")]
        [SerializeField] private bool silentUpdates;

        /// <summary>The variable this driver writes to.</summary>
        protected abstract T Variable { get; }

        /// <summary>Whether updates skip event notifications.</summary>
        protected bool SilentUpdates => silentUpdates;

        protected virtual void OnEnable()
        {
            if (Variable == null)
                Debug.LogWarning($"Variable is not assigned on {gameObject.name}", this);
        }
    }
}
