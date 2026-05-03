# Coding Standards

## Language

C# — Unity conventions apply unless overridden here.

## Nullable Reference Types

Nullable reference type analysis should be enabled for all assemblies in this project.

- Use compiler response files (`.rsp`) with `-nullable:enable`.
- Add `Assets/csc.rsp` for predefined Unity assemblies.
- Add `<AssemblyName>.rsp` next to each custom `.asmdef` to enforce nullability checks in that assembly.

See [nullable-reference-types.md](nullable-reference-types.md) for the full setup guide and checklist.

Do not write null guards or use the null coalescing operator (`??`) for non-nullable reference types. The static analysis will catch potential null references. If a value can be null, it should be declared as nullable (`string?`, `MyClass?`). Avoid the null forgiveness operator (`!`) except in cases where you have verified that a value is non-null but the compiler cannot infer it, add comments or XML documentation to indicate the expected nullability.

## Source of Truth

The repository `.editorconfig` is the authoritative source for code style, formatting, and analyzer severity settings.

- If this document and `.editorconfig` conflict, follow `.editorconfig`.
- Use this document for high-level conventions (architecture, naming intent, Unity-specific guidance).
- Use tooling (`dotnet format`, IDE analyzers) to enforce `.editorconfig` automatically.

## Naming

| Element | Convention | Example |
|---------|-----------|---------|
| Class | PascalCase | `PlayerController` |
| Method | PascalCase | `TakeDamage()` |
| Private field | camelCase with `_` prefix | `_health` |
| Public property | PascalCase | `Health` |
| Constant | SCREAMING_SNAKE | `MAX_HEALTH` |
| Interface | `I` prefix + PascalCase | `IDamageable` |
| Enum | PascalCase | `GameState` |
| ScriptableObject | PascalCase + SO suffix | `ItemDataSO` |

## Files

- One class per file.
- File name matches class name exactly.
- Group files by feature, not by type (e.g. `Player/`, not `Scripts/Controllers/`).

## Namespaces

Namespaces should reflect architectural boundaries first, then feature.

- Core/domain logic: `__GAME_NAMESPACE__.Core.<Feature>`
- Unity integration/adapters: `__GAME_NAMESPACE__.Unity.<Feature>`
- UI/presentation: `__GAME_NAMESPACE__.UI.<Feature>`

Examples:

- `__GAME_NAMESPACE__.Core.Sample`
- `__GAME_NAMESPACE__.Unity.Sample`
- `__GAME_NAMESPACE__.UI.Sample`

Rules:

- Avoid placing domain/business logic under `__GAME_NAMESPACE__.Unity.*` or `__GAME_NAMESPACE__.UI.*` namespaces.
- Keep namespace structure aligned with assembly boundaries and folder structure.

## Assembly Organization

Core business/game logic must be implemented in dedicated assemblies and kept separate from UI and Unity-specific code.

- Organize core logic into one or more feature assemblies.
- Core assemblies should target pure domain logic and avoid direct `UnityEngine`/UI dependencies.
- Keep MonoBehaviours, scene orchestration, and UI in Unity-facing assemblies.
- Dependency direction must remain one-way: UI/Unity layers depend on core assemblies.
- Do not place core decision logic inside MonoBehaviour classes unless it is only glue/adaptation.

Recommended layering:

- `__GAME_NAMESPACE__.Core.*` assemblies: domain entities, rules, use-cases.
- `__GAME_NAMESPACE__.Unity.*` assemblies: adapters, MonoBehaviours, scene integration.
- `__GAME_NAMESPACE__.UI.*` assemblies: HUD, menus, view models, and presentation adapters.

## Data Transfer Objects (DTOs) and Result Types

Prefer positional records for simple data-transfer and result objects.

- Use positional record syntax for DTOs used across layer boundaries (e.g., service results, query responses).
- Include XML documentation on the record declaration to document parameters.
- Example: `public record OperationResult(bool Success, string? Message = null);`
- Positional records provide immutability, value equality, auto-generated `ToString()`, and concise initialization out-of-the-box.

**Important:** Assemblies targeting netstandard2.1 and using positional records must include the `IsExternalInit` polyfill. Add this to any assembly core file:

```csharp
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
```

This enables the `init` keyword required by record types on older .NET frameworks.

## UI Implementation Standard (UI Toolkit)

- Use Unity UI Toolkit for runtime UI (`VisualElement` custom controls).
- Do not implement new runtime screens with legacy uGUI/canvas workflows.
- Each custom UI control should live in its own folder under the corresponding UI assembly.

Control naming and files:

