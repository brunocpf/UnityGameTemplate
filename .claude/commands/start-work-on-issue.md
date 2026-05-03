Start execution on a single GitHub issue: validate dependency readiness, move the project board item to In Progress, create and push a dedicated work branch, and leave an audit comment on the issue.

$ARGUMENTS

## Required Inputs

- `issue_number` (required): numeric GitHub issue number.
- `owner` (optional): defaults to repository owner.
- `repo` (optional): defaults to current repository.
- `project_number` (optional): defaults to value in `.claude/.delivery.config.json`.
- `allow_blocked` (optional, default `false`): only proceed on blocked issues when explicitly set.

## Preconditions

1. GitHub CLI authenticated and repository access available.
2. Local git worktree is clean enough to branch safely.
3. Issue exists and is open.
4. Issue is not blocked by open dependencies unless `allow_blocked=true`.

## Workflow

1. Validate local git context:
   - `git status --short --branch`

2. Read issue metadata:
   - `gh issue view <issue_number> --json number,title,state,url,labels,assignees,projectItems`

3. Validate dependency readiness:
   - `gh api graphql -f query='query { repository(owner:"<owner>", name:"<repo>") { issue(number:<issue_number>) { number title blockedBy(first:20) { nodes { number title url state } } } } }'`
   - If any blockers are `OPEN` and `allow_blocked=false`, stop and report blockers.

4. Normalize issue assignment and labels:
   - Assign to current user (fallback to explicit login when `@me` is unavailable).

5. Sync project board status to `In Progress`:
   - Identify project item ID: `gh project item-list <project_number> --owner <owner> --limit 250 --format json`
   - Identify field and option IDs: `gh project field-list <project_number> --owner <owner> --format json`
   - Update status field: `gh project item-edit --id <item_id> --project-id <project_id> --field-id <status_field_id> --single-select-option-id <in_progress_option_id>`

6. Create branch name from issue metadata:
   - Preferred format: `feat/<backlog-id>-<slug>`
   - Example: issue title `MVP-003 — Implement elemental affinity matrix` → branch `feat/mvp-003-elemental-affinity-matrix`
   - Fallback when backlog ID is missing: `feat/issue-<issue_number>-<slug>`

7. Create and publish branch:
   - `git checkout -b <branch_name>`
   - `git push -u origin <branch_name>`

8. Leave issue audit comment:
   - "Work has started on branch `<branch_name>`. Planned PR will include: `Closes #<issue_number>`."

## Idempotency Rules

- If branch already exists locally, switch to it instead of recreating.
- If remote branch already exists, set upstream and continue.
- If issue is already assigned to the current user, keep it unchanged.
- If project item is already `In Progress`, do not re-edit unless needed.

## Safety Rules

- Do not close the issue.
- Do not mark project status `Done` during start-work.
- Do not force-push.
- Do not proceed on blocked work unless explicitly instructed.
- If required project field IDs cannot be resolved, report and stop before mutating project data.

## Output Format

Return a compact execution report with:

1. **Issue Update** — issue link, final labels/assignee, blocked/unblocked decision.
2. **Project Update** — project name/number and resulting status field value.
3. **Git Update** — branch name, tracking status, and any fallback behavior used.
4. **Audit Trail** — link to the posted issue comment.
