# Workflow

## Branching Strategy

```
main                       — always shippable
├── feat/<short-description>   — new features
├── fix/<short-description>    — bug fixes
└── art/<short-description>    — art / asset drops
```

## Commit Messages

Use conventional commits:

```
feat: add double-jump mechanic
fix: player stuck on wall geometry
art: update player idle animation
audio: add footstep sounds for stone surface
docs: add scene-structure guide
refactor: extract health logic to HealthComponent
```

## Pull Requests

1. Branch off `main`.
2. Keep PRs focused — one feature or fix per PR.
3. All PRs require: no compile errors, Play Mode passes, one reviewer approval.
4. Merge strategy: **Squash and merge** into `main`.

## GitHub Projects & Issue Management

This project is managed through GitHub Projects.

- Create issues using `.github/ISSUE_TEMPLATE/work-item.md`.
- Every issue must include description, acceptance criteria, and relevant labels/tags.
- Keep project fields (status, priority, iteration/milestone) updated throughout the issue lifecycle.
- Use the issue checklist to track code review, testing, and documentation updates.

Link pull requests back to issues using `Closes #<issue-number>` so project tracking updates automatically.

## Scene Merging

Unity scenes are binary-like YAML and conflict badly. Rules:
- Only one person edits a scene at a time.
- Communicate in the team channel before opening a shared scene.
- Prefer prefab-based workflows to reduce direct scene edits.
