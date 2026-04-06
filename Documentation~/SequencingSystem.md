# Sequencing System — Tutorials and Guided Workflows

**Navigation:** [Documentation home](README.md) · **Previous:** [VariableContainer.md](VariableContainer.md) · **Next:** [FAQ.md](FAQ.md)

> **Quick Reference**
> **Create Sequence:** Right-click > Create > Shababeek > Sequencing > Sequence
> **Create Branching Sequence:** Right-click > Create > Shababeek > Sequencing > Branching Sequence
> **Behaviour Component:** Add Component > SequenceBehaviour / BranchingSequenceBehaviour
> **Use For:** Step-by-step tutorials, training sequences, guided experiences, cutscenes, onboarding flows

---

## What It Does

The **Sequencing System** lets you build ordered flows — things that happen in a specific order, one step at a time. You define a **Sequence** as a project asset (a list of steps), then drop a **Behaviour** component into your scene to run it. Each step can play audio, fire events, and wait for a condition to be met before moving on.

**Perfect for:**
- Tutorials and onboarding
- Training simulations
- Guided assembly tasks
- Interactive storytelling and cutscenes
- Step-by-step procedures

---

## Core Concepts

*Inspector screenshots for this guide were removed so the docs stay usable without image binaries; you can add figures under `Documentation~/images/` in your fork.*

### Sequence
A **Sequence** is a ScriptableObject asset that contains an ordered list of steps. Sequences execute steps one at a time, in order. When the last step completes, the sequence is done.

### Step
A **Step** is one moment within a sequence — "show this UI panel," "wait for the player to walk here," "play this voiceover." Each step can play audio when it starts, fire Unity events, and wait for a completion condition before the sequence advances.

### Action
An **Action** is a scene component that decides **when a step is done**. Without an action, you'd complete a step manually (via code or a UnityEvent calling `CompleteStep()`). Actions automate this — a `ProximityAction` completes the step when the player gets close enough, an `AnimationAction` completes it when an animation finishes, etc.

### Branching Sequence
A **Branching Sequence** adds conditional transitions between steps. Instead of always going from step 1 to step 2, you can define conditions: "if score > 10, go to step 3; otherwise go to step 5." Useful for dialogue trees, adaptive tutorials, or any flow where the path depends on game state.

---

## Quick Example

> **Goal:** Create a "Pick up the tool" tutorial step

```
Sequence: "Tool Tutorial"
├── Step 1: "Welcome"        (EventAction - auto-complete after 2s)
├── Step 2: "Walk to table"  (ProximityAction - within 2m of table)
├── Step 3: "Pick up tool"   (TriggerAction - object enters trigger)
└── Step 4: "Well done!"     (Audio Only - completes when voiceover ends)
```

---

## Creating a Linear Sequence

### Step 1: Create the Asset

1. Right-click in the Project window
2. Select **Create > Shababeek > Sequencing > Sequence**
3. Name it something descriptive (e.g., "TutorialSequence")


**Sequence Settings:**

| Setting | What It Does |
|---|---|
| **Pitch** | Audio pitch multiplier for all steps (0.1–2.0) |
| **Volume** | Audio volume level for all steps (0–1) |

### Step 2: Add Steps

Select the Sequence asset. In the Inspector, use the **Add Step** button to create steps. You can reorder them by dragging and rename them by clicking the name field.


### Step 3: Place It in the Scene

1. Create an empty GameObject in your scene
2. **Add Component > SequenceBehaviour**
3. Assign your Sequence asset to the **Sequence** field


**SequenceBehaviour Settings:**

| Setting | What It Does |
|---|---|
| **Sequence** | The sequence asset to execute |
| **Start On Awake** | Start automatically when the scene loads |
| **Delay** | Seconds to wait before starting (even with Start On Awake) |
| **On Sequence Started** | UnityEvent fired when the whole sequence begins |
| **On Sequence Completed** | UnityEvent fired when the last step finishes |
| **Enable Debug Controls** | Lets you press keyboard keys to skip or rewind steps during play mode |
| **Next Step Key** | Key to skip forward (default: N) |
| **Previous Step Key** | Key to go back (default: P) |
| **Enable Analytics** | Logs how long the sequence took from first step to last |

