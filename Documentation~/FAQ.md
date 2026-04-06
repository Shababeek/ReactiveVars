# FAQ and troubleshooting

**Navigation:** [Documentation home](README.md) · **Previous:** [SequencingSystem.md](SequencingSystem.md) · **Next:** [ScriptableVariable.md](ScriptableVariable.md)

---

## Binder does not update when the value changes

- Confirm the **same variable asset** is assigned everywhere (not a duplicate asset with the same filename in another folder).
- If a **driver** uses **Silent updates** or code uses **`SetValueWithoutNotify`**, switch the binder **Update Mode** to **Poll**, or turn off silent mode where appropriate ([Glossary](Glossary.md)).

## Binder looks stuck during a tween

- Tweens may update values without events. Use **Poll** on binders that must follow smooth motion, or ensure the tween path raises changes if you need Subscribe.

## Variable container load returns false or skips fields

- Names in JSON must match **variable names** in the container list. Renamed assets break name-based lookup until save data is migrated.
- **FromData** / **Load** paths report failure if **no** variable deserialized successfully (e.g. all missing names). See [VariableContainer.md](VariableContainer.md).

## Input drivers do not appear or compile errors

- Install Unity’s **Input System** package. Without it, input-specific drivers are not compiled (`REACTIVE_VARS_INPUT_SYSTEM`).

## Double subscription / memory warnings

- When subscribing in code (`OnValueChanged.Subscribe`), dispose subscriptions in `OnDisable` / `OnDestroy`. See [Reference.md](Reference.md).

## Where is the full binder list?

- [Binders.md](Binders.md).

## Where is the API for variables?

- [ScriptableVariable.md](ScriptableVariable.md) and [Reference.md](Reference.md).

## Where can I see all variables and their values at once?

- In the Editor: **Shababeek > ReactiveVars > Reactive Vars Window** ([Reference.md](Reference.md)). Uncheck **Scene Refs Only** if you need every variable in the project, not just those referenced by open scenes.
- In a build / Game view: add **VariableDebugOverlay** to a scene object for a runtime HUD (see Runtime utilities in [Reference.md](Reference.md)).

---

**Next:** [Getting Started](GettingStarted.md) if you are new; [Binders](Binders.md) for component details.
