using System.Collections.Generic;
using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Captures ScriptableVariable values on Awake and restores them on OnDestroy.
    /// Ensures runtime changes don't persist across play sessions or scene reloads.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Utility/Variable Resetter")]
    public class VariableResetter : MonoBehaviour
    {
        [Tooltip("The variables to snapshot and restore.")]
        [SerializeField] private List<ScriptableVariable> variables = new();

        private readonly Dictionary<ScriptableVariable, object> _snapshots = new();

        private void Awake()
        {
            CaptureSnapshot();
        }

        private void OnDestroy()
        {
            RestoreSnapshot();
        }

        private void CaptureSnapshot()
        {
            _snapshots.Clear();
            foreach (var variable in variables)
            {
                if (variable == null) continue;
                _snapshots[variable] = variable.GetValue();
            }
        }

        private void RestoreSnapshot()
        {
            foreach (var kvp in _snapshots)
            {
                if (kvp.Key == null) continue;
                kvp.Key.SetValue(kvp.Value);
            }

            _snapshots.Clear();
        }
    }
}
