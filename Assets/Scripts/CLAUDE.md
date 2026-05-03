# Assembly Structure & Coding Standards

## Repository Layout

```
Assets/Scripts/
  Core/<Feature>/      → __GAME_NAMESPACE__.Core.<Feature>     (engine-free domain)
  UI/<Feature>/        → __GAME_NAMESPACE__.UI.<Feature>       (UI Toolkit panels and controls)
  UI/Utilities/        → __GAME_NAMESPACE__.UI.Utilities       (Panel/Router/MarkupResources helpers)
  Unity/<Feature>/     → __GAME_NAMESPACE__.Unity.<Feature>    (MonoBehaviours, scene wiring)
Assets/Tests/
  EditMode/Core/       → unit tests for Core assemblies
  EditMode/UI/         → unit tests for UI assemblies
  PlayMode/            → play-mode integration tests
```

The template ships with a single `Sample` slice in each layer (`Core.Sample`, `UI.Sample`, `Unity.Sample`) that demonstrates the layering. Replace these with your own feature assemblies as the project grows.

Each assembly root contains: `<Assembly>.asmdef`, `<Assembly>.rsp` (`-nullable:enable`), `AssemblyInfo.cs`, `IsExternalInit.cs`.

## Allowed Dependency Direction

```
__GAME_NAMESPACE__.Core.<FeatureA>
  ↑
__GAME_NAMESPACE__.Core.<FeatureB>      (may reference other Core.* assemblies)

__GAME_NAMESPACE__.UI.Utilities
  ↑
__GAME_NAMESPACE__.UI.<Feature>         (may reference Core.* and UI.Utilities)
  ↑
__GAME_NAMESPACE__.Unity.<Feature>      (may reference Core.*, UI.*, and Unity.*)
```

Hard rules:
- `__GAME_NAMESPACE__.Core.*` must NOT reference `__GAME_NAMESPACE__.UI.*`, `__GAME_NAMESPACE__.Unity.*`, or `UnityEngine`.
- UI assemblies depend on Core, never the reverse.
- Unity adapters depend on UI and Core, never the reverse.

## Coding Standards

Authoritative source: `docs/technical/coding-standards.md` and `.editorconfig`.

### Naming

| Element | Convention |
| --- | --- |
| Classes / methods / properties | `PascalCase` |
| Private fields | `_camelCase` |
| Constants | `SCREAMING_SNAKE` |
| Interfaces | `I`-prefix — e.g. `IDamageable` |
| ScriptableObjects | `SO`-suffix — e.g. `EnemyDataSO` |
| Namespaces | Mirror assembly: `__GAME_NAMESPACE__.Core.<Feature>` etc. |
| File names | Match class name exactly; one class per file |

### Types & Style

- **Explicit types** — prefer `int x` over `var x`.
- **Braces** — Allman style, always required, even for single-line `if`.
- **Nullable** — enabled everywhere. Do not use `!` without a comment. No null guards on non-nullable types.
- **Records** — positional syntax for cross-layer DTOs. Requires `IsExternalInit.cs` polyfill.

### Patterns

- **Async** — UniTask only. No `Task`/`async void`.
- **Reactive state** — R3 observables. No polling in `Update()`.
- **UI** — `VisualElement`-based controls or `Panel`/`MenuPanel` from `__GAME_NAMESPACE__.UI.Utilities`. No uGUI/Canvas.
- **UI resources** — UXML/USS under `Resources/UI/<Feature>/<ControlName>/`, loaded via `UiMarkupResources.CloneInto(...)`.

## Adding a New Assembly

1. Create the folder under `Assets/Scripts/{Core|UI|Unity}/<Feature>/`.
2. Add `<AssemblyName>.asmdef` with the right `name`, `rootNamespace`, and `references` (see existing Sample assemblies).
3. Add `<AssemblyName>.rsp` containing `-nullable:enable`.
4. Add `AssemblyInfo.cs` with `[assembly: InternalsVisibleTo("<AssemblyName>.Tests")]` if you want test access to internals.
5. Add `IsExternalInit.cs` if the assembly uses positional records.
6. Run `python3 scripts/check_asmdef_deps.py` — must stay clean.
