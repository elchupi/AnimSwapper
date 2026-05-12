using System;
using System.Collections.Generic;
using Dalamud.Configuration;

namespace AnimSwapper;

public class AnimationSwapRule
{
    public bool Enabled = true;
    public byte SourceRace;        // 0 = any/current
    public byte TargetRace;        // Race whose animations to use
    public bool SwapRun  = true;
    public bool SwapWalk = true;
    public bool SwapIdle = false;
    public bool UseOppositeGender = false; // use opposite gender's animation set
    public ushort TerritoryId;     // 0 = any territory
    public string TerritoryName = "";
}

public class GlamourerTerritoryOverride
{
    public ushort TerritoryId;
    public string TerritoryName = "";
    public string DesignName = "";
}

public class JobAnimSwapRule
{
    public bool Enabled = true;
    public uint SourceJob;         // ClassJob RowId (0 = any/current)
    public uint HoldTargetJob;
    public uint MoveTargetJob;
    public uint AttackTargetJob;
    public ushort TerritoryId;
    public string TerritoryName = "";
}

public class Configuration : PluginConfiguration, IPluginConfiguration
{
    public int Version { get; set; } = 0;

    // ---- Race animation swaps ----
    public bool EnableAnimationSwaps = false;
    public List<AnimationSwapRule> AnimationSwapRules = new();

    // ---- Job animation swaps ----
    public bool EnableJobAnimationSwaps = false;
    public List<JobAnimSwapRule> JobAnimSwapRules = new();

    // ---- Glamourer territory automation ----
    public bool EnableGlamourerTerritoryAuto = false;
    public List<GlamourerTerritoryOverride> GlamourerTerritoryOverrides = new();

    // Debounced save support (simplified — immediate saves).
    private DateTime _lastSaveRequest;
    private bool _pendingSave;
    private const float SaveDebounceSeconds = 2f;

    public void SaveDebounced()
    {
        _lastSaveRequest = DateTime.UtcNow;
        _pendingSave = true;
    }

    public void TickSaveDebounce()
    {
        if (!_pendingSave) return;
        if ((DateTime.UtcNow - _lastSaveRequest).TotalSeconds >= SaveDebounceSeconds)
        {
            _pendingSave = false;
            Save();
        }
    }

    public void FlushSaveDebounce()
    {
        if (_pendingSave)
        {
            _pendingSave = false;
            Save();
        }
    }
}
