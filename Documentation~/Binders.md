# The Binder System

**Navigation:** [Documentation home](README.md) · **Previous:** [Recipes](Recipes.md) · **Next:** [TweenSystem.md](TweenSystem.md)

Binders are components that **read** from variables and automatically update scene objects whenever the variable changes. This guide covers all 50+ binder types and shows you how to use them.

---

## What Is a Binder?

A **Binder** watches a variable and pushes its value into a component (UI text, image fill, animator parameter, etc.). You assign the variable in the inspector—no code required.

### How It Works

1. You add a binder component to a GameObject
2. You assign a variable in the inspector
3. The binder subscribes to the variable
4. Whenever the variable changes, the binder updates the target component
5. That's it!

```
Variable (FloatVariable: Health = 75)
    ↓
  [Binder reads value]
    ↓
  [Updates target: Image.fillAmount = 0.75]
```

---

## Update Modes

Every binder exposes an **Update Mode** dropdown with two options:

### Subscribe Mode (Default)

- Binder reacts to variable change events
- **Zero overhead** when the variable isn't changing
- Ideal for normal use cases
- Fires once per value change

**Use when:** Variable changes occasionally (player takes damage, score updates, etc.)

### Poll Mode

- Binder reads the value every frame in `Update()`
- Use when a **Tween** or **Driver** is silently pushing values each frame
- Pairs with driver **Silent Updates** for efficiency

**Use when:**
- A `TweenableNumerical` is smoothly interpolating a variable
- A driver with **Silent Updates** enabled is writing per-frame
- You need continuous monitoring regardless of events

---

## Numerical Binders

Binders that work with any `INumericalVariable` (FloatVariable, IntVariable, DoubleVariable).

### NumericalTextBinder

Display a number as text with optional formatting.

**Add to:** TextMeshPro or Text component

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ScriptableVariable | The numerical variable to bind |
| **Format String** | String | Format like `"{0:F1}"` for one decimal place, `"{0:P0}"` for percentage |
| **Update Mode** | Enum | Subscribe or Poll |

**Example Setup:**
```
TextMeshPro object:
├─ Component: TextMeshProUGUI
└─ Component: NumericalTextBinder
   ├─ Variable: FloatVariable (PlayerHealth)
   └─ Format String: "Health: {0:F0}"
```

**Result:** Displays "Health: 75" as the variable changes.

### NumericalFillBinder

Bind a numerical variable to an Image component's `fillAmount` (0 to 1).

**Add to:** Image component

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ScriptableVariable | The numerical variable |
| **Max Value** | Float | The value that represents 100% fill (e.g., 100 for health) |
| **Inverse** | Bool | If true, fill decreases as value increases (inverted progress) |
| **Update Mode** | Enum | Subscribe or Poll |

**Example Setup:**
```
Health Bar (Image):
├─ Component: Image (fillAmount = 0.5)
└─ Component: NumericalFillBinder
   ├─ Variable: FloatVariable (PlayerHealth)
   ├─ Max Value: 100
   └─ Inverse: Off
```

**Result:** Fills from 0 (empty) to 1 (full) as health goes from 0 to 100.

### NumericalPositionBinder

Bind a numerical variable to move a GameObject along a single axis.

**Add to:** Any GameObject with Transform

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ScriptableVariable | The numerical variable |
| **Axis** | Enum | X, Y, or Z axis |
| **Space** | Enum | Local or World space |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Move an elevator up as a progress variable goes from 0 to 100.

### NumericalRotationBinder

Bind a numerical variable to rotate a GameObject around a single axis.

**Add to:** Any GameObject with Transform

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ScriptableVariable | The numerical variable (0-360 for degrees) |
| **Axis** | Enum | X, Y, or Z axis |
| **Space** | Enum | Local or World space |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Spin a steering wheel as an input variable changes.

### NumericalScaleBinder

Bind a numerical variable to scale a GameObject.

**Add to:** Any GameObject with Transform

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ScriptableVariable | The scale multiplier |
| **Axis** | Enum | X, Y, Z, or Uniform (all axes) |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Grow a health orb as its size variable increases.

### NumericalAnimatorBinder

Bind a numerical variable to an Animator parameter (float, int, or trigger).

**Add to:** GameObject with Animator component

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ScriptableVariable | The numerical variable |
| **Parameter Name** | String | Animator parameter name (e.g., "Speed", "Damage") |
| **Parameter Type** | Enum | Float, Int, or Trigger |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Drive animation speed from a velocity variable.

### NumericalAudioBinder

Bind a numerical variable to AudioSource volume or pitch.

