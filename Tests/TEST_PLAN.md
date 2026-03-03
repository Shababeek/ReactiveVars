# ReactiveVars — Test Suite Plan

## Testing Framework

Use **Unity Test Framework** (built into Unity 6). Tests live in an assembly definition referencing `Shababeek.ReactiveVars`.

```
Tests/
├── Tests.asmdef                  (references Runtime + Editor assemblies)
├── Runtime/                      (Play Mode tests — need a scene context)
│   ├── VariableTests.cs
│   ├── NumericalVariableTests.cs
│   ├── BinderTests.cs
│   ├── DriverTests.cs
│   ├── TweenTests.cs
│   ├── EventTests.cs
│   ├── UtilityTests.cs
│   ├── SequenceTests.cs
│   └── BranchingSequenceTests.cs
└── Editor/                       (Edit Mode tests — no scene needed)
    ├── SerializationTests.cs
    └── VariableContainerTests.cs
```

## Test Categories

### 1. Variable Tests (Edit Mode)

These are the foundation — they test ScriptableVariable behavior in isolation.

```
ScriptableVariable<T>:
  ✓ Value setter triggers OnValueChanged
  ✓ Value setter triggers OnRaised
  ✓ SetValueWithoutNotify does NOT trigger events
  ✓ Raise() manually triggers subscribers
  ✓ Multiple subscribers all receive notifications
  ✓ Disposed subscription stops receiving

FloatVariable / IntVariable (INumericalVariable):
  ✓ Add/Subtract/Multiply/Divide update value correctly
  ✓ Divide by zero logs warning, doesn't crash
  ✓ Clamp constrains value
  ✓ GetNormalized returns correct 0-1 range
  ✓ SetFromNormalized maps correctly
  ✓ LerpTo interpolates correctly
  ✓ MoveTowards respects maxDelta
  ✓ AsFloat and AsInt convert correctly
  ✓ SetFromFloat on IntVariable rounds correctly
  ✓ SetFromFloatWithoutNotify doesn't fire events
  ✓ Operator overloads (+, -, *, /) return correct values

BoolVariable:
  ✓ Toggle flips the value
  ✓ Logical operators (&, |, !) work correctly

DoubleVariable:
  ✓ AsFloat converts correctly
  ✓ Operator overloads work

VariableExtensions:
  ✓ WhenTrue only fires on true transitions
  ✓ WhenFalse only fires on false transitions
  ✓ WhenAbove filters correctly
  ✓ WhenBelow filters correctly
  ✓ WhenInRange filters correctly
  ✓ Distinct skips duplicate values
```

### 2. Event Tests (Edit Mode)

```
GameEvent:
  ✓ Raise() notifies all OnRaised subscribers
  ✓ UnityEvent invoked on Raise()
  ✓ Multiple listeners all receive

GameEvent<T>:
  ✓ Raise(data) passes data to OnRaisedData
  ✓ Raise() without data uses DefaultValue
```

### 3. Binder Tests (Play Mode)

These require a scene context for MonoBehaviour lifecycle.

```
VariableBinder<T> base:
  ✓ Subscribe mode: OnVariableChanged called when variable changes
  ✓ Poll mode: reads value every frame
  ✓ Null variable logs warning, doesn't crash
  ✓ OnDisable disposes subscription
  ✓ Re-enable re-subscribes

Specific binders (sample):
  ✓ GameObjectActiveBinder sets active state correctly
  ✓ GameObjectActiveBinder invert flag works
  ✓ EnableComponentBinder enables/disables target
  ✓ SpriteRendererBinder updates sprite
  ✓ SpriteRendererFlipBinder flips correct axis
  ✓ RectTransformBinder sets correct property (sizeDelta, anchoredPosition, etc.)

TwoWayVariableBinder<T>:
  ✓ Variable change updates UI
  ✓ UI change updates variable
  ✓ Recursion guard prevents infinite loop
```

### 4. Driver Tests (Play Mode)

```
VariableDriver<T> base:
  ✓ SilentUpdates uses SetValueWithoutNotify
  ✓ Non-silent uses Value setter
  ✓ Null variable logs warning

CollisionDriver / TriggerDriver:
  ✓ Sets BoolVariable true on enter
  ✓ Sets BoolVariable false on exit
  ✓ Empty tag filter matches any
  ✓ Specific tag filter only matches tagged objects
  ✓ GameObjectVariable receives the other object

TimerDriver:
  ✓ CountDown reaches 0 and fires GameEvent
  ✓ CountUp reaches duration and fires GameEvent
  ✓ Loop restarts after completion
  ✓ Stop/Resume/Reset work correctly

CooldownDriver:
  ✓ Trigger sets true then auto-resets to false
  ✓ Cancel immediately resets
  ✓ Remaining time variable updates each frame
```

