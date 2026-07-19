# BalintHelper

BalintHelper is a custom helper mod for Celeste, primarily made for **Just Another Theo Map** (by me), plus some other stuff.

## Installation

1. Download the latest release package.
2. Put the zip into your Celeste `Mods` folder (Olympus/Gamebanana install not yet available).
3. Launch Everest.

## Development

### Prerequisites

- .NET SDK 8.0+
- A local Celeste install with Everest
- an IDE (pick your poison)

### Configure Celeste references

The project needs access to Celeste assemblies.

You can provide this in either of these ways:

- Set `CelesteDir` (or `CelestePrefix`) to your Celeste directory, **or**
- Keep assemblies in the fallback location expected by your local setup.

### Build

```powershell
dotnet build BalintHelper.csproj
```
