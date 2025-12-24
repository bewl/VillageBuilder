using System;
using System.Collections.Generic;

namespace VillageBuilder.Engine.World
{
    public class TerrainGenerator
    {
        private readonly int _width;
        private readonly int _height;
        private readonly Random _random;
        private float[,] _heightMap;
        private float[,] _moistureMap;

        public TerrainGenerator(int width, int height, int seed)
        {
            _width = width;
            _height = height;
            _random = new Random(seed);
        }

        public Tile[,] Generate()
        {
            // Generate base noise maps
            _heightMap = GeneratePerlinNoise(_width, _height, 8, 0.5f, 2.0f);
            _moistureMap = GeneratePerlinNoise(_width, _height, 6, 0.6f, 1.8f);

            var tiles = new Tile[_width, _height];

            // Generate rivers
            var rivers = GenerateRivers(3);

            // Generate ponds/lakes
            var waterBodies = GeneratePonds(5);

            // Create tiles based on height and moisture
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var height = _heightMap[x, y];
                    var moisture = _moistureMap[x, y];
                    
                    // Check if this is part of a river or pond
                    var isWater = IsWaterTile(x, y, rivers, waterBodies);
                    
                    TileType type;
                    if (isWater)
                    {
                        type = TileType.Water;
                    }
                    else if (height > 0.7f)
                    {
                        type = TileType.Mountain;
                    }
                    else if (height > 0.6f && moisture < 0.4f)
                    {
                        type = TileType.Mountain; // Rocky hills
                    }
                    else if (height < 0.3f && moisture > 0.6f)
                    {
                        type = TileType.Water; // Marshland
                    }
                    else if (moisture > 0.65f && height > 0.35f && height < 0.6f)
                    {
                        type = TileType.Forest; // Dense forest in moist areas
                    }
                    else if (moisture > 0.45f && _random.NextDouble() < 0.3)
                    {
                        type = TileType.Forest; // Scattered trees
                    }
                    else
                    {
                        type = TileType.Grass; // Default grassland
                    }

                    tiles[x, y] = new Tile(x, y, type);
                }
            }

                // Smooth terrain - reduce noise
                SmoothTerrain(tiles);

                // NEW: Add decorations for visual variety and life
                PlaceTerrainDecorations(tiles);

