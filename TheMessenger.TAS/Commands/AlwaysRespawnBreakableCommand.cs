using HarmonyLib;
using TAS;
using TAS.Core.Input.Commands;
using TAS.Core.Utils;

namespace TheMessenger.TAS.Commands; 

[HarmonyPatch]
public class AlwaysRespawnBreakableCommand : PluginComponent {
    private const string commandName = "AlwaysRespawnBreakable";
    private static bool alwaysRepair;

    [DisableRun]
    private static void DisableRun() {
        alwaysRepair = false;
    }

    [TasCommand(commandName, LegalInMainGame = false)]
    public static void AlwaysRespawnBreakable(string[] args) {
        if (args.IsEmpty()) {
            ShowToastAndDisableTAS($"{commandName} Command Failed\nRequired true or false");
            return;
        }

        if (bool.TryParse(args[0], out bool value)) {
            alwaysRepair = value;
        } else {
            ShowToastAndDisableTAS($"{commandName} Command Failed\nPlease specify true or false");
        }
    }

    [HarmonyPatch(typeof(BreakableCollision),nameof(BreakableCollision.OnEnterRoom))]
    public static void Prefix(BreakableCollision __instance) {
        if (alwaysRepair) {
            __instance.repairOnEnterRoom = true;
        }
    }
}