# Architecture

## Engine & Renderer

- **Engine:** Unity 6 (URP)
- **Render Pipeline:** Universal Render Pipeline (URP) 17.x
- **Input:** Unity Input System 1.19
- **Navigation:** Unity AI Navigation 2.x
- **Timeline:** Unity Timeline 1.8

## High-Level Architecture

```
┌──────────────┐    ┌───────────────┐    ┌──────────────────┐
│   Input      │───▶│  Game Systems │───▶│  Presentation    │
│  (actions)   │    │  (managers)   │    │  (UI / camera)   │
└──────────────┘    └───────────────┘    └──────────────────┘
                            │
                    ┌───────▼───────┐
                    │     Data      │
                    │ (ScriptableObjects / Save) │
                    └───────────────┘
```

## Assembly Boundaries (Required)

Core business/game logic must live in dedicated C# assemblies, separate from UI and Unity-specific runtime code.

- Create feature-focused core assemblies for each gameplay domain.
- Core assemblies should contain rules, calculations, state transitions, and domain models.
- Core assemblies should avoid direct dependencies on `UnityEngine`, scene objects, MonoBehaviour lifecycle methods, and UI frameworks.
- Unity-facing assemblies should act as adapters (input, scene wiring, presentation, persistence integration).
- UI assemblies should depend on core assemblies, not the other way around.

This separation improves testability, reduces coupling, and makes long-term maintenance/refactoring safer.

## Assembly Dependency Matrix

The template ships with a single illustrative `Sample` slice. **Replace these with your own feature assemblies as the project grows.**

| Assembly | May Reference |
|----------|---------------|
| `__GAME_NAMESPACE__.Core.Sample` | External domain-safe libraries only |
| `__GAME_NAMESPACE__.UI.Utilities` | External UI/runtime support libraries only |
| `__GAME_NAMESPACE__.UI.Sample` | `__GAME_NAMESPACE__.Core.Sample`, `__GAME_NAMESPACE__.UI.Utilities` |
| `__GAME_NAMESPACE__.Unity.Sample` | `__GAME_NAMESPACE__.Core.Sample`, `__GAME_NAMESPACE__.UI.Sample` |

Rules:

- Core assemblies must not reference `__GAME_NAMESPACE__.UI.*`, `__GAME_NAMESPACE__.Unity.*`, or `UnityEngine`.
- UI assemblies may depend on core assemblies and UI utilities, but should not own gameplay rules.
- Unity authoring assets (ScriptableObjects) must remain in Unity-facing assemblies and be mapped to core DTOs before runtime use.
- Unity-facing assemblies should adapt scene/runtime behavior and keep domain decisions in core assemblies.
- Test assemblies may reference their target runtime assembly plus test-only helpers, but should preserve the same layering intent as production code.

## Reactive Architecture (Required)

The project adopts a reactive design approach for gameplay flow, state updates, and async orchestration.

- Use **R3** observables/subjects for state streams and event propagation.
- Use **UniTask** for async workflows (loading, transitions, effects timing, network/file operations).
- Prefer observable pipelines and async flows over polling-heavy `Update()` loops.
- Keep reactive domain streams in core assemblies; Unity/UI layers subscribe and adapt them for presentation.
- Model game state changes as explicit streams (for example: input state, resource changes, progression updates).

Design goals:
- Clear data flow
- Easier testing of gameplay logic
- Lower coupling between systems

Validation guidance for these boundaries lives in [dependency-validation.md](../development/dependency-validation.md).

## Key Systems

| System | Implementation | Notes |
|--------|---------------|-------|
| Input | Input System actions asset | See `InputSystem_Actions.inputactions` |
| Reactive flow | R3 observables | Event/state stream composition across systems |
| Async flow | UniTask | Non-blocking operations and deterministic async flow control |
| Navigation | NavMesh / AI Navigation | |
| Audio | Unity Audio | |
| Save/Load | | TBD |

## Scene Loading Strategy

_TODO: describe how scenes are loaded (single / additive / streaming) for your project._

## Dependencies & Third-Party Packages

See [Packages/manifest.json](../../Packages/manifest.json) for the full list.
