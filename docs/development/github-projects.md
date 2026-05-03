# GitHub Projects Workflow

This project is tracked and managed through GitHub Projects.

## Issue Template

Use the standard issue template when creating new work items:

- `.github/ISSUE_TEMPLATE/work-item.md`

The template includes required sections for:

- Description
- Acceptance Criteria
- Labels / Tags
- GitHub Project tracking fields
- Common completion checklist

## Required Issue Sections

Every issue should include:

1. **Description**
2. **Acceptance Criteria**
3. **Labels / Tags**
4. **GitHub Project Tracking**
5. **Common Completion Checklist**

## Labeling Guidance

Apply labels across these dimensions:

- **Type:** feature, bug, tech-debt, docs, chore
- **Area:** (configure per project — e.g. ui, gameplay, audio, build)
- **Priority:** p0, p1, p2

## Phase Prefixes

Issue titles can be prefixed with the active phase tag (for example `MVP-`, `P1-`, `P2-`) to make milestone grouping easier on the project board.

(Configure phase prefixes, board columns, and required labels in `.claude/.delivery.config.json`.)

## Common Completion Checklist

Each issue should track completion of:

- Code implementation
- Code review
- Testing
- Documentation updates

The canonical checklist lives in the issue template itself to ensure consistency.

## Recommended Flow

1. Create issue from template.
2. Add labels and project fields.
3. Move status to `in-progress` when work starts.
4. Link PR to issue (`Closes #<issue-number>`).
5. Before closing, confirm acceptance criteria and checklist items are complete.
6. Move status to `done` when merged.
