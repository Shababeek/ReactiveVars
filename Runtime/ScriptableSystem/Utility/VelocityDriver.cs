using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Reads a Rigidbody's velocity each frame and writes it to ScriptableVariables.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Drivers/Velocity Driver")]
    public class VelocityDriver : MonoBehaviour
    {
        [Tooltip("When true, updates variables without raising change events.")]
        [SerializeField] private bool silentUpdates;

        [Header("Output Variables")]
        [Tooltip("Optional Vector3Variable to receive the velocity vector.")]
        [SerializeField] private Vector3Variable velocityVariable;

        [Tooltip("Optional FloatVariable to receive the speed (velocity magnitude).")]
        [SerializeField] private FloatVariable speedVariable;

        [Tooltip("Optional Vector3Variable to receive the angular velocity.")]
        [SerializeField] private Vector3Variable angularVelocityVariable;

        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody == null)
                Debug.LogWarning($"VelocityDriver requires a Rigidbody on {gameObject.name}", this);
        }

        private void FixedUpdate()
        {
            if (_rigidbody == null) return;

            if (velocityVariable != null)
            {
                if (silentUpdates)
                    velocityVariable.SetValueWithoutNotify(_rigidbody.linearVelocity);
                else
                    velocityVariable.Value = _rigidbody.linearVelocity;
            }

            if (speedVariable != null)
            {
                float speed = _rigidbody.linearVelocity.magnitude;
                if (silentUpdates)
                    speedVariable.SetValueWithoutNotify(speed);
                else
                    speedVariable.Value = speed;
            }

            if (angularVelocityVariable != null)
            {
                if (silentUpdates)
                    angularVelocityVariable.SetValueWithoutNotify(_rigidbody.angularVelocity);
                else
                    angularVelocityVariable.Value = _rigidbody.angularVelocity;
            }
        }
    }
}
