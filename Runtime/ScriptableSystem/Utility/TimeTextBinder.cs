using System;
using TMPro;
using UnityEngine;

namespace Shababeek.ReactiveVars
{
    [AddComponentMenu("ReactiveVars/Binders/Time Text Binder")]
    [RequireComponent(typeof(TMP_Text))]
    public class TimeTextBinder : NumericalVariableBinder
    {
        [SerializeField] private ScriptableVariable variable;

        [Tooltip("TimeSpan format string.\n{0:m\\:ss} → 3:25\n{0:mm\\:ss} → 03:25\n{0:h\\:mm\\:ss} → 1:03:25")]
        [SerializeField] private string format = @"{0:m\:ss}";

        private TMP_Text _text;

        protected override ScriptableVariable Variable => variable;

        private void Awake() => _text = GetComponent<TMP_Text>();

        protected override void BindNumerical() => Refresh();
        protected override void OnNumericalValueChanged() => Refresh();

        private void Refresh()
        {
            _text.text = string.Format(format, TimeSpan.FromSeconds(NumericalVariable.AsFloat));
        }
    }
}
