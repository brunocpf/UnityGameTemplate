# Dependency Validation

## Purpose

This project relies on assembly boundaries for long-term maintainability. This guide describes how to validate those boundaries during implementation and review.

Read this with:

- [../technical/architecture.md](../technical/architecture.md)
- [../technical/coding-standards.md](../technical/coding-standards.md)

## Target Dependency Direction

The production dependency flow is intentionally one-way:

```text
__GAME_NAMESPACE__.Core.Sample           (replace with your own core feature assemblies)
        ↑
__GAME_NAMESPACE__.UI.Utilities
        ↑
__GAME_NAMESPACE__.UI.Sample             (replace with your own UI feature assemblies)
        ↑
__GAME_NAMESPACE__.Unity.Sample          (replace with your own Unity feature assemblies)
```

Interpretation:

- Core assemblies do not reference UI or Unity assemblies.
- UI assemblies adapt core state for presentation.
- Unity assemblies wire runtime and scene behavior around UI and core layers.

## Fast Review Checks

Before merging architecture-sensitive work, verify:

- no `UnityEngine` imports were introduced into `Assets/Scripts/Core/`
- no `__GAME_NAMESPACE__.UI.*` or `__GAME_NAMESPACE__.Unity.*` imports were introduced into core assemblies
- no domain rules were moved into `MonoBehaviour` or panel/view classes
- new test assemblies still match the Edit Mode or Play Mode naming convention
- nullable response files remain present for new assemblies

## Suggested Search Commands

Use fast searches during review:

```bash
rg "using UnityEngine" Assets/Scripts/Core
rg "using __GAME_NAMESPACE__\.UI|using __GAME_NAMESPACE__\.Unity" Assets/Scripts/Core
rg "FindObjectOfType|Resources\.Load|Debug\.Log" Assets/Scripts/UI Assets/Scripts/Unity
rg "TODO|FIXME|HACK|XXX|NotImplementedException" Assets/Scripts
```

Expected outcome:

- Core searches above should usually return no matches for UI or Unity references.
- Any `Resources.Load` usage in runtime UI should be justified or replaced by `UiMarkupResources`.
- `Debug.Log` in gameplay or UI paths should be temporary and reviewed aggressively.

## What to Add Tests For

When a bug reveals a boundary problem, add the smallest useful regression test:

- disposal bugs → unit tests around teardown and idempotency
- view-model/view coupling bugs → Edit Mode view-model tests
- panel navigation bugs → Edit Mode panel stack tests
- scene/runtime glue bugs → Play Mode tests

## Review Outcomes

If a boundary violation is intentional, document the reason in code review or nearby docs. If it is not intentional, fix the dependency direction before adding more code on top of it.
