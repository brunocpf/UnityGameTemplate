# Using UnityGameTemplate

This is a **GitHub Template repository**. To start a new Unity 6 project from it:

1. Click **"Use this template"** on GitHub → **"Create a new repository"**.
2. Clone your new repo locally.
3. Run the bootstrap script:
   ```bash
   ./bootstrap.sh
   ```
   The script asks for:
   - Project display name (e.g. `My Cool Game`)
   - Namespace (PascalCase, e.g. `MyCoolGame`)
   - Company name
   - Unity version (defaults to whatever the template ships with)
   - GitHub username (used in `CODEOWNERS`)
   - License holder name (defaults to `git config user.name`)
   - Optional Git remote URL

   It then:
   - Renames every `__GAME_NAMESPACE__`, `__GAME_DISPLAY_NAME__`, `__COMPANY_NAME__`, `__UNITY_VERSION__`, `__GITHUB_USERNAME__`, `__LICENSE_HOLDER__`, `__CURRENT_YEAR__` placeholder across all source/text files.
   - Renames file/folder paths containing `__GAME_NAMESPACE__`.
   - Wipes the template's git history and re-initializes a fresh `main` branch.
   - Deletes itself and this `TEMPLATE.md`.
4. Open the project in Unity. `Library/` regenerates and Unity creates `.meta` files for all source files. Commit those metas as part of your first follow-up commit.
5. Create or open `Assets/Scenes/SampleScene.unity` (the template ships without a hand-written scene to avoid Unity-version churn). Add a GameObject with `SampleSceneController` + `UIDocument` components. Save the scene. Add it to `EditorBuildSettings`.
6. Configure `.claude/.delivery.config.json` with your GitHub Project number once you've created the project board.

## Continuous integration

`.github/workflows/ci.yml` runs three jobs: `dotnet format`, asmdef dependency check, and Unity Test Runner (EditMode + PlayMode).

To enable Unity tests on CI, add the following GitHub repository secrets:

| Secret | Source |
| --- | --- |
| `UNITY_LICENSE` | Unity Personal license file content (see [game-ci docs](https://game.ci/docs/github/activation)) |
| `UNITY_EMAIL` | Unity account email |
| `UNITY_PASSWORD` | Unity account password |

Until those secrets are set, the `test` job will fail; the other two jobs run without secrets.

## Updating Unity packages

UPM dependencies are listed in `Packages/manifest.json`. Dependabot does not natively support UPM, so update them manually via Unity's **Window → Package Manager → Updates available** panel. GitHub Actions and pip dependencies in `scripts/` are auto-updated via `.github/dependabot.yml`.

## What's in the box

- **Layered architecture**: `Core/UI/Unity` assembly split with `noEngineReferences: true` on Core.
- **Hello-world vertical slice**: a `Greeter` domain class, a `SamplePanel` UI Toolkit panel, and a `SampleSceneController` MonoBehaviour — wired together end-to-end.
- **UI Toolkit token system**: three-tier USS architecture (`Primitives` → `Semantic` → component styles). Three canonical font slots. No raw colors at the semantic layer.
- **Reusable UI/Utilities library**: `UiMarkupResources`, `Router`, `Panel` base classes, focus manipulators.
- **Test scaffolding**: one EditMode and one PlayMode test, both passing out of the box.
- **Claude Code workflow**: three subagents (`gameplay`, `delivery`, `unity-dev`) and six slash commands for project delivery (`/project-status-report`, `/phase-backlog-bootstrap`, `/phase-delivery-loop`, `/phase-closeout`, `/start-work-on-issue`, `/architecture-audit`).
- **GitHub workflow**: PR template, structured issue templates (bug / feature / chore), CODEOWNERS, dependabot.
- **Docs skeleton**: 33 docs covering architecture, coding standards, UI Toolkit, testing, workflow, plus design TODO templates ready to fill.

## Removing the template scaffolding

`bootstrap.sh` is single-use. After running, it removes itself and `TEMPLATE.md`, leaving only the project files. A `.bootstrapped` sentinel file is written so accidental re-runs fail fast.
