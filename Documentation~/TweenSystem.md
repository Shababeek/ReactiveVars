# Tween System — Smooth Value Interpolation

**Navigation:** [Documentation home](README.md) · **Previous:** [Binders.md](Binders.md) · **Next:** [VariableContainer.md](VariableContainer.md)

The Tween System smoothly interpolates values over time instead of snapping them instantly. This adds visual polish to UI fades, camera movements, progress bar fills, and more.

---

## What Is Tweening?

**Tweening** is smooth interpolation between two values over a duration. Instead of health snapping from 100 to 50, it smoothly counts down. Instead of UI fading instantly, it gradually becomes transparent.

```
Without Tween:        With Tween:
Health: 100           Health: 100
Health: 50 (snap)     Health: 95
                      Health: 90
                      Health: 80
                      Health: 60
                      Health: 50 (over 1 second)
```

---

## Setting Up the Tween System

### Add a VariableTweener

First, you need a **VariableTweener** component in your scene to manage all tweens:

1. Create an empty GameObject (e.g., "Tweener")
2. Add component: **VariableTweener**
3. Adjust **Tween Scale** if needed (default 1.0 = normal speed, 2.0 = double speed)

The tweener updates all active tweens each frame. You only need **one tweener per scene**.

### Inspector Fields

| Field | Type | Purpose |
|-------|------|---------|
| **Tween Scale** | Float | Global speed multiplier (1.0 = normal, 0.5 = half speed, 2.0 = double) |

---

## Tweenable Types

The Tween System includes several tweenable types, all supporting `AnimationCurve` for custom easing.

### TweenableFloat

Smoothly interpolates a float value using `Mathf.Lerp`.

**Usage:**
```csharp
[SerializeField] private VariableTweener tweener;
private TweenableFloat alphaTween;

void Start()
{
    // Create a tweenable that updates canvasGroup.alpha
    alphaTween = new TweenableFloat(
        tweener: tweener,
        onChange: v => canvasGroup.alpha = v,
        rate: 2f,  // Speed multiplier
        value: 1f  // Initial value
    );
}

void FadeOut()
{
    alphaTween.Value = 0f;  // Tweens from current to 0
}

void FadeIn()
{
    alphaTween.Value = 1f;  // Tweens from current to 1
}
```

**Constructor Parameters:**

| Parameter | Type | Purpose |
|-----------|------|---------|
| `tweener` | VariableTweener | The manager that drives this tween |
| `onChange` | Action<float> | Called each frame with the interpolated value |
| `rate` | Float | Speed multiplier (how fast the tween progresses) |
| `value` | Float | Initial value (default 0) |
| `curve` | AnimationCurve | Easing curve (null = linear) |

### TweenableColor

Smoothly interpolates a color using `Color.Lerp`.

**Usage:**
```csharp
private TweenableColor colorTween;

void Start()
{
    colorTween = new TweenableColor(
        tweener: tweener,
        onChange: c => image.color = c,
        rate: 1.5f,
        value: Color.white
    );
}

void TakeDamage()
{
    colorTween.Value = Color.red;  // Flash red
    Invoke(nameof(ResetColor), 0.5f);
}

void ResetColor()
{
    colorTween.Value = Color.white;
}
```

### TweenableVector2

Smoothly interpolates a Vector2 using `Vector2.Lerp`.

**Usage:**
```csharp
private TweenableVector2 positionTween;

void Start()
{
    positionTween = new TweenableVector2(
        tweener: tweener,
        onChange: v => rectTransform.anchoredPosition = v,
        rate: 2f,
        value: Vector2.zero
    );
}

void MoveToCorner()
{
    positionTween.Value = new Vector2(500f, 500f);
}
```

### TweenableVector3

Smoothly interpolates a Vector3 using `Vector3.Lerp`.

**Usage:**
```csharp
private TweenableVector3 cameraTween;

void Start()
{
    cameraTween = new TweenableVector3(
        tweener: tweener,
        onChange: v => camera.transform.position = v,
        rate: 1f,
        value: Vector3.zero
    );
}

void MoveCamera(Vector3 target)
{
    cameraTween.Value = target;
}
```

