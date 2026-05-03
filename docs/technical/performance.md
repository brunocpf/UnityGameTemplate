# Performance

## Budgets

| Platform | Target FPS | Draw Calls | Triangles | Texture Memory |
|----------|-----------|------------|-----------|----------------|
| PC (min) | 60 | TBD | TBD | TBD |
| PC (rec) | 60+ | TBD | TBD | TBD |

## Profiling Baseline

_Record baselines here as the project matures._

| Scene | Date | Avg FPS | Worst Frame | Notes |
|-------|------|---------|-------------|-------|
| | | | | |

## Optimisation Notes

### CPU
- Keep view models, panel transitions, and frequently-invoked code paths allocation-light in steady-state flows.
- Prefer initialization-time setup over repeated runtime tree rebuilding when the UI structure is stable.
- Treat disposal errors and duplicate subscriptions as performance and correctness issues.
- Keep `Update()` lean — push logic to systems/managers where possible.

### GPU / Rendering
- Using URP; minimise overdraw and shader complexity.

### Memory
- Atlas sprites where possible.
- Restrict `Resources` usage to small UI Toolkit UXML/USS assets that are always shipped with the build.
- Use `Addressables` (or `AssetBundles`) for large or remotely delivered assets once scope is defined.

### Audio
- Compress audio appropriately: Vorbis for music, ADPCM for short SFX.
