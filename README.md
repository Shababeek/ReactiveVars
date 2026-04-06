<p align="center">
  <h1 align="center">ReactiveVars</h1>
  <p align="center">
    A ScriptableObject-based reactive variable system for Unity.<br/>
    Decouple your game systems. Wire everything in the inspector.
  </p>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Unity-6000.0%2B-black?logo=unity" alt="Unity 6+"/>
  <img src="https://img.shields.io/badge/version-1.0.0-blue" alt="Version"/>
  <img src="https://img.shields.io/badge/UniRx-required-orange" alt="UniRx"/>
  <img src="https://img.shields.io/badge/TMP-required-orange" alt="TextMeshPro"/>
  <img src="https://img.shields.io/badge/Input%20System-optional-green" alt="Input System"/>
</p>

---

## What Is ReactiveVars?

ReactiveVars lets you build game systems that talk to each other without being directly connected. Instead of one script finding another script to read its data, both scripts point to the same **Variable** asset — a small file that lives in your Project folder. When one script changes the variable, anything watching it updates automatically.

This means you can build things like health bars, score displays, sound effects, and UI toggles without writing code or creating complex chains of references between objects.

### Core Concepts at a Glance

| Concept | What It Does | Who Uses It |
|---|---|---|
| **Variable** | A shared value (number, bool, color, etc.) that lives as a project asset | Everyone — the foundation of the system |
| **Binder** | Reads a variable and pushes the value into a component (text, image, animator, etc.) | Designers — drag a variable onto a binder, done |
| **Driver** | Writes into a variable from a source (input, physics, timer, distance, etc.) | Designers — feeds data into the system |
| **Tween** | Smoothly interpolates a variable's value over time | Designers/Programmers — smooth transitions |
| **Event** | A signal with no permanent state — fire and forget | Everyone — trigger responses to moments |
| **Sequence** | An ordered series of steps that execute one after another, with optional branching | Designers — tutorials, cutscenes, guided flows |

### How It Fits Together

