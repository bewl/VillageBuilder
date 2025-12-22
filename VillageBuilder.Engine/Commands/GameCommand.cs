using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillageBuilder.Engine.Core;

namespace VillageBuilder.Engine.Commands
{
    public abstract class GameCommand : ICommand
    {
        public Guid CommandId { get; }
        public int PlayerId { get; }
        public int TargetTick { get; }

        protected GameCommand(int playerId, int targetTick)
        {
            CommandId = Guid.NewGuid();
            PlayerId = playerId;
            TargetTick = targetTick;
        }

        public abstract CommandResult Execute(GameEngine engine);
        public abstract bool CanExecute(GameEngine engine);

        protected bool ValidateGameState(GameEngine engine)
        {
            // Basic validation that can be overridden
            return engine != null;
        }
    }
}
