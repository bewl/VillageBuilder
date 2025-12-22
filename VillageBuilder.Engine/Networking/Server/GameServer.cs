using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillageBuilder.Engine.Commands;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Networking.Protocol;
using VillageBuilder.Engine.Networking.Serialization;

namespace VillageBuilder.Engine.Networking.Server
{
    /// <summary>
    /// Authoritative game server that validates and processes commands
    /// </summary>
    public class GameServer
    {
        private readonly GameEngine _engine;
        private readonly ICommandSerializer _serializer;
        private readonly Dictionary<int, PlayerConnection> _players;
        private readonly object _lock = new();
        private bool _isRunning;
        private int _tickRate; // Ticks per second

        public int CurrentTick => _engine.CurrentTick;
        public int PlayerCount => _players.Count;
        public bool IsRunning => _isRunning;

        public GameServer(GameEngine engine, int tickRate = 20)
        {
            _engine = engine;
            _serializer = new JsonCommandSerializer();
            _players = new Dictionary<int, PlayerConnection>();
            _tickRate = tickRate;
        }

        public void Start()
        {
            _isRunning = true;
            Console.WriteLine($"🌐 Game Server Started (Game ID: {_engine.GameId}, Tick Rate: {_tickRate}hz)");
        }

        public void Stop()
        {
            _isRunning = false;
            Console.WriteLine("🌐 Game Server Stopped");
        }

        /// <summary>
        /// Register a new player connection
        /// </summary>
        public PlayerConnection RegisterPlayer(int playerId, string playerName)
        {
            lock (_lock)
            {
                if (_players.ContainsKey(playerId))
                    throw new InvalidOperationException($"Player {playerId} already connected");

                var connection = new PlayerConnection(playerId, playerName);
                _players[playerId] = connection;

                BroadcastMessage(NetworkMessage.CreateTickSync(_engine.GameId, _engine.CurrentTick));
                
                Console.WriteLine($"✅ Player {playerName} (ID: {playerId}) joined");
                return connection;
            }
        }

        /// <summary>
        /// Process incoming command from client
        /// </summary>
        public NetworkMessage ProcessCommandMessage(NetworkMessage message)
        {
            lock (_lock)
            {
                // Validate player
                if (!_players.ContainsKey(message.SenderId))
                {
                    return NetworkMessage.CreateCommandAck(
                        _engine.GameId,
                        Guid.Empty,
                        false,
                        "Player not connected"
                    );
                }

                // Deserialize command
                var command = _serializer.Deserialize(message.Payload);
                if (command == null)
                {
                    return NetworkMessage.CreateCommandAck(
                        _engine.GameId,
                        Guid.Empty,
                        false,
                        "Invalid command format"
                    );
                }

                // Validate command ownership
                if (command.PlayerId != message.SenderId)
                {
                    return NetworkMessage.CreateCommandAck(
                        _engine.GameId,
                        command.CommandId,
                        false,
                        "Command player ID mismatch"
                    );
                }

                // Server authority: Adjust target tick if needed (prevent client cheating)
                int minValidTick = _engine.CurrentTick + 1;
                if (command.TargetTick < minValidTick)
                {
                    // Client sent command for past tick, reject it
                    return NetworkMessage.CreateCommandAck(
                        _engine.GameId,
                        command.CommandId,
                        false,
                        $"Command tick {command.TargetTick} is in the past (current: {_engine.CurrentTick})"
                    );
                }

                // Validate command can execute
                if (!command.CanExecute(_engine))
                {
                    return NetworkMessage.CreateCommandAck(
                        _engine.GameId,
                        command.CommandId,
                        false,
                        "Command validation failed"
                    );
                }

                // Accept command and queue it
                _engine.SubmitCommand(command);
                
                // Broadcast command to all clients for synchronization
                BroadcastMessage(message);

                return NetworkMessage.CreateCommandAck(
                    _engine.GameId,
                    command.CommandId,
                    true,
                    "Command accepted"
                );
            }
        }

        /// <summary>
        /// Simulate one server tick
        /// </summary>
        public void Tick()
        {
            lock (_lock)
            {
                var results = _engine.SimulateTick();

                // Broadcast tick sync to all clients
                var tickMessage = NetworkMessage.CreateTickSync(_engine.GameId, _engine.CurrentTick);
                BroadcastMessage(tickMessage);

                // Log command results
                foreach (var result in results)
                {
                    if (result.Result.IsSuccess)
                    {
                        Console.WriteLine($"  ✅ {result.Result.Message}");
                    }
                    else
                    {
                        Console.WriteLine($"  ❌ {result.Result.Message}");
                    }
                }
            }
        }

        private void BroadcastMessage(NetworkMessage message)
        {
            foreach (var player in _players.Values)
            {
                player.SendMessage(message);
            }
        }

        public GameEngine GetEngine() => _engine;
    }

    public class PlayerConnection
    {
        public int PlayerId { get; }
        public string PlayerName { get; }
        public Queue<NetworkMessage> MessageQueue { get; }
        public DateTime ConnectedAt { get; }

        public PlayerConnection(int playerId, string playerName)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            MessageQueue = new Queue<NetworkMessage>();
            ConnectedAt = DateTime.UtcNow;
        }

        public void SendMessage(NetworkMessage message)
        {
            MessageQueue.Enqueue(message);
        }

        public NetworkMessage? ReceiveMessage()
        {
            return MessageQueue.Count > 0 ? MessageQueue.Dequeue() : null;
        }
    }
}
