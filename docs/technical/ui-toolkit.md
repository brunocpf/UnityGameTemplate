# UI Toolkit Architecture

## Decision

__GAME_DISPLAY_NAME__ uses **Unity UI Toolkit** for all runtime UI.

- Legacy uGUI/canvas-driven UI should not be used for new UI implementation.
- Runtime UI should be composed from `VisualElement`-based custom controls.

## Control Model

Each custom UI component's script lives in the corresponding UI assembly. Markup and style assets live in a **centralized** `Assets/Resources/` tree — **not colocated with the script**.

Script layout:

```text
Assets/Scripts/UI/<Feature>/
  <ControlName>/
    <ControlName>.cs
```

Resource layout:

```text
Assets/Resources/UI/<Feature>/<ControlName>/
    <ControlName>Markup.uxml      (optional)
    <ControlName>Styles.uss       (optional)
```

Naming rules:

- Class: `<ControlName>.cs`
- Markup: `<ControlName>Markup.uxml`
- Styling: `<ControlName>Styles.uss`
- Resource path: `UI/<Feature>/<ControlName>/<ControlName>Markup`

All custom controls derive from `VisualElement` (simple controls) or `MenuPanel` (router-managed views — see Router section below).

## USS Naming Convention (BEM)

USS class names should follow BEM conventions:

- Block: `.sample-panel`
- Element: `.sample-panel__header`
- Modifier: `.sample-panel--compact`

Guidelines:

- Use one block per control root where possible.
- Use elements for internal structure.
- Use modifiers for state/variant styling.
- Keep class names lowercase and hyphenated.

## General UXML Structure

```uxml
<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:uie="UnityEditor.UIElements" editor-extension-mode="False">
    <ui:Style src="./ComponentStyles.uss" />
    <!-- Markup goes here -->
</ui:UXML>
```

## Markup Loading Pattern

Custom controls load their UXML/USS through the centralized `Assets/Resources/` hierarchy using the `UiMarkupResources` helper from `__GAME_NAMESPACE__.UI.Utilities`.

Rules:

1. Place each control's markup and styles under `Assets/Resources/UI/<Feature>/<ControlName>/`.
2. Use globally unique resource paths such as `UI/Sample/SampleMarkup`.
3. Load and cache markup with `UiMarkupResources.CloneInto(...)` — never call `Resources.Load` directly in a control constructor.
4. Restrict `Resources` usage to small UI Toolkit markup and style assets always shipped with the build.
5. The helper throws `InvalidOperationException` for missing asset paths.

**Standard usage:**

```csharp
using __GAME_NAMESPACE__.UI.Utilities;
using UnityEngine.UIElements;

[UxmlElement]
public sealed partial class SamplePanel : VisualElement
{
    private const string MARKUP_PATH = "UI/Sample/SampleMarkup";
    private const string ROOT_CLASS = "sample-panel";

    public SamplePanel()
    {
        UiMarkupResources.CloneInto(this, MARKUP_PATH, ROOT_CLASS);
    }
}
```

## Directory and Naming Example

```text
Assets/Scripts/UI/Sample/
    SamplePanel/
        SamplePanel.cs

Assets/Resources/UI/Sample/
    SampleMarkup.uxml
    SampleStyles.uss
```

## Theming and Styling

