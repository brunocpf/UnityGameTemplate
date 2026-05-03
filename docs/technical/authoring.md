# Authoring System

This document describes the recommended pattern for using the Unity Editor authoring layer to define gameplay data as ScriptableObject assets that are mapped to core-domain runtime objects.

## Purpose

The authoring layer allows designers to define gameplay data entirely through the Unity Inspector, without writing code. These assets are converted to core-domain objects at the point they are consumed by the runtime — keeping all domain logic in `__GAME_NAMESPACE__.Core.*` assemblies.

A typical authoring assembly (e.g. `__GAME_NAMESPACE__.Unity.Sample` — *replace with your own feature assemblies*) depends on:
- One or more `__GAME_NAMESPACE__.Core.*` assemblies (for the domain types and factories)

It must not be referenced by any `__GAME_NAMESPACE__.Core.*` assembly.

---

## Principles

1. **Inspector-driven definitions.** Designers create and edit ScriptableObject assets to describe entities, items, abilities, and any other gameplay data.
2. **Eager runtime conversion.** A `BuildRuntime*()` method on each authoring asset returns the corresponding core-domain object. The conversion is eager — runtime objects are typically built in `Awake` before gameplay starts.
3. **Composition through references.** Authoring assets reference other authoring assets to compose larger structures (e.g. an entity references a list of abilities; an ability references a tree of step nodes).
4. **No domain logic in SOs.** Authoring assets only hold serialized configuration and the `BuildRuntime*()` mapper. All rules and behavior live in the core assemblies.

---

## Example Asset Type

### ItemDataSO

**Menu:** `__GAME_DISPLAY_NAME__/Items/Item Definition`
**File (suggested):** `Assets/Scripts/Unity/Sample/Items/ItemDataSO.cs`

(Define your own SOs as needed — `ItemDataSO` is shown as a placeholder example.)

| Field | Type | Description |
|-------|------|-------------|
| `Name` | `string` | Display name |
| `Description` | `string` | Flavor description |
| `IconId` | `string` | Address/key for icon sprite |
| `StackSize` | `int` | Maximum stack size |

**`BuildRuntimeItem()`** converts this asset to a core-domain `IItem` by calling the appropriate factory in `__GAME_NAMESPACE__.Core.Sample`.

---

## Composable Node Patterns

Where gameplay data forms a tree (e.g. ability pipelines, condition trees, dialogue branches), prefer abstract base SO types that subclasses inherit from. Each subclass holds serialized configuration and implements a single `BuildRuntime*()` method that returns the matching core interface.

This keeps the authoring asset graph mirroring the domain object graph, with eager conversion at consumption time.

---

## Data Flow

```text
Designer creates assets in Unity Inspector
        │
        ▼
ItemDataSO  (or any other authoring SO)
        │  BuildRuntimeItem()
        ▼
IItem  (in __GAME_NAMESPACE__.Core.Sample)
        │
        ▼
Runtime systems consume domain objects directly
```

The conversion is eager — all runtime objects are instantiated when `BuildRuntime*()` is called (typically in a scene's `Awake`), before gameplay starts.

---

## Adding a New Authoring Asset Type

1. Create `MyNewDefinitionSO.cs` in the appropriate authoring folder under `Assets/Scripts/Unity/<Feature>/`.
2. Inherit from `ScriptableObject` (or your project's authoring base) and decorate with `[CreateAssetMenu(...)]`.
3. Expose Inspector fields for all configuration.
4. Implement `BuildRuntime*()` — return the corresponding core-domain object from `__GAME_NAMESPACE__.Core.<Feature>`.
5. Add a `.meta` file (Unity will generate one automatically when the script is compiled).

No changes to `.asmdef` are needed unless the new type lives in a different assembly.
