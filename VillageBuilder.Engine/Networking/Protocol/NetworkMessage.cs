using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VillageBuilder.Engine.Networking.Protocol
{
    /// <summary>
    /// Base network message for all multiplayer communication
    /// </summary>
    public class NetworkMessage
    {
        public MessageType Type { get; set; }
        public int GameId { get; set; }
        public int SenderId { get; set; }
        public int TargetTick { get; set; }
        public long Timestamp { get; set; }
        public string Payload { get; set; } = string.Empty;

        public NetworkMessage()
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public static NetworkMessage CreateCommandMessage(int gameId, int playerId, int targetTick, string commandJson)
        {
            return new NetworkMessage
            {
                Type = MessageType.CommandSubmit,
                GameId = gameId,
                SenderId = playerId,
                TargetTick = targetTick,
                Payload = commandJson
            };
        }

        public static NetworkMessage CreateTickSync(int gameId, int currentTick)
        {
            return new NetworkMessage
            {
                Type = MessageType.TickSync,
                GameId = gameId,
                SenderId = 0, // Server
                TargetTick = currentTick,
                Payload = currentTick.ToString()
            };
        }

        public static NetworkMessage CreateCommandAck(int gameId, Guid commandId, bool success, string message)
        {
            return new NetworkMessage
            {
                Type = success ? MessageType.CommandAck : MessageType.CommandReject,
                GameId = gameId,
                SenderId = 0, // Server
                Payload = System.Text.Json.JsonSerializer.Serialize(new
                {
                    CommandId = commandId,
                    Success = success,
                    Message = message
                })
            };
        }
    }
}