### Step 4: Wire Up Step Logic

For each step, you have two options:

**Option A — UnityEvents on the Step:** Use the step's **On Started** event to trigger scene logic directly. Call `CompleteStep()` from a UnityEvent or code when you want to advance.

**Option B — Actions:** Add an Action component to a scene GameObject, assign the step it should listen to, and let the action handle completion automatically. See the [Action Types](#action-types) section.


### Shortcut: Create In Scene

The Sequence Inspector has a **"Create Sequence in Scene"** button that auto-generates a GameObject with a `SequenceBehaviour` and a `StepEventListener` pre-wired with all your steps.


---

## Step Configuration

Each step has these settings:


### Audio Settings

| Setting | What It Does |
|---|---|
| **Audio Clip** | Audio to play when this step starts (voiceover, sound effect, etc.) |
| **Audio Delay** | Seconds to wait before playing audio (default: 0.1s) |
| **Audio Only** | If checked, the step auto-completes when the audio finishes. Great for narration steps |
| **Override Pitch** | Use a custom pitch for this step instead of the sequence's default |
| **Pitch** | Custom pitch value (0.1–2.0) |

### Behavior Settings

| Setting | What It Does |
|---|---|
| **Can Be Finished Before Started** | Allows a completion signal to arrive before the step has begun. The step will complete immediately when it starts. Useful when player actions can happen out of order |

### Events

| Event | When It Fires |
|---|---|
| **On Started** | When the step begins — wire up any scene logic here (show UI, enable objects, etc.) |
| **On Completed** | When the step finishes |

---

## Action Types

Actions are MonoBehaviour components that listen to a step and control when it completes. Attach them to GameObjects in the scene and assign which step they belong to.

### AnimationAction

Triggers an animation and optionally auto-completes when it finishes.


| Setting | What It Does |
|---|---|
| **Step** | The step this action listens to |
| **Animator** | The Animator component to control |
| **Animation Trigger Name** | The trigger parameter to set when the step starts |
| **Auto Complete On Animation End** | If checked, completes the step when the animation state finishes |
| **Animation Layer** | Which animator layer to monitor (default: 0) |

**Use Case:** "Wait for the character's wave animation to end"

---

### EventAction

Fires UnityEvents and optionally auto-completes after a delay.


| Setting | What It Does |
|---|---|
| **Step** | The step this action listens to |
| **On Step Started** | UnityEvent fired when the step begins |
| **On Step Completed** | UnityEvent fired when the step completes |
| **Auto Complete** | If checked, completes the step automatically after a delay |
| **Auto Complete Delay** | Seconds to wait before auto-completing |

**Use Case:** "Show a UI panel, then auto-advance after 3 seconds"

---

### TriggerAction

Completes the step when a physics trigger collider is entered.


| Setting | What It Does |
|---|---|
| **Step** | The step this action listens to |
| **Object Tag** | Optional tag filter — only objects with this tag trigger completion. Leave empty to accept any object |
| **On Trigger Enter** | UnityEvent fired when the trigger is entered |

**Use Case:** "Place the item in the container"

---

### ProximityAction

Completes the step based on distance between two transforms.


| Setting | What It Does |
|---|---|
| **Step** | The step this action listens to |
| **Target** | The transform to track (usually the player) |
| **Reference Point** | The point to measure distance from |
| **Proximity Distance** | How close the target must be (in units) |
| **Condition** | When to complete: **Enter** (comes within range), **Exit** (leaves range), or **StayDuration** (stays in range for a set time) |
| **Required Stay Duration** | For StayDuration mode — how many seconds the target must stay within range |

**Use Case:** "Step completes when player is within 2m of the NPC for 3 seconds"

---

### MultiConditionAction

Combines multiple other actions as conditions. Completes when enough of them are met.


| Setting | What It Does |
|---|---|
| **Step** | The step this action listens to |
| **Mode** | **All** (every condition must be met), **Any** (at least one), or **Count** (a specific number) |
| **Required Count** | For Count mode — how many conditions must be met |
| **Conditions** | List of other AbstractSequenceAction components to monitor |
| **Auto Find Child Actions** | If checked, automatically discovers action components on child GameObjects |

**Use Case:** "Step completes when the player has done 2 of 3 tasks"

---

### SequenceControlAction

Starts or waits for another sequence from within a step.


| Setting | What It Does |
|---|---|
| **Step** | The step this action listens to |
| **Target Sequence** | The other sequence to control |
| **Operation** | **StartAndWait** (start target, complete this step when target finishes), **StartOnly** (start target, complete this step immediately), **WaitForCompletion** (wait for an already-running target to finish), **WaitForStep** (wait for a specific step index in the target) |
| **Target Step Index** | For WaitForStep — which step index to wait for |

**Use Case:** "Play a sub-sequence, then continue when it finishes"

---

### Writing Custom Actions

Extend `AbstractSequenceAction` to create your own completion logic:

```csharp
public class MyCustomAction : AbstractSequenceAction
{
    [SerializeField] private float customValue;

    protected override void OnStepStatusChanged(SequenceStatus status)
    {
        if (status == SequenceStatus.Started)
        {
            // Your logic here
            // Call CompleteStep() when your condition is met
        }
    }
}
```

The base class handles subscribing to the step, managing disposables, and cleanup. You just implement `OnStepStatusChanged` and call `CompleteStep()` when ready.

---

## Event Listeners

### StepEventListener

A bridge between steps and UnityEvents. Add this to a GameObject, assign steps, and wire up **On Step Started** / **On Step Completed** events for each one — no code needed.

The **"Create Sequence in Scene"** button on the Sequence Inspector creates one of these automatically with all steps pre-wired.


| Setting | What It Does |
|---|---|
| **Step List** | Steps to monitor with their associated UnityEvents |

**Use Case:** Show UI elements when specific steps start, hide them when steps complete.

### MultiStepListener

Listens to multiple steps at once and fires when **any** of them start or complete.

| Setting | What It Does |
|---|---|
| **Steps** | Array of steps to monitor |
| **On Started** | UnityEvent when any monitored step starts |
| **On Ended** | UnityEvent when any monitored step completes |

Has a `Current` property that returns true if any monitored step is currently active.

### AudioPlayerInSequence

Utility component for playing additional audio clips during a sequence, separate from the step's built-in audio.

---

## Branching Sequences

### Creating a Branching Sequence

1. Right-click in Project > **Create > Shababeek > Sequencing > Branching Sequence**
2. Double-click the asset (or click **Open Graph View** in the Inspector) to open the visual graph editor
3. Add steps as nodes, position them visually, and draw transitions between them


**Branching Sequence Settings:**

| Setting | What It Does |
|---|---|
| **Pitch** | Audio pitch multiplier (0.1–2.0) |
| **Volume** | Audio volume level (0–1) |
| **Entry Step** | The first step to execute |
| **Steps** | All steps in the branching sequence |

### Transitions

Each step can have multiple outgoing transitions, evaluated top-to-bottom. The first matching condition wins. If no transition matches, the sequence ends.

| Setting | What It Does |
|---|---|
| **Label** | Descriptive name shown in the editor and graph |
| **Condition Variable** | ReactiveVars variable to evaluate (leave empty for an unconditional/always-true transition) |
| **Comparison** | Equals, NotEquals, GreaterThan, LessThan, GreaterOrEqual, LessOrEqual |
| **Target Value** | The value to compare against (type depends on the variable: bool, int, float, or string) |
| **Target Step** | The step to transition to (leave empty to end the sequence) |
| **Transition Event** | Optional GameEvent raised when this transition is taken |

### Supported Variable Types

| Type | Available Comparisons |
|---|---|
| **BoolVariable** | Equals, NotEquals |
| **IntVariable** | All 6 comparison operators |
| **FloatVariable** | All 6 comparison operators |
| **TextVariable** | Equals, NotEquals |

### Running in Scene

Add a **BranchingSequenceBehaviour** component and assign the asset. Works the same way as `SequenceBehaviour` — Start On Awake, delay, events, etc.

---

## Graph View

The Branching Sequence Graph View provides a visual node-based editor for BranchingSequence assets.


### Visual Elements

| Element | What It Means |
|---|---|
| **Step Nodes** | Draggable nodes with input/output ports |
| **Entry Badge** | Green title bar with "ENTRY" badge on the entry step |
| **Audio Info** | Audio clip name displayed on nodes that have audio |
| **Green Edges** | Unconditional (default) transitions |
| **Blue Edges** | Conditional transitions |
| **Edge Tooltips** | Hover over a transition to see its condition details |

### Toolbar

| Button | What It Does |
|---|---|
| **Sequence Name** | Click to select the asset in the Project window |
| **Frame All** | Fit all nodes into view |
| **Auto Layout** | BFS-based layered arrangement of nodes |
| **Refresh** | Rebuild the graph from asset data |

### Transition Detail Panel

Select a transition edge to reveal an editable detail panel on the right side of the graph. You can edit the label, condition variable, comparison, target value, target step, and event directly from the graph.


### Runtime Visualization

During Play mode the graph provides live feedback:


| Visual | What It Means |
|---|---|
| **Yellow border** | Currently executing step |
| **Green edge** | Transition condition is currently met |
| **Red edge** | Transition condition is not met |
| **Panel status** | Shows checkmark or cross for selected transition |

### Interactions

| Action | What It Does |
|---|---|
| **Drag node** | Reposition (saved to asset) |
| **Drag port-to-port** | Create a new transition |
| **Select edge + Delete** | Remove a transition |
| **Double-click node** | Ping and select the Step asset |
| **Scroll wheel** | Zoom |
| **Middle-mouse drag** | Pan |

---

## Audio

Each Sequence creates a single AudioSource when it starts. All steps in that sequence share this AudioSource. Step audio settings (clip, delay, pitch override) control what plays through it.

The Sequence asset itself has **Pitch** and **Volume** settings that apply to all steps by default. Individual steps can override pitch with the **Override Pitch** toggle.

---

## Common Patterns

### Tutorial Flow
```
Step 1: EventAction (auto-complete 2s)     - "Welcome to the tutorial"
Step 2: ProximityAction (enter, 2m)        - "Walk to the control panel"
Step 3: TriggerAction (tag: "Tool")        - "Pick up the tool"
Step 4: AnimationAction (auto-complete)    - "Watch the demonstration"
Step 5: Audio Only                         - "Great job! Tutorial complete"
```

### Branching Dialogue
```
Step 1: "NPC greeting" → Audio Only
  ├── Transition: choiceVar == 1 → Step 2a ("Friendly response")
  └── Transition: choiceVar == 2 → Step 2b ("Hostile response")
Step 2a → unconditional → Step 3
Step 2b → unconditional → Step 3
Step 3: "Conversation continues..."
```

### Multi-Object Assembly
```
Step 1: TriggerAction (Part A enters slot 1)
Step 2: TriggerAction (Part B enters slot 2)
Step 3: TriggerAction (Part C enters slot 3)
Step 4: Audio Only - "Assembly complete!"
```

### Nested Sequences
```
Main Sequence:
├── Step 1: SequenceControlAction (StartAndWait → "Intro Sequence")
├── Step 2: ProximityAction - "Walk to station"
├── Step 3: SequenceControlAction (StartAndWait → "Station Tutorial")
└── Step 4: Audio Only - "All done!"
```

---

## Best Practices

**One Sequence Per Flow** — Keep sequences focused on a single tutorial, cutscene, or procedure. Use `SequenceControlAction` to chain them together.

**Meaningful Names** — Name sequences and steps descriptively. "ToolPickupTutorial" and "Step_WalkToTable" are much easier to maintain than "Sequence1" and "Step3."

**Clear Audio** — Use clear, concise audio instructions. Set appropriate delays so audio doesn't overlap with visual cues.

**Step Granularity** — Break complex tasks into smaller steps. Each step should have one clear goal the player can understand.

**Visual Feedback** — Combine step events with UI feedback (highlights, arrows, particle effects) using the **On Started** and **On Completed** UnityEvents.

**Fallback Completion** — For steps where the player might get stuck, consider adding an `EventAction` with auto-complete as a timeout fallback, or enable **Debug Controls** during development for easy skipping.

---

## Troubleshooting

| Problem | Solution |
|---|---|
| Step won't complete | Check that the action's conditions are achievable and its references are assigned |
| Audio not playing | Verify the AudioClip is assigned and the sequence's Volume is above 0 |
| Actions not triggering | Verify the action's Step field is assigned and the GameObject is active |
| Sequence not starting | Check **Start On Awake** is enabled, or call `StartSequence()` manually |
| Steps fire out of order | Make sure only one action is completing each step — check for duplicate actions |
| Debug keys not working | Enable **Enable Debug Controls** on the SequenceBehaviour and ensure Input System is installed |

---

## Scripting API

<details>
<summary><strong>Code Reference</strong></summary>

### Starting a Sequence
```csharp
SequenceBehaviour behaviour = GetComponent<SequenceBehaviour>();
behaviour.StartSequence();
```

### Subscribing to Events
```csharp
sequence.OnRaisedData
    .Where(status => status == SequenceStatus.Completed)
    .Subscribe(_ => Debug.Log("Sequence completed"))
    .AddTo(this);
```

### Manually Completing a Step
```csharp
step.CompleteStep();
```

### Sequence API

| Member | Description |
|---|---|
| `void Begin()` | Start the sequence from the first step |
| `void GoToPreviousStep()` | Move back one step |
| `void Reset()` | Reset all steps to Inactive so the sequence can be restarted |
| `void PlayClip(AudioClip)` | Play a clip through the sequence's shared AudioSource |
| `Step CurrentStep` | The currently executing step |
| `List<Step> Steps` | All steps in the sequence |
| `bool Started` | Whether the sequence is currently running |

### Step API

| Member | Description |
|---|---|
| `void Begin()` | Start this step (plays audio, fires events) |
| `void CompleteStep()` | Mark this step as complete and notify the parent sequence |
| `SequenceStatus StepStatus` | Current status: Inactive, Started, or Completed |

### SequenceBehaviour API

| Member | Description |
|---|---|
| `void StartSequence()` | Start the sequence manually |
| `void RestartSequence()` | Reset and restart from the beginning |
| `void SkipCurrentStep()` | Force-complete the current step |
| `void GoToPreviousStep()` | Go back one step |

### AbstractSequenceAction API

| Member | Description |
|---|---|
| `Step Step` | The step this action is attached to |
| `bool Started` | Whether the action is currently active |
| `CompositeDisposable StepDisposable` | Disposable for subscriptions that auto-clean when the step ends |
| `void CompleteStep()` | Convenience method to complete the associated step |
| `abstract void OnStepStatusChanged(SequenceStatus)` | Override to respond to step lifecycle changes |

### BranchCondition API

| Member | Description |
|---|---|
| `bool Evaluate()` | Returns true if the condition is met (or if no variable is assigned) |
| `ScriptableVariable Variable` | The variable being evaluated |
| `ComparisonType Comparison` | The comparison operator |

### SequenceStatus Enum

| Value | Meaning |
|---|---|
| `Inactive` | Not yet started or has been reset |
| `Started` | Currently executing |
| `Completed` | Finished execution |

</details>

---

## FAQ

**Can I skip steps?**
Yes — call `SkipCurrentStep()` on `SequenceBehaviour`, or use Debug Controls (press N by default). Programmatically, call `step.CompleteStep()` on any step.

**Can steps run in parallel?**
No, steps within a single sequence run sequentially. For parallel flows, use separate sequences and coordinate them with `SequenceControlAction`.

**How do I reset a sequence?**
Call `RestartSequence()` on `SequenceBehaviour`, or call `sequence.Reset()` directly.

**Can I modify sequences at runtime?**
Sequences are ScriptableObjects. Clone them if runtime changes are needed to avoid modifying the original asset.
