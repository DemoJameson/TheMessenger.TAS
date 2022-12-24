using System;

namespace TAS.Shared.Communication.GameToStudio; 

[Serializable]
public record struct ReturnGameDataMessage(string Data) : IGameToStudioMessage {
    public readonly string Data = Data;
}