using VillageBuilder.Engine.Commands;

namespace VillageBuilder.Engine.Networking.Serialization
{
    /// <summary>
    /// Interface for command serialization (JSON, Binary, etc.)
    /// </summary>
    public interface ICommandSerializer
    {
        string Serialize(ICommand command);
        ICommand? Deserialize(string data);
        byte[] SerializeToBytes(ICommand command);
        ICommand? DeserializeFromBytes(byte[] data);
    }
}