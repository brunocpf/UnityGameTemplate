# UI Implementation Guide

## Purpose

This guide explains how to add new runtime UI to __GAME_DISPLAY_NAME__ while preserving the existing architecture boundaries between core systems, UI presentation, and Unity runtime wiring.

Read this together with:

- [coding-standards.md](coding-standards.md)
- [ui-toolkit.md](ui-toolkit.md)
- [architecture.md](architecture.md)

## Choose the Right UI Primitive

Use a plain `VisualElement`-based custom control when:

- the element renders local state only
- it does not participate in stack navigation
- it does not need enter/exit/reveal/cover transitions
- it is a reusable part of a larger screen

Use a `Panel` (extends `VisualElement`, implements `IPanelLayer`) when:

- the layer has visual content pushed onto a `PanelStackController`
- the layer needs focus restoration
- the layer owns a temporary navigation layer or submenu flow
- the layer needs explicit transition hooks

Use a plain `IPanelLayer` (no `VisualElement`) when:

- the layer has no visual content of its own
- it delegates interaction (focus, action map) to an existing view that stays on screen

Examples (template `Sample` slice):

- `SampleStatusBadge` is a control
- `SampleViewPanel` and `SampleSelectionPanel` are stack-managed views

## Recommended Layer Split

Keep responsibilities narrow:

- `__GAME_NAMESPACE__.Core.*`: state, calculations, command handling, gameplay rules
- `__GAME_NAMESPACE__.UI.*`: visual composition, UI state projection, menu interactions, focus behavior
- `__GAME_NAMESPACE__.Unity.*`: `UIDocument`, scene bootstrap, MonoBehaviour lifecycle glue

Do not move gameplay rules or domain validation into a `VisualElement`, `Panel`, or `MonoBehaviour`.

## Control Layout Pattern

Each custom control should stay colocated with its UXML and USS.

Recommended layout:

```text
Assets/Scripts/UI/<Feature>/<ControlName>/
    <ControlName>.cs
    Resources/
        UI/
            <Feature>/
                <ControlName>/
                    <ControlName>Markup.uxml
                    <ControlName>Styles.uss
```

Load markup through `UiMarkupResources.CloneInto(...)` instead of direct `Resources.Load` calls.

## UxmlAttribute vs Bind vs Constructor Preview

Custom controls have three distinct input channels. Using the wrong one for a given piece of data is the most common structural mistake.

### `[UxmlAttribute]` — structural configuration

Use when the value:

- is set by a **designer or author** in UXML markup or UI Builder, not by game data
- describes **how the control is configured** (variant, orientation, feature flags, slot labels)
- does not require a subscription or runtime update loop
- makes sense to configure once, before any data arrives

```csharp
[UxmlAttribute]
public EBarVariant Variant
{
    get => _variant;
    set => UpdateVariant(value);   // updates CSS class + label, no Render call
}
```

The setter must be side-effect-safe: it must not call `Render`, must not depend on stream state, and must not crash if called before the first `Bind`.

Never expose game data (current value, label string, enabled state) as a `[UxmlAttribute]`. Attributes are authoring-time knobs, not a data binding channel.

### `Bind(...)` — runtime game data

Use when the value:

- comes from a **view model or domain observable**
- can change during a session
- requires a subscription lifetime

The `Bind` method is the only entry point for game data. View state records must contain only data — never structural configuration that belongs in a `[UxmlAttribute]`.

```csharp
// Correct — record carries only values
public sealed record ResourceBarViewState(int Current, int Max);

// Wrong — EBarVariant is structural config, not game data
public sealed record ResourceBarViewState(int Current, int Max, EBarVariant Variant);
```

### Constructor preview state — design-time visibility

When a control's visual output depends entirely on `Bind`, UI Builder will render it blank. To show a meaningful preview without polluting `[UxmlAttribute]` with data fields, call `Render` with a representative default at the end of the constructor:

```csharp
public ResourceBar()
{
    UiMarkupResources.CloneInto(this, _markupPath, _rootClass);
    // ... query elements, register callbacks ...

    Render(new ResourceBarViewState(75, 100));   // preview only
}
```

This works because `_hasBound` is `false` at construction time, so the first real `Render` call from `Bind` snaps to correct values without animation, silently replacing the preview.


### Decision table

| Source of the value | Channel |
|---|---|
| Designer sets it in UXML / UI Builder | `[UxmlAttribute]` |
| Game state, changes at runtime | `Bind(...)` |
| Placeholder so UI Builder isn't blank | constructor `Render(...)` |

## ViewModel and View Binding Pattern

Prefer one-way rendering from a view model into focused observable streams.

### Roles

