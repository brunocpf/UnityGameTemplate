# Nullable Reference Types (NRT) in Unity

This project enables nullable reference type analysis through both compiler response files and MSBuild properties.

## Goal

Enable `-nullable:enable` for every assembly so nullability warnings are visible and actionable across all code.

## Project-Level MSBuild Setting

At the repository root, use `Directory.Build.props` to enable nullable analysis for IDE/MSBuild scenarios:

```xml
<Project>
   <PropertyGroup>
      <Nullable>enable</Nullable>
   </PropertyGroup>
</Project>
```

File location:
- `Directory.Build.props`

This ensures generated solution/project builds (for example from `dotnet` and IDE tooling) also run with nullable enabled.

## Unity Compiler Setting (`.rsp`)

Unity supports compiler response files (`.rsp`) to pass additional C# compiler arguments.

Use this approach:

1. For predefined assemblies (`Assembly-CSharp`, `Assembly-CSharp-Editor`), create a root response file:
   - `Assets/csc.rsp`
2. For each custom asmdef assembly, create an assembly-specific response file in the same folder as the asmdef:
   - `<AssemblyName>.rsp`

Example:
- `Assets/Scripts/Core/Sample/__GAME_NAMESPACE__.Core.Sample.asmdef`
- `Assets/Scripts/Core/Sample/__GAME_NAMESPACE__.Core.Sample.rsp`

## Required Compiler Argument

Add this line to each response file:

```text
-nullable:enable
```

Minimal file example:

```text
-nullable:enable
```

## Setup Checklist

1. Add `Directory.Build.props` with `<Nullable>enable</Nullable>` at repository root.
2. Add `Assets/csc.rsp` with `-nullable:enable`.
3. For every asmdef in the project, add a matching `<AssemblyName>.rsp` in the asmdef folder.
4. Reimport scripts or restart Unity so compilation picks up the new response files.
5. Fix nullable warnings incrementally, prioritizing core assemblies first (`__GAME_NAMESPACE__.Core.*`).

## Automation Script

Use the repository helper script to create missing nullable response files:

```bash
./scripts/setup-nullable-rsp.sh
```

What it does:

- Ensures `Assets/csc.rsp` exists with `-nullable:enable`.
- Scans for all `*.asmdef` files under `Assets/`.
- Creates missing `<AssemblyName>.rsp` files next to each asmdef.
- Does not overwrite existing `.rsp` files.

## Recommended Conventions

- Keep only compiler arguments in `.rsp` files (one per line).
- Prefer enabling NRT in all assemblies instead of partially enabling by feature.
- Treat nullable warnings as code quality debt and resolve them as part of normal feature/fix work.

## Troubleshooting

- If warnings do not appear, verify file naming and folder placement next to the target asmdef.
- Ensure files are plain text and committed to source control.
- Confirm both mechanisms are present: `Directory.Build.props` (MSBuild) and `.rsp` files (Unity compiler).
- If a specific warning must be deferred, use targeted pragmas in code with a short rationale, not broad suppression.
