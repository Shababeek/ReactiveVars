# Contributing to ReactiveVars

Thank you for helping improve ReactiveVars. This document covers how the package is laid out, how to run tests, and what tooling the code expects.

## Project layout

| Path | Purpose |
|------|---------|
| `Runtime/` | Core library (`Shababeek.ReactiveVars` assembly) |
| `Editor/` | Inspectors, graph tooling (`Shababeek.ReactiveVars.Editor`), **Reactive Vars** window (`ReactiveVarsEditorWindow`) |
| `Tests/Runtime/` | Play Mode tests (where used) |
| `Tests/Editor/` | Edit Mode tests (`Shababeek.ReactiveVars.EditorTests`) |
| `Documentation~/` | User-facing Markdown (Unity hides `~` folders from the asset database). Start at [Documentation~/README.md](Documentation~/README.md). |

## Assemblies

- **Runtime** — `Runtime/Shababeek.ReactiveVars.asmdef`  
  References **UniRx**, **TextMeshPro**, and optionally the **Input System** (see below).

- **Editor** — `Editor/Shababeek.ReactiveVars.Editor.asmdef`  
  References the Runtime assembly, Editor APIs, and UniRx where needed.

- **Tests** — `Tests/*.asmdef` files reference Runtime and/or Editor as appropriate for the test type.

`InternalsVisibleTo` is used so editor code and editor tests can access internal sequencing APIs.

## Dependencies

- **UniRx** — Declared in `package.json` as `com.neuecc.unirx` (Git URL with `?path=Assets/Plugins/UniRx/Scripts`). Required for `IObservable` / `Subject` on variables.

- **TextMeshPro** — Unity package `com.unity.textmeshpro` (declared in `package.json`).

- **Input System (optional)** — When the project includes `com.unity.inputsystem`, the Runtime asmdef sets scripting define **`REACTIVE_VARS_INPUT_SYSTEM`**, enabling input action drivers. No manual define is required if the Input System package is installed.

## Running tests

1. Open the Unity project that references this package.
2. **Window > General > Test Runner**
3. Run **Edit Mode** tests under assemblies named like `Shababeek.ReactiveVars.*`.

Add or extend tests next to related features (`Tests/Editor`, `Tests/Runtime`). Prefer Edit Mode for pure logic (variables, serialization) when no scene is required.

## Code guidelines

- Match existing naming, serialization patterns, and inspector attributes.
- Keep XML documentation small and accurate; avoid duplicate or nested `<summary>` tags on the same member.
- Prefer focused changes: fix the issue without unrelated refactors.

## Pull requests

- Describe the behavior change and any migration notes.
- If you fix a bug, add or adjust a test when practical.
- Ensure the project still compiles in Unity 6000.0+ with dependencies restored from `package.json`.
