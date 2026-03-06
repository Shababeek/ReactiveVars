using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Sequencing
{
    /// <summary>Detects collider triggers and completes the step when a matching object enters.</summary>
    [AddComponentMenu(menuName : "Shababeek/Sequencing/Actions/TriggerAction")]
    public class TriggerAction : AbstractSequenceAction
    {
        [Tooltip("Tag to match on trigger (leave empty to match any object).")]
        [SerializeField] private string objectTag;

        [Tooltip("Event to invoke when the trigger condition is met.")]
        [SerializeField] private UnityEvent onTriggerEnter;

        private bool _active = false;



        private void OnTriggerEnter(Collider other)
        {
            if (!_active) return;
            if (string.IsNullOrEmpty(objectTag) || other.attachedRigidbody.CompareTag(objectTag))
            {
                _active = false;
                onTriggerEnter.Invoke();
                CompleteStep();
            }
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            _active = status == SequenceStatus.Started;
        }
    }
}