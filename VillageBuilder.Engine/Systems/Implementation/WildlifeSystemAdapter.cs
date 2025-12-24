using VillageBuilder.Engine.Config;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Entities.Wildlife;
using VillageBuilder.Engine.Systems.Interfaces;
using VillageBuilder.Engine.World;
using VillageBuilder.Engine.Buildings; // For Vector2Int

namespace VillageBuilder.Engine.Systems.Implementation
{
    /// <summary>
    /// Wildlife system implementation wrapping existing WildlifeManager.
    /// Manages wildlife entities, AI, and ecosystem balance.
    /// </summary>
    public class WildlifeSystemAdapter : IWildlifeSystem
    {
        private readonly WildlifeManager _manager;
        private readonly VillageGrid _grid;

        public List<WildlifeEntity> AllWildlife => _manager.Wildlife;
        public WildlifeConfig Config { get; private set; }

        public WildlifeSystemAdapter(VillageGrid grid, int seed, WildlifeConfig? config = null)
        {
            Config = config ?? GameConfig.Instance.Wildlife;
            _grid = grid;
            _manager = new WildlifeManager(grid, seed);
        }

        public void InitializeWildlife(VillageGrid grid)
        {
            // Use config initial populations
            foreach (var kvp in Config.InitialPopulation)
            {
                for (int i = 0; i < kvp.Value; i++)
                {
                    _manager.SpawnWildlife(kvp.Key);
                }
            }
        }

        public void UpdateWildlife(GameTime time, VillageGrid grid)
        {
            _manager.UpdateWildlife(); // Takes no arguments
        }

        public WildlifeEntity? SpawnWildlife(WildlifeType type, int x, int y)
        {
            // Find a suitable spawn position near the requested location
            var tile = _grid.GetTile(x, y);
            if (tile != null && tile.IsWalkable) // Property, not method
            {
                var wildlife = new WildlifeEntity(_manager.Wildlife.Count + 1, type, new Vector2Int(x, y));
                _manager.Wildlife.Add(wildlife);
                return wildlife;
            }
            return null;
        }

        public void CleanupDeadWildlife(VillageGrid grid)
        {
            // Remove dead wildlife from list
            _manager.Wildlife.RemoveAll(w => !w.IsAlive);
        }

        public int GetPopulationCount(WildlifeType type)
        {
            return AllWildlife.Count(w => w.Type == type && w.IsAlive);
        }

        public int GetTotalPopulation()
        {
            return AllWildlife.Count(w => w.IsAlive);
        }

        public bool IsAtCapacity(WildlifeType type)
        {
            return GetPopulationCount(type) >= Config.MaxPopulationPerType;
        }
    }
}
