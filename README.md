# AnimSwapper

FFXIV Dalamud plugin for race/job animation swaps and Glamourer territory automation.

## Features

- **Race Animation Swaps** — swap walk/run movement animations between races via Penumbra IPC. Detects your visual race (Glamourer-aware) so swaps follow your apparent model, not the underlying customize.
- **Opposite Gender** — per-rule toggle to use the opposite gender's animations. Combine with "Any (use my race)" as the target to gender-swap on your own race without changing race.
- **Territory Filter** — restrict a rule to a specific zone, leaving the rest of the world on default animations.
- **Job Animation Swaps** — borrow another job's weapon-hold, movement, and auto-attack animations.
- **Glamourer Territory Automation** — auto-apply a Glamourer design when entering a zone, revert on leave.
- **Duty Glamour** — set a global glamour for all duties, with per-territory overrides.

## Requirements

- [Dalamud](https://github.com/goatcorp/Dalamud) (API level 15+)
- [Penumbra](https://github.com/xivdev/Penumbra) — required for animation swaps
- [Glamourer](https://github.com/Ottermandias/Glamourer) — required for glamour automation features

## Installation

This repository ships source only; there is no prebuilt DLL.

**Once approved on the official Dalamud plugin repo:** install through Dalamud's plugin installer.

**Until then (build from source):**

```
git clone https://github.com/elchupi/AnimSwapper.git
cd AnimSwapper
dotnet build -c Release
```

Then in Dalamud: **Settings → Experimental → Dev Plugin Locations** → add the path to `bin/Release/AnimSwapper.dll`, and enable it under **Dev Tools → Installed Plugins**.

## License

For personal use. Not affiliated with Square Enix.
