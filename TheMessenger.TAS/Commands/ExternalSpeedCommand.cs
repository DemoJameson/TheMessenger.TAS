using TAS.Core.Input.Commands;
using TAS.Core.Utils;
using TheMessenger.TAS.Utils;
using UnityEngine;

namespace TheMessenger.TAS.Commands;

public class ExternalSpeedCommand : PluginComponent {
    public const string CommandName = "ExternalSpeed";

    enum SpeedType {
        EnemyReceiveHit, GraplouKnockback
    }

    [TasCommand(CommandName, LegalInMainGame = false)]
    private static void ExternalSpeed(string[] args) {
        if (args.IsEmpty()) {
            ShowToastAndDisableTAS($"{CommandName} Command Failed\nRequired specify type and speed");
            return;
        }

        if (!EnumHelpers<SpeedType>.TryParse(args[0], out SpeedType speedType)) {
            ShowToastAndDisableTAS($"{CommandName} Command Failed\nInvalid type");
            return;
        }

        if (!float.TryParse(args[1], out float x)) {
            ShowToastAndDisableTAS($"{CommandName} Command Failed\nInvalid speed");
            return;
        }

        if (GetPlayer() is { } player) {
            float damping = speedType == SpeedType.EnemyReceiveHit ? 0.93f : 0.98f;
            player.SetExternalVelocity(new ExternalVelocity(speedType.ToString(), damping, new Vector2(x, 0)));
        }
    }

    public static string GenerateCommand() {
        if (GetPlayer() is {} player && player.externalVelocities.TryGetValue(ExternalVelocityConstant.ENEMY_RECEIVE_HIT, out var externalVelocity) && externalVelocity.velocity.x != 0) {
            return $"{CommandName} {externalVelocity.key} {externalVelocity.velocity.x}";
        }

        return null;
    }
}