                return tiles;
            }

        private float[,] GeneratePerlinNoise(int width, int height, int octaves, float persistence, float scale)
        {
            var noise = new float[width, height];
            var offsetX = _random.Next(-100000, 100000);
            var offsetY = _random.Next(-100000, 100000);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float amplitude = 1f;
                    float frequency = 1f;
                    float noiseValue = 0f;
                    float maxValue = 0f;

                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x + offsetX) / scale * frequency;
                        float sampleY = (y + offsetY) / scale * frequency;

                        float perlinValue = (float)PerlinNoise(sampleX, sampleY);
                        noiseValue += perlinValue * amplitude;

                        maxValue += amplitude;
                        amplitude *= persistence;
                        frequency *= 2;
                    }

                    noise[x, y] = noiseValue / maxValue;
                }
            }

            return noise;
        }

        private double PerlinNoise(float x, float y)
        {
            int xi = (int)Math.Floor(x);
            int yi = (int)Math.Floor(y);
            
            float xf = x - xi;
            float yf = y - yi;

            float u = Fade(xf);
            float v = Fade(yf);

            int a = Perm(xi) + yi;
            int b = Perm(xi + 1) + yi;

            return Lerp(v,
                Lerp(u, Grad(Perm(a), xf, yf), Grad(Perm(b), xf - 1, yf)),
                Lerp(u, Grad(Perm(a + 1), xf, yf - 1), Grad(Perm(b + 1), xf - 1, yf - 1)));
        }

        private float Fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);
        private float Lerp(float t, float a, float b) => a + t * (b - a);

        private float Grad(int hash, float x, float y)
        {
            int h = hash & 15;
            float u = h < 8 ? x : y;
            float v = h < 4 ? y : h == 12 || h == 14 ? x : 0;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        private int Perm(int x)
        {
            return _permutation[(x & 255)];
        }

        private readonly int[] _permutation = new int[256];

        private List<List<(int x, int y)>> GenerateRivers(int count)
        {
            var rivers = new List<List<(int x, int y)>>();

            for (int i = 0; i < count; i++)
            {
                var river = new List<(int x, int y)>();
                
                // Start from a high point
                int startX = _random.Next(_width);
                int startY = _random.Next(_height / 4); // Start from top area
                
                int x = startX;
                int y = startY;
                
                // Flow downhill
                for (int step = 0; step < _height && y < _height; step++)
                {
                    river.Add((x, y));
                    
                    // Find lowest neighbor
                    float lowestHeight = _heightMap[x, y];
                    int nextX = x;
                    int nextY = y + 1; // Bias towards flowing down

                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = 0; dy <= 1; dy++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;
                            
                            if (nx >= 0 && nx < _width && ny >= 0 && ny < _height)
                            {
                                if (_heightMap[nx, ny] < lowestHeight)
                                {
                                    lowestHeight = _heightMap[nx, ny];
                                    nextX = nx;
                                    nextY = ny;
                                }
                            }
                        }
                    }

                    x = nextX;
                    y = nextY;
                }

                rivers.Add(river);
            }

            return rivers;
        }

        private List<(int x, int y, int radius)> GeneratePonds(int count)
        {
            var ponds = new List<(int x, int y, int radius)>();

            for (int i = 0; i < count; i++)
            {
                int x = _random.Next(10, _width - 10);
                int y = _random.Next(10, _height - 10);
                int radius = _random.Next(2, 6);
                
                // Only place ponds in low-lying areas
                if (_heightMap[x, y] < 0.4f)
                {
                    ponds.Add((x, y, radius));
                }
            }

            return ponds;
        }

        private bool IsWaterTile(int x, int y, List<List<(int x, int y)>> rivers, List<(int x, int y, int radius)> ponds)
        {
            // Check rivers
            foreach (var river in rivers)
            {
                foreach (var (rx, ry) in river)
                {
                    int dist = Math.Abs(x - rx) + Math.Abs(y - ry);
                    if (dist <= 1) return true; // River width
                }
            }

            // Check ponds
            foreach (var (px, py, radius) in ponds)
            {
                int distSq = (x - px) * (x - px) + (y - py) * (y - py);
                if (distSq <= radius * radius) return true;
            }

            return false;
        }

        private void SmoothTerrain(Tile[,] tiles)
        {
            var smoothed = (Tile[,])tiles.Clone();

            for (int x = 1; x < _width - 1; x++)
            {
                for (int y = 1; y < _height - 1; y++)
                {
                    // Count neighbor types
                    var counts = new Dictionary<TileType, int>();
                    
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            var type = tiles[x + dx, y + dy].Type;
                            counts[type] = counts.GetValueOrDefault(type) + 1;
                        }
                    }

                    // If isolated, convert to most common neighbor
                    if (counts[tiles[x, y].Type] <= 2)
                    {
                        var mostCommon = TileType.Grass;
                        int maxCount = 0;
                        
                        foreach (var (type, count) in counts)
                        {
                            if (type != TileType.Water && count > maxCount)
                            {
                                maxCount = count;
                                mostCommon = type;
                            }
                        }

                        smoothed[x, y] = new Tile(x, y, mostCommon);
                    }
                }
            }

            // Copy back
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    tiles[x, y] = smoothed[x, y];
                }
            }
        }

                // Initialize permutation table
                public void InitializePermutation(int seed)
                {
                    var rng = new Random(seed);
                    for (int i = 0; i < 256; i++)
                    {
                        _permutation[i] = i;
                    }

                    // Fisher-Yates shuffle
                    for (int i = 255; i > 0; i--)
                    {
                        int j = rng.Next(i + 1);
                        (_permutation[i], _permutation[j]) = (_permutation[j], _permutation[i]);
                    }
                }

                /// <summary>
                /// Place terrain decorations to add visual variety and life
                /// </summary>
                private void PlaceTerrainDecorations(Tile[,] tiles)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        for (int y = 0; y < _height; y++)
                        {
                            var tile = tiles[x, y];
                            var height = _heightMap[x, y];
                            var moisture = _moistureMap[x, y];

                            // Place decorations based on tile type and environment
                            switch (tile.Type)
                            {
                                case TileType.Grass:
                                    PlaceGrassDecorations(tile, height, moisture);
                                    break;

                                case TileType.Forest:
                                    PlaceForestDecorations(tile, height, moisture);
                                    break;

                                case TileType.Water:
                                    PlaceWaterDecorations(tile);
                                    break;

                                case TileType.Mountain:
                                    PlaceMountainDecorations(tile, height);
                                    break;
                            }
                        }
                    }
                }

                private void PlaceGrassDecorations(Tile tile, float height, float moisture)
                {
                    // Grass tufts (REDUCED for visual clarity: 30% → 8%)
                    if (_random.NextDouble() < 0.08)
                    {
                        tile.Decorations.Add(new TerrainDecoration(
                            DecorationType.GrassTuft, 
                            tile.X, 
                            tile.Y, 
                            _random.Next(4)
                        ));
                    }

                    // Wildflowers (REDUCED for visual clarity: 15% → 5%)
                    if (moisture > 0.5f && _random.NextDouble() < 0.05)
                    {
                        tile.Decorations.Add(new TerrainDecoration(
                            DecorationType.FlowerWild, 
                            tile.X, 
                            tile.Y,
                            _random.Next(4)
                        ));
                    }

                    // Rare flowers (very uncommon)
                    if (_random.NextDouble() < 0.03)
                    {
                        tile.Decorations.Add(new TerrainDecoration(
                            DecorationType.FlowerRare, 
                            tile.X, 
                            tile.Y,
                            _random.Next(3)
                        ));
                    }

                    // Scattered bushes
                    if (moisture > 0.55f && _random.NextDouble() < 0.08)
                    {
                        var bushType = _random.NextDouble() < 0.7 
                            ? DecorationType.BushRegular 
                            : DecorationType.BushBerry;
                        tile.Decorations.Add(new TerrainDecoration(
                            bushType, 
                            tile.X, 
                            tile.Y,
                            _random.Next(3)
                        ));
                    }

                    // Occasional rocks (REDUCED for visual clarity: 5% → 2%)
                    if (_random.NextDouble() < 0.02)
                    {
                        var rockType = _random.NextDouble() < 0.3 
                            ? DecorationType.RockBoulder 
                            : DecorationType.RockPebble;
                        tile.Decorations.Add(new TerrainDecoration(
                            rockType, 
                            tile.X, 
                            tile.Y,
                            _random.Next(3)
                        ));
                    }

                    // Tall grass patches (REDUCED for visual clarity: 12% → 4%)
                    if (moisture > 0.6f && _random.NextDouble() < 0.04)
                    {
                        tile.Decorations.Add(new TerrainDecoration(
                            DecorationType.TallGrass, 
                            tile.X, 
                            tile.Y
                        ));
                    }

                    // Wildlife - rabbits (rare)
                    if (_random.NextDouble() < 0.01)
                    {
                        tile.Decorations.Add(new TerrainDecoration(
                            DecorationType.RabbitSmall, 
                            tile.X, 
                            tile.Y
                        ));
                    }

                    // Wildlife - grazing deer (very rare)
                    if (_random.NextDouble() < 0.005)
                    {
                        tile.Decorations.Add(new TerrainDecoration(
                            DecorationType.DeerGrazing, 
                            tile.X, 
                            tile.Y
                        ));
                    }

                    // Butterflies (uncommon, more in flowery areas)
                    if (moisture > 0.5f && _random.NextDouble() < 0.08)
                    {
                        tile.Decorations.Add(new TerrainDecoration(
                            DecorationType.Butterfly, 
                            tile.X, 
                            tile.Y,
                            _random.Next(3)
                        ));
                    }
                }

                private void PlaceForestDecorations(Tile tile, float height, float moisture)
                {
                    // Dense trees (Forest tiles should have trees!)
                    if (_random.NextDouble() < 0.7)
                    {
                        var treeType = moisture > 0.6f 
                            ? DecorationType.TreeOak 
                            : DecorationType.TreePine;

                        tile.Decorations.Add(new TerrainDecoration(
                            treeType, 
                            tile.X, 
                            tile.Y,
                            _random.Next(2)
                        ));
                    }

                    // Undergrowth
                    if (_random.NextDouble() < 0.4)
                    {
                        tile.Decorations.Add(new TerrainDecoration(
                            DecorationType.Fern, 
                            tile.X, 
                            tile.Y
                        ));
                    }

                    // Bushes in forest
                    if (_random.NextDouble() < 0.3)
                    {
                        var bushType = _random.NextDouble() < 0.5 
                            ? DecorationType.BushRegular 
                            : DecorationType.BushBerry;
                        tile.Decorations.Add(new TerrainDecoration(
                            bushType, 
                            tile.X, 
                            tile.Y,
                            _random.Next(3)
                        ));
                    }

                    // Mushrooms
                    if (_random.NextDouble() < 0.15)
                    {
                        tile.Decorations.Add(new TerrainDecoration(
                            DecorationType.Mushroom, 
                            tile.X, 
                            tile.Y,
                            _random.Next(3)
                        ));
                    }

                    // Old stumps
                    if (_random.NextDouble() < 0.05)
                    {
                        tile.Decorations.Add(new TerrainDecoration(
                            DecorationType.StumpOld, 
                            tile.X, 
                            tile.Y
                        ));
                    }

                    // Fallen logs
                    if (_random.NextDouble() < 0.08)
                    {
                        tile.Decorations.Add(new TerrainDecoration(
                            DecorationType.LogFallen, 
                            tile.X, 
                            tile.Y
                        ));
                    }

                    // Dead trees (rare)
                    if (_random.NextDouble() < 0.03)
                    {
                        tile.Decorations.Add(new TerrainDecoration(
                            DecorationType.TreeDead, 
                            tile.X, 
                            tile.Y
                        ));
                    }

                    // Birds perched on trees
                    if (_random.NextDouble() < 0.1)
                    {
                        tile.Decorations.Add(new TerrainDecoration(
                            DecorationType.BirdPerched, 
                            tile.X, 
                            tile.Y
                        ));
                    }

                    // Flying birds
                    if (_random.NextDouble() < 0.05)
                    {
                        tile.Decorations.Add(new TerrainDecoration(
                            DecorationType.BirdFlying, 
                            tile.X, 
                            tile.Y,
                            _random.Next(3)
                        ));
                    }
                }

                private void PlaceWaterDecorations(Tile tile)
                {
                    // Reeds along edges (check if near land)
                    bool nearLand = false;
                    for (int dx = -1; dx <= 1 && !nearLand; dx++)
                    {
                        for (int dy = -1; dy <= 1 && !nearLand; dy++)
                        {
                            int nx = tile.X + dx;
                            int ny = tile.Y + dy;
                            if (nx >= 0 && nx < _width && ny >= 0 && ny < _height)
                            {
                                if (_heightMap[nx, ny] > 0.3f) // Not water
                                {
                                    nearLand = true;
                                }
                            }
                        }
                    }

                    if (nearLand && _random.NextDouble() < 0.3)
                    {
                        tile.Decorations.Add(new TerrainDecoration(
                            DecorationType.Reeds, 
                            tile.X, 
                            tile.Y
                        ));
                    }

                    // Fish swimming
                    if (_random.NextDouble() < 0.1)
                    {
                        tile.Decorations.Add(new TerrainDecoration(
                            DecorationType.FishInWater, 
                            tile.X, 
                            tile.Y
                        ));
                    }
                }

                private void PlaceMountainDecorations(Tile tile, float height)
                {
                    // Rocks and boulders (common on mountains)
                    if (_random.NextDouble() < 0.4)
                    {
                        tile.Decorations.Add(new TerrainDecoration(
                            DecorationType.RockBoulder, 
                            tile.X, 
                            tile.Y,
                            _random.Next(3)
                        ));
                    }

                    // Scattered pine trees on lower slopes
                    if (height < 0.75f && _random.NextDouble() < 0.2)
                    {
                        tile.Decorations.Add(new TerrainDecoration(
                            DecorationType.TreePine, 
                            tile.X, 
                            tile.Y,
                            _random.Next(2)
                        ));
                    }

                    // Birds of prey
                    if (_random.NextDouble() < 0.05)
                    {
                        tile.Decorations.Add(new TerrainDecoration(
                            DecorationType.BirdFlying, 
                            tile.X, 
                            tile.Y,
                            0  // Eagles/hawks variant
                        ));
                    }
                }
            }
        }
