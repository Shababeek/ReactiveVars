# Getting Started with ReactiveVars

Welcome! This guide will walk you through the fundamentals of ReactiveVars and have you building reactive systems in minutes.

---

## What Is ReactiveVars?

ReactiveVars is a **data-driven, event-based system** for decoupling your game logic from your UI and gameplay systems. Instead of having scripts directly reference each other, they all point to shared **Scriptable Variable assets** that live in your project.

### The Core Idea

Think of it like a message board:
- One system writes a value (e.g., `PlayerHealth = 50`)
- Other systems watch the board and react when the value changes
- Nobody has to know about anybody else

This eliminates hard-coded references, simplifies testing, and makes your systems reusable.

---

## Five-Minute Quick Start

### Step 1: Create a Variable

1. In the **Project window**, right-click in any folder
2. Select **Create > Shababeek > ReactiveVars > Variables > FloatVariable**
3. Name it `PlayerHealth`

You now have a shared asset that any GameObject can read or write to.

### Step 2: Create a UI Text Display

1. Add a **TextMeshPro** object to your scene (if you don't have one, create **UI > TextMeshPro - Text**)
2. Select it and add the component **NumericalTextBinder**
3. Drag your `PlayerHealth` variable into the **Variable** field

The text on your TextMeshPro will now display the variable's current value.

### Step 3: Write a Value

Create a simple script and attach it to any object:

```csharp
using Shababeek.ReactiveVars;
using UnityEngine;

public class PlayerDamage : MonoBehaviour
{
    public FloatVariable playerHealth;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerHealth.Value -= 10f;  // Ouch!
        }
    }
}
```

Assign your `PlayerHealth` variable to the `playerHealth` field in the inspector.

### Step 4: Test It

Press Play. Every time you press **Space**, the health decreases and the UI text updates automatically.

**That's the entire pattern.** Variables are read/written. Binders display them. Drivers feed data in. No manual event wiring needed.

---

## Core Concepts

### Variables

A variable is a **ScriptableObject asset** that holds a single value (float, int, bool, color, vector, etc.). Any system can read or write it.

**Create one:**
- Right-click in Project > **Create > Shababeek > ReactiveVars > Variables** > pick a type

**Access it from code:**
```csharp
public FloatVariable myHealth;

void TakeDamage(float amount)
{
    myHealth.Value -= amount;  // All binders watching this variable update automatically
}

void SubscribeToChanges()
{
    myHealth.OnValueChanged.Subscribe(newValue =>
        Debug.Log($"Health is now: {newValue}")
    );
}
```

### Binders

A **Binder** is a component that watches a variable and updates something visual (UI text, image fill, animator parameter, etc.) whenever the variable changes.

**Add one:**
1. Select a GameObject (e.g., a TextMeshPro or Image)
2. Add component: **NumericalTextBinder** (or any binder type)
3. Drag your variable into the **Variable** field
4. Done!

**No code required.** The binder handles the subscription and updates automatically.

### Drivers

A **Driver** is a component that **writes** values into a variable from external sources like player input, physics, timers, or distance checks.

**Add one:**
1. Select a GameObject (e.g., the player or a timer)
2. Add component: **TimerDriver** (or any driver type)
3. Assign the variable you want to write to
4. Adjust settings (e.g., count direction, loop)
5. Done!

The driver feeds data in; binders push it out. They work together.

### Events

An **Event** is a one-time signal with no permanent state. Use it when you care about a **moment** (player died, level completed, button pressed) rather than a **value** (health, score).

**Create one:**
- Right-click in Project > **Create > Shababeek > ReactiveVars > Events > GameEvent**

**Listen to it in the scene:**
- Add **GameEventListener** component to any GameObject
- Drag the event asset in
- Wire up UnityEvents to respond

---

## Using Variables in the Inspector (No Code)

### Creating Variables

Variables are created like any other ScriptableObject asset:

```
Right-click in Project > Create > Shababeek > ReactiveVars > Variables
```

**Variable Types Available:**

| Type | Use Case | Example |
|------|----------|---------|
| **FloatVariable** | Health, damage, speed, progress | Player health |
| **IntVariable** | Score, ammo, level | Ammunition count |
| **BoolVariable** | Toggles, conditions | Is paused, is dead |
| **ColorVariable** | Colors, gradients | Current theme color |
| **Vector3Variable** | Positions, directions | Target position |
| **Vector2Variable** | Screen positions, input | Joystick input |
| **TextVariable** | Strings, messages | Player name |
| **SpriteVariable** | Images, icons | Current weapon sprite |
| **AudioClipVariable** | Audio files | Background music |

### Inspector Fields for Variables

When you select a variable asset in the Project, the Inspector shows:

| Field | What It Does |
|-------|--------------|
| **Value** | The current stored value. Edit here to change it. |
| **Debug Name** | (Optional) A name for debugging purposes. |

That's it! Variables are intentionally simple.

---

## Setting Up Your First Binder

Let's bind a variable to a health bar UI element.

### Example 1: Health Bar Text

```
Scene setup:
└─ Canvas
   └─ HealthText (TextMeshPro)
```

**Steps:**
1. Select `HealthText`
2. Add component: **NumericalTextBinder**
3. Drag your `PlayerHealth` variable into the **Variable** field
4. In the **Format String** field, type: `Health: {0:F1}` (displays as "Health: 75.5")
5. Set **Update Mode** to **Subscribe** (default)

Now the text will automatically update whenever `PlayerHealth` changes.

### Example 2: Health Bar Image Fill

```
Scene setup:
└─ Canvas
   └─ HealthBar (Image with fillAmount)
```

**Steps:**
1. Select `HealthBar` (Image component)
2. Add component: **NumericalFillBinder**
3. Drag your `PlayerHealth` variable into the **Variable** field
4. Set **Max Value** to `100` (the full health)
5. Set **Inverse** to `Off` (fill grows as health increases)

The image fill will now represent health as a percentage (0 = empty, 1 = full).

### Update Modes

Every binder has an **Update Mode** dropdown:

| Mode | When to Use |
|------|-------------|
| **Subscribe** (default) | Normal usage. Binder reacts to variable change events. Zero overhead when idle. |
| **Poll** | Use when a **Tween** or **Driver with Silent Updates** is pushing values every frame. Binder reads the value each frame instead of waiting for events. |

---

## Setting Up Your First Driver

Drivers feed data into variables. Let's set up a timer.

### Example: Simple Countdown Timer

```
Scene setup:
└─ GameManager (empty GameObject)
```

**Steps:**
1. Select `GameManager`
2. Add component: **TimerDriver**
3. Drag a **FloatVariable** (e.g., `RoundTimer`) into the **Variable** field
4. Set **Start Value** to `60` (start at 60 seconds)
5. Set **Count Direction** to **Down** (count toward zero)
6. Check **Auto Start** (begins immediately)
7. Set **Loop** to **Off** (stops at zero)

Create a TextMeshPro UI element with a **NumericalTextBinder** pointing to the same `RoundTimer` variable, format it as `Time: {0:F1}`, and you have a working countdown timer with **zero code**.

### Driver Silent Updates

Every driver has a **Silent Updates** toggle:

- **Off (default)**: Each value change fires an event. Binders react immediately.
- **On**: Values are written without events. Use with **Poll** mode binders if the driver updates every frame (more efficient).

---

## Inspector Field Reference

### Variable Inspector

| Field | Type | Purpose |
|-------|------|---------|
| **Value** | T (varies) | The current stored value |

### Binder Inspector (Common Fields)

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ScriptableVariable | The variable to bind to |
| **Update Mode** | Enum | Subscribe (event-driven) or Poll (frame-driven) |

### Driver Inspector (Common Fields)

| Field | Type | Purpose |
|-------|------|---------|
| **Variable** | ScriptableVariable | The variable to write to |
| **Silent Updates** | Bool | If true, don't fire events each frame |
| **Auto Start** | Bool | If true, start immediately on Awake |

---

## Common Workflows

### Workflow 1: Player Health System

**Goal:** Display player health and respond to damage.

**Setup:**
1. Create **FloatVariable** → `PlayerHealth` (default value: 100)
2. Create a **Canvas > TextMeshPro - Text** → add **NumericalTextBinder** → assign `PlayerHealth`
3. Create a **Canvas > Image** → add **NumericalFillBinder** → assign `PlayerHealth`, set max to 100
4. Create a script:

```csharp
using Shababeek.ReactiveVars;
using UnityEngine;

public class Player : MonoBehaviour
{
    public FloatVariable health;
    public float maxHealth = 100f;

    void Start()
    {
        health.Value = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        health.Value -= amount;
        if (health.Value <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        // This fires when health reaches 0
        Debug.Log("Player died!");
    }
}
```

5. Attach this script to your player. Assign the `PlayerHealth` variable in the inspector.

**Result:** Health bar text and fill update automatically as you call `TakeDamage()`. No manual UI updates needed.

### Workflow 2: Score Display

**Goal:** Track and display player score.

**Setup:**
1. Create **IntVariable** → `PlayerScore` (default: 0)
2. Create **Canvas > TextMeshPro - Text** → add **NumericalTextBinder** → assign `PlayerScore`
3. In your scoring script:

```csharp
public IntVariable score;

void OnEnemyDefeated()
{
    score.Value += 100;  // UI updates automatically
}
```

### Workflow 3: Distance-Based Mechanic

**Goal:** Trigger an effect when the player is within 5 meters of an object.

**Setup:**
1. Create **FloatVariable** → `DistanceToTarget`
2. Add **DistanceDriver** to the player
3. Set it to track distance to your target object
4. Assign `DistanceToTarget` variable
5. Create a script that subscribes:

```csharp
public FloatVariable distanceToTarget;
public ParticleSystem proximityEffect;

void Start()
{
    distanceToTarget.OnValueChanged.Subscribe(distance =>
    {
        if (distance < 5f && !proximityEffect.isPlaying)
            proximityEffect.Play();
        else if (distance >= 5f && proximityEffect.isPlaying)
            proximityEffect.Stop();
    });
}
```

---

## Extension Methods

ReactiveVars includes reactive extension methods for cleaner code:

```csharp
using Shababeek.ReactiveVars;

// Filter events
myBool.WhenTrue().Subscribe(_ => Debug.Log("Became true"));
myBool.WhenFalse().Subscribe(_ => Debug.Log("Became false"));

// Numerical filters
myFloat.WhenAbove(50f).Subscribe(_ => Debug.Log("Exceeded 50"));
myFloat.WhenBelow(10f).Subscribe(_ => Debug.Log("Dropped below 10"));
myFloat.WhenInRange(20f, 80f).Subscribe(_ => Debug.Log("In range"));

// Throttle updates
myFloat.Throttled(TimeSpan.FromSeconds(0.5f)).Subscribe(_ => { });

// Skip duplicate values
myVar.Distinct().Subscribe(_ => { });
```

---

## Next Steps

Now that you understand the basics:

1. **[Binders.md](Binders.md)** — Explore all 50+ binder types and when to use each
2. **[TweenSystem.md](TweenSystem.md)** — Smooth animations with tweens
3. **[VariableContainer.md](VariableContainer.md)** — Organize related variables for saving/loading
4. **Main README** — Full reference of all systems

---

## Troubleshooting

### "Binder isn't updating when variable changes"

**Solution:** Make sure you're using the **same variable asset** in both the binder and the code that changes it. If they're different assets, the binder won't see changes.

### "I want to tween a variable smoothly"

**Solution:** Use the **Tween System** (see [TweenSystem.md](TweenSystem.md)).

### "Silent Updates and Poll mode — what's the difference?"

- **Silent Updates (Driver)**: The driver writes without firing events (efficient for per-frame updates).
- **Poll Mode (Binder)**: The binder reads the value every frame instead of waiting for events.

Use both together when a driver is pushing values every frame to avoid event spam.

### "Can I use a constant value instead of a variable?"

**Yes!** Use **VariableReference** types in your scripts:

```csharp
public FloatReference moveSpeed;  // Inspector: toggle between [Constant ▾] 5.0  OR  [Variable ▾] → speed asset
```

Prototype with constants, swap to shared variables later—no code changes needed.

---

## Key Takeaways

- **Variables** are shared assets. Write to them with `Variable.Value = x`.
- **Binders** display variables. Add them to UI/visual objects. No code needed.
- **Drivers** feed data in from external sources (input, physics, timers).
- **Events** are one-time signals for moments, not values.
- **No wiring between objects needed.** Everything goes through variables.

You're ready to build decoupled, reactive systems!
