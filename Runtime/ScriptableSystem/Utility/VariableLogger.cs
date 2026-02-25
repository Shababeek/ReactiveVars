using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Logs value changes of one or more ScriptableVariables to the console. Useful for debugging.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Utility/Variable Logger")]
    public class VariableLogger : MonoBehaviour
    {
        [Tooltip("The variables to watch.")]
        [SerializeField] private List<ScriptableVariable> variables = new();

        [Tooltip("Optional prefix for log messages. Format: prefix_variableName = value")]
        [SerializeField] private string logPrefix = "";

        [Tooltip("Log level for the messages.")]
        [SerializeField] private LogLevel logLevel = LogLevel.Log;

        public enum LogLevel { Log, Warning }

        private CompositeDisposable _subscriptions;

        private void OnEnable()
        {
            _subscriptions = new CompositeDisposable();

            foreach (var variable in variables)
            {
                if (variable == null) continue;

                var captured = variable;
                captured.OnRaised
                    .Subscribe(_ => LogVariable(captured))
                    .AddTo(_subscriptions);
            }
        }

        private void OnDisable()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }

        private void LogVariable(ScriptableVariable variable)
        {
            string prefix = string.IsNullOrEmpty(logPrefix) ? "" : $"{logPrefix}_";
            string message = $"[{prefix}{variable.name}] = {variable.GetValue()}";

            switch (logLevel)
            {
                case LogLevel.Log:
                    Debug.Log(message, this);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(message, this);
                    break;
            }
        }
    }
}
