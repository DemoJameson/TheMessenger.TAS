using System;

namespace TAS.Shared.Communication.StudioToGame; 

[Serializable]
public record struct GetGameDataMessage(string DataType) : IStudioToGameMessage {
    public readonly string DataType = DataType;
}