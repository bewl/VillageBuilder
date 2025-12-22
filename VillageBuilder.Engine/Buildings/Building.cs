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
        public int Id { get; }
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
        public bool DoorsOpen { get; set; } // Track if doors are open (allows entry)
        public List<Person> Residents { get; } // People who live in this building (for houses)

        public Building(BuildingType type, int x, int y, BuildingRotation rotation = BuildingRotation.North, int id = 0)
        {
            Id = id;
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
            DoorsOpen = true; // Buildings start with doors open
            Residents = new List<Person>();
        }

        /// <summary>
        /// Get the world positions of this building's doors
        /// </summary>
        public List<Vector2Int> GetDoorPositions()
        {
            return Definition.GetDoorPositions(X, Y, Rotation);
        }
        
        /// <summary>
        /// Get interior floor positions where people can stand inside the building
        /// </summary>
        public List<Vector2Int> GetInteriorPositions()
        {
            var interiorPositions = new List<Vector2Int>();
            
            // Get all occupied tiles
            var occupiedTiles = GetOccupiedTiles();
            
            foreach (var tilePos in occupiedTiles)
            {
                var buildingTile = GetTileAtWorldPosition(tilePos.X, tilePos.Y);
                
                // Include floor tiles (not walls, not doors)
                if (buildingTile.HasValue && buildingTile.Value.Type == BuildingTileType.Floor)
                {
                    interiorPositions.Add(tilePos);
                }
            }
            
            return interiorPositions;
        }

        /// <summary>
        /// Get all tiles occupied by this building
        /// </summary>
        public List<Vector2Int> GetOccupiedTiles()
        {
            return Definition.GetOccupiedTiles(X, Y, Rotation);
        }

        /// <summary>
        /// Get the building tile at a world position
        /// </summary>
        public BuildingTile? GetTileAtWorldPosition(int worldX, int worldY)
        {
            // Convert world coordinates to local offset from building origin
            int offsetX = worldX - X;
            int offsetY = worldY - Y;
            
            // Get the tile - Definition.GetTileAt() handles rotation internally
            var tile = Definition.GetTileAt(offsetX, offsetY, Rotation);
            
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

        public void Work(Season currentSeason)
        {
            if (!IsConstructed) return;

            // Production happens when workers are assigned
            if (Workers.Count == 0) return;

            // Different buildings produce different resources
            // Production scales with number of workers
            var productionMultiplier = Workers.Count;

            switch (Type)
            {
                case BuildingType.Farm:
                    // Farms produce grain (more in summer/fall)
                    var grainAmount = currentSeason == Season.Summer || currentSeason == Season.Fall ? 
                        2 * productionMultiplier : productionMultiplier;
                    Storage.Add(ResourceType.Grain, grainAmount);
                    break;

                case BuildingType.Lumberyard:
                    // Lumberyards produce wood
                    Storage.Add(ResourceType.Wood, 1 * productionMultiplier);
                    break;

                case BuildingType.Mine:
                    // Mines produce stone
                    Storage.Add(ResourceType.Stone, 1 * productionMultiplier);
                    break;

                case BuildingType.Workshop:
                    // Workshops produce tools (requires wood and stone)
                    // Check if we have materials
                    if (Storage.Get(ResourceType.Wood) >= 2 && Storage.Get(ResourceType.Stone) >= 1)
                    {
                        Storage.Remove(ResourceType.Wood, 2);
                        Storage.Remove(ResourceType.Stone, 1);
                        Storage.Add(ResourceType.Tools, 1 * productionMultiplier);
                    }
                    break;
            }
        }

        /// <summary>
        /// Check if building is occupied (has people inside)
        /// Used for lighting effects at night
        /// </summary>
        public bool IsOccupied()
        {
            // Houses are occupied if they have residents
            if (Type == BuildingType.House)
            {
                return Residents.Any(r => r.IsAlive);
            }
            
            // Other buildings are occupied if they have workers present
            return Workers.Any(w => w.IsAlive && w.IsAtWork());
        }

        /// <summary>
        /// Check if this building should show lights (occupied at night)
        /// </summary>
        public bool ShouldShowLights(GameTime time)
        {
            if (!IsConstructed) return false;
            if (!time.IsNight()) return false;
            
            return IsOccupied();
        }
    }
}