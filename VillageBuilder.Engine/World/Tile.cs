using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Entities;

namespace VillageBuilder.Engine.World
{
    /// <summary>
    /// Represents the type of terrain for a tile
    /// </summary>
    public enum TileType
    {
        Grass,
        Forest,
        Water,
        Mountain,
        Field,
        Road,
        BuildingFoundation
    }

    /// <summary>
    /// Represents a single tile in the game world grid.
    /// Contains terrain type, buildings, decorations, and people tracking.
    /// </summary>
    public class Tile
    {
        /// <summary>
        /// X coordinate in the world grid
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Y coordinate in the world grid
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// The terrain type of this tile
        /// </summary>
        public TileType Type { get; set; }

        /// <summary>
        /// Building placed on this tile (if any)
        /// </summary>
        public Building? Building { get; set; }

        /// <summary>
        /// All people currently standing on this tile
        /// </summary>
        public List<Person> PeopleOnTile { get; }

        /// <summary>
        /// Visual decorations for terrain variety (trees, rocks, flowers, wildlife, etc.)
        /// </summary>
        public List<TerrainDecoration> Decorations { get; } = new();

        /// <summary>
        /// Terrain visual variant (0-3) for same tile types to look different
        /// </summary>
        public int TerrainVariant { get; set; }

        /// <summary>
        /// Determines if this tile can be walked on by people
        /// </summary>
        public bool IsWalkable
        {
            get
            {
                // Water and mountains are never walkable
                if (Type == TileType.Water || Type == TileType.Mountain)
                    return false;

                // Check if decorations block movement (large trees, boulders, etc.)
                if (Decorations.Any(d => d.IsBlocking()))
                    return false;

                // If no building, tile is walkable
                if (Building == null)
                    return true;

                // If there's a building, check what type of building tile this is
                var buildingTile = Building.GetTileAtWorldPosition(X, Y);
                if (buildingTile.HasValue)
                {
                    // Floors are always walkable (interior navigation)
                    if (buildingTile.Value.Type == BuildingTileType.Floor)
                        return true;

                    // Doors are walkable if building doors are open
                    if (buildingTile.Value.Type == BuildingTileType.Door && Building.DoorsOpen)
                        return true;
                }

                // Walls and other building tiles are not walkable
                return false;
            }
        }

                /// <summary>
                /// Creates a new tile at the specified world coordinates
                /// </summary>
                /// <param name="x">X coordinate in world grid</param>
                /// <param name="y">Y coordinate in world grid</param>
                /// <param name="type">Initial terrain type</param>
                public Tile(int x, int y, TileType type)
                {
                    X = x;
                    Y = y;
                    Type = type;
                    PeopleOnTile = new List<Person>();
                    TerrainVariant = Random.Shared.Next(0, 4); // Random visual variant 0-3
                }

                /// <summary>
                /// Gets a human-readable name for the terrain type
                /// </summary>
                public string GetTerrainName()
                {
                    return Type switch
                    {
                        TileType.Grass => "Grassland",
                        TileType.Forest => "Forest",
                        TileType.Water => "Water",
                        TileType.Mountain => "Mountain",
                        TileType.Field => "Farmland",
                        TileType.Road => "Road",
                        TileType.BuildingFoundation => "Building Site",
                        _ => "Unknown"
                    };
                }

                /// <summary>
                /// Gets the total count of decorations on this tile
                /// </summary>
                public int GetDecorationCount()
                {
                    return Decorations.Count;
                }

                /// <summary>
                /// Checks if this tile has any blocking decorations (large trees, boulders, etc.)
                /// </summary>
                public bool HasBlockingDecorations()
                {
                    return Decorations.Any(d => d.IsBlocking());
                }

                /// <summary>
                /// Gets a summary description of this tile for inspection UI
                /// </summary>
                public string GetInspectionSummary()
                {
                    var parts = new List<string>();

                    // Terrain
                    parts.Add($"{GetTerrainName()} (Variant {TerrainVariant})");

                    // Walkability
                    parts.Add(IsWalkable ? "Walkable" : "Not Walkable");

                    // Decorations
                    if (Decorations.Count > 0)
                    {
                        parts.Add($"{Decorations.Count} decoration(s)");
                    }

                    // People
                    if (PeopleOnTile.Count > 0)
                    {
                        parts.Add($"{PeopleOnTile.Count} person(s)");
                    }

                                // Building
                                if (Building != null)
                                {
                                    parts.Add($"Building: {Building.Name}");
                                }

                                return string.Join(" • ", parts);
                            }
                        }
                    }