### TweenableQuaternion

Smoothly interpolates a rotation using `Quaternion.Slerp`.

**Usage:**
```csharp
private TweenableQuaternion rotationTween;

void Start()
{
    rotationTween = new TweenableQuaternion(
        tweener: tweener,
        onChange: q => transform.rotation = q,
        rate: 1.5f,
        value: Quaternion.identity
    );
}

void TurnAround()
{
    rotationTween.Value = Quaternion.Euler(0, 180, 0);
}
```

### TweenableNumerical

Smoothly interpolates any `INumericalVariable` (FloatVariable, IntVariable, etc.) **directly**.

This is special: it writes into the variable using `SetFromFloatWithoutNotify()` during interpolation (no event spam), then fires a single notification when complete.

**Usage:**
```csharp
[SerializeField] private FloatVariable health;
[SerializeField] private VariableTweener tweener;
private TweenableNumerical healthTween;

void Start()
{
    healthTween = new TweenableNumerical(
        variable: health,
        tweener: tweener,
        rate: 2f  // How fast health animates
    );
}

void HealTo(float target)
{
    healthTween.Target = target;
}
```

**Important:** Set your health bar binder to **Poll** mode so it reads the smoothly-animating value each frame.

```csharp
// In your health bar binder inspector:
// Update Mode: Poll (not Subscribe)
```

This pairs the driver's silent updates with a poll-mode binder for smooth animation without event overhead.

### TransformTweenable

Simultaneously tweens position and rotation.

**Usage:**
```csharp
private TransformTweenable cameraTween;

void Start()
{
    cameraTween = new TransformTweenable(
        tweener: tweener,
        targetTransform: cameraTransform,
        positionRate: 1.5f,
        rotationRate: 1.5f,
        positionValue: Vector3.zero,
        rotationValue: Quaternion.identity,
        space: Space.World
    );
}

void CutToScene(Vector3 pos, Quaternion rot)
{
    cameraTween.SetTarget(pos, rot);
}
```

---

## Using Animation Curves for Easing

By default, tweens use linear interpolation. You can add easing curves for more natural motion:

```csharp
// Create with a linear default
var tween = new TweenableFloat(tweener, onChangeCallback, rate: 2f);

// Add an easing curve at runtime
AnimationCurve easeInOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
tween.SetCurve(easeInOutCurve);
```

**Common Easing Patterns:**

```csharp
// Ease In (slow start, fast end)
AnimationCurve.EaseInOut(0, 0, 1, 1);

// Linear (default)
null  // Passing null uses linear interpolation

// Custom curve
var customCurve = new AnimationCurve(
    new Keyframe(0, 0, 0, 1),
    new Keyframe(1, 1, 0, 0)
);
```

---

## Rate-Based vs. Duration-Based Tweening

The Tween System uses **rate-based tweening**: the `rate` parameter controls how fast `t` progresses from 0 to 1 per second.

- `rate: 1.0` — Takes ~1 second to complete
- `rate: 2.0` — Takes ~0.5 seconds to complete
- `rate: 0.5` — Takes ~2 seconds to complete

**Formula:** `t += rate * deltaTime * tweenScale`

The `VariableTweener.TweenScale` global multiplier affects all tweens:

```csharp
tweener.TweenScale = 0.5f;  // All tweens half speed
tweener.TweenScale = 2.0f;  // All tweens double speed
```

---

## Common Patterns

### Pattern 1: Fade UI Panel In/Out

```csharp
[SerializeField] private VariableTweener tweener;
[SerializeField] private CanvasGroup panel;
private TweenableFloat fadeTween;

void Start()
{
    fadeTween = new TweenableFloat(
        tweener,
        v => panel.alpha = v,
        rate: 3f,  // Fast fade
        value: 1f
    );
}

void ShowPanel()
{
    panel.gameObject.SetActive(true);
    fadeTween.Value = 1f;
}

void HidePanel()
{
    fadeTween.Value = 0f;
    // Hide after tween completes
}
```

### Pattern 2: Damage Flash (Color)

