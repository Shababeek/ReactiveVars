using TMPro;
using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds an IntVariable to a TMP_Dropdown with two-way synchronization.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Dropdown Binder")]
    [RequireComponent(typeof(TMP_Dropdown))]
    public class DropdownBinder : TwoWayVariableBinder<IntVariable>
    {
        [SerializeField] private IntVariable dropdownValue;

        private TMP_Dropdown _dropdown;

        protected override IntVariable Variable => dropdownValue;

        private void Awake()
        {
            _dropdown = GetComponent<TMP_Dropdown>();
        }

        protected override void UpdateUIFromVariable()
        {
            _dropdown.value = dropdownValue.Value;
        }

        protected override void SubscribeToUI()
        {
            SubscribeUIEvent(_dropdown.onValueChanged, value =>
                GuardedUpdate(() => dropdownValue.Value = value));
        }
    }
}
