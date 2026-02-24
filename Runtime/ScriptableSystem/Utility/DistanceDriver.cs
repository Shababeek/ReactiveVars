using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Writes the distance between two transforms to a FloatVariable each frame.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Drivers/Distance Driver")]
    public class DistanceDriver : VariableDriver<FloatVariable>
    {
        [Tooltip("The FloatVariable to receive the distance value.")]
        [SerializeField] private FloatVariable variable;

        [Tooltip("The first transform (defaults to this GameObject if not set).")]
        [SerializeField] private Transform targetA;

        [Tooltip("The second transform to measure distance to.")]
        [SerializeField] private Transform targetB;

        [Tooltip("Use squared distance for better performance when exact distance isn't needed.")]
        [SerializeField] private bool useSquaredDistance;

        protected override FloatVariable Variable => variable;

        protected override void OnEnable()
        {
            if (targetA == null)
                targetA = transform;

            base.OnEnable();

            if (targetB == null)
                Debug.LogWarning($"TargetB is not assigned on {gameObject.name}", this);
        }

        private void Update()
        {
            if (Variable == null || targetA == null || targetB == null) return;

            float dist = useSquaredDistance
                ? (targetA.position - targetB.position).sqrMagnitude
                : Vector3.Distance(targetA.position, targetB.position);

            if (SilentUpdates)
                Variable.SetValueWithoutNotify(dist);
            else
                Variable.Value = dist;
        }
    }
}
