

# ReactiveVars

A ScriptableObject-based reactive variable system for Unity.  
Decouple your game systems. Wire everything in the inspector.





---

## What is ReactiveVars?

ReactiveVars lets different parts of your game share data **through assets**, not through chained references. You create a **Variable** (a small ScriptableObject). **Binders** on UI and objects **read** it; **Drivers** and scripts **write** it. When the value changes, everything wired to that asset updates.

**Full manual (designer-first, with programmer reference at the end):** [Documentation~/README.md](Documentation~/README.md)

---

## Core concepts


| Concept      | Role                                                         |
| ------------ | ------------------------------------------------------------ |
| **Variable** | Shared value as a project asset                              |
| **Binder**   | Pushes a variable into a component (text, fill, animator, …) |
| **Driver**   | Writes into a variable from input, physics, timers, …        |
| **Tween**    | Smoothly interpolates values over time                       |
| **Event**    | One-shot signal (no persistent value)                        |
| **Sequence** | Ordered steps (tutorials, beats, branching logic)            |


---

## Installation

**Package Manager** — **+** → *Add package from git URL*:

```
https://github.com/Shababeek/ReactiveVars.git
```

**Local folder** — Copy the package into your project’s `Packages` directory.

**Dependencies:** **UniRx** and **TextMeshPro** are declared in `package.json` and resolve with the package. If you copy scripts only, add UniRx: `https://github.com/neuecc/UniRx.git?path=Assets/Plugins/UniRx/Scripts`

**Optional:** [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest) enables input drivers (`REACTIVE_VARS_INPUT_SYSTEM`). See [CONTRIBUTING.md](CONTRIBUTING.md).

---

## Quick start (three steps)

1. **Create a variable:** Project window → **Create > Shababeek > ReactiveVars > Variables > FloatVariable** → name it e.g. `PlayerHealth`.
2. **Bind UI:** Add **NumericalTextBinder** to a TextMeshPro object → assign `PlayerHealth` → optional format: `Health: {0:F0}`.
3. **Change the value** from your own script (`playerHealth.Value -= amount`) or add a **TimerDriver** / other driver — binders update automatically.

More step-by-step setups: [Documentation~/Recipes.md](Documentation~/Recipes.md).

---

## Documentation map


|                                           |                                                                                                                                                                                       |
| ----------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Start here (everyone)**                 | [Documentation~/README.md](Documentation~/README.md)                                                                                                                                  |
| **Recipes (designers)**                   | [Documentation~/Recipes.md](Documentation~/Recipes.md)                                                                                                                                |
| **Glossary**                              | [Documentation~/Glossary.md](Documentation~/Glossary.md)                                                                                                                              |
| **FAQ**                                   | [Documentation~/FAQ.md](Documentation~/FAQ.md)                                                                                                                                        |
| **Binders / tweens / saves / sequencing** | [Binders](Documentation~/Binders.md) · [Tweens](Documentation~/TweenSystem.md) · [Containers](Documentation~/VariableContainer.md) · [Sequencing](Documentation~/SequencingSystem.md) |
| **Programmer reference**                  | [ScriptableVariable.md](Documentation~/ScriptableVariable.md) · [Reference.md](Documentation~/Reference.md)                                                                           |


---

## Features (summary)

Variables (many types), 50+ binders, drivers (input, physics, timers, …), tween system, game events, linear and **branching** sequences with graph authoring, variable containers with JSON persistence, runtime debug overlay, editor tooling.

---

## Getting help

- **Contributing:** [CONTRIBUTING.md](CONTRIBUTING.md)
- **Issues:** [GitHub Issues](https://github.com/Shababeek/ReactiveVars/issues)
- **Discussions:** [GitHub Discussions](https://github.com/Shababeek/ReactiveVars/discussions)

---

## License

See [LICENSE](LICENSE).

**Last updated:** April 2026