using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Core;

namespace VillageBuilder.Engine.Commands.BuildingCommands
{
    public class ConstructBuildingCommand : GameCommand
    {
        public BuildingType BuildingType { get; }
        public int X { get; }
        public int Y { get; }
        public BuildingRotation Rotation { get; }

        public ConstructBuildingCommand(int playerId, int targetTick, BuildingType buildingType, int x, int y, BuildingRotation rotation = BuildingRotation.North)
            : base(playerId, targetTick)
        {
            BuildingType = buildingType;
            X = x;
            Y = y;
            Rotation = rotation;
        }

        public override bool CanExecute(GameEngine engine)
        {
            if (!ValidateGameState(engine))
                return false;

            // Create temp building to check all occupied tiles
            var building = new Building(BuildingType, X, Y, Rotation);
            var occupiedTiles = building.GetOccupiedTiles();

            // Check if all tiles are available
            foreach (var tilePos in occupiedTiles)
            {
                var tile = engine.Grid.GetTile(tilePos.X, tilePos.Y);
                if (tile == null || !tile.IsWalkable || tile.Building != null)
                    return false;
            }

            // Check resources
            var costs = building.GetConstructionCost();
            
            foreach (var cost in costs)
            {
                if (!engine.VillageResources.Has(cost.Key, cost.Value))
                    return false;
            }

            return true;
        }

        public override CommandResult Execute(GameEngine engine)
        {
            if (!CanExecute(engine))
                return CommandResult.Failed($"Cannot construct {BuildingType} at ({X}, {Y})");

            var building = new Building(BuildingType, X, Y, Rotation);
            var occupiedTiles = building.GetOccupiedTiles();
            var costs = building.GetConstructionCost();

            // Deduct resources
            foreach (var cost in costs)
            {
                if (!engine.VillageResources.Remove(cost.Key, cost.Value))
                    return CommandResult.InsufficientResources($"Not enough {cost.Key}");
            }

            // Place building - mark all occupied tiles
            building.IsConstructed = true;
            foreach (var tilePos in occupiedTiles)
            {
                var tile = engine.Grid.GetTile(tilePos.X, tilePos.Y);
                if (tile != null)
                {
                    tile.Building = building; // IsWalkable will automatically become false
                }
            }
            
            engine.Buildings.Add(building);

            return CommandResult.Success(
                $"Constructed {BuildingType} at ({X}, {Y}) with rotation {Rotation}",
                new Dictionary<string, object>
                {
                    { "BuildingType", BuildingType },
                    { "Position", new { X, Y } },
                    { "Rotation", Rotation }
                }
            );
        }
    }
}
