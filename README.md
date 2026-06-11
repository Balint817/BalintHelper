# BalintHelper

A small Everest helper for Celeste that adds a **Dash Cooldown Reset Trigger** to Lönn.

---

## What it does

Vanilla Celeste has a short internal `dashCooldownTimer` (0.2 s) that starts after every dash.
Until it expires, the player cannot dash again even if they still have a dash available.
This trigger zeroes that timer on demand, letting the player re-dash immediately.

---

## Trigger: `BalintHelper/DashCooldownResetTrigger`

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `resetOnEnter` | bool | `true` | Reset dash CD the moment the player enters the trigger area |
| `resetOnStay` | bool | `false` | Reset dash CD every frame the player is inside the area |
| `resetOnLeave` | bool | `false` | Reset dash CD the moment the player exits the area |
| `oneUse` | bool | `false` | If `true`, the trigger fires **at most once** per room load (non-persistent) |

More than one of `resetOnEnter / resetOnStay / resetOnLeave` can be active simultaneously.

### Notes
- The reset is a **no-op** when the player's dash CD is already at 0, so `oneUse` won't be "wasted" on a free dash.
- `oneUse` is **non-persistent**: it resets when the player respawns or transitions back into the room.
- The trigger still resets on room revisit / respawn regardless of `oneUse`, because `hasActivated` is a plain instance field.

---

## Project structure

```
BalintHelper/
├── BalintHelper.csproj          ← C# project file (set CelesteDir property)
├── everest.yaml                 ← Everest mod manifest
├── README.md
├── Source/
│   ├── BalintHelperModule.cs    ← EverestModule entry point
│   └── Triggers/
│       └── DashCooldownResetTrigger.cs   ← The trigger implementation
└── Loenn/
    └── triggers/
        └── dashCooldownResetTrigger.lua  ← Lönn plugin
```

---

## Building

1. Open `BalintHelper.csproj` in your IDE.
2. Set the `CelesteDir` MSBuild property to your Celeste install folder, e.g. via a `Directory.Build.props`:

   ```xml
   <Project>
     <PropertyGroup>
       <CelesteDir>C:\Program Files (x86)\Steam\steamapps\common\Celeste</CelesteDir>
     </PropertyGroup>
   </Project>
   ```

3. Build → a `BalintHelper.dll` is produced.
4. Create the zip structure:

   ```
   BalintHelper.zip
   ├── everest.yaml
   ├── BalintHelper.dll
   └── Loenn/
       └── triggers/
           └── dashCooldownResetTrigger.lua
   ```

5. Drop the zip into `<Celeste>/Mods/` and launch with Everest.

---

## How `dashCooldownTimer` is accessed

The field is `private float dashCooldownTimer` in `Celeste.Player`.
Because it is private, the trigger uses **`System.Reflection`** to read and write it at runtime:

```csharp
private static readonly FieldInfo DashCooldownTimerField =
    typeof(Player).GetField(
        "dashCooldownTimer",
        BindingFlags.Instance | BindingFlags.NonPublic
    );
```

The `FieldInfo` is cached statically so the lookup only happens once.
