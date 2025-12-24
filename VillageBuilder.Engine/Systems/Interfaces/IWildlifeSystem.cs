using VillageBuilder.Engine.Config;
using VillageBuilder.Engine.Entities.Wildlife;

namespace VillageBuilder.Engine.Systems.Interfaces
{
    /// <summary>
    /// Manages wildlife entities, spawning, AI, and ecosystem balance.
    /// </summary>
    public interface IWildlifeSystem
    {
        /// <summary>
        /// All living wildlife entities in the world
        /// </summary>
        List<WildlifeEntity> AllWildlife { get; }
        
        /// <summary>
        /// Wildlife configuration
        /// </summary>
        WildlifeConfig Config { get; }
        
        /// <summary>
        /// Spawn initial wildlife population across the map
        /// </summary>
        void InitializeWildlife(World.VillageGrid grid);

        /// <summary>
        /// Update all wildlife (AI, movement, needs) for one tick
        /// </summary>
        void UpdateWildlife(Core.GameTime time, World.VillageGrid grid);

        /// <summary>
        /// Spawn a new wildlife entity at a specific position
        /// </summary>
        WildlifeEntity? SpawnWildlife(WildlifeType type, int x, int y);

        /// <summary>
        /// Remove dead wildlife and manage population
        /// </summary>
        void CleanupDeadWildlife(World.VillageGrid grid);
        
        /// <summary>
        /// Get wildlife count by type
        /// </summary>
        int GetPopulationCount(WildlifeType type);
        
        /// <summary>
        /// Get total wildlife population
        /// </summary>
        int GetTotalPopulation();
        
        /// <summary>
        /// Check if population is at capacity
        /// </summary>
        bool IsAtCapacity(WildlifeType type);
    }
}
