using TAS;
using TAS.Core.Input.Commands;
using TheMessenger.TAS.Components;
using UnityEngine;

namespace TheMessenger.TAS.Commands;

public class LevelTimeCommand : PluginComponent {
    private const string commandName = "LevelTime";
    [TasCommand(commandName, AliasNames = new[] {$"{commandName}:", $"{commandName}："}, CalcChecksum = false)]
    private static void UpdateTime() {
        string time = GameInfoHelper.FormatTime(GameInfoHelper.LevelTime + Time.deltaTime);
        MetadataCommand.UpdateAll(commandName, _ => time, command => command.Frame == Manager.Controller.CurrentFrameInTas);
    }
}