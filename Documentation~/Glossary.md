# Glossary

**Navigation:** [Documentation home](README.md) · **Previous:** [Recipes](Recipes.md) · **Next:** [Binders](Binders.md)

One-line meanings for ReactiveVars terms.

---

**Variable** — A ScriptableObject asset holding one typed value (float, bool, color, …). Anything can read or write it; listeners update when it changes.

**Binder** — A MonoBehaviour that **reads** a variable and applies it to something in the scene (UI text, image fill, transform, animator parameter, …).

**Driver** — A MonoBehaviour that **writes** into a variable from something in the scene (timer, input, distance, collision, …).

**Event (`GameEvent`)** — A signal with no “stored gameplay value.” Raised once; listeners react. Use for moments (“door opened”) vs ongoing state (“door is open”), which is often a **BoolVariable**.

**Sequence** — A linear list of **Steps** defined on a **Sequence** asset, usually run by a **SequenceBehaviour** in a scene.

**Branching sequence** — Like a sequence, but transitions depend on **conditions** (e.g. variable compares). Authored on a **BranchingSequence** asset with graph tooling in the editor.

**Variable container** — One asset that **lists** many variables (and events) for organization, bulk reset/raise, and **JSON save/load**.

**Subscribe (update mode)** — Binder updates when the variable **fires change notifications**. Efficient when values change rarely.

**Poll (update mode)** — Binder reads the variable **every frame**. Use when values change every frame **without** raising events (see **Silent updates**).

**Silent updates** — On some drivers (and `SetValueWithoutNotify` in code), the value changes but **no** change event is raised. Binders set to **Subscribe** will not see those frames; use **Poll** on the reader.

**Tween** — Smooth interpolation of a value over time, often via **VariableTweener** and tweenable components ([TweenSystem.md](TweenSystem.md)).

**`VariableReference<T>`** — Inspector-friendly field: either a **constant** or a **variable asset** (useful for tuning without code changes).

**Reactive Vars window** — Unity Editor window (**Shababeek > ReactiveVars > Reactive Vars Window**, title **Reactive Vars**). **Variables** tab: lists all matching variables, grouped by parent asset, with current values in **Play mode** (editable for common types). **Events** tab: lists `GameEvent` assets (not scriptable variables) and can **Fire** them in play mode. Toolbar: search, **Type** filter, **Scene Refs Only** (when on, only assets referenced by the open scenes), **Refresh**. A dot marker means the asset is referenced in the scene. For an in-game HUD, use **VariableDebugOverlay** instead.

---

## Create menu (typical)

Unity turns `CreateAssetMenu` paths into nested menus. Most ReactiveVars assets live under:

**Create > Shababeek > ReactiveVars > …**

Examples:

- Variables: **… > ReactiveVars > Variables > FloatVariable**
- Events: **… > ReactiveVars > Events > GameEvent**
- Container: **… > ReactiveVars > Variable Container**
- Sequencing: **Create > Shababeek > Sequencing > Sequence** (or **BranchingSequence**)
- Editor window (not an asset): top menu **Shababeek > ReactiveVars > Reactive Vars Window**

Some older types still use **Create > ReactiveVars > …** only (e.g. certain sprite/material variable menus). If unsure, use the Project window **Create** search box and type the asset **class name**.

---

**Related:** [Recipes](Recipes.md) · [FAQ](FAQ.md) · [Reference.md](Reference.md)
