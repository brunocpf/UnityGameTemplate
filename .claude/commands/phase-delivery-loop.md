Run the day-to-day delivery execution loop for the active phase: identify ready-to-start issues, respect the dependency graph, advance project board statuses, and maintain issue hygiene. Returns a compact phase snapshot with ready/in-progress/blocked breakdown and recommended next picks.

$ARGUMENTS

## Inputs

- Project number or URL (default: read from `.claude/.delivery.config.json`).
- Phase issue prefix (e.g. `MVP-`, `P1-`, `P2-`).
- Desired WIP policy (e.g. limit on `In Progress`).
- Optional assignee filter.

## Core Workflow

1. Pull project items and statuses.
2. Pull issue dependencies and detect blocked issues.
3. Generate a "ready now" set:
   - Open issues
   - Not blocked by open prerequisites
   - Status not already `Done`
4. Move selected ready items to `In Progress`.
5. Keep labels and checklists aligned with template intent.
6. Encourage PR linkage with `Closes #<issue-number>` when coding starts.
7. Periodically validate status drift between issue labels and the project status field.

## Required Rules

- Never mark an item `Done` until acceptance criteria are confirmed complete.
- Do not move blocked work to `In Progress` unless explicitly instructed.
- Keep status transitions auditable in the final report.
- Prefer small, repeatable batches rather than massive one-shot updates.

## Tooling Guidance

- Read board state using `gh project item-list`.
- Update project fields with `gh project item-edit` when status changes are needed.
- Read issue metadata and dependencies via `gh issue list` and `gh api graphql`.

## Validation Checklist

- No blocked issues in active `In Progress` set (unless explicitly allowed).
- `Done` items have closed loop on acceptance criteria/checklists.
- Project status field reflects actual execution state.
- PR-linked issues follow closure conventions.

## Output Format

Return a compact phase snapshot:

1. Ready now.
2. In progress.
3. Blocked (with blocker IDs).
4. Drift warnings (if any).
5. Recommended next 3–5 issues to pull.