**Add to:** GameObject with AudioSource

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ScriptableVariable | The control variable (0 to 1 for volume, pitch scale) |
| **Property** | Enum | Volume or Pitch |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Fade audio out as distance to player increases.

### NumericalMaterialBinder

Bind a numerical variable to a material shader property.

**Add to:** Renderer component (MeshRenderer, SkinnedMeshRenderer)

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ScriptableVariable | The numerical variable |
| **Property Name** | String | Shader property name (e.g., "_Metallic", "_Smoothness") |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Adjust a material's metallic value based on a quality variable.

### NumericalParticleBinder

Bind a numerical variable to a Particle System emission rate or velocity.

**Add to:** GameObject with ParticleSystem

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ScriptableVariable | The rate or velocity value |
| **Property** | Enum | Emission Rate or Velocity Magnitude |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Increase particle emission as damage is taken.

### NumericalNavMeshBinder

Bind a numerical variable to a NavMeshAgent's desired speed.

**Add to:** GameObject with NavMeshAgent

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ScriptableVariable | The desired speed |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Speed up an NPC as they become more alert.

### NumericalPositionSpeedBinder

Smoothly move toward a target based on a speed variable.

**Add to:** GameObject with Transform

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ScriptableVariable | Speed value |
| **Target** | Transform | What to move toward |
| **Space** | Enum | Local or World space |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Chase player at variable speed.

### NumericalRotationSpeedBinder

Smoothly rotate toward a target based on a speed variable.

**Add to:** GameObject with Transform

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ScriptableVariable | Angular speed value |
| **Target** | Transform | What to rotate toward |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Look at player at variable turn speed.

---

## Boolean Binders

Binders that bind `BoolVariable` to visual or functional states.

### GameObjectActiveBinder

Show/hide a GameObject based on a bool variable.

**Add to:** Any GameObject

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | BoolVariable | The bool variable |
| **Inverted** | Bool | If true, hides when true (inverts the logic) |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Show/hide a pause menu based on `isPaused` variable.

### EnableComponentBinder

Enable/disable a component based on a bool variable.

**Add to:** Any GameObject with a component to control

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | BoolVariable | The bool variable |
| **Target Component** | Component | The component to enable/disable |
| **Inverted** | Bool | If true, disables when true |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Disable a collider when `isPhasing` is true.

### BoolToggleBinder

Bind a bool variable to a Toggle (UI).

**Add to:** GameObject with Toggle component

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | BoolVariable | The bool variable |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Sync a settings toggle with a game state boolean.

### BoolAnimatorBinder

Bind a bool variable to an Animator parameter.

**Add to:** GameObject with Animator

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | BoolVariable | The bool variable |
| **Parameter Name** | String | Animator parameter name |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Play death animation when `isDead` becomes true.

### BoolCanvasGroupBinder

Show/hide a UI element by controlling CanvasGroup alpha and interactability.

**Add to:** Canvas element with CanvasGroup

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | BoolVariable | The bool variable |
| **Alpha When False** | Float | Alpha value when variable is false (default 0 = hidden) |
| **Alpha When True** | Float | Alpha value when variable is true (default 1 = visible) |
| **Block Raycast** | Bool | If true, also blocks raycasts based on variable |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Fade UI panel in/out smoothly.

### BoolParticleBinder

Play/stop a Particle System based on a bool variable.

**Add to:** GameObject with ParticleSystem

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | BoolVariable | The bool variable |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Emit healing particles when `isHealing` is true.

---

## Color Binders

Binders that bind `ColorVariable` to visual elements.

### ColorImageBinder

Bind a color variable to an Image component's color.

**Add to:** Image component

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ColorVariable | The color variable |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Change button color based on state.

### ColorSpriteBinder

Bind a color variable to a SpriteRenderer's color.

**Add to:** GameObject with SpriteRenderer

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ColorVariable | The color variable |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Tint an enemy red when taking damage.

### ColorLineRendererBinder

Bind a color variable to a LineRenderer's color.

**Add to:** GameObject with LineRenderer

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ColorVariable | The color variable |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Change a laser beam color based on charge level.

### ColorTextMeshProBinder

Bind a color variable to TextMeshPro text color.

**Add to:** TextMeshPro object

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ColorVariable | The color variable |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Warn player with red text when health is low.

### GradientSamplerBinder

Sample a Gradient based on a numerical variable (0 to 1) and apply the sampled color.

**Add to:** Any visual element (SpriteRenderer, Image, etc.)

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ScriptableVariable | The numerical variable (0-1) |
| **Gradient** | Gradient | The gradient to sample |
| **Target Property** | Enum | Which component to color (Image, SpriteRenderer, etc.) |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Color-code a progress bar (red → yellow → green).

