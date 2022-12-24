using TAS;

namespace TheMessenger.TAS.Components.Assists;

public class NameReplacer : PluginComponent {
    private static bool replace = false;

    [EnableRun]
    private static void EnableRun() {
        replace = true;
    }

    [DisableRun]
    private static void DisableRun() {
        replace = false;
    }

    private void Update() {
        if (!replace) {
            return;
        }

        if (Manager<UIManager>.instance is { } uiManager && uiManager.GetView<InGameHud>() is { } inGameHud) {
            replace = false;
            if (inGameHud.hud_8 is { } hud8) {
                hud8.playerName.SetText($"TAS  v{MyPluginInfo.PLUGIN_VERSION}");
            }

            if (inGameHud.hud_16 is { } hud16) {
                hud16.playerName.SetText($"TAS  v{MyPluginInfo.PLUGIN_VERSION}");
            }
        }
    }
}