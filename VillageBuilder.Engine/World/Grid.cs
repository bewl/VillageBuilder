using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Entities;

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
        public List<Person> PeopleOnTile { get; } // Track all people on this tile
        
        public bool IsWalkable
        {
            get
            {
                // Water and mountains are never walkable
                if (Type == TileType.Water || Type == TileType.Mountain)
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

        public Tile(int x, int y, TileType type)
        {
            X = x;
            Y = y;
            Type = type;
            PeopleOnTile = new List<Person>();
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