---

## Transform Binders

Binders that bind vectors to transform properties.

### Vector3PositionBinder

Bind a Vector3 variable to a GameObject's position.

**Add to:** Any GameObject with Transform

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | Vector3Variable | The position vector |
| **Space** | Enum | Local or World space |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Move a camera to a target position variable.

### QuaternionRotationBinder

Bind a Quaternion variable to a GameObject's rotation.

**Add to:** Any GameObject with Transform

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | QuaternionVariable | The rotation quaternion |
| **Space** | Enum | Local or World space |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Rotate a turret toward a target rotation variable.

### TransformBinder

Bind separate variables to position and rotation simultaneously.

**Add to:** Any GameObject with Transform

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Position Variable** | Vector3Variable | The position |
| **Rotation Variable** | QuaternionVariable | The rotation |
| **Space** | Enum | Local or World space |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Sync a networked player position and rotation.

### TransformFollowerBinder

Smoothly follow a target's position and rotation at variable speed.

**Add to:** Any GameObject with Transform

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Target** | Transform | What to follow |
| **Position Speed** | Float | How fast to follow position |
| **Rotation Speed** | Float | How fast to follow rotation |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Camera smoothly follows player.

### Vector2SpaceBinder

Bind a Vector2 variable to world or screen space position.

**Add to:** GameObject (often UI)

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | Vector2Variable | The position vector |
| **Space** | Enum | World or Screen space |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Position UI element based on touch input variable.

---

## Physics Binders

Binders that control physics bodies.

### Rigidbody3DBinder

Bind a Vector3 variable to a Rigidbody's velocity.

**Add to:** GameObject with Rigidbody

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | Vector3Variable | The velocity vector |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Move a physics object based on input variable.

### Rigidbody2DBinder

Bind a Vector2 variable to a Rigidbody2D's velocity.

**Add to:** GameObject with Rigidbody2D

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | Vector2Variable | The velocity vector |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Move a 2D physics object based on input variable.

### AngularVelocityBinder

Bind a numerical variable to angular velocity (rotation speed).

**Add to:** GameObject with Rigidbody or Rigidbody2D

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ScriptableVariable | The angular velocity magnitude |
| **Rigidbody Dimension** | Enum | 3D or 2D |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Spin a physics object at variable speed.

---

## UI Binders (Two-Way)

These binders can both read **and** write, useful for UI controls that need to sync with variables.

### SliderBinder

Bidirectional sync between a Slider and a numerical variable.

**Add to:** GameObject with Slider

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ScriptableVariable | The numerical variable |
| **Min Value** | Float | Slider minimum |
| **Max Value** | Float | Slider maximum |
| **Whole Numbers** | Bool | If true, rounds to integers |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:**
- Moving slider writes to variable
- Code changing variable updates slider

### InputFieldBinder

Bidirectional sync between an InputField and a text variable.

**Add to:** GameObject with InputField

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | TextVariable | The text variable |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Player name input syncs with a TextVariable.

### DropdownBinder

Bidirectional sync between a Dropdown and an int variable (selected index).

**Add to:** GameObject with Dropdown

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | IntVariable | The selected index variable |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Difficulty selector syncs with difficulty level variable.

### StringListDropdownBinder

Dropdown bound to a StringListVariable for dynamic options.

**Add to:** GameObject with Dropdown

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | StringListVariable | The list of options |
| **Selected Index Variable** | IntVariable | (Optional) tracks selected item |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Weapon selector with dynamic weapon list.

### ScrollRectBinder

Bidirectional sync for ScrollRect normalized position.

**Add to:** GameObject with ScrollRect

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | Vector2Variable | The normalized scroll position (0-1) |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Save/restore scroll position in a list.

---

## Text Binders

Binders for text components.

### TextMeshProBinder

Bind a TextVariable to TextMeshPro text content.

**Add to:** TextMeshPro object

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | TextVariable | The text variable |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Display player name from a text variable.

---

## Visual Binders

Binders for rendering and visual elements.

### SpriteRendererBinder

Bind a SpriteVariable to a SpriteRenderer.

**Add to:** GameObject with SpriteRenderer

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | SpriteVariable | The sprite variable |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Change character sprite based on equipment state.

### SpriteRendererFlipBinder

Bind a bool variable to SpriteRenderer flip X or Y.

**Add to:** GameObject with SpriteRenderer

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | BoolVariable | The flip state |
| **Flip Axis** | Enum | X or Y |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Flip sprite when character changes direction.

### LineRendererWidthBinder

Bind a numerical variable to LineRenderer width.

**Add to:** GameObject with LineRenderer

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ScriptableVariable | The width value |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Thicken a laser beam as power increases.

