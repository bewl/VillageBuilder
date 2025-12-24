using VillageBuilder.Engine.Config;
using VillageBuilder.Engine.Systems.Interfaces;
using VillageBuilder.Engine.World;

namespace VillageBuilder.Engine.Systems.Implementation
{
    /// <summary>
    /// World system adapter wrapping existing Grid management.
    /// </summary>
    public class WorldSystemAdapter : IWorldSystem
    {
        public VillageGrid Grid { get; private set; }
        public int Width => Grid.Width;
        public int Height => Grid.Height;

        public WorldSystemAdapter(VillageGrid grid)
        {
            Grid = grid;
        }

        public void GenerateWorld(int width, int height, int seed, TerrainConfig? config = null)
        {
            Grid = new VillageGrid(width, height, seed);
        }

        public Tile? GetTile(int x, int y)
        {
            return Grid.GetTile(x, y);
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public List<(int X, int Y)>? FindPath(int startX, int startY, int endX, int endY)
        {
            // VillageGrid doesn't have FindPath - this would need PathfindingSystem
            // For now, return null (not implemented)
            return null;
        }

        public List<Tile> GetTilesInRadius(int centerX, int centerY, int radius)
        {
            return Grid.GetTilesInRadius(centerX, centerY, radius).ToList();
        }
    }
}
