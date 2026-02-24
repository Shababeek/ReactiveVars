using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Sets a BoolVariable on collision enter/exit, with optional tag filtering.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Drivers/Collision Driver")]
    public class CollisionDriver : VariableDriver<BoolVariable>
    {
        [Tooltip("The BoolVariable to set on collision enter/exit.")]
        [SerializeField] private BoolVariable variable;

        [Tooltip("Optional tag filter. Leave empty to detect any collision.")]
        [SerializeField] private string filterTag = "";

        [Tooltip("Optional GameObjectVariable to store the other colliding GameObject.")]
        [SerializeField] private GameObjectVariable otherGameObject;

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

        private void OnCollisionEnter(Collision collision)
        {
            if (MatchesFilter(collision.gameObject))
                SetVariable(true, collision.gameObject);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (MatchesFilter(collision.gameObject))
                SetVariable(false, collision.gameObject);
        }
    }
}
