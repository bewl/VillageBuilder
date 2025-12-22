using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace VillageBuilder.Engine.Buildings
{
    /// <summary>
    /// Represents the rotation of a building
    /// </summary>
    public enum BuildingRotation
    {
        North = 0,   // 0 degrees
        East = 90,   // 90 degrees
        South = 180, // 180 degrees
        West = 270   // 270 degrees
    }

    /// <summary>
    /// Tile type within a building
    /// </summary>
    public enum BuildingTileType
    {
        Empty,      // Not part of building
        Floor,      // Interior floor
        Wall,       // Wall
        Door        // Door/entrance
    }

    /// <summary>
    /// Represents a single tile in a building layout
    /// </summary>
    public struct BuildingTile
    {
        public BuildingTileType Type { get; set; }
        public bool IsWalkable { get; set; }
        public char Glyph { get; set; }

        public BuildingTile(BuildingTileType type, char glyph = ' ')
        {
            Type = type;
            Glyph = glyph;
            IsWalkable = type == BuildingTileType.Floor || type == BuildingTileType.Door;
        }
    }

    /// <summary>
    /// Defines the layout and properties of a building type
    /// </summary>
    public class BuildingDefinition
    {
        public BuildingType Type { get; }
        public int Width { get; }
        public int Height { get; }
        public BuildingTile[,] Layout { get; }
        public List<Vector2Int> DoorPositions { get; }
        
        public BuildingDefinition(BuildingType type, int width, int height, BuildingTile[,] layout, List<Vector2Int> doorPositions)
        {
            Type = type;
            Width = width;
            Height = height;
            Layout = layout;
            DoorPositions = doorPositions;
        }

        /// <summary>
        /// Get door positions in world space given building origin and rotation
        /// </summary>
        public List<Vector2Int> GetDoorPositions(int buildingX, int buildingY, BuildingRotation rotation)
        {
            var worldDoors = new List<Vector2Int>();
            foreach (var door in DoorPositions)
            {
                var rotated = RotateOffset(door, rotation);
                worldDoors.Add(new Vector2Int(buildingX + rotated.X, buildingY + rotated.Y));
            }
            return worldDoors;
        }

        /// <summary>
        /// Get all occupied tiles for this building at given position and rotation
        /// </summary>
        public List<Vector2Int> GetOccupiedTiles(int buildingX, int buildingY, BuildingRotation rotation)
        {
            var tiles = new List<Vector2Int>();

            // Loop through Y (rows) first, then X (columns) to match row-major array layout
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    // Layout is stored in row-major order [row, column] = [y, x]
                    if (Layout[y, x].Type != BuildingTileType.Empty)
                    {
                        var rotated = RotateOffset(new Vector2Int(x, y), rotation);
                        tiles.Add(new Vector2Int(buildingX + rotated.X, buildingY + rotated.Y));
                    }
                }
            }
            return tiles;
        }

        /// <summary>
        /// Get the building tile at a specific local position with rotation applied
        /// </summary>
        public BuildingTile GetTileAt(int localX, int localY, BuildingRotation rotation)
        {
            var original = UnrotateOffset(new Vector2Int(localX, localY), rotation);
            
            if (original.X >= 0 && original.X < Width && original.Y >= 0 && original.Y < Height)
            {
                // Layout is stored in row-major order [row, column] = [y, x]
                var tile = Layout[original.Y, original.X];
                
                // Rotate the glyph based on building rotation
                if (tile.Type != BuildingTileType.Empty && rotation != BuildingRotation.North)
                {
                    tile.Glyph = RotateGlyph(tile.Glyph, rotation);
                }
                
                return tile;
            }
            
            return new BuildingTile(BuildingTileType.Empty, ' ');
        }

        /// <summary>
        /// Rotate box-drawing characters based on building rotation
        /// </summary>
        private static char RotateGlyph(char glyph, BuildingRotation rotation)
        {
            // Only rotate once per 90° increment
            var rotations = rotation switch
            {
                BuildingRotation.East => 1,   // 90° clockwise
                BuildingRotation.South => 2,  // 180°
                BuildingRotation.West => 3,   // 270° clockwise
                _ => 0
            };
            
            var result = glyph;
            for (int i = 0; i < rotations; i++)
            {
                result = RotateGlyphOnce(result);
            }
            return result;
        }

        /// <summary>
        /// Rotate a box-drawing character 90° clockwise
        /// </summary>
        private static char RotateGlyphOnce(char glyph)
        {
            return glyph switch
            {
                // Single-line corners (rotate clockwise)
                '┌' => '┐',  // Top-left → Top-right
                '┐' => '┘',  // Top-right → Bottom-right
                '┘' => '└',  // Bottom-right → Bottom-left
                '└' => '┌' , // Bottom-left → Top-left
                
                // Single-line T-junctions (rotate clockwise)
                '├' => '┬',  // Left T → Top T
                '┬' => '┤',  // Top T → Right T
                '┤' => '┴',  // Right T → Bottom T
                '┴' => '├',  // Bottom T → Left T
                
                // Straight lines - these ALTERNATE, not rotate
                '─' => '│',  // Horizontal → Vertical
                '│' => '─',  // Vertical → Horizontal
                
                // Double-line corners
                '╔' => '╗',
                '╗' => '╝',
                '╝' => '╚',
                '╚' => '╔',
                
                // Double-line straight
                '═' => '║',
                '║' => '═',
                
                // Double-line T-junctions
                '╠' => '╦',
                '╦' => '╣',
                '╣' => '╩',
                '╩' => '╠',
                
                // Mixed single/double T-junctions (added these missing ones)
                '╞' => '╥',
                '╥' => '╡',
                '╡' => '╨',
                '╨' => '╞',
                
                '╟' => '╤',
                '╤' => '╢',
                '╢' => '╧',
                '╧' => '╟',
                
                // Cross (stays the same - symmetric)
                '┼' => '┼',
                '╬' => '╬',
                '╪' => '╫',  // Vertical line with horizontal double → Horizontal line with vertical double
                '╫' => '╪',
                
                // Block characters (stay the same - symmetric)
                '█' => '█',
                '▓' => '▓',
                '▒' => '▒',
                '░' => '░',
                '■' => '■',
                '□' => '□',
                
                // Dots and small symbols (stay the same)
                '.' => '.',
                '·' => '·',
                '○' => '○',
                '●' => '●',
                '◦' => '◦',
                
                // Directional arrows
                '↑' => '→',
                '→' => '↓',
                '↓' => '←',
                '←' => '↑',
                
                '▲' => '▶',
                '▶' => '▼',
                '▼' => '◀',
                '◀' => '▲',
                
                // Diagonal lines
                '/' => '\\',
                '\\' => '/',
                
                // Diamond shapes (stay the same - symmetric)
                '◆' => '◆',
                '◇' => '◇',
                '◊' => '◊',
                
                // Card suits (stay the same - mostly symmetric)
                '♠' => '♠',
                '♣' => '♣',
                '♥' => '♥',
                '♦' => '♦',
                
                // Smileys (stay the same)
                '☺' => '☺',
                '☻' => '☻',
                
                // Default: return unchanged
                _ => glyph
            };
        }

        /// <summary>
        /// Get building dimensions after rotation
        /// </summary>
        public (int width, int height) GetRotatedDimensions(BuildingRotation rotation)
        {
            return rotation switch
            {
                BuildingRotation.North or BuildingRotation.South => (Width, Height),
                BuildingRotation.East or BuildingRotation.West => (Height, Width),
                _ => (Width, Height)
            };
        }

        // Correct rotation helpers that rotate around the layout's top-left origin,
        // keeping coordinates inside [0..Width-1]/[0..Height-1] as expected.
        private Vector2Int RotateOffset(Vector2Int offset, BuildingRotation rotation)
        {
            return rotation switch
            {
                BuildingRotation.North => offset,
                // 90° clockwise: (x,y) -> (Height - 1 - y, x)
                BuildingRotation.East => new Vector2Int(Height - 1 - offset.Y, offset.X),
                // 180°: (x,y) -> (Width - 1 - x, Height - 1 - y)
                BuildingRotation.South => new Vector2Int(Width - 1 - offset.X, Height - 1 - offset.Y),
                // 270° clockwise: (x,y) -> (y, Width - 1 - x)
                BuildingRotation.West => new Vector2Int(offset.Y, Width - 1 - offset.X),
                _ => offset
            };
        }

        private Vector2Int UnrotateOffset(Vector2Int offset, BuildingRotation rotation)
        {
            return rotation switch
            {
                BuildingRotation.North => offset,
                // Inverse of East: (x',y') = (Height - 1 - y, x) => (y' = x, x' = Height - 1 - y) => (y', Height - 1 - x')
                BuildingRotation.East => new Vector2Int(offset.Y, Height - 1 - offset.X),
                // Inverse of South: same as South (180° is self-inverse)
                BuildingRotation.South => new Vector2Int(Width - 1 - offset.X, Height - 1 - offset.Y),
                // Inverse of West: (x',y') = (y, Width - 1 - x) => (x' = y, y' = Width - 1 - x) => (Width - 1 - y', x')
                BuildingRotation.West => new Vector2Int(Width - 1 - offset.Y, offset.X),
                _ => offset
            };
        }

        /// <summary>
        /// Static factory methods for building definitions
        /// </summary>
        public static class Definitions
        {
            // Simple rectangular house with single door
            public static BuildingDefinition House => new(
                BuildingType.House,
                width: 4,
                height: 4,
                layout: new BuildingTile[,]
                {
                    { Wall('┌'), Wall('─'), Wall('─'), Wall('┐') },
                    { Wall('│'), Floor('.'), Floor('.'), Wall('│') },
                    { Wall('│'), Floor('.'), Floor('.'), Wall('│') },
                    { Wall('└'), Door('▒'), Wall('─'), Wall('┘') }
                },
                doorPositions: new List<Vector2Int> { new(1, 3) }
            );

            // Large warehouse - rectangular with double doors
            public static BuildingDefinition Warehouse => new(
                BuildingType.Warehouse,
                width: 6,
                height: 4,
                layout: new BuildingTile[,]
                {
                    { Wall('┌'), Wall('─'), Wall('─'), Wall('─'), Wall('─'), Wall('┐') },  // Top row
                    { Wall('│'), Floor('.'), Floor('.'), Floor('.'), Floor('.'), Wall('│') },  // Middle
                    { Wall('│'), Floor('.'), Floor('.'), Floor('.'), Floor('.'), Wall('│') },  // Middle
                    { Wall('└'), Door('▒'), Door('▒'), Wall('─'), Wall('─'), Wall('┘') }   // Bottom row
                },
                doorPositions: new List<Vector2Int> { new(1, 3), new(2, 3) }
            );

            // Workshop - L-shaped with side entrance
            public static BuildingDefinition Workshop => new(
                BuildingType.Workshop,
                width: 5,
                height: 5,
                layout: new BuildingTile[,]
                {
                    { Wall('┌'), Wall('─'), Wall('─'), Wall('┐'), Empty() },
                    { Wall('│'), Floor('.'), Floor('.'), Wall('│'), Empty() },
                    { Wall('│'), Floor('.'), Floor('.'), Wall('├'), Wall('┐') },
                    { Wall('│'), Floor('.'), Floor('.'), Floor('.'), Wall('│') },
                    { Wall('└'), Door('▒'), Wall('─'), Wall('─'), Wall('┘') }
                },
                doorPositions: new List<Vector2Int> { new(1, 4) }
            );

            // Farm - U-shaped barn with front opening
            public static BuildingDefinition Farm => new(
                BuildingType.Farm,
                width: 6,
                height: 5,
                layout: new BuildingTile[,]
                {
                    { Wall('┌'), Wall('─'), Wall('─'), Wall('─'), Wall('─'), Wall('┐') },
                    { Wall('│'), Floor('.'), Floor('.'), Floor('.'), Floor('.'), Wall('│') },
                    { Wall('│'), Floor('.'), Floor('.'), Floor('.'), Floor('.'), Wall('│') },
                    { Wall('│'), Floor('.'), Floor('.'), Floor('.'), Floor('.'), Wall('│') },
                    { Wall('└'), Door('▒'), Empty(), Empty(), Door('▒'), Wall('┘') }
                },
                doorPositions: new List<Vector2Int> { new(1, 4), new(4, 4) }
            );

            // Mine entrance - small structure
            public static BuildingDefinition Mine => new(
                BuildingType.Mine,
                width: 3,
                height: 3,
                layout: new BuildingTile[,]
                {
                    { Wall('┌'), Wall('─'), Wall('┐') },
                    { Wall('│'), Floor('.'), Wall('│') },
                    { Wall('└'), Door('▒'), Wall('┘') }
                },
                doorPositions: new List<Vector2Int> { new(1, 2) }
            );

            // Lumberyard - open-sided storage
            public static BuildingDefinition Lumberyard => new(
                BuildingType.Lumberyard,
                width: 5,
                height: 4,
                layout: new BuildingTile[,]
                {
                    { Wall('┌'), Wall('─'), Wall('─'), Wall('─'), Wall('┐') },
                    { Wall('│'), Floor('.'), Floor('.'), Floor('.'), Wall('│') },
                    { Wall('│'), Floor('.'), Floor('.'), Floor('.'), Empty() },
                    { Wall('└'), Door('▒'), Wall('─'), Wall('─'), Empty() }
                },
                doorPositions: new List<Vector2Int> { new(1, 3) }
            );

            // Market - large open plaza with multiple entrances
            public static BuildingDefinition Market => new(
                BuildingType.Market,
                width: 7,
                height: 6,
                layout: new BuildingTile[,]
                {
                    { Wall('┌'), Wall('─'), Wall('─'), Wall('─'), Wall('─'), Wall('─'), Wall('┐') },
                    { Wall('│'), Floor('.'), Floor('.'), Floor('.'), Floor('.'), Floor('.'), Wall('│') },
                    { Door('▒'), Floor('.'), Floor('.'), Floor('.'), Floor('.'), Floor('.'), Door('▒') },
                    { Wall('│'), Floor('.'), Floor('.'), Floor('.'), Floor('.'), Floor('.'), Wall('│') },
                    { Wall('│'), Floor('.'), Floor('.'), Floor('.'), Floor('.'), Floor('.'), Wall('│') },
                    { Wall('└'), Door('▒'), Wall('─'), Wall('─'), Wall('─'), Door('▒'), Wall('┘') }
                },
                doorPositions: new List<Vector2Int> { new(0, 2), new(6, 2), new(1, 5), new(5, 5) }
            );

            // Well - tiny single-tile structure
            public static BuildingDefinition Well => new(
                BuildingType.Well,
                width: 1,
                height: 1,
                layout: new BuildingTile[,]
                {
                    { Floor('○') }
                },
                doorPositions: new List<Vector2Int>() // No doors
            );

            // Town Hall - impressive building with central entrance
            public static BuildingDefinition TownHall => new(
                BuildingType.TownHall,
                width: 7,
                height: 6,
                layout: new BuildingTile[,]
                {
                    { Wall('┌'), Wall('─'), Wall('─'), Wall('─'), Wall('─'), Wall('─'), Wall('┐') },
                    { Wall('│'), Floor('.'), Floor('.'), Floor('.'), Floor('.'), Floor('.'), Wall('│') },
                    { Wall('│'), Floor('.'), Wall('┌'), Wall('┐'), Floor('.'), Floor('.'), Wall('│') },
                    { Wall('│'), Floor('.'), Wall('└'), Wall('┘'), Floor('.'), Floor('.'), Wall('│') },
                    { Wall('│'), Floor('.'), Floor('.'), Floor('.'), Floor('.'), Floor('.'), Wall('│') },
                    { Wall('└'), Wall('─'), Door('▒'), Door('▒'), Wall('─'), Wall('─'), Wall('┘') }
                },
                doorPositions: new List<Vector2Int> { new(2, 5), new(3, 5) }
            );

            private static BuildingTile Wall(char glyph) => new(BuildingTileType.Wall, glyph);
            private static BuildingTile Floor(char glyph) => new(BuildingTileType.Floor, glyph);
            private static BuildingTile Door(char glyph) => new(BuildingTileType.Door, glyph);
            private static BuildingTile Empty() => new(BuildingTileType.Empty, ' ');

            public static BuildingDefinition Get(BuildingType type)
            {
                return type switch
                {
                    BuildingType.House => House,
                    BuildingType.Warehouse => Warehouse,
                    BuildingType.Workshop => Workshop,
                    BuildingType.Farm => Farm,
                    BuildingType.Mine => Mine,
                    BuildingType.Lumberyard => Lumberyard,
                    BuildingType.Market => Market,
                    BuildingType.Well => Well,
                    BuildingType.TownHall => TownHall,
                    _ => House
                };
            }
        }
    }

    /// <summary>
    /// Simple 2D integer vector
    /// </summary>
    public struct Vector2Int
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Vector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Vector2Int operator +(Vector2Int a, Vector2Int b) =>
            new(a.X + b.X, a.Y + b.Y);

        public static Vector2Int operator -(Vector2Int a, Vector2Int b) =>
            new(a.X - b.X, a.Y - b.Y);

        public override string ToString() => $"({X}, {Y})";
        
        public override bool Equals(object? obj) => obj is Vector2Int other && X == other.X && Y == other.Y;
        public override int GetHashCode() => HashCode.Combine(X, Y);
    }
}
