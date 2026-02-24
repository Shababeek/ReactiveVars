using UnityEngine;
using UnityEngine.AI;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds a numeric variable to a NavMeshAgent's speed or stopping distance.
    /// </summary>
    /// <remarks>Requires the AI Navigation module.</remarks>
    [AddComponentMenu("Shababeek/Scriptable System/Numerical NavMesh Binder")]
    [RequireComponent(typeof(NavMeshAgent))]
    public class NumericalNavMeshBinder : NumericalVariableBinder
    {
        [SerializeField] private ScriptableVariable variable;

        [Header("Target Property")]
        [SerializeField] private NavMeshProperty targetProperty = NavMeshProperty.Speed;

        [Header("Value Mapping")]
        [SerializeField] private float minValue = 0f;
        [SerializeField] private float maxValue = 1f;

        [Tooltip("Agent property value when variable equals minValue.")]
        [SerializeField] private float minOutput = 0f;

        [Tooltip("Agent property value when variable equals maxValue.")]
        [SerializeField] private float maxOutput = 5f;

        private NavMeshAgent _agent;

        protected override ScriptableVariable Variable => variable;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        protected override void BindNumerical()
        {
            UpdateAgentProperty(NumericalVariable.AsFloat);
        }

        protected override void OnNumericalValueChanged()
        {
            UpdateAgentProperty(NumericalVariable.AsFloat);
        }

        private void UpdateAgentProperty(float value)
        {
            if (_agent == null || !_agent.isOnNavMesh) return;

            float t = Mathf.InverseLerp(minValue, maxValue, value);
            float output = Mathf.Lerp(minOutput, maxOutput, t);

            switch (targetProperty)
            {
                case NavMeshProperty.Speed:
                    _agent.speed = Mathf.Max(0f, output);
                    break;
                case NavMeshProperty.AngularSpeed:
                    _agent.angularSpeed = Mathf.Max(0f, output);
                    break;
                case NavMeshProperty.Acceleration:
                    _agent.acceleration = Mathf.Max(0.01f, output);
                    break;
                case NavMeshProperty.StoppingDistance:
                    _agent.stoppingDistance = Mathf.Max(0f, output);
                    break;
            }
        }

        /// <summary>Forces a refresh from the current variable value.</summary>
        public void Refresh()
        {
            if (NumericalVariable != null)
                UpdateAgentProperty(NumericalVariable.AsFloat);
        }

        /// <summary>Defines which NavMeshAgent property to control.</summary>
        public enum NavMeshProperty
        {
            Speed,
            AngularSpeed,
            Acceleration,
            StoppingDistance
        }
    }
}
