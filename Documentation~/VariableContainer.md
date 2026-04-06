# Variable Containers — Organize and Persist Variables

**Navigation:** [Documentation home](README.md) · **Previous:** [TweenSystem.md](TweenSystem.md) · **Next:** [SequencingSystem.md](SequencingSystem.md)

Variable Containers let you group related variables and events into a single asset for easy organization, bulk operations, and save/load functionality.

---

## What Is a Variable Container?

A **VariableContainer** is a ScriptableObject that holds multiple ScriptableVariables and GameEvents as sub-assets. Instead of managing dozens of individual variable files, you organize them into logical groups (e.g., "Player Stats", "UI Settings", "Game Config").

**Benefits:**
- Organize related variables together
- Save all variables to JSON with one call
- Load all variables from JSON with one call
- Reset all variables to defaults with one call
- Share the container reference instead of individual variables

---

## Creating a Container

1. Right-click in your Project folder
2. Select **Create > Shababeek > ReactiveVars > Variable Container**
3. Name it (e.g., `PlayerStats`, `GameSettings`)
4. Open it in the Inspector

### Adding Variables

In the Inspector:
1. Expand **Variables** section
2. Click **+** to add slots
3. Drag variable assets into the slots

Or use the editor API:
```csharp
container.EditorAddVariable(playerHealthVariable);
container.EditorRemoveVariable(playerHealthVariable);
```

### Adding Events

Same process as variables:
1. Expand **Events** section
2. Click **+** to add slots
3. Drag event assets into the slots

---

## Using Containers in Code

### Basic Access

```csharp
using Shababeek.ReactiveVars;

[SerializeField] private VariableContainer playerStats;

void Start()
{
    // Get by index
    var firstVar = playerStats.GetVariable(0);

    // Get by name (typed)
    FloatVariable health = playerStats.GetVariable<FloatVariable>("Health");
    IntVariable score = playerStats.GetVariable<IntVariable>("Score");

    // Get by name (untyped)
    var anyVar = playerStats.GetVariable("SomeVariableName");

    // Safe access
    if (playerStats.TryGetVariable<FloatVariable>("Health", out var health))
    {
        Debug.Log($"Health: {health.Value}");
    }
}
```

### Reading Variable Properties

```csharp
// Count variables
int varCount = playerStats.VariableCount;
int eventCount = playerStats.EventCount;

// Check existence
bool hasHealth = playerStats.HasVariable("Health");
bool hasDeathEvent = playerStats.HasEvent("OnDeath");

// Get all variables of a type
IEnumerable<FloatVariable> floats = playerStats.GetAllVariables<FloatVariable>();
IEnumerable<IntVariable> ints = playerStats.GetAllVariables<IntVariable>();

// Get all numerical variables
IEnumerable<INumericalVariable> numericals = playerStats.GetAllNumerical();

// Get variable names
IEnumerable<string> names = playerStats.GetVariableNames();
```

### Bulk Operations

```csharp
// Raise all variables (notify subscribers)
playerStats.RaiseAllVariables();

// Raise all events
playerStats.RaiseAllEvents();

// Reset all variables to defaults
playerStats.ResetAllVariables();
```

---

## Save and Load

Variable Containers can persist variable values to JSON files.

### Save to Custom Path

```csharp
bool success = playerStats.SaveToFile("C:/saves/game_save.json");
if (success)
    Debug.Log("Save successful!");
```

### Save to Default Location

Saves to `Application.persistentDataPath/VariableContainers/`:

```csharp
// Save with default name (container name + .json)
playerStats.Save();

// Save with custom name
playerStats.Save("custom_save.json");
```

### Load from Custom Path

```csharp
bool success = playerStats.LoadFromFile("C:/saves/game_save.json");
if (success)
    Debug.Log("Load successful!");
```

### Load from Default Location

```csharp
// Load with default name
playerStats.Load();

// Load with custom name
playerStats.Load("custom_save.json");
```

### Check If Save Exists

```csharp
if (playerStats.SaveExists())
{
    Debug.Log("Save file found!");
    playerStats.Load();
}
else
{
    Debug.Log("No save file, starting fresh");
}
```

### Delete Save File

```csharp
playerStats.DeleteSave();

// Or with custom name
playerStats.DeleteSave("old_save.json");
```

### Get Save Path

```csharp
string path = playerStats.GetDefaultSavePath();
Debug.Log($"Saves to: {path}");
```

---

## Supported Variable Types for Serialization

The save/load system supports all common variable types:

| Type | Serialization | Notes |
|------|----------------|-------|
| FloatVariable | ✓ JSON text | Invariant culture for decimal compatibility |
| IntVariable | ✓ JSON text | — |
| BoolVariable | ✓ JSON text | — |
| TextVariable | ✓ JSON text | — |
| Vector2Variable | ✓ JSON | Serialized as `{x, y}` |
| Vector3Variable | ✓ JSON | Serialized as `{x, y, z}` |
| Vector2IntVariable | ✓ JSON | Integer vectors |
| QuaternionVariable | ✓ JSON | Serialized as `{x, y, z, w}` |
| ColorVariable | ✓ JSON | RGBA values |
| LayerMaskVariable | ✓ JSON | Layer mask as int |
| StringListVariable | ✓ JSON | List of strings |
| DoubleVariable | ✓ JSON text | Double precision |
| EnumVariable | ✓ JSON text | Enum value as int |

