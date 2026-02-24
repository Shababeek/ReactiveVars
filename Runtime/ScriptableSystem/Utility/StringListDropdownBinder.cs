using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Populates a TMP_Dropdown's options from a StringListVariable.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Binders/StringList Dropdown Binder")]
    [RequireComponent(typeof(TMP_Dropdown))]
    public class StringListDropdownBinder : VariableBinder<StringListVariable>
    {
        [SerializeField] private StringListVariable variable;

        [Header("Options")]
        [Tooltip("Whether to preserve the current selection index when the list updates.")]
        [SerializeField] private bool preserveSelection = true;

        [Tooltip("Default selection index when the list is populated (-1 for none).")]
        [SerializeField] private int defaultIndex = 0;

        private TMP_Dropdown _dropdown;

        protected override StringListVariable Variable => variable;

        private void Awake()
        {
            _dropdown = GetComponent<TMP_Dropdown>();
        }

        protected override void Bind()
        {
            PopulateDropdown(variable.Value);
        }

        protected override void OnVariableChanged()
        {
            int previousIndex = _dropdown.value;
            PopulateDropdown(variable.Value);

            if (preserveSelection && previousIndex < _dropdown.options.Count)
                _dropdown.value = previousIndex;
        }

        private void PopulateDropdown(List<string> items)
        {
            _dropdown.ClearOptions();

            if (items == null || items.Count == 0) return;

            _dropdown.AddOptions(items);

            if (defaultIndex >= 0 && defaultIndex < items.Count)
                _dropdown.value = defaultIndex;
        }

        /// <summary>Gets the currently selected text.</summary>
        public string SelectedText =>
            _dropdown.value >= 0 && _dropdown.value < _dropdown.options.Count
                ? _dropdown.options[_dropdown.value].text
                : null;

        /// <summary>Gets the currently selected index.</summary>
        public int SelectedIndex => _dropdown.value;

        /// <summary>Forces a refresh from the current variable value.</summary>
        public void Refresh()
        {
            if (variable != null && variable.Value != null)
                PopulateDropdown(variable.Value);
        }
    }
}
