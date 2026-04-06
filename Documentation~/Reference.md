# Reference for programmers

**Navigation:** [Documentation home](README.md) · **Previous:** [ScriptableVariable.md](ScriptableVariable.md)

Technical overview: source map, architecture, quick lookup tables, and links to the full variable API. For inspector-first tutorials, start with [README.md](README.md) in this folder (documentation home) or [GettingStarted.md](GettingStarted.md).

## Assemblies and namespaces


| Assembly                        | Namespace (typical)                                             | Contents                                               |
| ------------------------------- | --------------------------------------------------------------- | ------------------------------------------------------ |
| `Shababeek.ReactiveVars`        | `Shababeek.ReactiveVars`, `Shababeek.Sequencing`, tween helpers | Runtime variables, events, binders, drivers, sequences |
| `Shababeek.ReactiveVars.Editor` | Editor scripts                                                  | Custom inspectors, sequence graph                      |


The Runtime asmdef references **UniRx**, **TextMeshPro**, and optionally the **Input System** (scripting define `REACTIVE_VARS_INPUT_SYSTEM`). See [CONTRIBUTING.md](../CONTRIBUTING.md) in the package root.

## Reactive Vars window (editor)

Open **Shababeek > ReactiveVars > Reactive Vars Window** — implementation: [ReactiveVarsEditorWindow.cs](../Editor/ReactiveVarsEditorWindow.cs).

| Area | Purpose |
|------|---------|
| **Variables** / **Events** tabs | Browse scriptable variables (all types that inherit `ScriptableVariable`) or plain `GameEvent` assets |
| **Search** | Filter by name |
| **Type** | On Variables tab: filter by Int, Float, Bool, Text, Vector2/3, Color, Other |
| **Scene Refs Only** | When enabled (default), only lists assets referenced by the currently open scenes — good for focusing on what the level uses |
| **Refresh** | Rescans the project and scene references |
| **Foldouts** | Variables/events are grouped under their parent asset (e.g. variable container main asset); **Select** pings the parent in the Project window |
| **→** | Ping the individual variable or event |
| **Value column (Play mode)** | For many scalar types, edit **int / float / bool / string** live. Others show read-only text or fields depending on type |
| **Fire** (Events tab, Play mode) | Calls `Raise()` on that event |
| **Dot (●)** | That asset is referenced from the open scenes |

This is an **editor-only** tool. For a **runtime** on-screen variable list, use **VariableDebugOverlay** ([VariableDebugOverlay.cs](../Runtime/ScriptableSystem/Utility/VariableDebugOverlay.cs)).

## Key types (source)


| Type                                           | Role                                         | Source                                                                               |
| ---------------------------------------------- | -------------------------------------------- | ------------------------------------------------------------------------------------ |
| `ScriptableVariable` / `ScriptableVariable<T>` | Shared asset value, `OnValueChanged`, raise  | [ScriptableVariable.cs](../Runtime/ScriptableSystem/Variables/ScriptableVariable.cs) |
| `VariableReference<T>`                         | Inspector: constant or variable              | [VariableReference.cs](../Runtime/ScriptableSystem/Variables/VariableReference.cs)   |
| `VariableContainer`                            | Groups variables/events, bulk ops, save/load | [VariableContainer.cs](../Runtime/ScriptableSystem/Variables/VariableContainer.cs)   |
| `GameEvent` / `GameEvent<T>`                   | Fire-and-forget signals                      | [GameEvent.cs](../Runtime/ScriptableSystem/Events/GameEvent.cs)                      |
| `VariableBinder<T>`                            | Push variable → component                    | [VariableBinder.cs](../Runtime/ScriptableSystem/Utility/VariableBinder.cs)           |
| `VariableDriver<T>`                            | Source → variable                            | [VariableDriver.cs](../Runtime/ScriptableSystem/Utility/VariableDriver.cs)           |
| `Sequence`                                     | Linear ordered steps                         | [Sequence.cs](../Runtime/SequencingSystem/Core/Core/Sequence.cs)                     |
| `BranchingSequence`                            | Steps with conditional transitions           | [BranchingSequence.cs](../Runtime/SequencingSystem/Core/Core/BranchingSequence.cs)   |
| `ITweenable` / tweenables                      | Smooth value interpolation                   | [ITweenable.cs](../Runtime/TweenSystem/ITweenable.cs)                                |


