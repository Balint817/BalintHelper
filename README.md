# BalintHelper

BalintHelper is a custom helper mod for Celeste, primarily made for **Just Another Theo Map** (by me), plus some other stuff.

## Requirements

- Celeste
- Everest (min `1.5577.0`)

## Installation

1. Download the latest release package.
2. Put the zip into your Celeste `Mods` folder (Olympus/Gamebanana install not yet available).
3. Launch Everest.

## Development

### Prerequisites

- .NET SDK 8.0+
- A local Celeste install with Everest
- an IDE

### Configure Celeste references

The project needs access to Celeste assemblies (`Celeste.dll`, `MMHOOK_Celeste.dll`, `FNA.dll`).

You can provide this in either of these ways:

- Set `CelesteDir` (or `CelestePrefix`) to your Celeste directory, **or**
- Keep assemblies in the fallback location expected by your local setup.

### Build

```powershell
dotnet build BalintHelper.csproj
```

After build, the project automatically:

- copies `BalintHelper.dll`/`BalintHelper.pdb` into `bin/`
- stages mod folder at `BalintHelper/`
  - `BalintHelper/everest.yaml`
  - `BalintHelper/bin/...`
  - files from `Assets/`

## Project Layout

- `Source/` - C# module code (entities, triggers, utilities)
- `Assets/` - game assets and Loenn plugin files
- `everest.yaml` - mod metadata
