using TAS;
using TAS.Core.Input.Commands;
using TheMessenger.TAS.Components;
using UnityEngine;

namespace TheMessenger.TAS.Commands;

public class TimeCommand : PluginComponent {
    private const string commandName = "Time";

    [TasCommand(commandName, AliasNames = new[] {$"{commandName}:", $"{commandName}："}, CalcChecksum = false)]
    private static void UpdateTime() {
        string time = GameInfoHelper.FormatTime(GameInfoHelper.RoomTime + Time.deltaTime);
        MetadataCommand.UpdateAll(commandName, _ => time, command => command.Frame == Manager.Controller.CurrentFrameInTas);
    }
}