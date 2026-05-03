# Asset Pipeline

## Folder Structure (Assets/)

```
Assets/
├── Art/
│   ├── Characters/
│   ├── Environment/
│   ├── UI/
│   └── VFX/
├── Audio/
│   ├── Music/
│   └── SFX/
├── Prefabs/
├── Scenes/
├── Scripts/
│   ├── Core/
│   │   └── <Feature>/
│   ├── UI/
│   │   └── <Feature>/
│   └── Unity/
│       └── <Feature>/
├── ScriptableObjects/
│   └── <Feature>/
└── Settings/
```

## Texture Import Settings

| Type | Max Size | Compression | Generate Mipmaps |
|------|----------|-------------|-----------------|
| Character diffuse | 1024 | BC7 / ASTC | Yes |
| Environment diffuse | 2048 | BC7 / ASTC | Yes |
| UI sprite | 512 | BC7 / ASTC | No |
| Normal map | 1024 | BC5 | Yes |

## Model Import Settings

- Import scale: 1
- Read/Write: Disabled unless required
- Optimise Mesh: Enabled
- Generate Lightmap UVs: Enabled for static geometry

## Audio Import Settings

| Type | Format | Quality | Load Type |
|------|--------|---------|-----------|
| Music | Vorbis | 70% | Streaming |
| Short SFX | ADPCM | — | Decompress on Load |
| Long SFX | Vorbis | 70% | Compressed in Memory |

## Source Control Notes

- Check in source files (`.fbx`, `.psd`, `.wav`) alongside Unity assets.
- Use Git LFS for binary assets larger than 5 MB.
