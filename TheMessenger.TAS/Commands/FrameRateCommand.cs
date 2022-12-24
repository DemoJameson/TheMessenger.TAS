using TAS;
using TAS.Core.Input.Commands;
using TAS.Core.Utils;
using TheMessenger.TAS.Components;
using UnityEngine;

namespace TheMessenger.TAS.Commands;

public class FrameRateCommand : PluginComponent {
    [TasCommand("FrameRate", LegalInMainGame = true)]
    private static void SetFrameRate(string[] args) {
        if (args.IsEmpty()) {
            return;
        }

        if (int.TryParse(args[0], out int frameRate)) {
            GameComponent.FixedFrameRate = Mathf.Clamp(frameRate, 1, int.MaxValue);
        }
    }

    [DisableRun]
    private static void Disable() {
        GameComponent.FixedFrameRate = GameComponent.DefaultFixedFrameRate;
    }
}