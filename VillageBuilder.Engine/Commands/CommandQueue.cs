using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillageBuilder.Engine.Core;

namespace VillageBuilder.Engine.Commands
{
    /// <summary>
    /// Manages command execution queue for deterministic multiplayer gameplay.
    /// Commands are executed in tick order to ensure synchronization.
    /// </summary>
    public class CommandQueue
    {
        private readonly SortedDictionary<int, List<ICommand>> _commandsByTick;
        private readonly List<CommandExecutionRecord> _executionHistory;
        private int _currentTick;

        public CommandQueue()
        {
            _commandsByTick = new SortedDictionary<int, List<ICommand>>();
            _executionHistory = new List<CommandExecutionRecord>();
            _currentTick = 0;
        }

        public int CurrentTick => _currentTick;
        public IReadOnlyList<CommandExecutionRecord> History => _executionHistory.AsReadOnly();

        /// <summary>
        /// Add command to queue for future execution
        /// </summary>
        public void EnqueueCommand(ICommand command)
        {
            if (!_commandsByTick.ContainsKey(command.TargetTick))
            {
                _commandsByTick[command.TargetTick] = new List<ICommand>();
            }

            _commandsByTick[command.TargetTick].Add(command);
        }

        /// <summary>
        /// Process all commands scheduled for current tick
        /// </summary>
        public List<CommandExecutionRecord> ProcessTick(GameEngine engine)
        {
            var results = new List<CommandExecutionRecord>();

            if (_commandsByTick.TryGetValue(_currentTick, out var commands))
            {
                // Sort commands by CommandId for deterministic order
                var sortedCommands = commands.OrderBy(c => c.CommandId).ToList();

                foreach (var command in sortedCommands)
                {
                    var result = command.Execute(engine);
                    var record = new CommandExecutionRecord(command, result, _currentTick);
                    
                    _executionHistory.Add(record);
                    results.Add(record);
                }

                _commandsByTick.Remove(_currentTick);
            }

            _currentTick++;
            return results;
        }

        /// <summary>
        /// Get pending commands count
        /// </summary>
        public int GetPendingCommandCount()
        {
            return _commandsByTick.Values.Sum(list => list.Count);
        }

        /// <summary>
        /// Clear all pending commands (for testing/reset)
        /// </summary>
        public void Clear()
        {
            _commandsByTick.Clear();
        }
    }

    public class CommandExecutionRecord
    {
        public ICommand Command { get; }
        public CommandResult Result { get; }
        public int ExecutedAtTick { get; }
        public DateTime Timestamp { get; }

        public CommandExecutionRecord(ICommand command, CommandResult result, int executedAtTick)
        {
            Command = command;
            Result = result;
            ExecutedAtTick = executedAtTick;
            Timestamp = DateTime.UtcNow;
        }
    }
}
