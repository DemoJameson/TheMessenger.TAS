using TAS.Core.Input.Commands;
using TAS.Core.Utils;

namespace TheMessenger.TAS.Commands;

public class HpCommand : PluginComponent {
    [TasCommand("HP", LegalInMainGame = false)]
    private static void Hp(string[] args) {
        if (args.IsEmpty()) {
            ShowToastAndDisableTAS("HP Command Failed\nRequired specify HP");
            return;
        }

        if (int.TryParse(args[0], out int hp)) {
            if (Manager<PlayerManager>.instance is {player: { } player}) {
                if (hp <= 0) {
                    hp = 1;
                }

                player.SetHP(hp);
            }
        } else {
            ShowToastAndDisableTAS("HP Command Failed\nInvalid HP");
        }
    }
}