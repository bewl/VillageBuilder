using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VillageBuilder.Engine.Networking.Protocol
{
    public enum MessageType
    {
        // Connection
        ClientConnect,
        ClientDisconnect,
        ServerWelcome,
        
        // Game State
        GameStateSync,
        TickSync,
        
        // Commands
        CommandSubmit,
        CommandAck,
        CommandReject,
        
        // Events
        GameEvent,
        PlayerJoined,
        PlayerLeft
    }
}
