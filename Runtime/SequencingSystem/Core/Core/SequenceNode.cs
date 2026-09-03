using System;
using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Represents the status of a sequence node in the sequencing system.
    /// </summary>
    public enum SequenceStatus
    {
        Inactive,
        Started,
        Completed
    }

    /// <summary>
    /// Base class for sequence nodes in the sequencing system.
    /// This class provides the structure for sequence nodes like steps and Sequences
    /// </summary>
    public abstract class SequenceNode : ScriptableObject
    {
        [SerializeField] protected SequenceStatus status = SequenceStatus.Inactive;
        [SerializeField, ReadOnly] internal AudioSource audioObject;

        private new readonly Subject<SequenceStatus> _onRaised = new();

        private static int _restoreDepth;

        /// <summary>
        /// True while a sequence is replaying earlier steps to restore saved progress.
        /// </summary>
        public static bool IsRestoring => _restoreDepth > 0;

        // These ScriptableObjects outlive play mode, so a counter left above zero by an exception
        // would silence every sequence for the rest of the editor session. Callers pair these in
        // try/finally, and this resets the count on entering play mode regardless.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRestoreDepth() => _restoreDepth = 0;

        /// <summary>Marks the start of a restore replay. Always pair with <see cref="EndRestoreScope"/> in a finally.</summary>
        protected static void BeginRestoreScope() => _restoreDepth++;

        /// <summary>Marks the end of a restore replay.</summary>
        protected static void EndRestoreScope() => _restoreDepth = _restoreDepth > 0 ? _restoreDepth - 1 : 0;

        /// <summary>
        /// Observable stream for when this event is raised with data.
        /// </summary>
        /// <remarks>
        /// Subscribe to this observable to receive notifications when the event is raised.
        /// This provides a reactive programming approach to event handling.
        /// </remarks>
        public IObservable<SequenceStatus> OnRaised => _onRaised;
        
        
        /// <summary>
        /// Begins execution of this sequence node.
        /// </summary>
        public abstract void Begin();

        /// <summary>
        /// Gets the audio pitch for this sequence node.
        /// </summary>
        internal virtual float SequencePitch => 1f;

        /// <summary>
        /// Called when a child step completes execution.
        /// </summary>
        internal virtual void CompleteStep(Step step) { }

        
        /// <summary>
        /// Raises the event with the provided data.
        /// </summary>
        /// <param name="data">The data to raise the event with</param>
        /// <remarks>
        /// This method raises both the base event and the data event with the specified value.
        /// Any errors during the raise process are logged but do not prevent the event from firing.
        /// </remarks>
        public void Raise(SequenceStatus data)
        {
            // A replayed step is not a step the player reached. Suppressing the notification here
            // rather than in each handler means every listener in the project gets this right by
            // default, instead of each one having to invent its own way to tell a restore apart
            // from real progress — which is a distinction none of them can actually make.
            if (IsRestoring) return;

            try
            { 
                _onRaised.OnNext(data);
            }
            catch (Exception e)
            {   
                Debug.LogError($"Error raising GameEvent: {e}");
            }
        }
    }
}