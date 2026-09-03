using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;

[assembly: InternalsVisibleTo("Shababeek.ReactiveVars.Editor")]
[assembly: InternalsVisibleTo("Shababeek.ReactiveVars.EditorTests")]

namespace Shababeek.Sequencing
{
    [Serializable]
    internal class StepNodePosition
    {
        public Step step;
        public Vector2 position;
    }

    /// <summary>
    /// A sequence that supports conditional branching between steps.
    /// Steps are connected via transitions that evaluate ScriptableVariable conditions.
    /// </summary>
    [CreateAssetMenu(menuName = "Shababeek/Sequencing/BranchingSequence")]
    public class BranchingSequence : SequenceNode
    {
        [HideInInspector]
        [SerializeField] internal List<StepNodePosition> nodePositions = new();

        [Tooltip("Audio pitch multiplier for the sequence (0.1 to 2.0).")]
        [SerializeField, Range(0.1f, 2)] internal float pitch = 1;

        [Tooltip("Audio volume level for the sequence (0 to 1).")]
        [SerializeField, Range(0, 1)] private float volume = .5f;

        [Tooltip("The first step to execute when the sequence begins.")]
        [SerializeField] private Step entryStep;

        [HideInInspector] [SerializeField] private List<Step> allSteps = new();
        [HideInInspector] [SerializeField] private List<StepTransitionGroup> transitionGroups = new();

        [Tooltip("When enabled, transitions are re-evaluated reactively whenever a watched variable changes mid-step.")]
        [SerializeField] private bool reactiveConditionMonitoring = false;

        [SerializeField, ReadOnly] private Step currentStep;
        private bool initialized;
        private bool _restoring;
        private readonly List<int> _executionPath = new();
        private Dictionary<Step, List<StepTransition>> _transitionCache;
        private CompositeDisposable _reactiveDisposable;

        internal override float SequencePitch => pitch;

        /// <summary>
        /// Gets whether the sequence has been started.
        /// </summary>
        public bool Started => status == SequenceStatus.Started;

        /// <summary>
        /// Gets the currently active step.
        /// </summary>
        public Step CurrentStep => currentStep;

        /// <summary>
        /// Gets all steps in the branching sequence.
        /// </summary>
        public List<Step> AllSteps => allSteps;

        /// <summary>
        /// Gets all transition groups in the branching sequence.
        /// </summary>
        public List<StepTransitionGroup> TransitionGroups => transitionGroups;

        /// <summary>
        /// Gets the entry step for this branching sequence.
        /// </summary>
        public Step EntryStep => entryStep;

        /// <summary>Gets the zero-based index of the current step in AllSteps.</summary>
        public int CurrentStepIndex => currentStep != null ? allSteps.IndexOf(currentStep) : -1;

        /// <summary>Gets the ordered list of step indices visited during execution.</summary>
        public IReadOnlyList<int> ExecutionPath => _executionPath;

        /// <summary>
        /// Gets or sets whether reactive condition monitoring is enabled.
        /// When true, transitions are re-evaluated whenever a watched variable changes mid-step.
        /// </summary>
        public bool ReactiveConditionMonitoring
        {
            get => reactiveConditionMonitoring;
            set => reactiveConditionMonitoring = value;
        }

        private void Awake()
        {
            initialized = false;
        }

        private void OnEnable()
        {
            Awake();
        }

        /// <summary>
        /// Begins the branching sequence at the entry step.
        /// </summary>
        public override void Begin()
        {
            Debug.Log($"Starting branching sequence '{name}'");

            DisposeReactiveSubscriptions();
            currentStep = null;
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

            BuildTransitionCache();

            foreach (var step in allSteps)
            {
                if (step == null) continue;
                step.audioObject = audioObject;
                step.Initialize(this);
            }

            _executionPath.Clear();

            if (entryStep == null)
            {
                Debug.LogError($"[BranchingSequence] '{name}' has no entry step assigned.");
                EndSequence();
                return;
            }

            RecordStep(entryStep);
            TransitionToStep(entryStep);
            Raise(SequenceStatus.Started);
        }