### 5. Sequence Tests (Play Mode)

```
Sequence:
  ✓ Begin() starts first step
  ✓ Steps execute in order
  ✓ CompleteStep advances to next step
  ✓ Completing last step marks sequence as Completed
  ✓ GoToPreviousStep moves backward
  ✓ Reset reinitializes to Inactive
  ✓ Audio plays on step start with correct delay
  ✓ AudioOnly step auto-completes when audio finishes
  ✓ Step onStarted/onCompleted UnityEvents fire correctly
  ✓ canBeFinishedBeforeStarted allows early completion

BranchingSequence:
  ✓ Begin() starts first step
  ✓ Transitions evaluate in order (first match wins)
  ✓ Correct target step is reached based on condition
  ✓ No matching transition ends the sequence
  ✓ TransitionEvent fires on transition
  ✓ Reset clears all state

BranchCondition:
  ✓ Equals comparison works for all variable types
  ✓ GreaterThan/LessThan work for numeric variables
  ✓ Null variable returns true (unconditional)
  ✓ NotEquals comparison works correctly

SequenceBehaviour:
  ✓ StartOnAwake triggers Begin with delay
  ✓ SkipCurrentStep force-completes current step
  ✓ RestartSequence resets and restarts

Actions:
  ✓ AnimationAction sets trigger and auto-completes on animation end
  ✓ EventAction fires events and auto-completes after delay
  ✓ TriggerAction completes on trigger enter with tag filter
  ✓ ProximityAction completes on Enter/Exit/StayDuration conditions
  ✓ MultiConditionAction completes in All/Any/Count modes
  ✓ SequenceControlAction StartAndWait/StartOnly/WaitForCompletion work
  ✓ Actions clean up subscriptions on step complete
```

### 6. Tween Tests (Play Mode)

```
TweenableFloat:
  ✓ Tween interpolates from start to target
  ✓ Returns true (completed) when t >= 1
  ✓ AnimationCurve easing applies correctly
  ✓ OnChange callback fires each frame
  ✓ OnFinished fires on completion

TweenableNumerical:
  ✓ Uses SetFromFloatWithoutNotify during interpolation
  ✓ Uses SetFromFloat on final frame
  ✓ Variable value reaches exact target

VariableTweener:
  ✓ AddTweenable prevents duplicates
  ✓ RemoveTweenable stops updating
  ✓ Clear removes all
  ✓ TweenScale affects interpolation speed
```

### 7. Serialization Tests (Edit Mode)

```
VariableContainer:
  ✓ SaveToFile creates valid JSON
  ✓ LoadFromFile restores values
  ✓ ResetAllVariables resets to defaults
  ✓ GetVariable<T> finds by name
  ✓ TryGetVariable returns false for missing

Sequence serialization:
  ✓ Step list persists correctly on Sequence asset
  ✓ BranchingSequence transitions and conditions persist
  ✓ Step references survive asset save/load
```

### 8. Utility Tests (Play Mode)

```
VariableResetter:
  ✓ Manual ResetAll raises all variables
  ✓ OnEnable trigger resets on enable
  ✓ GameEvent trigger resets on event

EventRelay:
  ✓ Source event triggers target event
  ✓ Delay waits specified time before relaying
  ✓ UnityEvent fires on relay

VariableLogger:
  ✓ Logs value changes to console
  ✓ Uses correct log level

VariableDebugOverlay:
  ✓ Toggle key shows/hides overlay
  ✓ Auto-discover finds scene variables
  ✓ AddVariable/RemoveVariable work at runtime
```

## How to Run

```bash
# From Unity Editor:
# Window > General > Test Runner
# Select "Play Mode" or "Edit Mode" tab
# Click "Run All"

# From command line:
Unity -runTests -testPlatform PlayMode -projectPath .
Unity -runTests -testPlatform EditMode -projectPath .
```

## Implementation Priority

1. **Variable Tests** — most critical, everything depends on them
2. **Event Tests** — small set, quick to write
3. **Sequence Tests** — validate step execution, branching, and actions
4. **Binder Tests** — many binders but test the base class thoroughly, spot-check specifics
5. **Driver Tests** — focus on collision/trigger tag filtering and timer logic
6. **Tween Tests** — verify numerical tween silent update behavior
7. **Serialization Tests** — ensure save/load round-trips correctly
8. **Utility Tests** — lowest risk, test last

## Assembly Definition

```json
{
    "name": "Shababeek.ReactiveVars.Tests",
    "references": [
        "Shababeek.ReactiveVars",
        "UniRx",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "optionalUnityReferences": [
        "TestAssemblies"
    ],
    "includePlatforms": [],
    "excludePlatforms": []
}
```
