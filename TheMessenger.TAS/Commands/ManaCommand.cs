using TAS.Core.Input.Commands;
using TAS.Core.Utils;
using UnityEngine;

namespace TheMessenger.TAS.Commands;

public class ManaCommand : PluginComponent {
    private const string commandName = "Mana";

    [TasCommand(commandName, LegalInMainGame = false)]
    private static void Mana(string[] args) {
        if (args.IsEmpty()) {
            ShowToastAndDisableTAS($"{commandName} Command Failed\nRequired specify mana");
            return;
        }

        if (int.TryParse(args[0], out int mana)) {
            if (Manager<PlayerManager>.instance is {player: { } player} playerManager) {
                if (Manager<ProgressionManager>.Instance.useWindmillShuriken) {
                    mana = Mathf.Clamp(mana, 0, 1);
                    player.windmillCharge = mana;
                } else {
                    mana = Mathf.Clamp(mana, 0, playerManager.GetMaxShuriken());
                    playerManager.PlayerShurikens = mana;
                }
            }
        } else {
            ShowToastAndDisableTAS($"{commandName} Command Failed\nInvalid mana");
        }
    }
}