---

## Light & Camera Binders

Binders for lighting and camera control.

### LightBinder

Bind variables to light properties (intensity, range, color).

**Add to:** GameObject with Light component

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Intensity Variable** | ScriptableVariable | Controls light intensity |
| **Range Variable** | ScriptableVariable | Controls light range |
| **Color Variable** | ColorVariable | Controls light color |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Pulse a warning light's intensity.

### CameraBinder

Bind variables to camera properties (field of view, position).

**Add to:** GameObject with Camera component

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **FOV Variable** | ScriptableVariable | Controls field of view |
| **Position Variable** | Vector3Variable | Controls camera position |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Zoom camera based on focus variable.

---

## Animation & Audio Binders

Binders for animation and sound control.

### EventAnimatorBinder

Trigger animator events (animation completion) and expose as variable changes.

**Add to:** GameObject with Animator

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Animation Event** | String | Name of the animation event to trigger |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Detect when an animation finishes.

### AnimatorBinder

Bind variables to all animator parameters simultaneously.

**Add to:** GameObject with Animator

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Animator** | Animator | Reference to the animator |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Sync entire animator state from variables.

### AudioSourceBinder

Control AudioSource volume, pitch, or playback.

**Add to:** GameObject with AudioSource

**Inspector Fields:**

| Field | Type | Purpose |
|-------|------|---------|
| **Volume Variable** | ScriptableVariable | Controls volume (0-1) |
| **Pitch Variable** | ScriptableVariable | Controls pitch (0.1-3) |
| **Update Mode** | Enum | Subscribe or Poll |

**Example:** Fade music volume based on scene state.

---

## Creating Custom Binders

All built-in binders inherit from base classes in the framework. To create your own:

```csharp
using Shababeek.ReactiveVars;
using UnityEngine;

public class MyCustomBinder : VariableBinder<FloatVariable>
{
    [SerializeField] private FloatVariable health;
    [SerializeField] private MyComponent myComponent;

    protected override FloatVariable Variable => health;

    protected override void Bind()
    {
        // Called once when binding starts
        OnVariableChanged();
    }

    protected override void OnVariableChanged()
    {
        // Called whenever the variable changes
        myComponent.SetValue(health.Value);
    }
}
```

**Base Classes:**

| Base | Use For |
|------|---------|
| `VariableBinder<T>` | Single variable → single target |
| `NumericalVariableBinder` | Any INumericalVariable |
| `TwoWayVariableBinder<T>` | Bidirectional UI sync with recursion guards |
| `VariableBinder<T1, T2>` | Two variables → one target |
| `VariableBinder<T1, T2, T3>` | Three variables → one target |

See the source code for implementation details.

---

## Common Patterns

### Pattern 1: Health Bar with Text and Fill

```
Canvas
├─ HealthBar (Image)
│  └─ NumericalFillBinder → PlayerHealth
└─ HealthText (TextMeshPro)
   └─ NumericalTextBinder → PlayerHealth (format: "HP: {0:F0}")
```

### Pattern 2: UI Slider Synced with Settings

```
Settings Panel
└─ VolumeSlider (Slider)
   └─ SliderBinder → VolumeLevel (bidirectional sync)
```

When user moves slider, variable updates. When code changes variable, slider updates.

### Pattern 3: Animated Progress Bar with Color

```
ProgressBar (Image)
├─ NumericalFillBinder → ProgressAmount
├─ GradientSamplerBinder → ProgressAmount (gradient: red → yellow → green)
└─ NumericalTextBinder → ProgressAmount (format: "{0:P0}")
```

---

## Troubleshooting

### "Binder isn't updating when variable changes"

**Check:**
1. Are you using the **same variable asset** in code and binder?
2. Is the binder's **GameObject active**?
3. Is the binder's **component enabled**?
4. Is the **Update Mode** set correctly (Subscribe for events, Poll for tweens)?

### "The binder updates, but my tween doesn't work"

**Solution:** If using `TweenableNumerical`, set the binder's Update Mode to **Poll** so it reads silently-tweened values each frame.

### "Can I bind multiple variables to one binder?"

**Yes!** Use binders that accept multiple variables:
- `VariableBinder<T1, T2>` — for two variables
- `VariableBinder<T1, T2, T3>` — for three variables

Or create a custom binder.

---

## Next Steps

- **[GettingStarted.md](GettingStarted.md)** — Basic setup and first steps
- **[TweenSystem.md](TweenSystem.md)** — Smooth animation with tweens
- **[VariableContainer.md](VariableContainer.md)** — Organize variables
- **Main README** — Full system reference
