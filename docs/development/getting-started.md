# Getting Started

## Prerequisites

| Tool | Version | Notes |
|------|---------|-------|
| Unity | 6.x (see `ProjectSettings/ProjectVersion.txt`) | Install via Unity Hub |
| Git | 2.x+ | |
| Git LFS | 3.x+ | Required for binary assets |
| Rider / VS / VS Code | Latest | Rider recommended |

## Setup

```bash
# 1. Clone the repository
git clone <repo-url>
cd __GAME_DISPLAY_NAME__

# 2. Pull LFS objects
git lfs pull

# 3. Open the project in Unity Hub
#    File → Open → select the project root folder
```

## First Run Checklist

- [ ] Unity imports without errors in the Console
- [ ] `SampleScene` opens and enters Play Mode
- [ ] No compilation errors in the Console

## Project Settings Reference

- Unity version: see `ProjectSettings/ProjectVersion.txt`
- Package list: `Packages/manifest.json`

## Recommended Editor Extensions

- **Rider** — ReSharper analysis + Unity integration
- **VS Code** — C# Dev Kit + Unity extension

## Troubleshooting

**Missing packages on first open:**
Open `Window > Package Manager` and let it resolve.

**Script compilation errors after pull:**
Close Unity, delete `Library/`, reopen — Unity will reimport.