Styling uses a three-tier token architecture. All tokens are loaded by `Theme.tss` (the panel's assigned TSS) and are inherited by every element on that panel.

### Tier 1 — Primitive Tokens (`Assets/UI Toolkit/Tokens/Primitives/`)

Raw, context-free values. No token in this tier may reference another custom property.

| File | Contains |
|------|----------|
| `Colors.uss` | Full `--clr-<scale>-<step>` palette |
| `Typography.uss` | Font assets (`--font-*`), text-size scale (`--text-xs` → `--text-3xl`), letter-spacing, text-outline widths |
| `Spacing.uss` | 4-point spacing scale (`--space-0` → `--space-24`) |
| `Borders.uss` | Border radius (`--rounded-*`) and border width (`--border-*`) primitives |
| `Shadows.uss` | Shadow color tints (`--shadow-color-*`), offsets, and blur radii |
| `Motion.uss` | Animation duration scale (`--duration-*`) and easing keyword aliases |
| `Opacity.uss` | Opacity level scale (`--opacity-0` → `--opacity-full`) |

### Tier 2 — Semantic Tokens (`Assets/UI Toolkit/Tokens/Semantic.uss`)

Contextual aliases that give meaning to primitive values. Prefixed with `--sys-`. These are what components should reference for colors, typography, radii, borders, spacing, and motion.

**Naming conventions:**
- All semantic tokens are prefixed with `--sys-`
- Color tokens use `--sys-clr-` (e.g. `--sys-clr-text-primary`, `--sys-clr-surface`)
- Never define raw color values in semantic tokens — always reference an existing primitive

**Canonical typefaces (configure in your project):**
- `--sys-font-primary` — general UI text
- `--sys-font-secondary` — numbers, highlighted values
- `--sys-font-tertiary` — titles, headers, labels

**Font size scale (targeting 1920×1080):**
- `--sys-font-size-xs/sm/md/lg/xl/2xl/3xl` — references `--text-xs` → `--text-3xl` primitives

**Other token groups:**
- Text colors: `--sys-clr-text-primary/muted/disabled/label/outline`
- Surfaces: `--sys-clr-surface/surface-strong/bg-main/bg-surface/panel-bg/…`
- Accents: `--sys-clr-accent-interactive/focus/…`
- Text outlines: `--sys-text-outline-width-sm/md/lg`, `--sys-clr-text-outline-strong`
- Layout: `--sys-radius-xs/sm/md/lg/xl/full`, `--sys-border-thin/medium/thick`, `--sys-space-*`
- Motion: `--sys-anim-snappy/fast/normal/fade/stately`

### Tier 3 — Component Tokens (per-component `*Styles.uss`)

Component-specific styles. Prefixed with `--cmp-`. Each component's USS file lives colocated with the component under `Resources/UI/<Feature>/<ControlName>/`.

**Structure rules:**
- The **first selector** in a USS file must be the component's root element class (e.g. `.sample-panel { }`)
- All component tokens must be defined in that root selector
- The component body selectors may only reference `--cmp-` or `--sys-` tokens — never `--clr-*` or other primitives directly
- Bold/display font variants that fall outside the canonical fonts may be referenced in the root selector token definitions

### Rules

- Never use raw `--clr-*` palette tokens in component USS body selectors — route through `--sys-clr-*` semantic aliases.
- Never define new color values in `Semantic.uss` — all colors must reference an existing primitive from `Colors.uss`.
- Semantic tokens should be named by meaning, not visual appearance, and not be specific to a single component.
- Never hardcode color or spacing values in component body selectors when a token exists.
- Avoid duplicating semantic token definitions across components.
- If a control's UXML references its USS via `<ui:Style src="./<ControlName>Styles.uss" />`, do not load the stylesheet separately in C#.
- Tokens/Primitives files import nothing. Semantic.uss imports nothing. Theme.tss orchestrates the load order.

### Import order in Theme.tss

```text
Tier 1 → Colors → Typography → Spacing → Borders → Shadows → Motion → Opacity
Tier 2 → Semantic.uss
(Tier 3 loaded per-component, not in Theme.tss)
```

## Router-Based Navigation

Larger views are managed through `Router<TKey>` from `__GAME_NAMESPACE__.UI.Utilities`. The router is a generic, key-based, reactive navigator with a serialized async operation queue — concurrent `GoTo`/`Back`/`Reset` calls are automatically chained.

### API

```csharp
Router<TKey> router = new();

// Register routes before navigating.
router.Register(MyKey.Home, new RouteSpec { Element = homePanel, Transition = RouteTransitions.Fade(0.15f) });
router.Register(MyKey.Settings, new RouteSpec { Element = settingsPanel });

// Navigate.
await router.GoTo(MyKey.Settings);   // push Settings (or unwind if already in stack)
await router.Back();                 // pop to previous
await router.Reset(MyKey.Home);      // clear stack, push Home
await router.Clear();                // exit all routes

// React to current route.
router.Current.Subscribe(key => Debug.Log($"Now at {key}"));
router.Stack.Subscribe(keys => /* full stack snapshot */);

router.Dispose();                    // detaches all routes, disposes observables
```

### RouteSpec

`RouteSpec` is the registration record for one route. All members are `init`-only:

```csharp
new RouteSpec
{
    Element      = myVisualElement,                          // auto-managed (enabled, picking, focus)
    Transition   = RouteTransitions.Fade(0.2f),             // animation (default: RouteTransitions.None)
    OnEnter      = ct => DoWorkAsync(ct),                   // optional async hook (runs with transition)
    OnExit       = ct => ...,
    OnCover      = ct => ...,
    OnReveal     = ct => ...,
    OnRestoreFocus = () => myElement.Focus(),               // for Element-less routes
}
```

### IRouteTransition

Implement `IRouteTransition` for custom animations. Built-in implementations in `RouteTransitions`:

- `RouteTransitions.None` — instant, no animation (default)
- `RouteTransitions.Fade(float duration, float coveredOpacity = 0.5f)` — opacity tween via LitMotion

### MenuPanel base class

`MenuPanel` is a lightweight `VisualElement` base class that enables focusable, delegated-focus behaviour. Use it for views registered with the router.

```csharp
[UxmlElement]
public sealed partial class MyView : MenuPanel
{
    public MyView()
    {
        UiMarkupResources.CloneInto(this, "UI/MyFeature/MyViewMarkup", "my-view");
    }
}
```

Focus, `pickingMode`, and interaction state are managed by `Router<TKey>` — `MenuPanel` only sets up the initial `focusable = true`, `delegatesFocus = true`.

### Router lifecycle ownership

- The router should be owned by a MonoBehaviour or composite view controller that controls the owning scene lifetime.
- Call `router.Dispose()` in `OnDestroy`/`OnDisable` to clean up observables and event callbacks.
- All registered `Element` callbacks are unregistered on `Dispose`.

See [ui-implementation-guide.md](ui-implementation-guide.md) for guidance on when to use the router vs. plain `VisualElement` controls.

## Reactive Subscription Guidance

UI Toolkit controls should subscribe to reactive state at the presentation boundary, not inside core systems.

Guidelines:

- Keep subscriptions in view models, controller adapters, or view classes that only map state to visuals.
- Prefer one-way rendering from immutable snapshots into controls.
- Dispose subscriptions with the owning object lifecycle.
- Do not embed gameplay-rule decisions inside subscription callbacks.
- If a `Panel` owns subscriptions directly, make the ownership explicit and ensure the subscriptions are released when the panel leaves active use.

Recommended split:

- `__GAME_NAMESPACE__.Core.*`: state, rules, transitions, calculations
- `__GAME_NAMESPACE__.UI.*`: rendering, menu flow, focus handling, input-to-command mapping
- `__GAME_NAMESPACE__.Unity.*`: scene and runtime bootstrap, `UIDocument` wiring, MonoBehaviour lifecycle integration

## Testing Considerations

- Keep panel state and navigation decisions in testable code paths.
- Validate `Push`/`Pop` transition ordering and focus restoration behavior.
- Test that theme + component styles both apply as expected.
- Prefer Edit Mode tests for view-model and panel-stack behavior that does not require full scene runtime.