Full member tables: [ScriptableVariable.md](ScriptableVariable.md).

## UniRx usage

- Subscribe in `OnEnable`, store `IDisposable`, **dispose in `OnDisable`** to avoid leaks across scene unloads.
- `OnValueChanged` is a hot observable: pair `SetValueWithoutNotify` on the write side with **Poll** mode on binders when updating every frame.

## Architecture

```
ScriptableObject
 └─ GameEvent                         event with OnRaised observable
     └─ GameEvent<T>                  typed event with data payload
         └─ ScriptableVariable        abstract base
             └─ ScriptableVariable<T>    Value, OnValueChanged, SetValueWithoutNotify
                  ├─ NumericalVariable<T> : INumericalVariable
                  │   ├─ FloatVariable
                  │   └─ IntVariable
                  ├─ BoolVariable
                  ├─ ColorVariable
                  ├─ Vector3Variable
                  ├─ SpriteVariable
                  ├─ MaterialVariable
                  ├─ DoubleVariable
                  └─ ... (more types)

 └─ SequenceNode                      base for sequences (status + audio)
     ├─ Sequence                      linear step execution
     └─ BranchingSequence             conditional step transitions

 └─ Step                              atomic unit in a sequence

MonoBehaviour
 ├─ VariableBinder<T>                 read, subscribe / poll
 │   └─ NumericalVariableBinder       INumericalVariable specialization
 ├─ TwoWayVariableBinder<T>           read + write, recursion guard
 ├─ VariableBinder<T1,T2>             multi-variable observer
 ├─ VariableBinder<T1,T2,T3>          multi-variable observer
 ├─ VariableDriver<T>                 write, silent option
 │    ├─ InputActionFloatDriver
 │    ├─ CollisionDriver / TriggerDriver
 │    ├─ TimerDriver / CooldownDriver
 │    └─ ... (more drivers)
 ├─ SequenceBehaviour                 runs a Sequence in the scene
 ├─ BranchingSequenceBehaviour        runs a BranchingSequence in the scene
 ├─ AbstractSequenceAction            base for step completion logic
 │    ├─ AnimationAction
 │    ├─ EventAction
 │    ├─ TriggerAction
 │    ├─ ProximityAction
 │    ├─ MultiConditionAction
 │    └─ SequenceControlAction
 ├─ StepEventListener                 step status → UnityEvent bridge
 ├─ MultiStepListener                 multi-step observer
 ├─ VariableResetter                  bulk reset on triggers
 ├─ EventRelay                        event forwarding with delay
 ├─ VariableLogger                    debug logging
 └─ VariableDebugOverlay              runtime HUD

ITweenable
 ├─ TweenableFloat
 ├─ TweenableColor
 ├─ TweenableVector2 / Vector3
 ├─ TweenableQuaternion
 ├─ TweenableNumerical                bridges INumericalVariable
 └─ TransformTweenable

VariableContainer                      organizes variables for save/load
```

## Variable types (quick lookup)


| Category      | Types                                                                    |
| ------------- | ------------------------------------------------------------------------ |
| **Numeric**   | FloatVariable, IntVariable, DoubleVariable                               |
| **Vector**    | Vector2Variable, Vector3Variable, Vector2IntVariable, QuaternionVariable |
| **Visual**    | ColorVariable, GradientVariable, SpriteVariable, MaterialVariable        |
| **Text**      | TextVariable, StringListVariable                                         |
| **Logic**     | BoolVariable, EnumVariable, LayerMaskVariable                            |
| **Reference** | TransformVariable, GameObjectVariable                                    |
| **Audio**     | AudioVariable, AudioClipVariable                                         |
| **Animation** | AnimationCurveVariable                                                   |


## Create menu paths (Project window)

