using System;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;

namespace AnimSwapper;

public static class PluginUI
{
    public static bool IsVisible;

    public static void Draw()
    {
        if (!IsVisible) return;

        ImGui.SetNextWindowSizeConstraints(new Vector2(450, 400) * ImGuiHelpers.GlobalScale,
            new Vector2(800, 1200) * ImGuiHelpers.GlobalScale);

        if (ImGui.Begin("AnimSwapper", ref IsVisible, ImGuiWindowFlags.None))
            DrawAnimSwapTab();

        ImGui.End();
    }

    private static void ConfigCheckbox(string id, ref bool val)
    {
        if (ImGui.Checkbox(id, ref val))
            AnimSwapper.Config.Save();
    }

    // ── Animation Swaps tab ─────────────────────────────────────
    private static (byte id, string name)[] _raceCache;
    private static (uint id, string name)[] _jobCache;

    private static unsafe void DrawAnimSwapTab()
    {
        ConfigCheckbox("Enable animation swaps", ref AnimSwapper.Config.EnableAnimationSwaps);
        ImGui.TextDisabled(
            "Transfer run/walk/idle animations from one race to another.\n" +
            "Pick your race and the race whose animations you want.");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var rules = AnimSwapper.Config.AnimationSwapRules;

        // Add button.
        if (ImGui.Button("+ Add Rule"))
        {
            rules.Add(new AnimationSwapRule());
            AnimSwapper.Config.Save();
        }

        ImGui.Spacing();

        // Draw each rule.
        int removeIdx = -1;
        for (int i = 0; i < rules.Count; i++)
        {
            var rule = rules[i];
            ImGui.PushID(i);

            string srcName = GetRaceName(rule.SourceRace);
            string tgtName = GetRaceName(rule.TargetRace);
            string terrTag = rule.TerritoryId != 0 ? $"  [{rule.TerritoryName}]" : "";
            string header = $"{srcName}  →  {tgtName}{terrTag}##rule";
            bool open = ImGui.CollapsingHeader(header, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.AllowItemOverlap);

            // Delete button.
            ImGui.SameLine(ImGui.GetContentRegionAvail().X - 20f * ImGuiHelpers.GlobalScale);
            ImGui.PushStyleColor(ImGuiCol.Text, 0xFF4444FFu);
            if (ImGui.SmallButton("X"))
                removeIdx = i;
            ImGui.PopStyleColor();

            if (open)
            {
                ImGui.Indent(10f);

                // Enabled.
                if (ImGui.Checkbox("Enabled", ref rule.Enabled))
                    AnimSwapper.Config.Save();

                // Source race (what you ARE).
                ImGui.SetNextItemWidth(180f * ImGuiHelpers.GlobalScale);
                if (ImGui.BeginCombo("My Race", srcName))
                {
                    EnsureRaceCache();
                    foreach (var (id, name) in _raceCache)
                    {
                        if (ImGui.Selectable(name, rule.SourceRace == id))
                        {
                            rule.SourceRace = id;
                            AnimSwapper.Config.Save();
                        }
                    }
                    ImGui.EndCombo();
                }

                // Target race (whose animations to USE).
                // "Any" is allowed: paired with Opposite Gender it means
                // "use my own race's opposite-gender animations" — saves
                // the user from picking their own race to gender-swap.
                ImGui.SetNextItemWidth(180f * ImGuiHelpers.GlobalScale);
                if (ImGui.BeginCombo("Use Anims From", tgtName))
                {
                    EnsureRaceCache();
                    foreach (var (id, name) in _raceCache)
                    {
                        string label = id == 0 ? "Any (use my race)" : name;
                        if (ImGui.Selectable(label, rule.TargetRace == id))
                        {
                            rule.TargetRace = id;
                            AnimSwapper.Config.Save();
                        }
                    }
                    ImGui.EndCombo();
                }

                ImGui.Spacing();

                if (ImGui.Checkbox("Opposite Gender", ref rule.UseOppositeGender))
                    AnimSwapper.Config.Save();

                ImGui.Spacing();

                // Territory filter.
                string terrDisplay = rule.TerritoryId == 0
                    ? "Any"
                    : $"{rule.TerritoryName} ({rule.TerritoryId})";
                ImGui.TextDisabled($"Territory: {terrDisplay}");
                ImGui.SameLine();
                if (ImGui.SmallButton("Set to Current##raceTerr"))
                {
                    ushort tid = AnimationSwap.GetCurrentTerritory();
                    rule.TerritoryId = tid;
                    rule.TerritoryName = AnimationSwap.LookupTerritoryName(tid);
                    AnimSwapper.Config.Save();
                }
                if (rule.TerritoryId != 0)
                {
                    ImGui.SameLine();
                    if (ImGui.SmallButton("Clear##raceTerr"))
                    {
                        rule.TerritoryId = 0;
                        rule.TerritoryName = "";
                        AnimSwapper.Config.Save();
                    }
                }

                ImGui.Unindent(10f);
            }

            ImGui.PopID();
            ImGui.Spacing();
        }

        if (removeIdx >= 0)
        {
            rules.RemoveAt(removeIdx);
            AnimSwapper.Config.Save();
        }

        // ── Job animation swaps ──
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ConfigCheckbox("Enable job animation swaps", ref AnimSwapper.Config.EnableJobAnimationSwaps);
        ImGui.TextDisabled("Swap weapon hold, movement, and auto-attack animations between jobs.");

        ImGui.Spacing();

        var jobRules = AnimSwapper.Config.JobAnimSwapRules;

        if (ImGui.Button("+ Add Job Rule"))
        {
            jobRules.Add(new JobAnimSwapRule());
            AnimSwapper.Config.Save();
        }

        ImGui.Spacing();

        int removeJobIdx = -1;
        for (int i = 0; i < jobRules.Count; i++)
        {
            var jr = jobRules[i];
            ImGui.PushID(1000 + i);

            string srcJobName = GetJobName(jr.SourceJob);
            string holdJobName = jr.HoldTargetJob == 0 ? "None" : GetJobName(jr.HoldTargetJob);
            string moveJobName = jr.MoveTargetJob == 0 ? "None" : GetJobName(jr.MoveTargetJob);
            string atkJobName = jr.AttackTargetJob == 0 ? "None" : GetJobName(jr.AttackTargetJob);
            string jobTerrTag = jr.TerritoryId != 0 ? $"  [{jr.TerritoryName}]" : "";
            string jobHeader = $"{srcJobName}  →  Hold: {holdJobName} / Move: {moveJobName} / Atk: {atkJobName}{jobTerrTag}##jobrule";
            bool jobOpen = ImGui.CollapsingHeader(jobHeader,
                ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.AllowItemOverlap);

            // Delete button.
            ImGui.SameLine(ImGui.GetContentRegionAvail().X - 20f * ImGuiHelpers.GlobalScale);
            ImGui.PushStyleColor(ImGuiCol.Text, 0xFF4444FFu);
            if (ImGui.SmallButton("X"))
                removeJobIdx = i;
            ImGui.PopStyleColor();

            if (jobOpen)
            {
                ImGui.Indent(10f);

                if (ImGui.Checkbox("Enabled", ref jr.Enabled))
                    AnimSwapper.Config.Save();

                // Source job (what you ARE).
                ImGui.SetNextItemWidth(180f * ImGuiHelpers.GlobalScale);
                if (ImGui.BeginCombo("My Job", srcJobName))
                {
                    EnsureJobCache();
                    // "Any" option.
                    if (ImGui.Selectable("Any", jr.SourceJob == 0))
                    {
                        jr.SourceJob = 0;
                        AnimSwapper.Config.Save();
                    }
                    foreach (var (id, name) in _jobCache)
                    {
                        if (ImGui.Selectable(name, jr.SourceJob == id))
                        {
                            jr.SourceJob = id;
                            AnimSwapper.Config.Save();
                        }
                    }
                    ImGui.EndCombo();
                }

                // Weapon hold target job.
                ImGui.SetNextItemWidth(180f * ImGuiHelpers.GlobalScale);
                if (ImGui.BeginCombo("Weapon Hold From", holdJobName))
                {
                    EnsureJobCache();
                    if (ImGui.Selectable("None", jr.HoldTargetJob == 0))
                    {
                        jr.HoldTargetJob = 0;
                        AnimSwapper.Config.Save();
                    }
                    foreach (var (id, name) in _jobCache)
                    {
                        if (ImGui.Selectable(name, jr.HoldTargetJob == id))
                        {
                            jr.HoldTargetJob = id;
                            AnimSwapper.Config.Save();
                        }
                    }
                    ImGui.EndCombo();
                }

                // Movement target job.
                ImGui.SetNextItemWidth(180f * ImGuiHelpers.GlobalScale);
                if (ImGui.BeginCombo("Movement From", moveJobName))
                {
                    EnsureJobCache();
                    if (ImGui.Selectable("None", jr.MoveTargetJob == 0))
                    {
                        jr.MoveTargetJob = 0;
                        AnimSwapper.Config.Save();
                    }
                    foreach (var (id, name) in _jobCache)
                    {
                        if (ImGui.Selectable(name, jr.MoveTargetJob == id))
                        {
                            jr.MoveTargetJob = id;
                            AnimSwapper.Config.Save();
                        }
                    }
                    ImGui.EndCombo();
                }

                // Auto-attack target job.
                ImGui.SetNextItemWidth(180f * ImGuiHelpers.GlobalScale);
                if (ImGui.BeginCombo("Auto-Attack From", atkJobName))
                {
                    EnsureJobCache();
                    if (ImGui.Selectable("None", jr.AttackTargetJob == 0))
                    {
                        jr.AttackTargetJob = 0;
                        AnimSwapper.Config.Save();
                    }
                    foreach (var (id, name) in _jobCache)
                    {
                        if (ImGui.Selectable(name, jr.AttackTargetJob == id))
                        {
                            jr.AttackTargetJob = id;
                            AnimSwapper.Config.Save();
                        }
                    }
                    ImGui.EndCombo();
                }

                ImGui.Spacing();

                // Territory filter.
                string jobTerrDisplay = jr.TerritoryId == 0
                    ? "Any"
                    : $"{jr.TerritoryName} ({jr.TerritoryId})";
                ImGui.TextDisabled($"Territory: {jobTerrDisplay}");
                ImGui.SameLine();
                if (ImGui.SmallButton("Set to Current##jobTerr"))
                {
                    ushort tid = AnimationSwap.GetCurrentTerritory();
                    jr.TerritoryId = tid;
                    jr.TerritoryName = AnimationSwap.LookupTerritoryName(tid);
                    AnimSwapper.Config.Save();
                }
                if (jr.TerritoryId != 0)
                {
                    ImGui.SameLine();
                    if (ImGui.SmallButton("Clear##jobTerr"))
                    {
                        jr.TerritoryId = 0;
                        jr.TerritoryName = "";
                        AnimSwapper.Config.Save();
                    }
                }

                ImGui.Unindent(10f);
            }

            ImGui.PopID();
            ImGui.Spacing();
        }

        if (removeJobIdx >= 0)
        {
            jobRules.RemoveAt(removeJobIdx);
            AnimSwapper.Config.Save();
        }

        // ── Glamourer territory automation ──
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ConfigCheckbox("Enable Glamourer territory automation", ref AnimSwapper.Config.EnableGlamourerTerritoryAuto);
        ImGui.TextDisabled("Apply a Glamourer design when entering a territory.\nReverts to your base Glamourer automation elsewhere.");

        ImGui.Spacing();

        var glamOverrides = AnimSwapper.Config.GlamourerTerritoryOverrides;

        if (ImGui.Button("+ Add Territory Override"))
        {
            glamOverrides.Add(new GlamourerTerritoryOverride());
            AnimSwapper.Config.Save();
        }
        ImGui.SameLine();
        if (ImGui.SmallButton("Refresh Designs"))
            GlamourerBridge.RefreshDesignCache();
        ImGui.SameLine();
        if (ImGui.SmallButton("Revert Now"))
            GlamourerBridge.ManualRevert();

        string activeLabel = GlamourerBridge.ActiveDesignName != null
            ? $"Active: {GlamourerBridge.ActiveDesignName}"
            : "Active: (base automation)";
        ImGui.TextDisabled($"{activeLabel}  |  {GlamourerBridge.Status}");

        ImGui.Spacing();

        // Fetch design list for dropdown.
        var glamDesigns = GlamourerBridge.GetDesignList();
        string[] designNames = null;
        if (glamDesigns != null && glamDesigns.Count > 0)
        {
            designNames = glamDesigns.Values.OrderBy(n => n).ToArray();
        }

        int removeGlamIdx = -1;
        for (int i = 0; i < glamOverrides.Count; i++)
        {
            var ov = glamOverrides[i];
            ImGui.PushID(2000 + i);

            string terrLabel = ov.TerritoryId != 0
                ? $"{ov.TerritoryName} ({ov.TerritoryId})"
                : "(no territory set)";
            string designLabel = !string.IsNullOrEmpty(ov.DesignName) ? ov.DesignName : "(no design)";
            string glamHeader = $"{terrLabel}  →  {designLabel}##glamoverride";
            bool glamOpen = ImGui.CollapsingHeader(glamHeader,
                ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.AllowItemOverlap);

            // Delete button.
            ImGui.SameLine(ImGui.GetContentRegionAvail().X - 20f * ImGuiHelpers.GlobalScale);
            ImGui.PushStyleColor(ImGuiCol.Text, 0xFF4444FFu);
            if (ImGui.SmallButton("X"))
                removeGlamIdx = i;
            ImGui.PopStyleColor();

            if (glamOpen)
            {
                ImGui.Indent(10f);

                // Territory.
                string terrDisplay = ov.TerritoryId != 0
                    ? $"{ov.TerritoryName} ({ov.TerritoryId})"
                    : "None";
                ImGui.TextDisabled($"Territory: {terrDisplay}");
                ImGui.SameLine();
                if (ImGui.SmallButton("Set to Current##glamTerr"))
                {
                    ushort tid = AnimationSwap.GetCurrentTerritory();
                    ov.TerritoryId = tid;
                    ov.TerritoryName = AnimationSwap.LookupTerritoryName(tid);
                    AnimSwapper.Config.Save();
                    GlamourerBridge.ForceReevaluate();
                }

                // Design dropdown.
                ImGui.SetNextItemWidth(250f * ImGuiHelpers.GlobalScale);
                if (designNames != null && designNames.Length > 0)
                {
                    if (ImGui.BeginCombo("Design", string.IsNullOrEmpty(ov.DesignName) ? "(select)" : ov.DesignName))
                    {
                        foreach (var dn in designNames)
                        {
                            if (ImGui.Selectable(dn, ov.DesignName == dn))
                            {
                                ov.DesignName = dn;
                                AnimSwapper.Config.Save();
                                GlamourerBridge.ForceReevaluate();
                            }
                        }
                        ImGui.EndCombo();
                    }
                }
                else
                {
                    // Fallback: text input if Glamourer IPC unavailable.
                    string designInput = ov.DesignName ?? "";
                    ImGui.SetNextItemWidth(250f * ImGuiHelpers.GlobalScale);
                    if (ImGui.InputText("Design##glamText", ref designInput, 256))
                    {
                        ov.DesignName = designInput;
                        AnimSwapper.Config.Save();
                        GlamourerBridge.ForceReevaluate();
                    }
                }

                // Test button.
                if (!string.IsNullOrEmpty(ov.DesignName))
                {
                    if (ImGui.SmallButton("Test Apply##glamTest"))
                        GlamourerBridge.TestApply(ov.DesignName);
                }

                ImGui.Unindent(10f);
            }

            ImGui.PopID();
            ImGui.Spacing();
        }

        if (removeGlamIdx >= 0)
        {
            glamOverrides.RemoveAt(removeGlamIdx);
            AnimSwapper.Config.Save();
            GlamourerBridge.ForceReevaluate();
        }

        // ── Diagnostic section ──
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Live status.
        try
        {
            var lp = DalamudApi.ObjectTable.LocalPlayer;
            if (lp != null)
            {
                var ch = (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)lp.Address;
                if (ch != null)
                {
                    ushort slot0 = ch->Timeline.TimelineSequencer.GetSlotTimeline(0);
                    string key = AnimationSwap.LookupTimelineKey(slot0);
                    byte race = ch->DrawData.CustomizeData.Race;
                    byte sex = ch->DrawData.CustomizeData.Sex;
                    ImGui.TextDisabled(
                        $"Slot 0 = {slot0} \"{key}\"  |  " +
                        $"Race = {AnimationSwap.LookupRaceName(race)} ({(sex == 0 ? "M" : "F")})");
                }
            }
        }
        catch { ImGui.TextDisabled("(unavailable)"); }

        if (AnimationSwap.VisualRaceId != 0)
            ImGui.TextDisabled(
                $"Visual: {AnimationSwap.LookupRaceName(AnimationSwap.VisualRaceId)} " +
                $"({AnimationSwap.VisualModelCode})");

        ImGui.TextDisabled($"Vtable calls: {AnimationSwap.TotalHookCalls}  |  " +
                           $"Penumbra swaps: {AnimationSwap.TotalSwaps}");
        ImGui.TextDisabled($"Status: {AnimationSwap.ResourceHookStatus}");

        // Buttons.
        if (ImGui.SmallButton("Force Redraw"))
            AnimationSwap.ForceRedraw();
        ImGui.SameLine();
        if (ImGui.SmallButton("Dump Diagnostic"))
            AnimationSwap.FlushDiag();
        ImGui.SameLine();
        ImGui.TextDisabled("animswap_diag.txt");
    }