- C# class: `<ControlName>.cs`
- Markup (optional): `<ControlName>Markup.uxml`
- Styles (optional): `<ControlName>Styles.uss`
- USS classes must use BEM naming conventions (`block`, `block__element`, `block--modifier`).


Asset loading:

- Place each control's UXML/USS under `Resources/UI/<Feature>/<ControlName>/` inside the control folder, matching the control's namespace and feature.
- Use globally unique resource paths such as `UI/Sample/SamplePanel/SamplePanelMarkup`.
- All custom controls and panels must load and cache their markup using the shared public helper `UiMarkupResources` in `__GAME_NAMESPACE__.UI.Utilities`.
- Restrict `Resources` usage to small UI Toolkit markup and style assets that are always shipped with the build.
- Do not use Addressables or Resources.Load directly in control constructors—always use the helper for consistency and caching.
- Fail fast on invalid or missing resource paths (the helper throws for missing assets).

**Standard usage:**

```csharp
using __GAME_NAMESPACE__.UI.Utilities;
using UnityEngine.UIElements;

[UxmlElement]
public sealed partial class SamplePanel : VisualElement
{
    private const string _markupPath = "UI/Sample/SamplePanel/SamplePanelMarkup";
    private const string _rootClass = "sample-panel";

    public SamplePanel()
    {
        UiMarkupResources.CloneInto(this, _markupPath, _rootClass);
    }
}
```

See [ui-toolkit.md](ui-toolkit.md) for full directory and naming conventions.
- Follow the standard UXML skeleton and `VisualElement` implementation template in [ui-toolkit.md](ui-toolkit.md).

Styling layers:

- Global theme styling via TSS applied at runtime.
- Component-level styling via per-control USS.
- Keep global tokens in TSS and local control details in USS.
- Derive shared USS values from theme tokens using custom USS variables instead of duplicating theme values in component USS.

Panel navigation:

- Larger views (inventory, character sheet, settings) should derive from a shared `Panel : VisualElement` base.
- Panel navigation should go through `PanelStackController` using `Push`, `Pop`, `Clear`, and `Peek`.
- `Panel` should provide shared focus handling and transition lifecycle hooks (`Enter`, `Exit`, `Reveal`, `Cover`).

## MonoBehaviour Guidelines

- Prefer composition over inheritance.
- Keep `Update()` lean — push logic to systems/managers where possible.
- Cache component references in `Awake()`, not in `Update()`.
- Avoid `Find()` and `SendMessage()`.
- Treat MonoBehaviours as integration points; keep domain decisions in core assemblies.

## Reactive & Async Guidelines

- Use **R3** as the default pattern for game event/state streams.
- Use **UniTask** as the default async abstraction instead of `Task` for Unity runtime code.
- Avoid placing business/game rules inside subscription callbacks in UI/MonoBehaviour classes.
- Keep subscriptions lifecycle-safe (dispose on `OnDisable`/`OnDestroy` or bind to object lifetime utilities).
- Prefer explicit stream names and intent-revealing methods (for example, `OnStateChanged`, `OnValueChanged`).
- Keep stream chains small and composable; extract complex operators into named methods.
- Do not use reactive pipelines as hidden global state; keep ownership clear per system.

Testing guidance:

- Core reactive pipelines should be unit-tested in core assemblies.
- Async flows should be testable with deterministic inputs and minimal Unity runtime dependencies.

## Comments

- Write comments for *why*, not *what*.
- Public API members should have XML doc-comments (`/// <summary>`).

## Events & Messaging

Prefer C# events or UnityEvents over direct coupling. Document significant events in the relevant system's documentation.

## ScriptableObjects

Use ScriptableObjects for shared configuration data. Suffix class names with `SO`.

## Version Control

- Commit small, focused changes.
- Prefix commit messages: `feat:`, `fix:`, `refactor:`, `docs:`, `art:`, `audio:`.

## Formatting (Command Line)

Code style and analyzer rules are defined in `.editorconfig` at the repository root.

Key settings currently enforced include:
- `indent_style = space` and `indent_size = 4` for C# files
- Allman braces (`csharp_new_line_before_open_brace = all`)
- Prefer explicit types over `var` in most cases
- Require braces (`csharp_prefer_braces = true:warning`)
- Analyzer and CA severities configured via `dotnet_diagnostic.*`

Use `dotnet format` from the project root:

```bash
# Apply formatting changes
dotnet format __GAME_NAMESPACE__.slnx

# CI check (fails if formatting changes are required)
dotnet format __GAME_NAMESPACE__.slnx --verify-no-changes
```

Optional (target a specific project file):

```bash
dotnet format __GAME_NAMESPACE__.Core.Sample.csproj
```
