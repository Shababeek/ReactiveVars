using TMPro;
using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds a TextVariable to a TMP_InputField with two-way synchronization.
    /// </summary>
    [AddComponentMenu("Shababeek/ReactiveVars/Binders/InputField Binder")]
    [RequireComponent(typeof(TMP_InputField))]
    public class InputFieldBinder : TwoWayVariableBinder<TextVariable>
    {
        [SerializeField] private TextVariable textVariable;

        private TMP_InputField _inputField;

        protected override TextVariable Variable => textVariable;

        private void Awake()
        {
            _inputField = GetComponent<TMP_InputField>();
        }

        protected override void UpdateUIFromVariable()
        {
            _inputField.text = textVariable.Value;
        }

        protected override void SubscribeToUI()
        {
            SubscribeUIEvent(_inputField.onValueChanged, value =>
                GuardedUpdate(() => textVariable.Value = value));
        }
    }
}