    private static void EnsureRaceCache()
    {
        if (_raceCache != null) return;
        try
        {
            var sheet = DalamudApi.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Race>();
            if (sheet == null) { _raceCache = new[] { ((byte)0, "Any") }; return; }

            var list = new System.Collections.Generic.List<(byte id, string name)>();
            list.Add((0, "Any"));
            foreach (var row in sheet)
            {
                string name = row.Masculine.ExtractText();
                if (string.IsNullOrEmpty(name)) continue;
                list.Add(((byte)row.RowId, name));
            }
            _raceCache = list.ToArray();
        }
        catch { _raceCache = new[] { ((byte)0, "Any") }; }
    }

    private static string GetRaceName(byte raceId)
    {
        if (raceId == 0) return "Any";
        EnsureRaceCache();
        foreach (var (id, name) in _raceCache)
            if (id == raceId) return name;
        return $"Race #{raceId}";
    }

    private static void EnsureJobCache()
    {
        if (_jobCache != null) return;
        try
        {
            var sheet = DalamudApi.DataManager.GetExcelSheet<Lumina.Excel.Sheets.ClassJob>();
            if (sheet == null) { _jobCache = Array.Empty<(uint, string)>(); return; }

            var list = new System.Collections.Generic.List<(uint id, string name)>();
            foreach (var row in sheet)
            {
                if (!AnimationSwap.JobWeaponFolder.ContainsKey(row.RowId)) continue;
                string name = row.Abbreviation.ExtractText();
                string full = row.Name.ExtractText();
                if (string.IsNullOrEmpty(name)) continue;
                string display = !string.IsNullOrEmpty(full)
                    ? $"{name} ({full})"
                    : name;
                list.Add((row.RowId, display));
            }
            _jobCache = list.ToArray();
        }
        catch { _jobCache = Array.Empty<(uint, string)>(); }
    }

    private static string GetJobName(uint jobId)
    {
        if (jobId == 0) return "Any";
        EnsureJobCache();
        foreach (var (id, name) in _jobCache)
            if (id == jobId) return name;
        return $"Job #{jobId}";
    }
}
