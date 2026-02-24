#if REACTIVE_VARS_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Reads a Vector2 value from an InputAction each frame and writes it to a Vector2Variable.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Drivers/Input Action Vector2 Driver")]
    public class InputActionVector2Driver : VariableDriver<Vector2Variable>
    {
        [Tooltip("The Vector2Variable to receive the input value.")]
        [SerializeField] private Vector2Variable variable;

        [Tooltip("The InputAction to read Vector2 values from.")]
        [SerializeField] private InputActionReference inputAction;

        protected override Vector2Variable Variable => variable;

        private InputAction _action;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (inputAction == null)
            {
                Debug.LogWarning($"InputAction is not assigned on {gameObject.name}", this);
                return;
            }

            _action = inputAction.action;
            _action?.Enable();
        }

        private void OnDisable()
        {
            _action?.Disable();
        }

        private void Update()
        {
            if (_action == null || Variable == null) return;

            Vector2 value = _action.ReadValue<Vector2>();
            if (SilentUpdates)
                Variable.SetValueWithoutNotify(value);
            else
                Variable.Value = value;
        }
    }
}
#endif