        /// <summary>
        /// Called when a child step completes. Evaluates transitions to determine the next step.
        /// </summary>
        internal override void CompleteStep(Step step)
        {
            if (currentStep != step)
            {
                Debug.LogWarning(
                    $"[BranchingSequence] Step '{step.name}' completed but is not the current step '{currentStep?.name}'.");
                return;
            }

            if (_restoring) return;

            EvaluateAndTransition();
        }

        /// <summary>
        /// Restores the sequence by replaying an execution path. The last index in the path becomes the active step.
        /// </summary>
        public void RestoreFromPath(IList<int> path)
        {
            if (allSteps == null || allSteps.Count == 0 || path == null || path.Count == 0) return;

            InitializeForRestore();

            _executionPath.Clear();
            // try/finally for the same reason Sequence.RestoreToStep documents: the replay invokes
            // each skipped step's onStarted/onCompleted UnityEvents, any of which can throw, and
            // these flags live on a ScriptableObject that survives leaving play mode. One throw
            // without this would wedge the sequence into a permanent restoring state.
            _restoring = true;
            BeginRestoreScope();
            try
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    int idx = Mathf.Clamp(path[i], 0, allSteps.Count - 1);
                    if (allSteps[idx] == null) continue;
                    _executionPath.Add(idx);
                    allSteps[idx].Begin();
                    allSteps[idx].CompleteStep();
                }
            }
            finally
            {
                EndRestoreScope();
                _restoring = false;
            }

