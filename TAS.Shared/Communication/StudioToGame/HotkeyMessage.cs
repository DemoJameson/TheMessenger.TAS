using System;

namespace TAS.Shared.Communication.StudioToGame;

[Serializable]
public record struct HotkeyMessage(HotkeyID HotkeyID, bool released) : IStudioToGameMessage {
    public readonly HotkeyID HotkeyID = HotkeyID;
    public readonly bool released = released;
}