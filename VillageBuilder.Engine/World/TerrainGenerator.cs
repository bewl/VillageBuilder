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
    }
}