            int targetIdx = Mathf.Clamp(path[path.Count - 1], 0, allSteps.Count - 1);
            _executionPath.Add(targetIdx);
            TransitionToStep(allSteps[targetIdx]);
            Raise(SequenceStatus.Started);
        }

        /// <summary>
        /// Restores the sequence directly to a specific step by index. Falls back to index-order replay when no path is available.
        /// </summary>
        public void RestoreToStep(int stepIndex)
        {
            if (allSteps == null || allSteps.Count == 0) return;
            stepIndex = Mathf.Clamp(stepIndex, 0, allSteps.Count - 1);

            InitializeForRestore();

            _executionPath.Clear();
            // try/finally for the same reason Sequence.RestoreToStep documents: the replay invokes
            // each skipped step's onStarted/onCompleted UnityEvents, any of which can throw, and
            // these flags live on a ScriptableObject that survives leaving play mode. One throw
            // without this would wedge the sequence into a permanent restoring state.
            _restoring = true;
            BeginRestoreScope();
            try
            {
                for (int i = 0; i < stepIndex; i++)
                {
                    if (allSteps[i] == null) continue;
                    _executionPath.Add(i);
                    allSteps[i].Begin();
                    allSteps[i].CompleteStep();
                }
            }
            finally
            {
                EndRestoreScope();
                _restoring = false;
            }

            _executionPath.Add(stepIndex);
            TransitionToStep(allSteps[stepIndex]);
            Raise(SequenceStatus.Started);
        }

        private void InitializeForRestore()
        {
            DisposeReactiveSubscriptions();
            currentStep = null;
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

            BuildTransitionCache();

            foreach (var step in allSteps)
            {
                if (step == null) continue;
                step.audioObject = audioObject;
                step.Initialize(this);
            }
        }

        private void TransitionToStep(Step nextStep)
        {
            DisposeReactiveSubscriptions();
            currentStep = nextStep;

            if (reactiveConditionMonitoring)
                SubscribeToCurrentStepVariables();

            currentStep.Begin();
        }

        private void EndSequence()
        {
            DisposeReactiveSubscriptions();
            currentStep = null;
            status = SequenceStatus.Completed;
            Raise(SequenceStatus.Completed);
        }

        private void EvaluateAndTransition()
        {
            if (!_transitionCache.TryGetValue(currentStep, out var transitions) || transitions.Count == 0)
            {
                EndSequence();
                return;
            }

            foreach (var transition in transitions)
            {
                if (!transition.Evaluate()) continue;

                if (transition.TargetStep == null)
                {
                    Debug.LogWarning(
                        $"[BranchingSequence] Transition from '{currentStep.name}' matched but has no target step.");
                    EndSequence();
                    return;
                }

                transition.TransitionEvent?.Raise();
                RecordStep(transition.TargetStep);
                TransitionToStep(transition.TargetStep);
                return;
            }

            // No transition matched
            Debug.LogWarning(
                $"[BranchingSequence] No matching transition from step '{currentStep.name}'. Ending sequence.");
            EndSequence();
        }

        #region Reactive Condition Monitoring

        private void SubscribeToCurrentStepVariables()
        {
            if (currentStep == null) return;
            if (!_transitionCache.TryGetValue(currentStep, out var transitions)) return;

            _reactiveDisposable = new CompositeDisposable();
            var watchedVariables = new HashSet<ScriptableVariable>();

            foreach (var transition in transitions)
            {
                var variable = transition.Condition?.Variable;
                if (variable == null) continue;
                if (!watchedVariables.Add(variable)) continue;

                variable.OnRaised
                    .Subscribe(_ => OnWatchedVariableChanged())
                    .AddTo(_reactiveDisposable);
            }
        }

        private void OnWatchedVariableChanged()
        {
            if (currentStep == null || status != SequenceStatus.Started) return;
            if (!_transitionCache.TryGetValue(currentStep, out var transitions)) return;

            foreach (var transition in transitions)
            {
                if (!transition.Evaluate()) continue;
                if (transition.TargetStep == null) continue;

                // Force-complete the current step and take this transition
                currentStep.CompleteStep();
                return;
            }
        }

        private void DisposeReactiveSubscriptions()
        {
            _reactiveDisposable?.Dispose();
            _reactiveDisposable = null;
        }

        #endregion

        private void RecordStep(Step step)
        {
            int idx = allSteps.IndexOf(step);
            if (idx >= 0) _executionPath.Add(idx);
        }

        private void BuildTransitionCache()
        {
            _transitionCache = new Dictionary<Step, List<StepTransition>>();

            foreach (var group in transitionGroups)
            {
                if (group?.fromStep == null) continue;

                if (!_transitionCache.ContainsKey(group.fromStep))
                    _transitionCache[group.fromStep] = new List<StepTransition>();

                foreach (var transition in group.transitions)
                {
                    if (transition != null)
                        _transitionCache[group.fromStep].Add(transition);
                }
            }
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
        /// Resets the sequence to its initial state.
        /// </summary>
        /// <summary>
        /// Resets the sequence to its initial state.
        /// </summary>
        public void Reset()
        {
            DisposeReactiveSubscriptions();
            foreach (var step in allSteps)
            {
                if (step != null)
                    step.Raise(SequenceStatus.Inactive);
            }

            currentStep = null;
            status = SequenceStatus.Inactive;
            initialized = false;
            _executionPath.Clear();
        }

        /// <summary>
        /// Initializes empty lists for a new branching sequence asset.
        /// </summary>
        public void Init()
        {
            allSteps = new List<Step>();
            transitionGroups = new List<StepTransitionGroup>();
        }

        internal Vector2 GetStepPosition(Step step)
        {
            if (nodePositions == null) return new Vector2(float.NaN, float.NaN);
            foreach (var np in nodePositions)
                if (np.step == step) return np.position;
            return new Vector2(float.NaN, float.NaN);
        }

        public void SetStepPosition(Step step, Vector2 position)
        {
            nodePositions ??= new List<StepNodePosition>();
            foreach (var np in nodePositions)
            {
                if (np.step != step) continue;
                np.position = position;
                return;
            }

            nodePositions.Add(new StepNodePosition { step = step, position = position });
        }
    }
}
