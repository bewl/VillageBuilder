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

            var building = new Building(BuildingType, X, Y, Rotation, engine.GetNextBuildingId());
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
            
            // Log construction as a narrative event
            EventLog.Instance.AddMessage(
                $"Construction complete: a new {building.Type} is now operational", 
                LogLevel.Success);
            
            // DEBUG: Log building placement details
            Console.WriteLine($"\n=== BUILDING PLACED ===");
            Console.WriteLine($"Type: {BuildingType}, Position: ({X}, {Y}), Rotation: {Rotation}");
            Console.WriteLine($"Definition: Width={building.Definition.Width}, Height={building.Definition.Height}");
            Console.WriteLine($"Occupied tiles: {occupiedTiles.Count}");
            
            // Show all corner tiles with their layout positions
            Console.WriteLine($"\nCorner tiles (world position -> layout position -> glyph):");
            var rotatedDims = building.Definition.GetRotatedDimensions(Rotation);
            Console.WriteLine($"Rotated dimensions: {rotatedDims.width}x{rotatedDims.height}");
            
            // Get the first and last few tiles to see corners
            var tilesToCheck = new[] { 
                occupiedTiles[0], 
                occupiedTiles[occupiedTiles.Count - 1],
                occupiedTiles[Math.Min(rotatedDims.width - 1, occupiedTiles.Count - 1)],
                occupiedTiles[Math.Min(occupiedTiles.Count - rotatedDims.width, occupiedTiles.Count - 1)]
            }.Distinct();
            
            foreach (var tilePos in tilesToCheck)
            {
                var buildingTile = building.GetTileAtWorldPosition(tilePos.X, tilePos.Y);
                var localX = tilePos.X - X;
                var localY = tilePos.Y - Y;
                Console.WriteLine($"  World({tilePos.X},{tilePos.Y}) Local({localX},{localY}): {(buildingTile.HasValue ? $"{buildingTile.Value.Type} '{buildingTile.Value.Glyph}'" : "NULL")}");
            }
            Console.WriteLine($"======================\n");
            
            foreach (var tilePos in occupiedTiles)
            {
                var tile = engine.Grid.GetTile(tilePos.X, tilePos.Y);
                if (tile != null)
                {
                    tile.Building = building; // IsWalkable will automatically become false
                }
            }
            
            engine.Buildings.Add(building);
            
            // Auto-assign house to a homeless family
            if (BuildingType == BuildingType.House)
            {
                AutoAssignHouseToFamily(engine, building);
            }

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
        
        /// <summary>
        /// Automatically assign a newly constructed house to a homeless family
        /// </summary>
        private void AutoAssignHouseToFamily(GameEngine engine, Building house)
        {
            // Find families without a home
            var homelessFamily = engine.Families
                .FirstOrDefault(f => f.Members.Any(m => m.IsAlive && m.HomeBuilding == null));
            
            if (homelessFamily == null)
                return; // No homeless families
            
            // Assign all family members to this house
            foreach (var member in homelessFamily.Members.Where(m => m.IsAlive))
            {
                member.HomeBuilding = house;
                
                // Add to building residents if not already there
                if (!house.Residents.Contains(member))
                {
                    house.Residents.Add(member);
                }
            }
            
            // Update family home position to the house
            var doorPositions = house.GetDoorPositions();
            if (doorPositions.Count > 0)
            {
                homelessFamily.HomePosition = doorPositions[0];
            }
            else
            {
                homelessFamily.HomePosition = new Vector2Int(house.X, house.Y);
            }
            
            EventLog.Instance.AddMessage(
                $"The {homelessFamily.FamilyName} family has moved into their new home",
                LogLevel.Success);
        }
    }
}
