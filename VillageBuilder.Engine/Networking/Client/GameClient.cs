using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillageBuilder.Engine.Commands;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Networking.Protocol;
using VillageBuilder.Engine.Networking.Serialization;

namespace VillageBuilder.Engine.Networking.Client
{
    /// <summary>
    /// Client-side game state manager with prediction and reconciliation
    /// </summary>
    public class GameClient
    {
        private readonly GameEngine _localEngine; // Client-side prediction
        private readonly ICommandSerializer _serializer;
        private readonly int _playerId;
        private readonly Queue<NetworkMessage> _incomingMessages;
        private readonly Queue<NetworkMessage> _outgoingMessages;
        private int _serverTick;
        private int _predictedTick;

        public int PlayerId => _playerId;
        public int ServerTick => _serverTick;
        public int PredictedTick => _predictedTick;
        public GameEngine LocalEngine => _localEngine;

        public GameClient(int playerId, int gameId, int seed)
        {
            _playerId = playerId;
            _localEngine = new GameEngine(gameId, seed);
            _serializer = new JsonCommandSerializer();
            _incomingMessages = new Queue<NetworkMessage>();
            _outgoingMessages = new Queue<NetworkMessage>();
            _serverTick = 0;
            _predictedTick = 0;
        }

        /// <summary>
        /// Submit command to server
        /// </summary>
        public void SubmitCommand(ICommand command)
        {
            // Serialize command
            var commandJson = _serializer.Serialize(command);

            // Create network message
            var message = NetworkMessage.CreateCommandMessage(
                _localEngine.GameId,
                _playerId,
                command.TargetTick,
                commandJson
            );

            // Queue for sending to server
            _outgoingMessages.Enqueue(message);

            // Immediately apply to local engine for prediction
            _localEngine.SubmitCommand(command);

            Console.WriteLine($"📤 Command submitted: {command.GetType().Name} for tick {command.TargetTick}");
        }

        /// <summary>
        /// Receive message from server
        /// </summary>
        public void ReceiveMessage(NetworkMessage message)
        {
            _incomingMessages.Enqueue(message);
        }

        /// <summary>
        /// Process incoming server messages
        /// </summary>
        public void ProcessMessages()
        {
            while (_incomingMessages.Count > 0)
            {
                var message = _incomingMessages.Dequeue();

                switch (message.Type)
                {
                    case MessageType.TickSync:
                        _serverTick = message.TargetTick;
                        break;

                    case MessageType.CommandSubmit:
                        // Another player's command - apply to local state
                        var command = _serializer.Deserialize(message.Payload);
                        if (command != null && command.PlayerId != _playerId)
                        {
                            _localEngine.SubmitCommand(command);
                        }
                        break;

                    case MessageType.CommandAck:
                        Console.WriteLine($"✅ Server acknowledged command");
                        break;

                    case MessageType.CommandReject:
                        Console.WriteLine($"❌ Server rejected command: {message.Payload}");
                        // TODO: Implement rollback/reconciliation
                        break;
                }
            }
        }

        /// <summary>
        /// Client-side prediction tick
        /// </summary>
        public void Tick()
        {
            ProcessMessages();
            _localEngine.SimulateTick();
            _predictedTick++;
        }

        public NetworkMessage? GetOutgoingMessage()
        {
            return _outgoingMessages.Count > 0 ? _outgoingMessages.Dequeue() : null;
        }
    }
}
