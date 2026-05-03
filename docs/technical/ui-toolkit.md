# UI Toolkit Architecture

## Decision

__GAME_DISPLAY_NAME__ uses **Unity UI Toolkit** for all runtime UI.

- Legacy uGUI/canvas-driven UI should not be used for new UI implementation.
- Runtime UI should be composed from `VisualElement`-based custom controls.

## Control Model

Each custom UI component lives in its own folder in the corresponding UI assembly.

Recommended layout:

```text
__GAME_NAMESPACE__.UI.<Feature>/
  <ControlName>/
    <ControlName>.cs
        Resources/
            UI/
                <Feature>/
                    <ControlName>/
                        <ControlName>Markup.uxml      (optional)
                        <ControlName>Styles.uss       (optional)
```

Naming rules:

- Class: `<ControlName>.cs`
- Markup: `<ControlName>Markup.uxml`
- Styling: `<ControlName>Styles.uss`
- Resource path: `UI/<Feature>/<ControlName>/<ControlName>Markup`

All custom controls should derive from `VisualElement`.

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

## Component-Local Resources Pattern

Custom controls that use UXML/USS should load those assets through a component-local `Resources` path so the control stays colocated and renders correctly in UI Builder.

Design intent:

- Constructor builds a ready-to-use visual tree.
- Markup and styles are encapsulated per control.
- The same control works in both runtime and UI Builder.


Recommended pattern:

1. Place each control's markup and styles under a `Resources/UI/<Feature>/<ControlName>/` subfolder inside the control folder, matching the control's namespace and feature.
2. Use globally unique resource paths such as `UI/Sample/SamplePanel/SamplePanelMarkup`.
3. Load and cache markup using the shared public helper `UiMarkupResources` from the `__GAME_NAMESPACE__.UI.Utilities` assembly.
4. Restrict `Resources` usage to small UI Toolkit markup and style assets that are always shipped with the build.
5. Do not use Addressables or Resources.Load directly in control constructors—always use the helper for consistency and caching.
6. The helper throws an `InvalidOperationException` for missing assets.

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

This pattern keeps markup colocated, minimizes boilerplate, and ensures UI Builder compatibility.

### Why this is the preferred pattern for this project

- Keeps UXML and USS colocated with the control implementation.
- Works in UI Builder without editor-only loaders or runtime bootstrap infrastructure.
- Keeps per-instance construction simple while avoiding repeated `Resources.Load` calls.
- Limits `Resources` usage to small always-shipped UI assets.


## Directory and Naming Example

```text
Assets/Scripts/UI/Sample/SamplePanel/
    SamplePanel.cs
    Resources/
        UI/
            Sample/
                SamplePanel/
                    SamplePanelMarkup.uxml
                    SamplePanelStyles.uss
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

## Panel Stack Navigation

Larger views (inventory, character sheet, settings, etc.) are managed through a `PanelStackController`. The stack works against `IPanelLayer` — not `Panel` directly — so layers may or may not be `VisualElement`s.

- `PanelStackController` API includes:
  - `Push`
  - `Pop`
  - `Clear`
  - `Peek`

Expected behavior:

- `Push`: cover current layer and enter the new one.
- `Pop`: exit top layer and reveal the previous one.
- `Clear`: empty the stack in a controlled order.
- `Peek`: inspect the current top layer without mutation.

Implementation guidance for deciding between plain `VisualElement` controls and stack-managed layers lives in [ui-implementation-guide.md](ui-implementation-guide.md).

## IPanelLayer Contract

`IPanelLayer` is the interface `PanelStackController` operates on. Most layers implement it by extending `Panel` (a `VisualElement` base class). Non-visual layers (e.g. interaction-only modes that delegate to existing views) implement `IPanelLayer` directly as plain C# objects, with no-op `AttachTo`/`Detach` methods.

```csharp
public interface IPanelLayer
{
    string ActionMapName { get; }

    // Visual attachment — no-op for non-visual layers.
    void AttachTo(VisualElement host);
    void Detach();

    // Interaction state.
    void SetStackInteractive(bool isInteractive);
    void RestoreFocus();

    // Lifecycle transitions.
    UniTask Enter();
    UniTask Exit();
    UniTask Cover();
    UniTask Reveal();
}
```

## Base Panel Contract

`Panel` is the standard `VisualElement`-based implementation of `IPanelLayer`. `AttachTo` handles DOM insertion and initial display setup; `Detach` removes and hides the element. Subclasses override the lifecycle transitions and `RestoreFocus` as needed.

Reference shape:

```csharp
public abstract class Panel : VisualElement, IPanelLayer
{
    public abstract string ActionMapName { get; }

    public Panel()
    {
        focusable = true;
        delegatesFocus = true;
        pickingMode = PickingMode.Position;

        RegisterCallback<FocusInEvent>(HandleFocusIn);
    }

    public virtual void AttachTo(VisualElement host) { /* sets display flex, adds to host */ }
    public virtual void Detach()                     { /* sets display none, removes from host */ }

    public virtual UniTask Enter()   => UniTask.CompletedTask;
    public virtual UniTask Exit()    => UniTask.CompletedTask;
    public virtual UniTask Reveal()  => UniTask.CompletedTask;
    public virtual UniTask Cover()   => UniTask.CompletedTask;

    public virtual void RestoreFocus()       { /* re-enables interaction, restores last focused child */ }
    public void SetStackInteractive(bool isInteractive) { /* SetEnabled + pickingMode + blur */ }

    private void HandleFocusIn(FocusInEvent evt)
    {
        if (_isStackInteractive && evt.target is VisualElement child && child != this)
        {
            _lastFocusedElement = child;
        }
    }

    private void RestoreFocusInternal()
    {
        if (_lastFocusedElement != null && _lastFocusedElement.panel != null && _lastFocusedElement.canGrabFocus)
        {
            _lastFocusedElement.Focus();
            return;
        }

        if (canGrabFocus)
        {
            Focus();
        }
    }

    private void BlurFocusedDescendant()
    {
        if (focusController?.focusedElement is not VisualElement focusedElement)
        {
            return;
        }

        if (Contains(focusedElement))
        {
            focusedElement.Blur();
        }
    }
}
```

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
