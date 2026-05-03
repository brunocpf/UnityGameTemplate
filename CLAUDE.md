# __GAME_DISPLAY_NAME__ — Claude Instructions

**__GAME_DISPLAY_NAME__** is a Unity 6 game project. <!-- TODO: replace this line with one sentence describing genre, platforms, and the core hook. -->
Engine: Unity __UNITY_VERSION__ · URP 17.x · C# netstandard2.1 · UniTask · R3 · UI Toolkit only.

**Use `docs/` as primary source of truth** before writing or proposing any code.

---

## Hard Rules

- `__GAME_NAMESPACE__.Core.*` must **never** reference `__GAME_NAMESPACE__.UI.*`, `__GAME_NAMESPACE__.Unity.*`, or `UnityEngine`.
- Never edit `.unity`, `.prefab`, `.mat`, or `.asset` files with text tools — use **unity-mcp** only.
- Tests: always `./scripts/run-tests.sh`. Never `dotnet test`. Unity must be closed.
- After any `.asmdef` change: `python3 scripts/check_asmdef_deps.py` must output `No core-layer violations detected.`

---

## Agent Routing

| Task | Agent |
| --- | --- |
| Gameplay / domain C# | `gameplay` subagent |
| Unity scene / prefab / asset | `unity-dev` subagent |
| GitHub issues / project board | `delivery` subagent |

---

## Quick Reference

| Task | Command |
| --- | --- |
| Compiler check | `dotnet build __GAME_NAMESPACE__.slnx` |
| Run all tests | `./scripts/run-tests.sh all` |
| Format verify | `dotnet format __GAME_NAMESPACE__.slnx --verify-no-changes` |
| Assembly dep check | `python3 scripts/check_asmdef_deps.py` |
| Project status | `/project-status-report` |
| Start issue | `/start-work-on-issue <n>` |
| Phase backlog | `/phase-backlog-bootstrap` |
| Delivery loop | `/phase-delivery-loop` |
| Phase closeout | `/phase-closeout` |
| Architecture review | `/architecture-audit` |

---

## Sub-Instructions (loaded on entry)

- **Assembly structure, dependency graph, coding standards, new-assembly checklist** → [`Assets/Scripts/CLAUDE.md`](Assets/Scripts/CLAUDE.md)
- **Build, test & validation details** → [`scripts/CLAUDE.md`](scripts/CLAUDE.md)
- **Docs navigation (what to read per task area)** → [`docs/CLAUDE.md`](docs/CLAUDE.md)
