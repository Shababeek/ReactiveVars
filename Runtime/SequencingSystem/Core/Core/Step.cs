using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Represents a step in a sequence of actions, which can be started and completed.
    /// </summary>
    /// <remarks>
    /// This ScriptableObject can only be created in a sequence, it should not be created manually.
    /// </remarks>
    [Serializable]
    public class Step : SequenceNode
    {
        [Tooltip("The audio clip to play when this step starts.")]
        [SerializeField] private AudioClip audioClip;
        
        [Tooltip("Enable to allow the step to be completed before it starts.")]
        [SerializeField] private bool canBeFinishedBeforeStarted;
        
        [Tooltip("When enabled, the step automatically completes when the audio finishes playing.")]
        [SerializeField] private bool audioOnly;
        
        [Tooltip("Delay in seconds before starting the audio playback.")]
        [SerializeField] private float audioDelay = .1f;
        
        [Tooltip("Unity event raised when the step starts.")]
        [SerializeField] private UnityEvent onStarted;
        
        [Tooltip("Unity event raised when the step completes.")]
        [SerializeField] private UnityEvent onCompleted;
        
        [Tooltip("When enabled, overrides the sequence's default pitch with a custom value.")]
        [SerializeField] private bool overridePitch = false;
        
        [Tooltip("Custom pitch for this step's audio (0.1 to 2.0).")]
        [SerializeField] [Range(0.1f, 2)] private float pitch;
        private SequenceNode _parentSequence;
        private bool _finished = false;
        private CancellationTokenSource _audioCts;

        /// <summary>
        /// Gets or sets the current status of the step.
        /// </summary>
        public SequenceStatus StepStatus
        {
            get => status;
            protected set
            {
                if (value == status) return;
                status = value;
                if (value != SequenceStatus.Inactive) Raise(value);
            }
        }

        /// <summary>
        /// Begins the step execution by starting audio and raising events.
        /// </summary>
        public override void Begin()
        {
            if (overridePitch) audioObject.pitch = pitch;
            StepStatus = SequenceStatus.Started;
            onStarted.Invoke();
            CheckAudioCompletion();
            if (_finished) CompleteStep();
        }

        /// <summary>
        /// Completes the step and moves to the next step in the sequence.
        /// </summary>
        public void CompleteStep()
        {
            if (status == SequenceStatus.Started)
            {
                CancelAudio();
                onCompleted.Invoke();
                Complete();
            }
            else if (canBeFinishedBeforeStarted)
            {
                _finished = true;
            }
        }

        /// <summary>
        /// Initializes the step with its parent sequence node.
        /// </summary>
        public void Initialize(SequenceNode parent)
        {
            CancelAudio();
            _finished = false;
            status = SequenceStatus.Inactive;
            _parentSequence = parent;
        }

        private async void CheckAudioCompletion()
        {
            audioObject.Stop();
            if (audioClip is null) return;

            _audioCts = new CancellationTokenSource();
            var token = _audioCts.Token;

            try
            {
                await Awaitable.WaitForSecondsAsync(audioDelay, token);
                audioObject.clip = audioClip;
                audioObject.Play();

                if (!audioOnly) return;

                // Wait one frame for the audio source to register as playing
                await Awaitable.NextFrameAsync(token);

                while (audioObject.isPlaying)
                {
                    await Awaitable.NextFrameAsync(token);
                }

                CompleteStep();
            }
            catch (OperationCanceledException)
            {
                // Step was completed or reset while audio was playing — expected
            }
        }

        private void CancelAudio()
        {
            if (_audioCts == null) return;
            _audioCts.Cancel();
            _audioCts.Dispose();
            _audioCts = null;
        }

        protected override SequenceStatus DefaultValue => status;

        private void Complete()
        {
            audioObject.pitch = _parentSequence.SequencePitch;
            StepStatus = SequenceStatus.Completed;
            _parentSequence.CompleteStep(this);
        }
    }
}