Most assets use **Create > Shababeek > …** with path segments matching `CreateAssetMenu` (slashes become submenus). Examples:

- Variables: **Create > Shababeek > ReactiveVars > Variables > FloatVariable** (and similar per type)
- Variable container: **Create > Shababeek > ReactiveVars > Variable Container**
- Game event: **Create > Shababeek > ReactiveVars > Events > GameEvent**
- Sequence: **Create > Shababeek > Sequencing > Sequence**
- Branching sequence: **Create > Shababeek > Sequencing > BranchingSequence**

A few types use a legacy `**ReactiveVars`–only** prefix (e.g. SpriteVariable, MaterialVariable). Use the **Create** window search box and type the asset name if in doubt.

## Binder categories (overview)

Full catalog: [Binders.md](Binders.md).

**Numerical:** NumericalTextBinder, NumericalFillBinder, NumericalPositionBinder, NumericalRotationBinder, NumericalScaleBinder, NumericalAnimatorBinder, NumericalAudioBinder, NumericalMaterialBinder, NumericalParticleBinder, NumericalNavMeshBinder, and more.

**Boolean:** GameObjectActiveBinder, EnableComponentBinder, BoolToggleBinder, BoolAnimatorBinder, BoolCanvasGroupBinder, BoolParticleBinder.

**Color:** ColorImageBinder, ColorSpriteBinder, ColorLineRendererBinder, ColorTextMeshProBinder, GradientSamplerBinder.

**Transform:** Vector3PositionBinder, QuaternionRotationBinder, TransformBinder, TransformFollowerBinder, Vector2SpaceBinder.

**Physics:** Rigidbody3DBinder, Rigidbody2DBinder, AngularVelocityBinder.

**UI (two-way):** SliderBinder, InputFieldBinder, DropdownBinder, StringListDropdownBinder, ScrollRectBinder.

**Visual:** SpriteRendererBinder, SpriteRendererFlipBinder, LineRendererWidthBinder.

**Light & camera:** LightBinder, CameraBinder.

**Animation & audio:** AnimatorBinder, EventAnimatorBinder, AudioSourceBinder.

## Driver types


| Driver                       | Source → Target                                                             |
| ---------------------------- | --------------------------------------------------------------------------- |
| **InputActionFloatDriver**   | Input System axis/trigger → FloatVariable                                   |
| **InputActionVector2Driver** | Input System move/look → Vector2Variable                                    |
| **InputActionButtonDriver**  | Input System button → BoolVariable                                          |
| **TransformDriver**          | Transform position/rotation/scale → Vector3/QuaternionVariable              |
| **CollisionDriver**          | Physics collision enter/exit → BoolVariable, GameObjectVariable             |
| **TriggerDriver**            | Physics trigger enter/exit → BoolVariable, GameObjectVariable               |
| **DistanceDriver**           | Distance between transforms → FloatVariable                                 |
| **TimerDriver**              | Count up/down timer → FloatVariable                                         |
| **RaycastDriver**            | Physics raycast hit/distance → BoolVariable, FloatVariable, Vector3Variable |
| **VelocityDriver**           | Rigidbody velocity magnitude → Vector3Variable, FloatVariable               |
| **AnimatorStateDriver**      | Animator parameter value → FloatVariable, IntVariable, BoolVariable         |
| **CooldownDriver**           | Timed cooldown state → BoolVariable, FloatVariable                          |


Input System drivers require the [Input System package](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest) in the project.

## Component quick-add

**Tweening:** Add **VariableTweener** to a scene object when using tweens (see [TweenSystem.md](TweenSystem.md)).

**Binder:** Select target object → **Add Component** → `*Binder` → assign variable.

**Driver:** Select source object → **Add Component** → `*Driver` → assign target variable.

**Runtime debug HUD:** **VariableDebugOverlay** — add to a scene object to show variable values on screen in play mode (not the same as the **Reactive Vars** editor window above).

---

**Related:** [ScriptableVariable.md](ScriptableVariable.md) · [CONTRIBUTING.md](../CONTRIBUTING.md) · [Documentation home](README.md)