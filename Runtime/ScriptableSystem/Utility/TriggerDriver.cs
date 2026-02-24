using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Sets a BoolVariable on trigger enter/exit, with optional tag filtering.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Drivers/Trigger Driver")]
    public class TriggerDriver : VariableDriver<BoolVariable>
    {
        [Tooltip("The BoolVariable to set on trigger enter/exit.")] [SerializeField]
        private BoolVariable variable;

        [Tooltip("Optional tag filter. Leave empty to detect any trigger.")] [SerializeField]
        private string filterTag = "";

        [Tooltip("Optional GameObjectVariable to store the other triggering GameObject.")] [SerializeField]
        private GameObjectVariable otherGameObject;

        protected override BoolVariable Variable => variable;

        private bool MatchesFilter(GameObject other)
        {
            return string.IsNullOrEmpty(filterTag) || other.CompareTag(filterTag);
        }

        private void SetVariable(bool value, GameObject other)
        {
            if (Variable == null) return;

            if (SilentUpdates)
                Variable.SetValueWithoutNotify(value);
            else
                Variable.Value = value;

            if (SilentUpdates)
                otherGameObject.SetValueWithoutNotify(value ? other : null);
            else
                otherGameObject.Value = value ? other : null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (MatchesFilter(other.gameObject))
                SetVariable(true, other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            if (MatchesFilter(other.gameObject))
                SetVariable(false, other.gameObject);
        }
    }
}