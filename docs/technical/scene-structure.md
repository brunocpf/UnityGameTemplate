# Scene Structure

## Hierarchy Convention

```
Scene Root
├── _Systems          (managers, services — DontDestroyOnLoad candidates)
├── _Cameras          (main camera, cinemachine brains)
├── Lighting          (directional light, probes, reflection probes)
├── Environment       (static geometry, terrain)
│   ├── Terrain
│   └── Props
├── Gameplay          (dynamic objects)
│   ├── Player
│   ├── Enemies
│   └── Interactables
└── UI                (UI Toolkit roots, UIDocument hosts, panel stack entry points)
```

The template ships with a single `SampleScene` demonstrating this layout. Replace or duplicate it as your project grows.

UI note:

- Runtime UI is implemented with Unity UI Toolkit custom controls.
- Do not introduce new legacy uGUI canvas-based runtime screens.

## Prefab Guidelines

- All reusable GameObjects must be prefabs stored under `Assets/Prefabs/<Feature>/`.
- Never nest prefab variants more than 2 levels deep.
- Name prefabs with a category prefix: `ENV_`, `CHAR_`, `UI_`, `FX_`, `SFX_`.

## Layer List

| Layer | Index | Use |
|-------|-------|-----|
| Default | 0 | |
| Player | 6 | |
| Enemy | 7 | |
| Terrain | 8 | |
| Interactable | 9 | |

_Update this table in sync with `ProjectSettings/TagManager.asset`._

## Tag List

| Tag | Use |
|-----|-----|
| Player | Player character root |
| Enemy | All enemy roots |
