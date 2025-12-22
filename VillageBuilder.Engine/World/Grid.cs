using VillageBuilder.Engine.Buildings;

namespace VillageBuilder.Engine.World
{
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

    public class Tile
    {
        public int X { get; }
        public int Y { get; }
        public TileType Type { get; set; }
        public Building? Building { get; set; }
        public bool IsWalkable => Type != TileType.Water && Type != TileType.Mountain && Building == null;

        public Tile(int x, int y, TileType type)
        {
            X = x;
            Y = y;
            Type = type;
        }
    }

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