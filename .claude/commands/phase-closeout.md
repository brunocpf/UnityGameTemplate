Close out a completed phase in a controlled, auditable way: confirm planned scope is done, classify carry-over work, normalize all issue and project board statuses, and produce a closeout report with carry-over recommendations and follow-up actions for the next phase.

$ARGUMENTS

## Inputs

- Project number or URL (default: read from `.claude/.delivery.config.json`).
- Phase issue prefix (e.g. `MVP-`, `P1-`, `P2-`).
- Definition of done for the phase (source doc or explicit criteria).
- Optional carry-over policy: move to next phase board, relabel, or close/defer.

## Core Workflow

1. Collect all phase issues and board items.
2. Partition into:
   - Completed and accepted
   - Completed but with acceptance drift
   - In progress
   - Blocked
   - Not started
3. Validate dependency integrity (no orphaned blocked edges).
4. Apply status cleanup:
   - Completed → `Done`
   - Explicitly deferred → `Blocked` or carry-over label
   - Remaining actionable → keep `Todo` or move to next phase project
5. Verify issue checklists and acceptance sections for any items marked complete.
6. Generate closeout report with carry-over recommendations.

## Required Rules

- Do not silently close unfinished issues.
- Explicitly annotate carry-over rationale per remaining issue.
- Keep a clear boundary between done scope and deferred scope.
- If docs define phase exit criteria, evaluate against those criteria.

## Tooling Guidance

- `gh issue list` for scope and state.
- `gh project item-list` and `gh project item-edit` for board normalization.
- `gh api graphql` for dependency inspection where needed.

## Validation Checklist

- Every phase issue has a final disposition.
- Project status distribution is intentional and explained.
- Carry-over set is explicit and next-phase ready.
- A closeout report is produced with links to key items.

## Output Format

Return a closeout report with:

1. Phase summary (done / in progress / blocked / todo counts).
2. Exit criteria pass/fail by criterion.
3. Carry-over list with reason and suggested target phase.
4. Follow-up actions for next phase bootstrap.
