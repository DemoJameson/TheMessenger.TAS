using TAS;
using TAS.Core.Input.Commands;
using TAS.Core.Utils;

namespace TheMessenger.TAS.Commands;

public class HasClimbedCommand : PluginComponent {
    private const string commandName = "HasClimbed";
    private static bool? climbed;

    [DisableRun]
    private static void DisableRun() {
        climbed = null;
    }

    [TasCommand(commandName, LegalInMainGame = false)]
    public static void HasClimbed(string[] args) {
        if (args.IsEmpty()) {
            ShowToastAndDisableTAS($"{commandName} Command Failed\nRequired true or false");
            return;
        }

        if (bool.TryParse(args[0], out bool value)) {
            climbed = value;
        } else {
            ShowToastAndDisableTAS($"{commandName} Command Failed\nPlease specify true or false");
        }
    }

    private void Update() {
        if (climbed.HasValue && Manager<PlayerManager>.instance is {player: { } player} &&
            player.stateMachine.currentState is PlayerDefaultState state) {
            state.allowWallJumpTolerance = climbed.Value;
            if (!climbed.Value) {
                climbed = null;
            }
        }
    }
}