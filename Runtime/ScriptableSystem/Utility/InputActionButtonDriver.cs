#if REACTIVE_VARS_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Reads button press state from an InputAction and writes it to a BoolVariable.
    /// Sets true on performed, false on canceled.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Drivers/Input Action Button Driver")]
    public class InputActionButtonDriver : VariableDriver<BoolVariable>
    {
        [Tooltip("The BoolVariable to set on button press/release.")]
        [SerializeField] private BoolVariable variable;

        [Tooltip("The InputAction to listen for button events.")]
        [SerializeField] private InputActionReference inputAction;

        protected override BoolVariable Variable => variable;

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
            if (_action == null) return;

            _action.performed += OnPerformed;
            _action.canceled += OnCanceled;
            _action.Enable();
        }

        private void OnDisable()
        {
            if (_action == null) return;
            _action.performed -= OnPerformed;
            _action.canceled -= OnCanceled;
            _action.Disable();
        }

        private void OnPerformed(InputAction.CallbackContext ctx)
        {
            if (Variable == null) return;
            Variable.Value = true;
        }

        private void OnCanceled(InputAction.CallbackContext ctx)
        {
            if (Variable == null) return;
            Variable.Value = false;
        }
    }
}
#endif
