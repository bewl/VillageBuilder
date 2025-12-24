using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Systems.Interfaces;
using VillageBuilder.Engine.World;

namespace VillageBuilder.Engine.Systems.Implementation
{
    /// <summary>
    /// Building system adapter wrapping existing building management.
    /// </summary>
    public class BuildingSystemAdapter : IBuildingSystem
    {
        public List<Building> AllBuildings { get; private set; }
        private int _nextBuildingId = 1;

        public BuildingSystemAdapter(List<Building> buildings)
        {
            AllBuildings = buildings;

            // Track highest building ID
            foreach (var building in buildings)
            {
                if (building.Id >= _nextBuildingId)
                    _nextBuildingId = building.Id + 1;
            }
        }

        public Building? TryPlaceBuilding(BuildingType type, int x, int y, int rotation, IResourceSystem resources)
        {
            // Create building rotation enum
            var buildingRotation = rotation switch
            {
                0 => BuildingRotation.North,
                1 => BuildingRotation.East,
                2 => BuildingRotation.South,
                3 => BuildingRotation.West,
                _ => BuildingRotation.North
            };

            // Create temporary building to calculate cost
            var tempBuilding = new Building(type, x, y, buildingRotation, _nextBuildingId);
            var cost = tempBuilding.GetConstructionCost();

            // Check resources
            if (!resources.HasResources(cost))
            {
                return null;
            }

            // Consume resources
            if (!resources.TryConsumeResources(cost))
            {
                return null;
            }

            // Use the temp building (it's already created with the correct ID)
            _nextBuildingId++;
            AllBuildings.Add(tempBuilding);

            return tempBuilding;
        }

        public void UpdateBuildings(GameTime time)
        {
            // Buildings update logic
            // Currently handled by GameEngine
            // This is a placeholder for future refactoring
        }

        public Building? GetBuildingAt(int x, int y)
        {
            return AllBuildings.FirstOrDefault(b => 
                b.GetOccupiedTiles().Any(tile => tile.X == x && tile.Y == y));
        }

        public bool CanPlaceBuilding(BuildingType type, int x, int y, int rotation, VillageGrid grid)
        {
            var definition = BuildingDefinition.Definitions.Get(type);
            var buildingRotation = rotation switch
            {
                0 => BuildingRotation.North,
                1 => BuildingRotation.East,
                2 => BuildingRotation.South,
                3 => BuildingRotation.West,
                _ => BuildingRotation.North
            };

            var tiles = definition.GetOccupiedTiles(x, y, buildingRotation);

            foreach (var tile in tiles)
            {
                var gridTile = grid.GetTile(tile.X, tile.Y);
                if (gridTile == null || !gridTile.IsWalkable) // Property, not method
                {
                    return false;
                }

                // Check if another building occupies this tile
                if (GetBuildingAt(tile.X, tile.Y) != null)
                {
                    return false;
                }
            }

            return true;
        }

        public void RemoveBuilding(Building building, VillageGrid grid)
        {
            AllBuildings.Remove(building);

            // Clear building from tiles
            foreach (var tile in building.GetOccupiedTiles())
            {
                var gridTile = grid.GetTile(tile.X, tile.Y);
                if (gridTile != null)
                {
                    gridTile.Building = null;
                }
            }
        }

        public List<Building> GetBuildingsByType(BuildingType type)
        {
            return AllBuildings.Where(b => b.Type == type).ToList();
        }
    }
}
