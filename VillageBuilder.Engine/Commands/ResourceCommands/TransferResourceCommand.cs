using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Resources;

namespace VillageBuilder.Engine.Commands.ResourceCommands
{
    public class TransferResourceCommand : GameCommand
    {
        public ResourceType ResourceType { get; }
        public int Amount { get; }
        public int FromBuildingX { get; }
        public int FromBuildingY { get; }
        public int ToBuildingX { get; }
        public int ToBuildingY { get; }

        public TransferResourceCommand(int playerId, int targetTick, ResourceType resourceType, int amount,
            int fromX, int fromY, int toX, int toY)
            : base(playerId, targetTick)
        {
            ResourceType = resourceType;
            Amount = amount;
            FromBuildingX = fromX;
            FromBuildingY = fromY;
            ToBuildingX = toX;
            ToBuildingY = toY;
        }

        public override bool CanExecute(GameEngine engine)
        {
            var fromBuilding = engine.Buildings.FirstOrDefault(b => b.X == FromBuildingX && b.Y == FromBuildingY);
            var toBuilding = engine.Buildings.FirstOrDefault(b => b.X == ToBuildingX && b.Y == ToBuildingY);

            if (fromBuilding == null || toBuilding == null)
                return false;

            return fromBuilding.Storage.Has(ResourceType, Amount);
        }

        public override CommandResult Execute(GameEngine engine)
        {
            var fromBuilding = engine.Buildings.FirstOrDefault(b => b.X == FromBuildingX && b.Y == FromBuildingY);
            var toBuilding = engine.Buildings.FirstOrDefault(b => b.X == ToBuildingX && b.Y == ToBuildingY);

            if (fromBuilding == null || toBuilding == null)
                return CommandResult.Failed("Source or destination building not found");

            if (!fromBuilding.Storage.Remove(ResourceType, Amount))
                return CommandResult.InsufficientResources($"Not enough {ResourceType} in {fromBuilding.Name}");

            toBuilding.Storage.Add(ResourceType, Amount);

            return CommandResult.Success(
                $"Transferred {Amount} {ResourceType} from {fromBuilding.Name} to {toBuilding.Name}",
                new Dictionary<string, object>
                {
                    { "ResourceType", ResourceType },
                    { "Amount", Amount },
                    { "From", fromBuilding.Name },
                    { "To", toBuilding.Name }
                }
            );
        }
    }
}
