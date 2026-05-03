# Tools & Utilities

## Unity Packages in Use

| Package | Version | Purpose |
|---------|---------|---------|
| Input System | 1.19 | Player input |
| AI Navigation | 2.x | NavMesh agents |
| Timeline | 1.8 | Cutscenes / sequences |
| URP | 17.x | Rendering |
| Visual Scripting | 1.9 | Prototype / designer scripts |
| Test Framework | 1.6 | Automated testing |

## Reactive Runtime Libraries

The project architecture is reactive-first and relies on:

| Library | Purpose |
|---------|---------|
| R3 | Observables, event streams, and reactive state flow |
| UniTask | Unity-friendly async/await workflows and performance-conscious async operations |

Notes:
- Core systems should publish/consume reactive streams rather than relying on polling loops.
- UI/Unity layers should subscribe to core streams and map them to presentation.

## UI Runtime Stack

| Technology | Purpose |
|------------|---------|
| Unity UI Toolkit | Runtime UI framework (custom `VisualElement` controls) |
| Resources | Component-local loading of small control UXML/USS assets |
| TSS (Theme Style Sheets) | Global theme and styling tokens applied across UI |

Notes:
- Runtime screens use UI Toolkit custom controls and panel stack navigation.
- `Resources` is reserved for colocated UI Toolkit markup and style assets only.
- Addressables remain available for large or remotely delivered content, but are not the default for local control markup.
- Legacy uGUI/canvas runtime UI is not the target architecture for this project.

## Useful Unity Menu Items

| Menu path | Purpose |
|-----------|---------|
| `Window > Analysis > Profiler` | Runtime performance profiling |
| `Window > Analysis > Frame Debugger` | Render call inspection |
| `Window > General > Test Runner` | Run unit / integration tests |
| `Window > AI > Navigation` | Bake NavMesh |
| `Edit > Project Settings > Input System` | Configure action maps |

## VS Code Extensions

Recommended extensions are defined in [`.vscode/extensions.json`](../../.vscode/extensions.json). VS Code will prompt you to install them when you open the workspace.

| Extension | ID | Purpose |
|-----------|----|---------|
| Unity (VS Tools for Unity) | `visualstudiotoolsforunity.vstuc` | Unity integration: debugging, log console, asset indexing |
| C# | `ms-dotnettools.csharp` | C# language support (IntelliSense, go-to-definition, diagnostics) |
| C# Dev Kit | `ms-dotnettools.csdevkit` | Solution explorer, test runner, enhanced C# tooling |
| EditorConfig | `editorconfig.editorconfig` | Enforces code style rules from `.editorconfig` |

## External Tools

| Tool | Purpose |
|------|---------|
| Git LFS | Large file versioning |

## Git Hooks (Pre-Commit Formatting)

This repository includes a versioned Git pre-commit hook at `.githooks/pre-commit`.

Behavior:
- Runs `dotnet format` on staged `*.cs` files only.
- Re-stages formatted files automatically.

One-time setup per local clone:

```bash
./scripts/setup-git-hooks.sh
```

This configures:
- `core.hooksPath=.githooks`
- Executable permissions for the pre-commit hook

Recommended CI enforcement (full solution check):

```bash
dotnet format __GAME_NAMESPACE__.slnx --verify-no-changes --verbosity minimal
```
