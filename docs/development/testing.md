# Testing

## Strategy

| Layer | Tool | Coverage target |
|-------|------|----------------|
| Unit tests | Unity Test Framework (Edit Mode) | Core logic, utilities |
| Integration | Unity Test Framework (Play Mode) | System interactions |
| Manual QA | Checklist below | Every milestone |

Core/domain assemblies should receive the majority of unit-test coverage. Keep core logic separate from Unity/UI code so rules and state transitions can be tested without scene/runtime dependencies.

## Test Assembly Naming and Placement

Follow these naming and folder rules consistently:

- Edit Mode unit-style assemblies use the `__GAME_NAMESPACE__.*.Tests` suffix and live under `Assets/Tests/EditMode/`.
- Play Mode runtime/integration assemblies use the `__GAME_NAMESPACE__.*.PlayModeTests` suffix and live under `Assets/Tests/PlayMode/`.
- Folder structure should mirror the feature and assembly name closely enough that the target layer is obvious from the path.

Examples:

- `__GAME_NAMESPACE__.Core.Sample.Tests` → `Assets/Tests/EditMode/Core/Sample/`
- `__GAME_NAMESPACE__.UI.Sample.PlayModeTests` → `Assets/Tests/PlayMode/UI/Sample/`

Use Edit Mode tests by default for pure domain logic, view models, and deterministic panel-stack behavior. Use Play Mode tests when validation depends on `UIDocument`, scene objects, runtime input wiring, or Unity-driven lifecycle behavior.

## Running Tests

```
Unity Menu → Window > General > Test Runner
```

- **Edit Mode** — runs without entering Play Mode, fastest.
- **Play Mode** — enters Play Mode, tests runtime behaviour.

## Running Tests (Command Line)

Use the project test runner script for local automation and CI.

```bash
# Run both Edit Mode and Play Mode tests
./scripts/run-tests.sh

# Run only Edit Mode tests
./scripts/run-tests.sh edit

# Run only Play Mode tests
./scripts/run-tests.sh play

# Optional: specify Unity executable path
UNITY_PATH="/Applications/Unity/Hub/Editor/<UNITY_VERSION>/Unity.app/Contents/MacOS/Unity" ./scripts/run-tests.sh

# Optional: change output folder for XML and logs
TEST_RESULTS_DIR="$(pwd)/TestResults" ./scripts/run-tests.sh all
```

Notes:
- The script writes NUnit-compatible XML and logs into `TestResults/` by default.
- Test mode arguments: `edit`, `play`, or `all` (default).
- Set `UNITY_PATH` if Unity is not installed under the default Unity Hub location.
- Keep Unity closed while running command-line tests to avoid project lock conflicts.
- Ensure the project has finished initial import before running tests in CI.

## Reactive and Lifecycle Testing Guidance

Prioritize tests that lock down the architecture decisions in this repository:

- Core state transitions should be covered in Edit Mode where possible.
- Subscription-backed systems should verify disposal and idempotent teardown.
- View-model tests should assert projected UI state and commands instead of asserting Unity visual tree internals.
- Panel navigation tests should verify `Push`, `Pop`, `Clear`, action-map handoff, and focus restoration behavior.
- When a bug is caused by disposal, command flow, or event sequencing, add a regression test in the closest unit-test assembly before relying on manual QA.

## Manual QA Checklist

Run before every milestone / release candidate:

- [ ] Game launches without errors
- [ ] All scenes load correctly
- [ ] Core loop completable start to finish
- [ ] Settings saved and loaded correctly
- [ ] No memory leaks over 10-minute session (check Profiler)
- [ ] Tested on minimum-spec machine

## Known Issues

_Track known issues here until they move to the bug tracker._

| # | Description | Severity | Status |
|---|-------------|----------|--------|
| | | | |
