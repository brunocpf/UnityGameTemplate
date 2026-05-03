#!/usr/bin/env python3
"""Validate Unity assembly dependency rules.

Rules enforced:
  - Tests are excluded from checks (asmdef name containing 'test').
  - External / non-Unity libs are allowed.
  - Core assemblies (matching CORE_PREFIX) MUST NOT depend on UI assemblies
    (matching UI_PREFIX) or Unity-specific assemblies.

Configuration is read from the GAME_NAMESPACE environment variable, falling
back to the placeholder `__GAME_NAMESPACE__` so the script works inside the
template even before bootstrap.
"""
import json
import os
import sys

GAME_NAMESPACE = os.environ.get("GAME_NAMESPACE", "__GAME_NAMESPACE__")
CORE_PREFIX = f"{GAME_NAMESPACE}.Core"
UI_PREFIX = f"{GAME_NAMESPACE}.UI"


def find_asmdefs(root: str) -> list[str]:
    paths: list[str] = []
    for dirpath, _dirnames, filenames in os.walk(root):
        if any(skip in dirpath for skip in ("/Library/", "/Temp/", "/obj/")):
            continue
        for fn in filenames:
            if fn.endswith(".asmdef"):
                paths.append(os.path.join(dirpath, fn))
    return sorted(paths)


def load_assemblies(paths: list[str], root: str) -> dict[str, dict]:
    assemblies: dict[str, dict] = {}
    for path in paths:
        try:
            with open(path, "r", encoding="utf-8-sig") as f:
                js = json.load(f)
        except Exception as e:
            print(f"ERROR: Could not parse {path}: {e}", file=sys.stderr)
            continue
        name = js.get("name")
        if not name:
            continue
        assemblies[name] = {
            "file": os.path.relpath(path, root),
            "refs": js.get("references") or [],
        }
    return assemblies


def is_unity_specific(ref: str) -> bool:
    rl = ref.lower()
    return (
        "unity" in rl
        or rl.startswith("unityengine")
        or rl.startswith("unityeditor")
        or "testrunner" in rl
    )


def main() -> int:
    root = os.getcwd()
    paths = find_asmdefs(root)
    assemblies = load_assemblies(paths, root)

    violations: list[tuple[str, str, str, str]] = []
    skipped: list[tuple[str, str]] = []

    for name, info in sorted(assemblies.items()):
        if "test" in name.lower():
            skipped.append((name, info["file"]))
            continue

        if not name.startswith(CORE_PREFIX):
            continue

        for ref in info["refs"]:
            if not isinstance(ref, str):
                continue
            if ref.startswith(UI_PREFIX):
                violations.append(
                    (name, info["file"], ref, "Core assemblies must not reference UI assemblies")
                )
                continue
            if is_unity_specific(ref):
                violations.append(
                    (name, info["file"], ref, "Core assemblies must not reference Unity-specific assemblies")
                )

    print(f"Found {len(assemblies)} asmdef files ({len(skipped)} skipped test assemblies)")
    print(f"Namespace: {GAME_NAMESPACE} (CORE_PREFIX={CORE_PREFIX}, UI_PREFIX={UI_PREFIX})")
    print()

    if violations:
        print("Violations (core assemblies referencing UI or Unity-specific assemblies):")
        for v in violations:
            print(f"- {v[0]} ({v[1]}) -> {v[2]} : {v[3]}")
        print()
        print("Summary:")
        print(f"- {len(violations)} violation(s) found (fix required)")
    else:
        print("No core-layer violations detected.")
        print()
        print("Summary:")
        print("- No violations found")

    print(f"- {len(skipped)} asmdefs skipped (tests)")
    return 2 if violations else 0


if __name__ == "__main__":
    sys.exit(main())
