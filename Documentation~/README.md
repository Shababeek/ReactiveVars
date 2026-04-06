# ReactiveVars — documentation

Inspector-first data binding, events, tweens, and sequencing for Unity. This folder is the **documentation home** for the package (Unity hides `Documentation~` from the asset database).

**Repository / package root:** see the [README](../README.md) for badges, install URLs, and support links.

---

## Where do I start?

- **Designer / level / UI:** Read **[Getting Started](GettingStarted.md)** (5 minutes), then **[Recipes](Recipes.md)** for copy-paste setups. Use **[Glossary](Glossary.md)** when a term is unclear. Stuck? **[FAQ](FAQ.md)**.
- **Programmer:** Skim Getting Started, then **[ScriptableVariable reference](ScriptableVariable.md)** and **[Reference for programmers](Reference.md)** (assemblies, architecture, driver/binder tables, UniRx notes).

---

## Five-minute path (first win)

1. Create a **FloatVariable** (see Create menu paths in [Glossary](Glossary.md)).
2. Add **NumericalTextBinder** to a TextMeshPro object; assign the variable.
3. Change the variable’s value in the inspector during Play mode — text updates.

Optional: add a **TimerDriver** and another binder to the same float for a countdown with no code ([Recipes](Recipes.md)).

---

## Table of contents


| #   | Guide                                             | Who it’s for                             |
| --- | ------------------------------------------------- | ---------------------------------------- |
| 1   | [Getting Started](GettingStarted.md)              | Everyone — concepts + first binder       |
| 2   | [Recipes](Recipes.md)                             | Designers — common scene setups          |
| 3   | [Glossary](Glossary.md)                           | Everyone — terminology                   |
| 4   | [Binders](Binders.md)                             | Detailed binder catalog + custom binders |
| 5   | [Tween System](TweenSystem.md)                    | Smooth value animation                   |
| 6   | [Variable Containers](VariableContainer.md)       | Grouping assets, save/load               |
| 7   | [Sequencing](SequencingSystem.md)                 | Linear and branching flows               |
| 8   | [FAQ / Troubleshooting](FAQ.md)                   | Common problems                          |
| 9   | [ScriptableVariable (API)](ScriptableVariable.md) | Programmers — members, numerical API     |
| 10  | [Reference (programmers)](Reference.md)           | Architecture, tables, source map         |


---

## Troubleshooting

Central index: **[FAQ.md](FAQ.md)**. Individual guides also include troubleshooting sections at the end.

---

## Contributing

[CONTRIBUTING.md](../CONTRIBUTING.md) — assemblies, tests, optional Input System define.