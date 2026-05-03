# Asset Naming Conventions

All asset file names use **PascalCase** with a category prefix separated by an underscore.

## Prefix Reference

| Prefix | Asset Type |
|--------|-----------|
| `CHAR_` | Character models / prefabs |
| `ENV_` | Environment props / terrain |
| `UI_` | UI sprites and elements |
| `FX_` | Visual effects / particles |
| `SFX_` | Sound effects |
| `MUS_` | Music tracks |
| `TEX_` | Standalone textures |
| `MAT_` | Materials |
| `ANIM_` | Animation clips |
| `SO_` | ScriptableObject data assets |

## Suffix Reference

| Suffix | Meaning |
|--------|---------|
| `_D` | Diffuse / Albedo texture |
| `_N` | Normal map |
| `_M` | Metallic map |
| `_R` | Roughness map |
| `_AO` | Ambient Occlusion map |
| `_E` | Emission map |

## Examples

```
CHAR_PlayerIdle_ANIM.anim
ENV_StoneWall_MAT.mat
TEX_StoneWall_D.png
TEX_StoneWall_N.png
UI_HealthBar.png
SFX_FootstepStone.wav
MUS_MainTheme.wav
```
