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

Variables are shared ScriptableObject assets. Any component can read or write them. When a variable changes, every subscriber gets notified automatically — no direct references between objects, no singletons, no manual event wiring.

**Binders** read variables and push values into components. **Drivers** write into variables from external sources like input, physics, and timers. **Tweenables** interpolate values smoothly over time. **Conditions** let you build complex boolean logic visually in a node graph. All configurable from the inspector, all zero-code by default.

```csharp
// A health system with zero coupling between damage, UI, and audio
public FloatVariable playerHealth;    // Shared asset — drag & drop in inspector

void TakeDamage(float amount) => playerHealth.Value -= amount;

// Meanwhile, a health bar binder on a completely different object
// automatically updates because it's subscribed to the same variable.
// No GetComponent. No Find. No event registration.
```

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
- [Variables](#variables)
- [Variable References](#variable-references)
- [Events](#events)
- [Binders (Read)](#binders-read)
- [Drivers (Write)](#drivers-write)
- [Conditions (Logic)](#conditions-logic)
- [Tween System](#tween-system)
- [Variable Containers](#variable-containers)
- [Utilities](#utilities)
- [Editor Tools](#editor-tools)
- [Architecture](#architecture)

---

## Installation

**Via Git URL** — In Unity Package Manager, click `+` > *Add package from git URL*:

```
https://github.com/Ahmadabobakr/ReactiveVars.git
```

**Via local folder** — Clone or copy the `ReactiveVars` folder into your project's `Packages` directory.

**Dependencies:** UniRx, TextMeshPro (auto-resolved).
**Optional:** Unity Input System — input action drivers are enabled automatically when the package is detected.

---

## Quick Start

1. **Create a variable:** Right-click in Project > *Create > ReactiveVars > Variables > FloatVariable*
2. **Write to it:** Drag the asset onto any script's `FloatVariable` field. Set `variable.Value = 10f;` from code.
3. **Read from it:** Add a `NumericalTextBinder` component to a TextMeshPro object. Drag in the same variable. Done — the text updates automatically.

No code needed for step 3. The 50+ included binders cover most common use cases out of the box.

---

## Variables

ScriptableObject assets that hold a single value and broadcast changes via UniRx observables.

```csharp
public FloatVariable playerHealth;

void Start()
{
    // Reactive subscription
    playerHealth.OnValueChanged.Subscribe(hp => Debug.Log($"Health: {hp}"));
}

void TakeDamage(float amount)
{
    playerHealth.Value -= amount;  // Notifies all subscribers
}

void SilentReset()
{
    playerHealth.SetValueWithoutNotify(100f);  // No events fired
}
```

### 23 Variable Types

| Category | Types |
|---|---|
| **Numeric** | `FloatVariable`, `IntVariable`, `DoubleVariable` |
| **Vector** | `Vector2Variable`, `Vector3Variable`, `Vector2IntVariable`, `QuaternionVariable` |
| **Visual** | `ColorVariable`, `GradientVariable`, `SpriteVariable`, `MaterialVariable` |
| **Text** | `TextVariable`, `StringListVariable` |
| **Logic** | `BoolVariable`, `EnumVariable`, `LayerMaskVariable` |
| **Reference** | `TransformVariable`, `GameObjectVariable` |
| **Audio** | `AudioVariable`, `AudioClipVariable` |
| **Animation** | `AnimationCurveVariable` |

Numeric variables implement `INumericalVariable` for type-agnostic operations: `Add`, `Subtract`, `Multiply`, `Divide`, `Clamp`, `GetNormalized`, `SetFromNormalized`, `LerpTo`, `MoveTowards`, and `SetFromFloatWithoutNotify`.

All variables expose:

| Member | Description |
|---|---|
| `Value` | Get/set the stored value. Setting triggers all subscribers. |
| `OnValueChanged` | `IObservable<T>` — typed reactive stream. |
| `OnRaised` | `IObservable<Unit>` — fires on any change (from base `GameEvent`). |
| `SetValueWithoutNotify(T)` | Write without triggering events. Pair with Poll mode binders. |
| `Raise()` | Manually notify all subscribers of the current value. |

### Extension Methods

```csharp
using Shababeek.ReactiveVars;

// Filter streams
myBool.WhenTrue().Subscribe(_ => Debug.Log("Became true"));
myBool.WhenFalse().Subscribe(_ => Debug.Log("Became false"));

// Numerical filters
myFloat.WhenAbove(50f).Subscribe(_ => Debug.Log("Exceeded 50"));
myFloat.WhenBelow(10f).Subscribe(_ => Debug.Log("Dropped below 10"));
myFloat.WhenInRange(20f, 80f).Subscribe(_ => Debug.Log("In range"));

// Utility
myFloat.OnFloatChanged().Subscribe(v => Debug.Log($"New float: {v}"));
myVar.Throttled(TimeSpan.FromSeconds(0.5f)).Subscribe(_ => { });
myVar.Distinct().Subscribe(_ => { });  // Skip duplicate values
```

---

## Variable References

`VariableReference<T>` can point to a ScriptableVariable asset **or** hold a constant value, toggled in the inspector. Prototype with constants, swap to shared variables later — no code changes.

```csharp
public FloatReference moveSpeed;  // Inspector: [Constant ▾] 5.0  OR  [Variable ▾] → MoveSpeed asset

void Update()
{
    transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
}
```

**Typed references:** `FloatReference`, `IntReference`, `BoolReference`, `ColorReference`, `Vector2Reference`, `Vector3Reference`, `SpriteReference`, `MaterialReference`, `DoubleReference`.

`NumericalReference` accepts either a `FloatVariable` or `IntVariable` with unified numeric access.

---

## Events

ScriptableObject-based events that decouple senders from listeners.

| Class | Description |
|---|---|
| `GameEvent` | No data payload. `Raise()` to fire. |
| `GameEvent<T>` | Typed data payload. `Raise(data)` to fire with data. |
| `GameEventListener` | MonoBehaviour bridge — triggers UnityEvents in the inspector. Zero code. |
| `ObjectLifecycleEvents` | Fires events on `OnEnable`, `OnDisable`, `OnDestroy`. |

```csharp
public GameEvent onPlayerDeath;

void Die() => onPlayerDeath.Raise();  // All listeners respond
```

---

## Binders (Read)

Binders **read** from a variable and push the value to a component. Assign the variable in the inspector — no code needed for the 50+ included binders.

### Update Modes

Every binder exposes an **Update Mode** dropdown:

| Mode | Behavior | Use When |
|---|---|---|
| **Subscribe** (default) | Reacts to variable change events | Normal usage — zero cost when idle |
| **Poll** | Reads the value every frame in `Update` | Variable is being tweened silently |

### Writing Custom Binders

Extend a base class. All UniRx usage is isolated in the base — your subclass stays framework-agnostic.

| Base Class | For |
|---|---|
| `VariableBinder<T>` | One variable → one target |
| `NumericalVariableBinder` | Any `INumericalVariable` |
| `TwoWayVariableBinder<T>` | Bidirectional UI sync with recursion guards |
| `VariableBinder<T1, T2>` | Two variables → one target |
| `VariableBinder<T1, T2, T3>` | Three variables → one target |

```csharp
public class HealthBarBinder : VariableBinder<FloatVariable>
{
    [SerializeField] private FloatVariable health;
    [SerializeField] private Image fillImage;

    protected override FloatVariable Variable => health;

    protected override void Bind() => OnVariableChanged();

    protected override void OnVariableChanged()
    {
        fillImage.fillAmount = health.Value / 100f;
    }
}
```

### Included Binders (50+)

<details>
<summary><strong>Numerical</strong> — position, rotation, scale, text, fill, audio, animator, particles, material, navmesh</summary>

`NumericalPositionBinder`, `NumericalRotationBinder`, `NumericalScaleBinder`, `NumericalTextBinder`, `NumericalFillBinder`, `NumericalAudioBinder`, `NumericalAnimatorBinder`, `NumericalParticleBinder`, `NumericalMaterialBinder`, `NumericalNavMeshBinder`, `NumericalPositionSpeedBinder`, `NumericalRotationSpeedBinder`, `FloatLerpPositionBinder`, `GradientSamplerBinder`, `ImageFilledBinder`

</details>

<details>
<summary><strong>Color</strong> — image, sprite, line renderer, text</summary>

`ColorImageBinder`, `ColorSpriteBinder`, `ColorLineRendererBinder`, `ColorTextMeshProBinder`

</details>

<details>
<summary><strong>Bool</strong> — toggle, animator, particles, canvas group, GameObject active, component enable</summary>

`BoolToggleBinder`, `BoolAnimatorBinder`, `BoolParticleBinder`, `BoolCanvasGroupBinder`, `GameObjectActiveBinder`, `EnableComponentBinder`

</details>

<details>
<summary><strong>Transform & Physics</strong> — transform, rigidbody, rotation, velocity, rect transform</summary>

`TransformBinder`, `TransformFollowerBinder`, `Vector3PositionBinder`, `QuaternionRotationBinder`, `Vector2SpaceBinder`, `Rigidbody3DBinder`, `Rigidbody2DBinder`, `AngularVelocityBinder`, `IntVariableRotationBinder`, `RectTransformBinder`

</details>

<details>
<summary><strong>Visual</strong> — sprite, line renderer width, material property</summary>

`SpriteRendererBinder`, `SpriteRendererFlipBinder`, `LineRendererWidthBinder`, `MaterialPropertyBinder`

</details>

<details>
<summary><strong>UI (Two-Way)</strong> — slider, dropdown, input field, scroll rect</summary>

`SliderBinder`, `DropdownBinder`, `InputFieldBinder`, `ScrollRectBinder`

</details>

<details>
<summary><strong>Other</strong> — text, camera, light, canvas group, animator, audio</summary>

`TextMeshProBinder`, `CameraBinder`, `LightBinder`, `CanvasGroupBinder`, `AnimatorBinder`, `EventAnimatorBinder`, `AudioEventPlayer`

</details>

---

## Drivers (Write)

Drivers **write** to a variable from an external source — the counterpart to binders.

Every driver has a **Silent Updates** toggle. When enabled, values are written via `SetValueWithoutNotify` to avoid per-frame event overhead. Pair with Poll mode binders on the read side.

### Writing Custom Drivers

```csharp
public class MousePositionDriver : VariableDriver<Vector3Variable>
{
    [SerializeField] private Vector3Variable mousePosition;
    protected override Vector3Variable Variable => mousePosition;

    private void Update()
    {
        if (SilentUpdates)
            Variable.SetValueWithoutNotify(Input.mousePosition);
        else
            Variable.Value = Input.mousePosition;
    }
}
```

### Included Drivers (12)

| Driver | Source | Target | Notes |
|---|---|---|---|
| `InputActionFloatDriver` | Input System axis/trigger | `FloatVariable` | Per-frame polling |
| `InputActionVector2Driver` | Input System move/look | `Vector2Variable` | Per-frame polling |
| `InputActionButtonDriver` | Input System button | `BoolVariable` | Event-driven |
| `TransformDriver` | Transform position/rotation/scale | `Vector3Variable`, `QuaternionVariable` | Local/world space toggle |
| `CollisionDriver` | Physics collision enter/exit | `BoolVariable`, `GameObjectVariable` | Optional tag filter |
| `TriggerDriver` | Physics trigger enter/exit | `BoolVariable`, `GameObjectVariable` | Optional tag filter |
| `DistanceDriver` | Distance between two transforms | `FloatVariable` | Squared distance option |
| `TimerDriver` | Count up/down timer | `FloatVariable` | Loop, auto-start, fires GameEvent |
| `RaycastDriver` | Physics raycast | `BoolVariable`, `FloatVariable`, `Vector3Variable` | Configurable direction/layer |
| `VelocityDriver` | Rigidbody velocity | `Vector3Variable`, `FloatVariable` | Speed + angular velocity |
| `AnimatorStateDriver` | Animator parameter | `FloatVariable`, `IntVariable`, `BoolVariable` | Hash-cached |
| `CooldownDriver` | Timed cooldown | `BoolVariable`, `FloatVariable` | Trigger/cancel API |

---

## Conditions (Logic)

Build complex boolean logic by wiring condition nodes together in a visual graph editor. The result drives UnityEvents or writes to a `BoolVariable` — no code needed.

### Condition Graph

Create via *Right-click > Create > ReactiveVars > Conditions > Condition Graph*. Double-click to open the visual node editor.

**Node types:**

| Node | Description |
|---|---|
| `ComparisonNode` | Compare a numerical variable (>, <, ==, !=, >=, <=) |
| `BoolCheckNode` | Check if a BoolVariable matches an expected value |
| `AndNode` | True when ALL connected inputs are true |
| `OrNode` | True when ANY connected input is true |
| `NotNode` | Inverts a single input |
| `RangeNode` | Check if a numerical variable is within min/max |

The graph automatically subscribes to all referenced variables and re-evaluates reactively. Right-click any node to set it as the output.

### Condition Listener

Add a `ConditionListener` component to respond to condition changes:

```csharp
// Configured entirely in the inspector:
// - Assign a ConditionGraph
// - Wire onConditionTrue / onConditionFalse / onConditionChanged UnityEvents
// - Optionally write to a BoolVariable
```

### Graph Editor Features

The visual editor supports zoom, pan, selection, node creation via right-click context menu, minimap, and undo/redo. Output nodes are highlighted with an orange border.

---

## Tween System

Smooth value interpolation managed by a `VariableTweener` MonoBehaviour. Add one to your scene — it updates all registered tweenables each frame with a configurable global speed multiplier.

### Tweenable Types

All types support `AnimationCurve` easing. Pass `null` for linear interpolation.

| Type | Interpolation | Use Case |
|---|---|---|
| `TweenableFloat` | `Mathf.Lerp` | UI alpha, progress bars |
| `TweenableColor` | `Color.Lerp` | Damage flash, day/night cycle |
| `TweenableVector3` | `Vector3.Lerp` | Smooth movement |
| `TweenableVector2` | `Vector2.Lerp` | UI anchoring |
| `TweenableQuaternion` | `Quaternion.Slerp` | Smooth rotation |
| `TweenableNumerical` | `Mathf.Lerp` → `SetFromFloat` | Tween any `INumericalVariable` directly |
| `TransformTweenable` | Position + Rotation Slerp | Camera transitions, pivots |

### Basic Usage

```csharp
[SerializeField] private VariableTweener tweener;
private TweenableFloat _alpha;

void Start()
{
    _alpha = new TweenableFloat(tweener, v => canvasGroup.alpha = v, rate: 3f);
}

void FadeIn()  => _alpha.Value = 1f;  // Tweens from current to 1
void FadeOut() => _alpha.Value = 0f;  // Tweens from current to 0
```

### Tweening Variables Directly

`TweenableNumerical` writes into any `INumericalVariable`. Uses `SetFromFloatWithoutNotify` during interpolation (no per-frame event overhead), then fires a single notification on completion.

```csharp
[SerializeField] private FloatVariable health;
[SerializeField] private VariableTweener tweener;
private TweenableNumerical _healthTween;

void Start()
{
    _healthTween = new TweenableNumerical(health, tweener, rate: 2f);
}

void HealTo(float target) => _healthTween.Target = target;
```

Set the health bar binder to **Poll** mode so it reads the silently-updating value each frame.

---

## Variable Containers

Group related variables and events into a single asset for organization and bulk operations.

```csharp
public VariableContainer playerStats;

void Start()
{
    var health = playerStats.GetVariable<FloatVariable>("Health");
    var score  = playerStats.GetVariable<IntVariable>("Score");
}

void SaveGame()  => playerStats.SaveToFile("save.json");
void LoadGame()  => playerStats.LoadFromFile("save.json");
void ResetAll()  => playerStats.ResetAllVariables();
```

---

## Utilities

### Variable Resetter

Reset variables to their default values on lifecycle events or when a GameEvent fires.

```
Add component: ReactiveVars > Utility > Variable Resetter
- Assign variables to reset
- Choose trigger: Manual, OnEnable, OnDisable, OnDestroy
- Optionally assign a GameEvent as reset trigger
```

### Event Relay

Forward a GameEvent to another GameEvent or UnityEvent, with optional delay.

### Variable Logger

Debug component that logs value changes to the console.

### Variable Debug Overlay

Runtime on-screen display of all ScriptableVariable values in the scene. Toggle with F1 (configurable). Auto-discovers variables from scene binders and drivers. Includes search filtering, color-coded types, and configurable screen position.

```
Add component: ReactiveVars > Utility > Variable Debug Overlay
- Press F1 in play mode to toggle
- Variables are color-coded by type
- Search bar filters by name
```

---

## Editor Tools

| Tool | Description |
|---|---|
| **Scriptable System Window** | Browse, search, and create all variables and events in the project |
| **Condition Graph Editor** | Visual node-graph editor for building condition logic |
| **Variable Inspector** | Shows which scene objects reference a selected variable |
| **GameEvent Inspector** | Debug events with a test Raise button |
| **ConditionGraph Inspector** | Summary view with "Open in Graph Editor" button and play-mode evaluation |
| **VariableReference Drawer** | Inline constant/variable toggle in the inspector |
| **NumericalReference Drawer** | Constant/variable toggle for numeric references |
| **ReadOnly Attribute** | `[ReadOnly]` — mark any serialized field as non-editable |

---

## Architecture

```
ScriptableObject
 └─ GameEvent                      event with OnRaised observable
     └─ GameEvent<T>               typed event with data payload
         └─ ScriptableVariable     abstract base
             └─ ScriptableVariable<T>   Value, OnValueChanged, SetValueWithoutNotify
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

 └─ ConditionGraph                 visual condition logic
     └─ ConditionNode [SerializeReference]
         ├─ ComparisonNode
         ├─ BoolCheckNode
         ├─ AndNode / OrNode / NotNode
         └─ RangeNode

MonoBehaviour
 ├─ VariableBinder<T>              read, subscribe / poll
 │   └─ NumericalVariableBinder    INumericalVariable specialization
 ├─ TwoWayVariableBinder<T>        read + write, recursion guard
 ├─ VariableBinder<T1,T2>          multi-variable observer
 ├─ VariableBinder<T1,T2,T3>       multi-variable observer
 ├─ VariableDriver<T>              write, silent option
 │    ├─ InputActionFloatDriver
 │    ├─ CollisionDriver / TriggerDriver
 │    ├─ TimerDriver / CooldownDriver
 │    └─ ... (8 more)
 ├─ ConditionListener              condition → UnityEvent bridge
 ├─ VariableResetter               bulk reset on triggers
 ├─ EventRelay                     event forwarding with delay
 ├─ VariableLogger                 debug logging
 └─ VariableDebugOverlay           runtime HUD

ITweenable
 ├─ TweenableFloat
 ├─ TweenableColor
 ├─ TweenableVector2 / Vector3
 ├─ TweenableQuaternion
 ├─ TweenableNumerical             bridges INumericalVariable
 └─ TransformTweenable
```

---

## License

See [LICENSE](LICENSE) for details.