A typical setup looks like this: a **Driver** writes a value (say, the player's health) into a **Variable**. One or more **Binders** on completely separate objects read that variable and update their components (a health bar fills, a warning sound plays, a screen effect activates). Nobody knows about anyone else — they only know about the shared variable.

For more complex flows like tutorials or narrative sequences, the **Sequencing System** lets you define ordered steps with audio, events, and completion conditions — all wired in the inspector.

---

## 📚 Documentation Structure

### 🎓 Getting Started
Start here if you're new to ReactiveVars.

| Document | Description |
|----------|-------------|
| **[Getting Started Guide](Documentation~/GettingStarted.md)** | Quick start for designers and programmers, core concepts, first five minutes |
| **[ScriptableVariable Reference](Documentation~/ScriptableVariable.md)** | Variable API and type reference |

---

### 🔧 Core Systems
Main documentation for each system.

| Document | Description |
|----------|-------------|
| **[Binders Guide](Documentation~/Binders.md)** | Complete guide to all 50+ binder types, update modes, custom binder creation |
| **[Tween System](Documentation~/TweenSystem.md)** | Smooth value interpolation with easing curves, all tweenable types |
| **[Variable Containers](Documentation~/VariableContainer.md)** | Organize variables, save/load to JSON, bulk operations |

---

### 📖 Advanced Topics
In-depth guides for specific systems.

| Document | Description |
|----------|-------------|
| **[Sequencing System](Documentation~/SequencingSystem.md)** | Tutorials, cutscenes, and branching sequences with actions |

---

## Key types (source)

| Type | Role | Source |
|------|------|--------|
| `ScriptableVariable` / `ScriptableVariable<T>` | Shared asset value, `OnValueChanged`, raise | [ScriptableVariable.cs](Runtime/ScriptableSystem/Variables/ScriptableVariable.cs) |
| `VariableReference<T>` | Inspector: constant or variable | [VariableReference.cs](Runtime/ScriptableSystem/Variables/VariableReference.cs) |
| `VariableContainer` | Groups variables/events, bulk ops, save/load | [VariableContainer.cs](Runtime/ScriptableSystem/Variables/VariableContainer.cs) |
| `GameEvent` / `GameEvent<T>` | Fire-and-forget signals | [GameEvent.cs](Runtime/ScriptableSystem/Events/GameEvent.cs) |
| `VariableBinder<T>` | Push variable → component | [VariableBinder.cs](Runtime/ScriptableSystem/Utility/VariableBinder.cs) |
| `VariableDriver<T>` | Source → variable | [VariableDriver.cs](Runtime/ScriptableSystem/Utility/VariableDriver.cs) |
| `Sequence` | Linear ordered steps | [Sequence.cs](Runtime/SequencingSystem/Core/Core/Sequence.cs) |
| `BranchingSequence` | Steps with conditional transitions | [BranchingSequence.cs](Runtime/SequencingSystem/Core/Core/BranchingSequence.cs) |
| `ITweenable` / tweenables | Smooth value interpolation | [ITweenable.cs](Runtime/TweenSystem/ITweenable.cs) |

API detail for variables: [ScriptableVariable.md](Documentation~/ScriptableVariable.md).

---

## 🎯 Quick Reference

### Variable Types (23 Total)

| Category | Types |
|---|---|
| **Numeric** | FloatVariable, IntVariable, DoubleVariable |
| **Vector** | Vector2Variable, Vector3Variable, Vector2IntVariable, QuaternionVariable |
| **Visual** | ColorVariable, GradientVariable, SpriteVariable, MaterialVariable |
| **Text** | TextVariable, StringListVariable |
| **Logic** | BoolVariable, EnumVariable, LayerMaskVariable |
| **Reference** | TransformVariable, GameObjectVariable |
| **Audio** | AudioVariable, AudioClipVariable |
| **Animation** | AnimationCurveVariable |

---

### Component Quick-Add

**Scene Setup:**
1. Create empty GameObject
2. Add component: **VariableTweener** (required for tweens)

**Add Binder to UI:**
1. Select a UI element (TextMeshPro, Image, etc.)
2. Add component: **[BinderType]Binder** (e.g., NumericalTextBinder)
3. Assign variable in inspector

**Add Driver:**
1. Select relevant GameObject (player, timer, physics object, etc.)
2. Add component: **[DriverType]Driver** (e.g., TimerDriver)
3. Assign variable in inspector

---

### Binder Categories (50+)

**Numerical Binders:** NumericalTextBinder, NumericalFillBinder, NumericalPositionBinder, NumericalRotationBinder, NumericalScaleBinder, NumericalAnimatorBinder, NumericalAudioBinder, NumericalMaterialBinder, NumericalParticleBinder, NumericalNavMeshBinder, and more.

**Boolean Binders:** GameObjectActiveBinder, EnableComponentBinder, BoolToggleBinder, BoolAnimatorBinder, BoolCanvasGroupBinder, BoolParticleBinder.

**Color Binders:** ColorImageBinder, ColorSpriteBinder, ColorLineRendererBinder, ColorTextMeshProBinder, GradientSamplerBinder.

**Transform Binders:** Vector3PositionBinder, QuaternionRotationBinder, TransformBinder, TransformFollowerBinder, Vector2SpaceBinder.

**Physics Binders:** Rigidbody3DBinder, Rigidbody2DBinder, AngularVelocityBinder.

**UI (Two-Way):** SliderBinder, InputFieldBinder, DropdownBinder, StringListDropdownBinder, ScrollRectBinder.

**Visual Binders:** SpriteRendererBinder, SpriteRendererFlipBinder, LineRendererWidthBinder.

**Light & Camera:** LightBinder, CameraBinder.

**Animation & Audio:** AnimatorBinder, EventAnimatorBinder, AudioSourceBinder.

See [Binders Guide](Documentation~/Binders.md) for complete descriptions.

---

### Driver Types (12 Total)

| Driver | Source → Target |
|--------|---|
| **InputActionFloatDriver** | Input System axis/trigger → FloatVariable |
| **InputActionVector2Driver** | Input System move/look → Vector2Variable |
| **InputActionButtonDriver** | Input System button → BoolVariable |
| **TransformDriver** | Transform position/rotation/scale → Vector3/QuaternionVariable |
| **CollisionDriver** | Physics collision enter/exit → BoolVariable, GameObjectVariable |
| **TriggerDriver** | Physics trigger enter/exit → BoolVariable, GameObjectVariable |
| **DistanceDriver** | Distance between transforms → FloatVariable |
| **TimerDriver** | Count up/down timer → FloatVariable |
| **RaycastDriver** | Physics raycast hit/distance → BoolVariable, FloatVariable, Vector3Variable |
| **VelocityDriver** | Rigidbody velocity magnitude → Vector3Variable, FloatVariable |
| **AnimatorStateDriver** | Animator parameter value → FloatVariable, IntVariable, BoolVariable |
| **CooldownDriver** | Timed cooldown state → BoolVariable, FloatVariable |

---

## 📖 How to Use This Documentation

Each guide follows a consistent structure:

1. **What It Does** — Quick overview
2. **Inspector Reference** — All settings explained
3. **Code Examples** — Common patterns
4. **Troubleshooting** — Common issues and solutions

### Getting Started Path

**New Users:**
1. Read [Getting Started Guide](Documentation~/GettingStarted.md) (5-10 minutes)
2. Build a simple health bar (using binders + drivers)
3. Explore [Binders Guide](Documentation~/Binders.md) for more component types
4. Add tweens with [Tween System](Documentation~/TweenSystem.md)

**Programmers:**
1. Skim [Getting Started Guide](Documentation~/GettingStarted.md)
2. Review [ScriptableVariable Reference](Documentation~/ScriptableVariable.md) for API
3. Check [Binders Guide](Documentation~/Binders.md) to write custom binders if needed
4. Explore [Sequencing System](Documentation~/SequencingSystem.md) for advanced flows

---

## Installation

**Via Git URL** — In Unity Package Manager, click `+` > *Add package from git URL*:

```
https://github.com/Ahmadabobakr/ReactiveVars.git
```

**Via local folder** — Clone or copy the `ReactiveVars` folder into your project's `Packages` directory.

**Dependencies:** Declared in `package.json`: **UniRx** ([`com.neuecc.unirx`](https://github.com/neuecc/UniRx)) and **TextMeshPro**. Unity Package Manager resolves them when you add this package from Git or from a local folder that includes the manifest.

**Manual UniRx:** If you embed only the Runtime sources without the package manifest, add UniRx via *Add package from git URL*: `https://github.com/neuecc/UniRx.git?path=Assets/Plugins/UniRx/Scripts`

**Optional:** Unity Input System — input action drivers compile when the Input System package is present (`REACTIVE_VARS_INPUT_SYSTEM`). See [CONTRIBUTING.md](CONTRIBUTING.md).

---

## Quick Start Example

### Five-Minute Setup

**Step 1:** Create a variable
```
Right-click in Project > Create > Shababeek > ReactiveVars > Variables > FloatVariable
Name it: PlayerHealth
```

**Step 2:** Add binder to UI
```
1. Create TextMeshPro element (UI > TextMeshPro - Text)
2. Add component: NumericalTextBinder
3. Drag PlayerHealth variable into the Variable field
4. Set Format String to: "Health: {0:F0}"
```

**Step 3:** Write to variable from code
```csharp
using Shababeek.ReactiveVars;

public FloatVariable playerHealth;

void TakeDamage(float amount)
{
    playerHealth.Value -= amount;  // UI updates automatically
}
```

**That's it!** No event wiring, no GetComponent calls, no manual UI updates.

See [Getting Started Guide](Documentation~/GettingStarted.md) for more examples.

---

## Core Features

### Variables
- 23 built-in types (float, int, bool, color, vector, etc.)
- Reactive subscription via UniRx observables
- Silent updates for high-frequency drivers
- Full arithmetic operator support on numerical types
- Extension methods (WhenTrue, WhenAbove, WhenInRange, Throttled, etc.)

### Binders
- 50+ built-in binders covering UI, transform, physics, animation, audio
- Subscribe or Poll update modes
- Two-way binders for bidirectional UI sync
- Custom binder base classes for extensibility

### Drivers
- 12 built-in drivers (input, physics, timers, distance, velocity, etc.)
- Silent updates for frame-by-frame feeding
- Optional event firing on completion
- Auto-start, loop, and timing controls

### Tweening
- Rate-based interpolation with duration control
- Multiple tweenable types (Float, Color, Vector2, Vector3, Quaternion, etc.)
- AnimationCurve easing support
- Direct variable tweening with TweenableNumerical
- Global speed multiplier for slow-motion effects

### Events
- Fire-and-forget signal system with no state
- Optional typed data payload
- Scene listeners for inspector-wired responses
- Automatic lifecycle events (enable, disable, destroy)

### Sequencing
- Linear sequences for ordered flows
- Branching sequences with conditional transitions
- Per-step audio, delays, and unityevent callbacks
- Scene actions for step completion logic (animation, trigger, proximity, etc.)
- Graph editor for visual sequence design

### Variable Containers
- Group related variables into single asset
- Save/load all variables to JSON
- Bulk reset, raise, and access operations
- Default save location in persistent data path

### Utilities
- Variable Resetter (bulk reset on lifecycle events)
- Event Relay (forward events with delay)
- Variable Logger (debug logging)
- Variable Debug Overlay (runtime on-screen HUD)
- Scriptable System Window (browse and create assets)

---

## Documentation Checklist

| Section | Status |
|---------|--------|
| Getting Started | ✅ Complete |
| ScriptableVariable Reference | ✅ Complete |
| Binders Guide | ✅ Complete |
| Tween System | ✅ Complete |
| Variable Containers | ✅ Complete |
| Sequencing System | ✅ Complete |
| Code Examples | ✅ Included in guides |
| Troubleshooting | ✅ In each guide |

---

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
                  └─ ... (13 more)

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
 │    └─ ... (8 more)
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

---

## Getting Help

- **Contributing:** [CONTRIBUTING.md](CONTRIBUTING.md) — assemblies, tests, optional defines
- **Documentation:** Start with [Getting Started Guide](Documentation~/GettingStarted.md)
- **Troubleshooting:** Check troubleshooting sections in relevant guides
- **Bug Reports:** [GitHub Issues](https://github.com/Ahmadabobakr/ReactiveVars/issues)
- **Feature Requests:** [GitHub Discussions](https://github.com/Ahmadabobakr/ReactiveVars/discussions)
- **Email:** Ahmadabobakr@gmail.com
- **Website:** [ahmadabobakr.github.io](https://ahmadabobakr.github.io)

---

## License

See [LICENSE](LICENSE) for details.

---

**Last Updated:** March 2026
