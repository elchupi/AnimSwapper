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
- [Glamourer](https://github.com/Ottermandias/Glamourer) — required for glamour automation

## Installation

AnimSwapper is distributed via a custom Dalamud plugin repository (it depends on Penumbra + Glamourer, so it isn't eligible for the official Dalamud plugin list).

1. Open Dalamud settings: `/xlsettings` in chat.
2. Go to the **Experimental** tab.
3. Under **Custom Plugin Repositories**, paste this URL and click **+**:

   ```
   https://raw.githubusercontent.com/elchupi/AnimSwapper/main/pluginmaster.json
   ```

4. Make sure the repo is **enabled** (checkbox), then **Save and Close**.
5. Open the plugin installer (`/xlplugins`) and search for **AnimSwapper**.

Make sure Penumbra and Glamourer are installed first.

## Building from source

```
git clone https://github.com/elchupi/AnimSwapper.git
cd AnimSwapper
dotnet build -c Release
```

Output lands in `bin/Release/AnimSwapper/`. Add the path to `bin/Release/AnimSwapper.dll` under Dalamud's **Settings → Experimental → Dev Plugin Locations** to load it directly.

## License

For personal use. Not affiliated with Square Enix.
