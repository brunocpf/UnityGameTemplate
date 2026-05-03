# Build, Test & Validation

## Compiler Check

```bash
dotnet build __GAME_NAMESPACE__.slnx                    # full solution
dotnet build __GAME_NAMESPACE__.Core.Sample.csproj      # single assembly
```

May fail transiently when Unity's `.csproj` files are unsynced — retry once.

## Tests

> **HARD RULE:** Always use `./scripts/run-tests.sh`. Never `dotnet test`. Unity must be closed.

```bash
./scripts/run-tests.sh edit    # Edit Mode only
./scripts/run-tests.sh play    # Play Mode only
./scripts/run-tests.sh all     # both (default)
```

Results → `TestResults/` (NUnit XML). Unity auto-detected at `/Applications/Unity/Hub/Editor/`; override with `UNITY_PATH`.

## Code Formatting

Pre-commit hook runs formatting automatically on staged `.cs` files. Manual verify:

```bash
dotnet format __GAME_NAMESPACE__.slnx --verify-no-changes
```

Warnings about PlayMode/UI test projects missing Unity references are expected — ignore.

## Assembly Dependency Validation

```bash
python3 scripts/check_asmdef_deps.py   # must be clean before merging .asmdef changes
```

CI runs this automatically (`.github/workflows/check-asmdef-deps.yml`) on `.asmdef` changes.

The script's namespace prefix is read from the `GAME_NAMESPACE` env var; the default matches the project's namespace once `bootstrap.sh` has run.
