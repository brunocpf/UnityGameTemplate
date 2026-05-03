Perform a deep architectural review of the __GAME_DISPLAY_NAME__ repository and return concrete findings ordered by severity. Covers assembly boundaries, dependency direction, coding standards adherence, documentation drift, dead code, debug artifacts, testing gaps, and scaling risks.

$ARGUMENTS

## Required Context

Read the relevant repository documentation before auditing code.

Minimum docs to consult:

1. `docs/README.md`
2. `docs/technical/architecture.md`
3. `docs/technical/coding-standards.md`
4. `docs/technical/ui-toolkit.md` when UI code is in scope
5. `docs/development/testing.md`

If the audit touches project workflows or issue management, also read `docs/development/github-projects.md`.

## Audit Workflow

### 1. Establish Intended Architecture

Extract the intended rules from docs before judging implementation:

- Layering model: `__GAME_NAMESPACE__.Core.*`, `__GAME_NAMESPACE__.UI.*`, `__GAME_NAMESPACE__.Unity.*`
- Dependency direction expectations
- Nullability expectations
- UI Toolkit implementation rules
- Testing strategy and assembly organization

Call out any ambiguity or conflict between docs before drawing conclusions.

### 2. Inspect Structural Boundaries

Audit assemblies, folders, and namespaces. Check for:

- `UnityEngine` usage inside core/domain assemblies
- UI or Unity layers leaking into core logic
- Namespace drift relative to folder and assembly boundaries
- Missing or incorrect `.rsp` nullability configuration
- Test assembly naming or scoping inconsistencies
- Unnecessary cross-feature references

### 3. Inspect Architecture in Code

Review representative implementations across core, UI, and Unity-facing layers. Check for:

- Business rules embedded in `MonoBehaviour` or view code
- Hidden global state, static singletons, or service locator patterns
- Tight coupling that will make replacement/testing difficult
- Overgrown classes that mix orchestration, state, and presentation
- Weak lifecycle/disposal handling
- Reactive/async misuse relative to the documented R3 and UniTask standards

### 4. Hunt for Quality and Cleanup Issues

Search systematically for:

- `TODO`, `FIXME`, `HACK`, `XXX`, `NotImplementedException`
- Leftover `Debug.Log` usage in production paths
- Dead code, unused helpers, stale sample/demo artifacts
- Direct `Resources.Load` usage where `UiMarkupResources` is required
- Old UI patterns that conflict with the UI Toolkit standard
- Magic-number-heavy code that should be configuration-driven

### 5. Check Documentation Drift

Compare docs against the current implementation. Flag:

- Docs describing systems as complete when implementation is partial
- Missing docs for patterns that now exist in code
- Stale roadmap or status statements
- Mismatch between documented conventions and actual project structure

### 6. Review Test and Validation Posture

Check whether the current test layout supports the intended architecture. Look for:

- Core logic lacking unit coverage
- UI/runtime logic that can only be manually verified
- Missing guardrails for dependency rules
- Lifecycle/disposal behavior that is untested

## Severity Model

- `RED FLAG`: architecture breakage, likely runtime failure, or a high-risk structural problem
- `High Priority`: important issue that should be fixed before broader scaling or release
- `Medium Priority`: real design debt or documentation drift that should be scheduled
- `Low Priority / Cleanup`: hygiene improvement, optional simplification, or clarity fix

Do not inflate severities. If something is an acceptable tradeoff for the current stage, say so.

## Output Requirements

Structure the response in this order:

1. Findings first, ordered by severity. Each finding must include: severity, file path, line reference when available, what is wrong, why it matters, recommended fix.
2. Open questions or assumptions.
3. Short overall assessment.
4. Optional next-step recommendations.

If no findings are discovered, state that explicitly and still mention residual risks or areas not deeply verified.

## Audit Checklist

- [ ] Docs consulted before code review
- [ ] Assembly graph and namespaces checked
- [ ] Nullability setup checked
- [ ] Core/UI/Unity boundary violations checked
- [ ] Reactive/async patterns sampled
- [ ] MonoBehaviour responsibilities sampled
- [ ] Search for debug markers and unimplemented members performed
- [ ] UI Toolkit rules checked against actual controls
- [ ] Test structure reviewed
- [ ] Documentation drift reviewed
- [ ] Cleanup opportunities captured
