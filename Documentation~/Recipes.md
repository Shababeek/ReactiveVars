# Recipes — designer setups

**Navigation:** [Documentation home](README.md) · **Previous:** [GettingStarted.md](GettingStarted.md) · **Next:** [Binders](Binders.md)

Short, repeatable setups with **minimal or no code**. Menu paths use **Create > Shababeek > …** unless noted ([Glossary](Glossary.md)).

**Related:** [FAQ](FAQ.md)

---

## 1. Health value on HUD text

**Goal:** Show a numeric health value on screen.

**Assets**

- `FloatVariable` — e.g. `PlayerHealth`, default `100`.

**Scene**

```
Canvas
└── HealthLabel (TextMeshPro)
```

**Inspector**

1. Select **HealthLabel** → **Add Component** → **NumericalTextBinder**.
2. **Variable** → drag `PlayerHealth`.
3. **Format String** → `Health: {0:F0}`.

**Test:** Edit **Value** on the asset in Play mode — text should follow.

---

## 2. Health bar fill (Image)

**Goal:** Fill amount mirrors health 0–max.

**Assets**

- Same `PlayerHealth` float (0–100).

**Scene**

```
Canvas
└── HealthFill (Image, Image Type = Filled)
```

**Inspector**

1. **Add Component** → **NumericalFillBinder**.
2. **Variable** → `PlayerHealth`.
3. **Max Value** → `100`.
4. **Inverse** → off (high health = more fill).

---

## 3. Countdown timer on UI (no code)

**Goal:** Seconds counting down, shown on text.

**Assets**

- `FloatVariable` — e.g. `RoundTimer`.

**Scene**

```
Canvas
└── TimerText (TextMeshPro)

GameManager (empty GameObject, optional name)
```

**Inspector — driver**

1. Select **GameManager** → **Add Component** → **TimerDriver**.
2. **Variable** → `RoundTimer`.
3. **Start Value** → `60`.
4. **Count Direction** → **Down**.
5. **Auto Start** → on.
6. **Loop** → off if you want it to stop at zero.

**Inspector — binder**

1. **TimerText** → **NumericalTextBinder** → variable `RoundTimer`.
2. Format e.g. `{0:F1}` or `Time: {0:F0}`.

**Note:** If you enable **Silent Updates** on the driver for performance, set the binder **Update Mode** to **Poll** ([Glossary](Glossary.md)).

---

## 4. Toggle panel visibility from a bool

**Goal:** A bool variable shows or hides a UI group.

**Assets**

- `BoolVariable` — e.g. `PauseMenuOpen`.

**Scene**

```
Canvas
└── PausePanel (parent with several children)
```

**Inspector**

1. Select **PausePanel** root → **Add Component** → **GameObjectActiveBinder** (or **BoolCanvasGroupBinder** if you prefer fade — see [Binders](Binders.md)).
2. **Variable** → `PauseMenuOpen`.

**Test:** Toggle the bool on the asset during Play — panel follows.

---

## 5. Float from slider (two-way)

**Goal:** Player drags a slider; a shared float updates (and other binders can read it).

**Assets**

- `FloatVariable` — e.g. `MusicVolume` (0–1).

**Scene**

```
Canvas
└── Slider (Unity UI Slider)
```

**Inspector**

1. On **Slider** → **Add Component** → **SliderBinder** (see [Binders](Binders.md) for fields).
2. Assign **MusicVolume** per the component’s fields (min/max if needed).

---

## 6. Simple linear beat (Sequence)

**Goal:** Ordered steps with optional audio — good for a short forced tutorial beat.

**Assets**

- **Create > Shababeek > Sequencing > Sequence** — configure **Steps** in the inspector on the asset.

**Scene**

```
Director (empty GameObject)
```

**Inspector**

1. Add **SequenceBehaviour** (or your project’s runner) → assign the **Sequence** asset.
2. Wire **Begin** from a button, zone, or **GameEvent** as your project prefers.

Details: [SequencingSystem.md](SequencingSystem.md).

---

## 7. Save and load a container (design-time awareness)

**Goal:** One asset holds many variables; you persist their values to disk from gameplay code.

**Assets**

- **Create > Shababeek > ReactiveVars > Variable Container** — assign subordinate variables in its list.

**Code (minimal)**

Your gameplay systems call `variableContainer.Save()` / `Load()` or `SaveToFile(path)` / `LoadFromFile(path)` — see [VariableContainer.md](VariableContainer.md).

---

## Next steps

- Full binder list: [Binders.md](Binders.md)
- Easing and tweens: [TweenSystem.md](TweenSystem.md)
- Branching tutorials: [SequencingSystem.md](SequencingSystem.md)

**Doc home:** [README.md](README.md)