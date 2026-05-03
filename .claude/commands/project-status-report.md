Produce an actionable project status report using GitHub CLI and local git state. Covers current branch/worktree cleanliness, recent commits, open PRs, open issues, blocked vs unblocked analysis, and a concrete Next Steps section.

$ARGUMENTS

## Required Inputs

- Repository owner/name (infer from current repo when possible).
- Optional project number or URL for GitHub Project board signals (default: read from `.claude/.delivery.config.json`).
- Optional phase prefix filter (e.g. `MVP-`, `P1-`, `P2-`) to scope analysis.

## Data Collection Checklist

1. Code/branch status (local):
   - `git status --short --branch`
   - `git log --oneline -n 10`

2. Pull requests:
   - `gh pr list --state open --limit 50 --json number,title,url,isDraft,reviewDecision,mergeStateStatus,updatedAt,author`
   - Optionally include recently merged: `gh pr list --state merged --limit 10 --json number,title,url,mergedAt,author`

3. Issues:
   - `gh issue list --state open --limit 200 --json number,title,url,labels,assignees,updatedAt`
   - If a prefix is provided, filter by title prefix or search query.

4. Blocked vs unblocked analysis:
   - Use GraphQL to fetch `blockedBy` and `blocking` for all open issues:
     ```
     gh api graphql -f query='{ repository(owner:"<owner>", name:"<repo>") { issues(states:OPEN, first:50) { nodes { number title blockedBy(first:10) { nodes { number title state } } blocking(first:10) { nodes { number title state } } } } } }'
     ```
   - Treat an issue as **blocked** if `blockedBy.nodes` contains at least one issue whose `state` is `OPEN`.
   - Treat an issue as **unblocked** if `blockedBy.nodes` is empty, or all entries have `state: CLOSED`.
   - Do NOT use `trackedInIssues` — it reflects task-list membership, not blocking relationships.
   - Note: `blockedBy` only reflects dependencies set via GitHub's native "blocked by" relationship UI. Issues with only body-text dependency notes will appear unblocked; call that out if relevant.

5. Optional project board signal (if project provided):
   - `gh project item-list <number> --owner <owner> --limit 250 --format json`
   - Summarize status distribution (`Todo`, `In Progress`, `Done`).

## Priority Heuristics

When labels are present:

- Prioritize `priority:p0` over `priority:p1` over `priority:p2`.
- Prefer unblocked issues for immediate pickup.
- If a blocked issue is high priority, highlight its blocker first in Next Steps.

## Output Requirements

Return two mandatory sections in this order:

1. **Summary**
   - Current branch and worktree cleanliness.
   - Recent commit highlights.
   - Open PR highlights and review/merge readiness.
   - Open issues snapshot.
   - Blocked vs unblocked counts.
   - Optional board status snapshot.

2. **Next Steps**
   - Concrete, ordered suggestions (3–7 items).
   - Suggestions must reflect blocked/unblocked reality.
   - If blockers exist, include explicit unblock actions first.

## Hyperlink Rules (Mandatory)

Always use markdown hyperlinks when referencing issues or PRs:

- Issue: `[#123](https://github.com/<owner>/<repo>/issues/123)`
- PR: `[#45](https://github.com/<owner>/<repo>/pull/45)`

Do not reference issue or PR numbers as plain text without links.

## Safety Rules

- Prefer read-only commands for status reporting.
- If required data is unavailable, state assumptions explicitly.
- Keep the report concise, but include enough detail to justify Next Steps.