```csharp
private TweenableColor colorTween;

void Start()
{
    colorTween = new TweenableColor(
        tweener,
        c => spriteRenderer.color = c,
        rate: 4f,
        value: Color.white
    );
}

void OnDamage()
{
    colorTween.Value = Color.red;
}

void Update()
{
    // Auto-reset after 0.2 seconds
    if (colorTween.Value == Color.red && Time.time - lastDamageTime > 0.2f)
    {
        colorTween.Value = Color.white;
    }
}
```

### Pattern 3: Smooth Camera Movement

```csharp
private TweenableVector3 cameraTween;

void Start()
{
    cameraTween = new TweenableVector3(
        tweener,
        v => camera.transform.position = v,
        rate: 1.5f,
        value: camera.transform.position
    );
}

void MoveToPoint(Vector3 target)
{
    cameraTween.Value = target;
}
```

### Pattern 4: Animated Variable Display

Tween a health variable and display it smoothly:

```csharp
[SerializeField] private FloatVariable playerHealth;
[SerializeField] private VariableTweener tweener;
private TweenableNumerical healthDisplay;

void Start()
{
    healthDisplay = new TweenableNumerical(playerHealth, tweener, rate: 2f);
}

void TakeDamage(float amount)
{
    // Tween from current to new value
    healthDisplay.Target = Mathf.Max(0, playerHealth.Value - amount);
}

// In scene, add a health bar with NumericalFillBinder → playerHealth
// Set binder to Poll mode so it reads the tweened value each frame
```

---

## Events and Completion

Each tweenable fires events when values change:

```csharp
var tween = new TweenableFloat(tweener, onChangeCallback, rate: 2f);

// Listen for completion
// (The OnFinished event is fired once when the tween reaches its target)
```

For `TweenableFloat` and other types, you can subscribe to completion:

```csharp
var tween = new TweenableFloat(tweener, onChangeCallback, rate: 2f);

tween.OnFinished += () =>
{
    Debug.Log("Tween complete!");
};
```

---

## Performance Tips

### 1. Use Silent Updates with TweenableNumerical

When tweening a variable that drives multiple binders, use `TweenableNumerical` to avoid per-frame events:

```csharp
// Good: One event when complete
var healthTween = new TweenableNumerical(healthVariable, tweener, rate: 2f);

// Less efficient: Event every frame
healthVariable.Value = newValue;  // Fires OnValueChanged each frame
```

Pair with **Poll mode binders** to read the value each frame.

### 2. Pool Tweens if Creating Many

If you create/destroy tweens frequently, consider object pooling.

### 3. Adjust Tween Scale for Global Control

```csharp
// Pause all tweens
tweener.TweenScale = 0f;

// Resume at normal speed
tweener.TweenScale = 1f;

// Slow motion effect
tweener.TweenScale = 0.25f;
```

---

## Troubleshooting

### "Tween isn't updating"

**Check:**
1. Is the `VariableTweener` component **active** and **enabled**?
2. Are you setting `tween.Value` to start the tween? (Just creating doesn't start it)
3. Is `rate` set to a positive value?

### "Binder isn't seeing the tweened value"

**Solution:** Set the binder's **Update Mode** to **Poll** instead of Subscribe. This makes it read the value each frame even though no events are firing.

### "Tween is too fast/slow"

**Adjust the `rate` parameter:**
```csharp
new TweenableFloat(tweener, onChangeCallback, rate: 4f);  // Faster
new TweenableFloat(tweener, onChangeCallback, rate: 1f);  // Slower
```

Or use the global `TweenScale`:
```csharp
tweener.TweenScale = 2f;  // All tweens 2x faster
```

### "I want a tween with a specific duration"

Calculate the rate from desired duration:

```csharp
float desiredDuration = 2f;  // seconds
float rate = 1f / desiredDuration;  // rate = 0.5

new TweenableFloat(tweener, onChangeCallback, rate: rate);
```

---

## Next Steps

- **[GettingStarted.md](GettingStarted.md)** — Basic setup
- **[Binders.md](Binders.md)** — All binder types
- **[VariableContainer.md](VariableContainer.md)** — Variable organization
- **Main README** — Full system reference
