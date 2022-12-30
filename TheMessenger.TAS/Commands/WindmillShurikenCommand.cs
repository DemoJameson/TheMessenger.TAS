using TAS.Core.Input.Commands;
using TAS.Core.Utils;

namespace TheMessenger.TAS.Commands;

public class WindmillShurikenCommand {
    private const string commandName = "WindmillShuriken";

    [TasCommand(commandName, LegalInMainGame = false)]
    public static void WindmillShuriken(string[] args) { 
        if (args.IsEmpty()) {
            ShowToastAndDisableTAS($"{commandName} Command Failed\nRequired true or false");
            return;
        }

        if (bool.TryParse(args[0], out bool value)) {
            if (Manager<ProgressionManager>.instance is { } progressionManager) {
                progressionManager.useWindmillShuriken = value;
                if (Manager<UIManager>.instance is { } uiManager) {
                    uiManager.GetView<InGameHud>()?.UpdateShurikenVisibility();
                }
            }
        } else {
            ShowToastAndDisableTAS($"{commandName} Command Failed\nPlease specify true or false");
        }
    }
}