**Not Serialized (yet):**
- SpriteVariable
- MaterialVariable
- AudioClipVariable
- GradientVariable
- AnimationCurveVariable
- TransformVariable
- GameObjectVariable
- AudioVariable

---

## Common Patterns

### Pattern 1: Game Settings Container

```
GameSettings (VariableContainer)
├─ MasterVolume (FloatVariable)
├─ MusicVolume (FloatVariable)
├─ SFXVolume (FloatVariable)
├─ Brightness (FloatVariable)
├─ IsFullscreen (BoolVariable)
└─ Resolution (IntVariable)

Usage:
playerStats.Save("settings.json");  // Save all settings at once
playerStats.Load("settings.json");  // Load all settings at once
```

### Pattern 2: Player Stats Container

```
PlayerStats (VariableContainer)
├─ Health (FloatVariable)
├─ MaxHealth (FloatVariable)
├─ Score (IntVariable)
├─ Level (IntVariable)
├─ Exp (FloatVariable)
├─ Lives (IntVariable)
└─ OnPlayerDeath (GameEvent)

Usage:
void SaveGame()
{
    playerStats.Save("playersave.json");
}

void LoadGame()
{
    playerStats.Load("playersave.json");
}

void ResetLevel()
{
    playerStats.ResetAllVariables();
}
```

### Pattern 3: Runtime Configuration Container

Load game balance values from a container at startup:

```csharp
[SerializeField] private VariableContainer gameBalance;

void LoadBalance()
{
    FloatVariable enemyHealth = gameBalance.GetVariable<FloatVariable>("EnemyHealthScale");
    FloatVariable playerDamage = gameBalance.GetVariable<FloatVariable>("PlayerDamageScale");

    // Use these variables throughout the game
}
```

### Pattern 4: Multi-Save System

```csharp
[SerializeField] private VariableContainer playerStats;

void SaveGame(int slotNumber)
{
    playerStats.Save($"save_slot_{slotNumber}.json");
}

void LoadGame(int slotNumber)
{
    if (playerStats.SaveExists($"save_slot_{slotNumber}.json"))
    {
        playerStats.Load($"save_slot_{slotNumber}.json");
    }
}

void DeleteGame(int slotNumber)
{
    playerStats.DeleteSave($"save_slot_{slotNumber}.json");
}
```

---

## Save File Format

Saved JSON looks like this:

```json
{
    "containerName": "PlayerStats",
    "savedAt": "2026-03-07 14:30:45",
    "variables": [
        {
            "name": "Health",
            "type": "FloatVariable",
            "value": "75.5"
        },
        {
            "name": "Score",
            "type": "IntVariable",
            "value": "1250"
        },
        {
            "name": "IsAlive",
            "type": "BoolVariable",
            "value": "True"
        }
    ]
}
```

This human-readable format is easy to debug and edit manually if needed.

---

## Editor-Only Methods

The container provides editor-only methods for building containers in code:

```csharp
#if UNITY_EDITOR
container.EditorAddVariable(variable);
container.EditorRemoveVariable(variable);
container.EditorRemoveVariableAt(index);
container.EditorAddEvent(gameEvent);
container.EditorRemoveEvent(gameEvent);
container.EditorRemoveEventAt(index);
container.EditorCleanupNulls();  // Remove null references
#endif
```

---

## Troubleshooting

### "Save file created but variables are empty when I load"

**Check:**
1. Are you calling `Save()` AFTER changing variables?
2. Is the path correct? Check `GetDefaultSavePath()`.
3. Are all your variables **serializable types** (see table above)?

### "Unsupported variable type for serialization" warning

**Solution:** That variable type doesn't support JSON serialization yet. You can:
1. Store its data in a serializable variable (e.g., sprite index → int)
2. Write custom serialization logic
3. Save it separately

### "Save file exists but Load returns false"

**Check:**
1. File path is correct: `Application.persistentDataPath/VariableContainers/{name}.json`
2. File format is valid JSON
3. Variable names in file match variable names in container
4. File isn't corrupted

---

## Performance Considerations

- **Save:** Creates JSON and writes to disk. Do this infrequently (on game save, not every frame).
- **Load:** Reads from disk and deserializes. Fast, but avoid doing this every frame.
- **GetVariable:** O(n) lookup by name. Cache references if calling frequently.
- **ResetAllVariables:** Uses reflection. Fast enough for reasonable container sizes.

---

## Next Steps

- **[GettingStarted.md](GettingStarted.md)** — Basic setup
- **[Binders.md](Binders.md)** — All binder types
- **[TweenSystem.md](TweenSystem.md)** — Smooth animations
- **Main README** — Full system reference
