using System;
using HarmonyLib;
using TAS;
using TAS.Core.Input.Commands;
using TAS.Core.Utils;
using TheMessenger.TAS.Utils;

namespace TheMessenger.TAS.Commands;

[HarmonyPatch]
public class NextKnockBackDirectionCommand {
    // ReSharper disable once UnusedMember.Local
    enum Direction {
        Right,
        Left
    }

    private const string commandName = "NextKnockBackDirection";
    private static Direction? nextKnockBackDirection;

    [DisableRun]
    private static void DisableRun() {
        nextKnockBackDirection = null;
    }

    [HarmonyPatch(typeof(PlayerKnockbackState), nameof(PlayerKnockbackState.InitKnockback))]
    [HarmonyPrefix]
    private static void InitKnockback(PlayerKnockbackState __instance) {
        HitData hitData = __instance.hitData;
        if (nextKnockBackDirection is { } direction && hitData.hitDirection != 0 && !hitData.verticalKnockBack) {
            if (hitData.hitDirection < 0 && direction == Direction.Right) {
                hitData.hitDirection -= (float) Math.Floor(hitData.hitDirection);
            } else if (hitData.hitDirection > 0 && direction == Direction.Left) {
                hitData.hitDirection -= (float) Math.Ceiling(hitData.hitDirection);
            }

            nextKnockBackDirection = null;
        }
    }

    [TasCommand(commandName, LegalInMainGame = true)]
    private static void NextKnockBackDirection(string[] args) {
        if (args.IsEmpty()) {
            ShowToastAndDisableTAS($"{commandName} Command Failed\nRequired specify direction");
            return;
        }

        if (EnumHelpers<Direction>.TryParse(args[0], out Direction direction)) {
            nextKnockBackDirection = direction;
        } else {
            ShowToastAndDisableTAS($"{commandName} Command Failed\nInvalid direction");
        }
    }
}