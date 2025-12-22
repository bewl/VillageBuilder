using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillageBuilder.Engine.Core;

namespace VillageBuilder.Engine.Commands
{
    /// <summary>
    /// Base interface for all game commands.
    /// Commands are deterministic and can be serialized for networking.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Unique command ID for networking and validation
        /// </summary>
        Guid CommandId { get; }
        
        /// <summary>
        /// Player who issued the command
        /// </summary>
        int PlayerId { get; }
        
        /// <summary>
        /// Game tick when command should execute
        /// </summary>
        int TargetTick { get; }
        
        /// <summary>
        /// Execute the command on the game engine
        /// </summary>
        /// <returns>Result of command execution</returns>
        CommandResult Execute(GameEngine engine);
        
        /// <summary>
        /// Validate if command can be executed
        /// </summary>
        bool CanExecute(GameEngine engine);
    }
}
