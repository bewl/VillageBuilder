using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Entities;
using VillageBuilder.Engine.Resources;

namespace VillageBuilder.Engine.Buildings
{
    public enum BuildingType
    {
        House,
        Warehouse,
        Workshop,
        Farm,
        Mine,
        Lumberyard,
        Market,
        Well,
        TownHall
    }

    public class Building
    {
        public BuildingType Type { get; }
        public string Name { get; set; }
        public int X { get; }
        public int Y { get; }
        public BuildingRotation Rotation { get; }
        public BuildingDefinition Definition { get; }
        public bool IsConstructed { get; set; }
        public int ConstructionProgress { get; set; }
        public List<Person> Workers { get; }
        public ResourceInventory Storage { get; }

        public Building(BuildingType type, int x, int y, BuildingRotation rotation = BuildingRotation.North)
        {
            Type = type;
            Name = type.ToString();
            X = x;
            Y = y;
            Rotation = rotation;
            Definition = BuildingDefinition.Definitions.Get(type);
            IsConstructed = false;
            ConstructionProgress = 0;
            Workers = new List<Person>();
            Storage = new ResourceInventory();
        }

        /// <summary>
        /// Get the world positions of this building's doors
        /// </summary>
        public List<Vector2Int> GetDoorPositions()
        {
            return Definition.GetDoorPositions(X, Y, Rotation);
        }

        /// <summary>
        /// Get all tiles occupied by this building
        /// </summary>
        public List<Vector2Int> GetOccupiedTiles()
        {
            return Definition.GetOccupiedTiles(X, Y, Rotation);
        }

        /// <summary>
        /// Get the building tile type at a world position
        /// </summary>
        public BuildingTile? GetTileAtWorldPosition(int worldX, int worldY)
        {
            int localX = worldX - X;
            int localY = worldY - Y;
            
            var tile = Definition.GetTileAt(localX, localY, Rotation);
            return tile.Type != BuildingTileType.Empty ? tile : null;
        }

        public Dictionary<ResourceType, int> GetConstructionCost()
        {
            // Cost scales with building size
            var occupiedTiles = GetOccupiedTiles().Count;
            var baseWoodCost = occupiedTiles * 10;
            var baseStoneCost = occupiedTiles * 5;

            return Type switch
            {
                BuildingType.House => new Dictionary<ResourceType, int>
                {
                    { ResourceType.Wood, baseWoodCost },
                    { ResourceType.Stone, baseStoneCost }
                },
                BuildingType.Warehouse => new Dictionary<ResourceType, int>
                {
                    { ResourceType.Wood, baseWoodCost * 2 },
                    { ResourceType.Stone, baseStoneCost }
                },
                BuildingType.Farm => new Dictionary<ResourceType, int>
                {
                    { ResourceType.Wood, baseWoodCost }
                },
                BuildingType.Workshop => new Dictionary<ResourceType, int>
                {
                    { ResourceType.Wood, baseWoodCost },
                    { ResourceType.Stone, baseStoneCost },
                    { ResourceType.Iron, 10 }
                },
                BuildingType.Market => new Dictionary<ResourceType, int>
                {
                    { ResourceType.Wood, baseWoodCost * 2 },
                    { ResourceType.Stone, baseStoneCost * 2 }
                },
                BuildingType.TownHall => new Dictionary<ResourceType, int>
                {
                    { ResourceType.Wood, baseWoodCost * 3 },
                    { ResourceType.Stone, baseStoneCost * 3 }
                },
                _ => new Dictionary<ResourceType, int> { { ResourceType.Wood, baseWoodCost } }
            };
        }

        public void Work(Season season)
        {
            if (!IsConstructed) return;

            // Different buildings produce different resources
            foreach (var worker in Workers)
            {
                switch (Type)
                {
                    case BuildingType.Lumberyard:
                        Storage.Add(ResourceType.Wood, 5);
                        break;
                    case BuildingType.Mine:
                        Storage.Add(ResourceType.Stone, 3);
                        Storage.Add(ResourceType.Iron, 1);
                        break;
                    case BuildingType.Farm when season != Season.Winter:
                        Storage.Add(ResourceType.Grain, 10);
                        break;
                }
            }
        }
    }
}