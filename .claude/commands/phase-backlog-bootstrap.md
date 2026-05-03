Create a complete phase backlog in GitHub from a markdown source file: ensure required labels exist, create or select a phase-specific GitHub Project, create issues using the repository issue template, add all issues to the project, wire native dependency links, and validate results.

$ARGUMENTS

## Inputs

- Backlog source file path (e.g. `docs/development/<phase>-tasks.md`).
- Repository owner and name (infer from current repo when possible).
- Project title to create or reuse.
- Label policy: create missing labels or fail-fast.
- Whether to dry-run first.

Project board metadata (project number, status field name, column names, label conventions) lives in `.claude/.delivery.config.json`. Read it before mutating the project board.

## Required Behavior

1. Read and parse the backlog source file.
2. Detect existing phase issues first to avoid duplication.
3. Use idempotent operations:
   - Reuse existing issues when title IDs already exist (e.g. `MVP-001`, `P1-003`).
   - Skip already-existing project item insertions.
   - Skip already-linked dependencies.
4. Keep issue body sections aligned with `.github/ISSUE_TEMPLATE/feature.yml` (or whichever template is configured for backlog items).
5. Create dependency links using native GitHub dependency mutations (`addBlockedBy`) after all issues exist.
6. Run verification after mutation steps and provide a concise audit summary.

## Tooling Guidance

Use GitHub CLI and GraphQL APIs.

Recommended command families:

- `gh project create`, `gh project view`, `gh project field-list`, `gh project item-add`, `gh project item-list`
- `gh label create`
- `gh issue create`, `gh issue list`
- `gh api graphql`

## Compatibility Notes

- Do not assume `gh issue create --json` exists on all installations.
- Prefer parsing the issue URL from stdout when JSON output is unavailable.
- If the CLI rejects a flag, fall back to the compatible path and continue.

## Validation Checklist

- Project exists and has the expected title.
- Total phase issues created or reused matches backlog count.
- All phase issues are present on the project board.
- All expected dependency edges exist.
- Missing labels were either created or explicitly reported.

## Output Format

Return a short operational report with:

1. Project URL.
2. Created/reused issue list (ID → issue number + URL).
3. Dependency linkage summary (applied / existing / missing).
4. Any deviations, retries, or compatibility fallbacks used.
