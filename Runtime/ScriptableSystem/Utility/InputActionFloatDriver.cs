#if REACTIVE_VARS_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Reads a float value from an InputAction each frame and writes it to a FloatVariable.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Drivers/Input Action Float Driver")]
    public class InputActionFloatDriver : VariableDriver<FloatVariable>
    {
        [Tooltip("The FloatVariable to receive the input value.")]
        [SerializeField] private FloatVariable variable;

        [Tooltip("The InputAction to read float values from.")]
        [SerializeField] private InputActionReference inputAction;

        protected override FloatVariable Variable => variable;

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

            float value = _action.ReadValue<float>();
            if (SilentUpdates)
                Variable.SetValueWithoutNotify(value);
            else
                Variable.Value = value;
        }
    }
}
#endif
