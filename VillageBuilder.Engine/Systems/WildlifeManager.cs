using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Entities.Wildlife;
using VillageBuilder.Engine.World;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VillageBuilder.Engine.Systems
{
    /// <summary>
    /// Manages wildlife population, spawning, and ecosystem balance
    /// </summary>
    public class WildlifeManager
    {
        private List<WildlifeEntity> _wildlife;
        private int _nextWildlifeId = 1;
        private Random _random;
        private VillageGrid _grid;
        
        // Population limits and balance
        private const int MAX_WILDLIFE = 150;
        private const int MIN_PREY_POPULATION = 20;
        private const int MAX_PREDATOR_RATIO = 5; // 1 predator per 5 prey
        
        // Tracking tiles with wildlife for efficient clearing
        private readonly HashSet<(int x, int y)> _wildlifeOccupiedTiles = new HashSet<(int x, int y)>();
        
        public List<WildlifeEntity> Wildlife => _wildlife;
        public int WildlifeCount => _wildlife.Count(w => w.IsAlive);
        
        public WildlifeManager(VillageGrid grid, int seed)
        {
            _wildlife = new List<WildlifeEntity>();
            _grid = grid;
            _random = new Random(seed);
        }
        
        /// <summary>
        /// Initialize the world with starting wildlife population
        /// </summary>
        public void InitializeWildlife()
        {
            SpawnInitialPrey();
            SpawnInitialPredators();
        }
        
        private void SpawnInitialPrey()
        {
            // Rabbits - common, fast breeding
            for (int i = 0; i < 30; i++)
            {
                SpawnWildlife(WildlifeType.Rabbit);
            }
            
            // Deer - moderate population
            for (int i = 0; i < 15; i++)
            {
                SpawnWildlife(WildlifeType.Deer);
            }
            
            // Boar - less common
            for (int i = 0; i < 8; i++)
            {
                SpawnWildlife(WildlifeType.Boar);
            }
            
            // Birds - common in grasslands
            for (int i = 0; i < 20; i++)
            {
                SpawnWildlife(WildlifeType.Bird);
            }
            
            // Ducks and Turkeys - less common
            for (int i = 0; i < 10; i++)
            {
                SpawnWildlife(_random.Next(2) == 0 ? WildlifeType.Duck : WildlifeType.Turkey);
            }
        }
        
        private void SpawnInitialPredators()
        {
            // Foxes - moderate predator
            for (int i = 0; i < 5; i++)
            {
                SpawnWildlife(WildlifeType.Fox);
            }
            
            // Wolves - pack hunters
            for (int i = 0; i < 3; i++)
            {
                SpawnWildlife(WildlifeType.Wolf);
            }
            
            // Bears - apex predators, rare
            for (int i = 0; i < 2; i++)
            {
                SpawnWildlife(WildlifeType.Bear);
            }
        }
        
        /// <summary>
        /// Spawn a new wildlife entity at a suitable location
        /// </summary>
        public WildlifeEntity? SpawnWildlife(WildlifeType type, Vector2Int? position = null)
        {
            if (_wildlife.Count >= MAX_WILDLIFE)
                return null;
            
            // Find suitable spawn location
            Vector2Int spawnPos = position ?? FindSuitableSpawnLocation(type);
            
            var wildlife = new WildlifeEntity(_nextWildlifeId++, type, spawnPos);
            wildlife.HomeTerritory = spawnPos;
            
            _wildlife.Add(wildlife);
            
            // Register on tile
            var tile = _grid.GetTile(spawnPos.X, spawnPos.Y);
            if (tile != null)
            {
                tile.WildlifeOnTile.Add(wildlife);
                _wildlifeOccupiedTiles.Add((spawnPos.X, spawnPos.Y));
            }
            
            return wildlife;
        }
        
        /// <summary>
        /// Find a suitable spawn location based on animal type and terrain preferences
        /// </summary>
        private Vector2Int FindSuitableSpawnLocation(WildlifeType type)
        {
            // Try to find appropriate terrain for animal type
            for (int attempts = 0; attempts < 100; attempts++)
            {
                int x = _random.Next(_grid.Width);
                int y = _random.Next(_grid.Height);
                
                var tile = _grid.GetTile(x, y);
                if (tile == null || !tile.IsWalkable) continue;
                
                // Check terrain suitability
                bool suitable = type switch
                {
                    WildlifeType.Rabbit or WildlifeType.Deer => 
                        tile.Type == TileType.Grass || tile.Type == TileType.Forest,
                    WildlifeType.Boar => 
                        tile.Type == TileType.Forest,
                    WildlifeType.Wolf or WildlifeType.Bear => 
                        tile.Type == TileType.Forest || tile.Type == TileType.Mountain,
                    WildlifeType.Fox => 
                        tile.Type == TileType.Grass || tile.Type == TileType.Forest,
                    WildlifeType.Bird or WildlifeType.Turkey => 
                        tile.Type == TileType.Grass,
                    WildlifeType.Duck => 
                        tile.Type == TileType.Grass || tile.Type == TileType.Water,
                    _ => false
                };
                
                if (suitable)
                {
                    return new Vector2Int(x, y);
                }
            }
            
            // Fallback: random walkable tile
            for (int attempts = 0; attempts < 50; attempts++)
            {
                int x = _random.Next(_grid.Width);
                int y = _random.Next(_grid.Height);
                var tile = _grid.GetTile(x, y);
                if (tile != null && tile.IsWalkable)
                {
                    return new Vector2Int(x, y);
                }
            }
            
            // Last resort: center of map
            return new Vector2Int(_grid.Width / 2, _grid.Height / 2);
        }
        
        /// <summary>
        /// Update all wildlife - called every tick from GameEngine
        /// </summary>
        public void UpdateWildlife()
        {
            // Clear tile registrations efficiently
            foreach (var (x, y) in _wildlifeOccupiedTiles)
            {
                var tile = _grid.GetTile(x, y);
                if (tile != null)
                {
                    tile.WildlifeOnTile.Clear();
                }
            }
            _wildlifeOccupiedTiles.Clear();
            
            // Update each wildlife entity
            var aliveWildlife = _wildlife.Where(w => w.IsAlive).ToList();
            foreach (var wildlife in aliveWildlife)
            {
                wildlife.UpdateStats();
                
                // Register on current tile
                var tile = _grid.GetTile(wildlife.Position.X, wildlife.Position.Y);
                if (tile != null)
                {
                    tile.WildlifeOnTile.Add(wildlife);
                    _wildlifeOccupiedTiles.Add((wildlife.Position.X, wildlife.Position.Y));
                }
            }
            
            // Remove dead wildlife periodically (corpses decay)
            _wildlife.RemoveAll(w => !w.IsAlive && _random.Next(100) < 5); // 5% chance per tick to remove corpse
        }
        
        /// <summary>
        /// Check ecosystem balance and spawn/cull as needed
        /// </summary>
        public void BalanceEcosystem()
        {
            var aliveWildlife = _wildlife.Where(w => w.IsAlive).ToList();
            
            int preyCount = aliveWildlife.Count(w => w.IsPrey);
            int predatorCount = aliveWildlife.Count(w => w.IsPredator);
            
            // Maintain minimum prey population
            if (preyCount < MIN_PREY_POPULATION && _wildlife.Count < MAX_WILDLIFE)
            {
                // Spawn more prey animals
                SpawnWildlife(WildlifeType.Rabbit);
            }
            
            // Too many predators for prey population
            if (predatorCount > preyCount / MAX_PREDATOR_RATIO && predatorCount > 3)
            {
                // Natural predator die-off (starvation)
                var predators = aliveWildlife.Where(w => w.IsPredator && w.Hunger > 70).ToList();
                if (predators.Count > 0)
                {
                    var victim = predators[_random.Next(predators.Count)];
                    victim.TakeDamage(victim.Health); // Kill from starvation
                }
            }
        }
        
        /// <summary>
        /// Get all wildlife within a certain radius of a position
        /// </summary>
        public List<WildlifeEntity> GetWildlifeInRange(Vector2Int position, int range)
        {
            return _wildlife
                .Where(w => w.IsAlive)
                .Where(w => GetDistance(position, w.Position) <= range)
                .ToList();
        }
        
        /// <summary>
        /// Get the nearest wildlife of a specific type
        /// </summary>
        public WildlifeEntity? GetNearestWildlife(Vector2Int position, WildlifeType? type = null)
        {
            var candidates = _wildlife.Where(w => w.IsAlive);
            
            if (type.HasValue)
            {
                candidates = candidates.Where(w => w.Type == type.Value);
            }
            
            return candidates
                .OrderBy(w => GetDistance(position, w.Position))
                .FirstOrDefault();
        }
        
        /// <summary>
        /// Get the nearest prey animal for a predator
        /// </summary>
        public WildlifeEntity? GetNearestPrey(WildlifeEntity predator)
        {
            if (!predator.IsPredator || predator.PreyTypes.Count == 0)
                return null;
            
            return _wildlife
                .Where(w => w.IsAlive && predator.PreyTypes.Contains(w.Type))
                .Where(w => GetDistance(predator.Position, w.Position) <= predator.DetectionRange)
                .OrderBy(w => GetDistance(predator.Position, w.Position))
                .FirstOrDefault();
        }
        
        /// <summary>
        /// Get the nearest predator that threatens a prey animal
        /// </summary>
        public WildlifeEntity? GetNearestPredator(WildlifeEntity prey)
        {
            if (!prey.IsPrey || prey.PredatorTypes.Count == 0)
                return null;
            
            return _wildlife
                .Where(w => w.IsAlive && prey.PredatorTypes.Contains(w.Type))
                .Where(w => GetDistance(prey.Position, w.Position) <= prey.DetectionRange)
                .OrderBy(w => GetDistance(prey.Position, w.Position))
                .FirstOrDefault();
        }
        
        /// <summary>
        /// Remove a wildlife entity (killed by hunting, etc.)
        /// </summary>
        public void RemoveWildlife(WildlifeEntity wildlife)
        {
            wildlife.Die();
            
            // Remove from tile immediately
            var tile = _grid.GetTile(wildlife.Position.X, wildlife.Position.Y);
            if (tile != null)
            {
                tile.WildlifeOnTile.Remove(wildlife);
            }
        }
        
        /// <summary>
        /// Get wildlife population statistics
        /// </summary>
        public Dictionary<WildlifeType, int> GetPopulationStats()
        {
            var stats = new Dictionary<WildlifeType, int>();
            
            foreach (WildlifeType type in Enum.GetValues(typeof(WildlifeType)))
            {
                stats[type] = _wildlife.Count(w => w.IsAlive && w.Type == type);
            }
            
            return stats;
        }
        
        private int GetDistance(Vector2Int a, Vector2Int b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
    }
}
