using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Entities;
using VillageBuilder.Engine.Resources;
using VillageBuilder.Engine.World;

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

    public enum ConstructionStage
    {
        Foundation,      // 0-25%
        Framing,        // 25-50%
        Walls,          // 50-75%
        Finishing       // 75-100%
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
        public int ConstructionWorkRequired { get; private set; }
        public List<Person> ConstructionWorkers { get; }
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

            // Calculate construction work required based on building area
            var occupiedTiles = Definition.GetOccupiedTiles(x, y, rotation).Count;
            ConstructionWorkRequired = CalculateConstructionWorkRequired(type, occupiedTiles);

            ConstructionWorkers = new List<Person>();
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
        /// Get an unoccupied interior position, prioritizing tiles with no people
        /// Returns null if all positions are occupied
        /// </summary>
        public Vector2Int? GetUnoccupiedInteriorPosition(VillageGrid grid)
        {
            var interiorPositions = GetInteriorPositions();

            if (interiorPositions.Count == 0)
                return null;

            // First, try to find a completely empty position
            foreach (var pos in interiorPositions)
            {
                var tile = grid.GetTile(pos.X, pos.Y);
                if (tile != null && tile.PeopleOnTile.Count == 0)
                {
                    return pos;
                }
            }

            // If all tiles have people, return the least crowded one
            var leastCrowded = interiorPositions
                .Select(pos => new { Position = pos, Count = grid.GetTile(pos.X, pos.Y)?.PeopleOnTile.Count ?? 0 })
                .OrderBy(x => x.Count)
                .FirstOrDefault();

            return leastCrowded?.Position;
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

        /// <summary>
        /// Calculate how much work is required to construct this building
        /// </summary>
        private int CalculateConstructionWorkRequired(BuildingType type, int area)
        {
            // Base work: 50 work units per tile
            int baseWork = area * 50;

            // Multiplier based on building complexity
            float complexityMultiplier = type switch
            {
                BuildingType.House => 1.0f,
                BuildingType.Farm => 0.8f,         // Simple structures
                BuildingType.Warehouse => 1.2f,    // Larger, sturdier
                BuildingType.Lumberyard => 0.9f,
                BuildingType.Mine => 1.5f,         // Requires excavation
                BuildingType.Workshop => 1.3f,     // Complex equipment
                BuildingType.Market => 1.4f,
                BuildingType.Well => 0.7f,         // Small structure
                BuildingType.TownHall => 2.0f,     // Grand building
                _ => 1.0f
            };

            return (int)(baseWork * complexityMultiplier);
        }

        /// <summary>
        /// Get current construction stage based on progress
        /// </summary>
        public ConstructionStage GetConstructionStage()
        {
            if (IsConstructed)
                return ConstructionStage.Finishing;

            float progress = (float)ConstructionProgress / ConstructionWorkRequired;

            if (progress < 0.25f)
                return ConstructionStage.Foundation;
            else if (progress < 0.5f)
                return ConstructionStage.Framing;
            else if (progress < 0.75f)
                return ConstructionStage.Walls;
            else
                return ConstructionStage.Finishing;
        }

        /// <summary>
        /// Get construction progress percentage (0-100)
        /// </summary>
        public int GetConstructionProgressPercent()
        {
            if (IsConstructed)
                return 100;

            return (int)((float)ConstructionProgress / ConstructionWorkRequired * 100);
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

            // Calculate effective workforce based on worker condition
            float effectiveWorkers = 0f;
            foreach (var worker in Workers)
            {
                float effectiveness = 1.0f;

                // Hunger penalty
                if (worker.Hunger >= 80)
                    effectiveness *= 0.3f; // Starving: 70% penalty
                else if (worker.Hunger >= 60)
                    effectiveness *= 0.6f; // Very hungry: 40% penalty
                else if (worker.Hunger >= 40)
                    effectiveness *= 0.8f; // Hungry: 20% penalty

                // Sickness penalty
                if (worker.IsSick)
                    effectiveness *= 0.5f; // Sick: 50% penalty

                // Low energy penalty
                if (worker.Energy < 20)
                    effectiveness *= 0.5f; // Exhausted: 50% penalty
                else if (worker.Energy < 40)
                    effectiveness *= 0.75f; // Tired: 25% penalty

                effectiveWorkers += effectiveness;
            }

            // Production scales with effective workforce
            var productionMultiplier = (int)Math.Ceiling(effectiveWorkers);
            if (productionMultiplier <= 0) return; // No production if workers too ineffective

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
            /// Check if this building has access to water from a nearby well
            /// </summary>
            public bool HasWaterAccess(List<Building> allBuildings, int maxDistance = 10)
            {
                // Only houses need water access
                if (Type != BuildingType.House) return true;

                // Check if there's a well within maxDistance tiles
                var wells = allBuildings.Where(b => b.Type == BuildingType.Well && b.IsConstructed);
                foreach (var well in wells)
                {
                    int distance = Math.Abs(X - well.X) + Math.Abs(Y - well.Y);
                    if (distance <= maxDistance)
                    {
                        return true;
                    }
                }

                return false;
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