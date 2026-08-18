using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Shababeek.ReactiveVars;
using UnityEngine;

[assembly: InternalsVisibleTo("Shababeek.ReactiveVars.Editor")]
[assembly: InternalsVisibleTo("Shababeek.GoodMorning")]

namespace Shababeek.Sequencing
{
    /// <summary>
    /// Represents a sequence of steps that can be executed in order.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Sequencing/Sequence")]
    public class Sequence : SequenceNode
    {
        [Tooltip("Audio pitch multiplier for the sequence (0.1 to 2.0).")]
        [SerializeField, Range(0.1f, 2)] internal float pitch = 1;

        internal override float SequencePitch => pitch;

        [Tooltip("Audio volume level for the sequence (0 to 1).")]
        [SerializeField, Range(0, 1)] private float volume = .5f;

        [Tooltip("Optional FloatVariable that is automatically driven with sequence progress (0 to 1).")]
        [SerializeField] private FloatVariable progress;

        [HideInInspector] [SerializeField] private List<Step> steps;

        [SerializeField, ReadOnly] private int currentStepIndex;
        private bool initialized;
        private bool _restoring;
        /// <summary>
        /// Gets whether the sequence has been started.
        /// </summary>
        public bool Started => status == SequenceStatus.Started;

        /// <summary>Gets the zero-based index of the current step.</summary>
        public int CurrentStepIndex => currentStepIndex;
        
        /// <summary>
        /// Gets the current step being executed in the sequence.
        /// </summary>
        public Step CurrentStep => currentStepIndex < steps.Count ? steps[currentStepIndex] : null;

        /// <summary>
        /// Gets the list of all steps in the sequence.
        /// </summary>
        public List<Step> Steps => steps;

        /// <summary>
        /// Gets or sets the progress variable driven by this sequence.
        /// </summary>
        public FloatVariable Progress
        {
            get => progress;
            set => progress = value;
        }

        public void NextStep() => CurrentStep.CompleteStep();

        private void Awake()
        {
            initialized = false;
        }

        private void OnEnable()
        {
            Awake();
        }

        /// <summary>
        /// Begins the sequence execution by starting the first step.
        /// </summary>
        public override void Begin()
        {
            Debug.Log($"starting sequence{name}");
            currentStepIndex = 0;
            // Sequences are ScriptableObjects, so this state outlives play mode. Clear it on every
            // fresh start rather than trusting the previous session to have unwound cleanly.
            _restoring = false;

            status = SequenceStatus.Started;
            if (!initialized)
            {
                initialized = true;
                audioObject = new GameObject($"{name}_AudioObject").AddComponent<AudioSource>();
                audioObject.loop = false;
                audioObject.playOnAwake = false;
                audioObject.pitch = pitch;
                audioObject.volume = volume;
            }

            foreach (var step in steps)
            {
                step.audioObject = audioObject;
                step.Initialize(this);
            }

            UpdateProgress();
            steps[currentStepIndex].Begin();
            Raise(SequenceStatus.Started);
        }

        internal override void CompleteStep(Step step)
        {
            if (steps[currentStepIndex] != step) return;
            if (_restoring) return;

            currentStepIndex++;

            UpdateProgress();

            if (currentStepIndex < steps.Count)
            {
                steps[currentStepIndex].Begin();
                return;
            }

            status = SequenceStatus.Completed;
            Raise(SequenceStatus.Completed);
        }

        /// <summary>
        /// Restores the sequence to a specific step, rapid-firing through previous steps to trigger side effects.
        /// </summary>
        public void RestoreToStep(int targetStepIndex)
        {
            if (steps == null || steps.Count == 0) return;
            targetStepIndex = Mathf.Clamp(targetStepIndex, 0, steps.Count - 1);

            currentStepIndex = 0;
            status = SequenceStatus.Started;

            if (!initialized)
            {
                initialized = true;
                audioObject = new GameObject($"{name}_AudioObject").AddComponent<AudioSource>();
                audioObject.loop = false;
                audioObject.playOnAwake = false;
                audioObject.pitch = pitch;
                audioObject.volume = volume;
            }

            foreach (var step in steps)
            {
                step.audioObject = audioObject;
                step.Initialize(this);
            }

            // finally is required, not optional: the replay invokes each skipped step's
            // onStarted/onCompleted UnityEvents, any of which can throw. _restoring lives on a
            // ScriptableObject, so it survives leaving play mode — a single throw here would
            // wedge the flag on until the next domain reload and make CompleteStep a silent
            // no-op for the rest of the session.
            _restoring = true;
            try
            {
                for (int i = 0; i < targetStepIndex; i++)
                {
                    currentStepIndex = i;
                    steps[i].Begin();
                    steps[i].CompleteStep();
                }
            }
            finally
            {
                _restoring = false;
            }

            currentStepIndex = targetStepIndex;
            UpdateProgress();
            steps[currentStepIndex].Begin();
            Raise(SequenceStatus.Started);
        }

        /// <summary>
        /// Plays an audio clip using the sequence's audio source.
        /// </summary>
        public void PlayClip(AudioClip clip)
        {
            audioObject.Stop();
            audioObject.clip = clip;
            audioObject.Play();
        }

        /// <summary>
        /// Goes back to the previous step in the sequence.
        /// </summary>
        public void GoToPreviousStep()
        {
            if (currentStepIndex <= 0) return;
            steps[currentStepIndex].Raise(SequenceStatus.Inactive);
            currentStepIndex--;
            UpdateProgress();
            steps[currentStepIndex].Begin();
        }

        /// <summary>
        /// Resets the sequence to its initial state so it can be started again.
        /// </summary>
        public void Reset()
        {
            if (steps == null) return;
            foreach (var step in steps)
            {
                step.Initialize(this);
            }
            currentStepIndex = 0;
            status = SequenceStatus.Inactive;
            initialized = false;
            _restoring = false;
            if (progress != null) progress.Value = 0f;
        }

        /// <summary>
        /// Sets the current step index directly without firing any events or side effects.
        /// Used by the save system to skip completed steps during restore.
        /// </summary>
        internal void SetCurrentStepDirect(int index)
        {
            if (steps == null || steps.Count == 0) return;
            index = Mathf.Clamp(index, 0, steps.Count - 1);
            _restoring = false;

            if (!initialized)
            {
                initialized = true;
                audioObject = new GameObject($"{name}_AudioObject").AddComponent<AudioSource>();
                audioObject.loop = false;
                audioObject.playOnAwake = false;
                audioObject.pitch = pitch;
                audioObject.volume = volume;
            }

            foreach (var step in steps)
            {
                step.audioObject = audioObject;
                step.Initialize(this);
            }

            currentStepIndex = index;
            status = SequenceStatus.Started;
        }

        /// <summary>
        /// Initializes the sequence by creating an empty steps list.
        /// </summary>
        public void Init() => steps = new List<Step>();

        private void UpdateProgress()
        {
            if (progress == null || steps == null || steps.Count == 0) return;
            progress.Value = (float)currentStepIndex / steps.Count;
        }
    }
}
