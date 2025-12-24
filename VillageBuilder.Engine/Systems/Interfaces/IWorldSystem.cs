using VillageBuilder.Engine.Config;
using VillageBuilder.Engine.World;

namespace VillageBuilder.Engine.Systems.Interfaces
{
    /// <summary>
    /// Manages the world grid, terrain, and spatial queries.
    /// Handles world generation, tile access, and pathfinding.
    /// </summary>
    public interface IWorldSystem
    {
        /// <summary>
        /// The world grid containing all tiles
        /// </summary>
        VillageGrid Grid { get; }
        
        /// <summary>
        /// World width in tiles
        /// </summary>
        int Width { get; }
        
        /// <summary>
        /// World height in tiles
        /// </summary>
        int Height { get; }
        
        /// <summary>
        /// Generate a new world with given seed
        /// </summary>
        void GenerateWorld(int width, int height, int seed, TerrainConfig? config = null);
        
        /// <summary>
        /// Get tile at position, or null if out of bounds
        /// </summary>
        Tile? GetTile(int x, int y);
        
        /// <summary>
        /// Check if position is within world bounds
        /// </summary>
        bool IsInBounds(int x, int y);
        
        /// <summary>
        /// Find a path between two positions
        /// </summary>
        List<(int X, int Y)>? FindPath(int startX, int startY, int endX, int endY);
        
        /// <summary>
        /// Get all tiles within a radius
        /// </summary>
        List<Tile> GetTilesInRadius(int centerX, int centerY, int radius);
    }
}
