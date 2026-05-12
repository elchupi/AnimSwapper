using Dalamud.Plugin;

namespace AnimSwapper;

public class AnimSwapper(IDalamudPluginInterface pluginInterface) : DalamudPlugin<Configuration>(pluginInterface), IDalamudPlugin
{
    protected override void Initialize()
    {
        GlamourerBridge.Initialize();
        DalamudApi.PluginInterface.UiBuilder.OpenMainUi += ToggleConfig;
    }

    protected override void ToggleConfig() => PluginUI.IsVisible ^= true;

    [PluginCommand("/animswapper", HelpMessage = "Opens / closes the AnimSwapper config.")]
    private void ToggleConfig(string command, string argument) => ToggleConfig();

    protected override void Update()
    {
        Config.TickSaveDebounce();
        AnimationSwap.Update();
        GlamourerBridge.Update();
    }

    protected override void Draw()
    {
        PluginUI.Draw();
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing) return;
        try { Config.FlushSaveDebounce(); } catch { }
        try { AnimationSwap.Dispose(); } catch { }
        try { GlamourerBridge.Dispose(); } catch { }
    }
}
