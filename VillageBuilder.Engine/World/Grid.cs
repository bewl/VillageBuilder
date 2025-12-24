using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Entities;

namespace VillageBuilder.Engine.World
{
    /// <summary>
    /// Represents the game world grid containing all tiles.
    /// Manages tile access, generation, and spatial queries.
    /// </summary>
    public class VillageGrid
    {
        public int Width { get; }
        public int Height { get; }
        private Tile[,] _tiles;

        public VillageGrid(int width, int height, int seed)
        {
            Width = width;
            Height = height;
            GenerateMap(seed);
        }

        private void GenerateMap(int seed)
        {
            var generator = new TerrainGenerator(Width, Height, seed);
            generator.InitializePermutation(seed);
            _tiles = generator.Generate();
        }

        public Tile? GetTile(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                return _tiles[x, y];
            return null;
        }

        public IEnumerable<Tile> GetTilesInRadius(int centerX, int centerY, int radius)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                for (int x = centerX - radius; x <= centerX + radius; x++)
                {
                    var tile = GetTile(x, y);
                    if (tile != null)
                        yield return tile;
                }
            }
        }
    }
}