| Role | Naming | Responsibility |
|---|---|---|
| **ViewModel** | `*ViewModel` class | Builds `IObservable<T>` from domain models using pipeline operators (`CombineLatest`, `Select`, `DistinctUntilChanged`). **No `Subscribe()` calls.** |
| **Stream payload** | `*Data` record | Immutable snapshot emitted by the stream. One type per view's scope. |
| **View** | `VisualElement` / `Panel` | Owns subscriptions. `Bind(...)` opens them; `DetachFromPanelEvent` closes them. No domain logic. |

### `Bind` signature rules

- **Leaf control** — `Bind(IObservable<TData> stream)`. Subscribes internally and owns the lifetime.
- **Composite view** — `Bind(TViewModel vm)`. Calls each child's `Bind` with the relevant observable from `vm`. No `Subscribe` calls in the method body.
- **Root panel** — same shape as composite, plus subscribes to panel-level streams with `AddTo(_bindings)`.

### Subscription lifetime

Every control that subscribes holds:

```csharp
private CompositeDisposable _subscriptions = new();
```

`Bind` replaces the previous subscriptions:

```csharp
public void Bind(IObservable<ResourceBarData> stream)
{
    _subscriptions.Dispose();
    _subscriptions = new CompositeDisposable();
    stream.Subscribe(Render).AddTo(_subscriptions);
}
```

`DetachFromPanelEvent` disposes them on teardown:

```csharp
RegisterCallback<DetachFromPanelEvent>(_ => _subscriptions.Dispose());
```

### Full flow example

```
SampleViewModel.GetEntityViewModel(entity)     →  EntityViewModel
EntityViewModel.Resource                       →  IObservable<ResourceBarData>
ResourceBar.Bind(vm.Resource)                  →  subscribes, renders on each emission
```

```
SampleViewPanel.Bind(SampleViewModel vm)       root — subscribes to panel-level streams,
                                               delegates to child composite views
  └─ EntityCard.Bind(EntityViewModel)          composite — calls children's Bind
       └─ _resourceBar.Bind(vm.Resource)       leaf — subscribes to focused stream
```

Recommended flow:

1. Core systems publish domain state via reactive properties.
2. ViewModel composes focused `IObservable<TData>` streams from those properties.
3. View subscribes in `Bind` and renders on each emission.
4. User interactions call back into the view model through commands or intent methods.

## Reactive Subscription Ownership

Subscriptions should live where ownership is obvious.

Prefer these ownership points:

- view models or binding adapters that project controller state into a view
- Unity adapters that subscribe during `OnEnable` and dispose during `OnDisable` or `OnDestroy`
- short-lived panels only when the subscription is truly panel-local

Rules:

- keep subscription callbacks small
- map state to visuals, do not apply game rules in callbacks
- release subscriptions deterministically
- add disposal regression tests when a subscription bug is fixed

## Panel Stack Rules

When a screen uses nested menus or modal overlays, route navigation through `PanelStackController`.

Expected behavior:

- `Push` covers the previous panel and activates the next panel
- `Pop` exits the current panel and reveals the previous panel
- `Clear` removes the full stack in a controlled order
- `Peek` inspects the current panel without mutation

Layers should:

- implement `IPanelLayer` — either by extending `Panel` (visual) or as a plain C# object (non-visual)
- keep transition methods side-effect-light and testable
- rely on `RestoreFocus` rather than ad hoc focus grabbing

Non-visual layers (interaction modes that delegate focus to an existing view rather than owning DOM content) implement `AttachTo`, `Detach`, and `SetStackInteractive` as no-ops and manage focus through `Enter`/`Exit`/`Cover`/`Reveal`.

## Composition Guidance

Direct instantiation of child controls inside a panel is acceptable when all of these are true:

- the children are local to the feature
- they do not require external services
- the parent is acting as a presentation composition root

If a child view starts to depend on external services, asset loading policies, or alternative implementations, extract creation behind a factory or dedicated view-model-owned composition step.

## Testing Guidance

Prefer Edit Mode tests for:

- view-model-to-view mapping
- panel stack transitions
- focus restoration logic
- submenu command routing

Use Play Mode tests for:

- `UIDocument` wiring
- scene bootstrap
- MonoBehaviour integration
- interactions that depend on Unity runtime behavior

## Review Checklist

Before merging a new UI component, verify:

- it uses UI Toolkit, not legacy uGUI
- markup is loaded through `UiMarkupResources`
- naming follows the control/resource conventions
- gameplay rules are not embedded in the control
- `[UxmlAttribute]` is used only for structural config — no game data or view state fields
- view state records contain only data — no structural config that belongs in an attribute
- subscriptions have clear ownership and teardown
- tests exist at the right layer for the behavior being added
