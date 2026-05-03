# __GAME_DISPLAY_NAME__

<!-- TODO: one-line description of the game (genre, hook, target platforms) -->

Unity __UNITY_VERSION__ project scaffolded from [UnityGameTemplate](https://github.com/__GITHUB_USERNAME__/UnityGameTemplate).

## Tech stack

- **Engine**: Unity __UNITY_VERSION__ (URP 17.x)
- **Language**: C# (`netstandard2.1`, nullable enabled)
- **UI**: UI Toolkit (UXML/USS, three-tier design tokens)
- **Async**: [UniTask](https://github.com/Cysharp/UniTask)
- **Reactive**: [R3](https://github.com/Cysharp/R3)
- **Tweening**: [LitMotion](https://github.com/annulusgames/LitMotion)
- **Asset loading**: Unity Addressables
- **Tests**: NUnit + Unity Test Framework (EditMode + PlayMode)

## Quick start

```bash
# Clone
git clone <your-repo-url>
cd <your-repo>

# Install Git hooks (formats staged C# on commit)
./scripts/setup-git-hooks.sh

# Open in Unity __UNITY_VERSION__ — Library/ regenerates on first open.

# Run tests headlessly
./scripts/run-tests.sh all

# Verify formatting
dotnet format __GAME_NAMESPACE__.slnx --verify-no-changes

# Verify assembly dependency rules
python3 scripts/check_asmdef_deps.py
```

## Repository layout

| Path | Purpose |
| --- | --- |
| `Assets/Scripts/Core/` | Pure-domain C# (engine-free) |
| `Assets/Scripts/UI/` | UI Toolkit panels and controls |
| `Assets/Scripts/UI/Utilities/` | Shared UI helpers (router, panels, markup loader) |
| `Assets/Scripts/Unity/` | MonoBehaviours, scene wiring |
| `Assets/Tests/EditMode/` | NUnit tests for Core/UI |
| `Assets/Tests/PlayMode/` | Unity-runtime integration tests |
| `Assets/UI Toolkit/` | Theme, root UXML, design tokens (Semantic + Primitives) |
| `Assets/Resources/UI/` | UXML/USS markup pairs (loaded via `UiMarkupResources`) |
| `docs/` | Project documentation (design, technical, development, art, audio) |
| `scripts/` | Build/test/validation scripts |
| `.claude/` | Claude Code agents, commands, and settings |
| `.githooks/` | Git hooks (pre-commit format, LFS hooks) |

## Documentation

- **Architecture & coding standards** → [`docs/technical/`](docs/technical/)
- **Workflow & process** → [`docs/development/`](docs/development/)
- **Game design** → [`docs/design/`](docs/design/) <!-- fill in as you go -->
- **AI agent setup** → [`CLAUDE.md`](CLAUDE.md)

## License

MIT — see [LICENSE](LICENSE).
