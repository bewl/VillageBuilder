using VillageBuilder.Engine.Config;
using VillageBuilder.Engine.Resources;

namespace VillageBuilder.Engine.Systems.Interfaces
{
    /// <summary>
    /// Manages village resources (wood, stone, food, etc.)
    /// Handles resource production, consumption, and storage.
    /// </summary>
    public interface IResourceSystem
    {
        /// <summary>
        /// Current resource stockpile
        /// </summary>
        Dictionary<ResourceType, int> Resources { get; }
        
        /// <summary>
        /// Try to consume resources. Returns true if successful, false if insufficient.
        /// </summary>
        bool TryConsumeResources(Dictionary<ResourceType, int> cost);
        
        /// <summary>
        /// Add resources to the stockpile
        /// </summary>
        void AddResources(ResourceType type, int amount);
        
        /// <summary>
        /// Add multiple resources at once
        /// </summary>
        void AddResources(Dictionary<ResourceType, int> resources);
        
        /// <summary>
        /// Check if we have enough resources for a cost
        /// </summary>
        bool HasResources(Dictionary<ResourceType, int> cost);
        
        /// <summary>
        /// Get amount of a specific resource
        /// </summary>
        int GetResourceAmount(ResourceType type);
        
        /// <summary>
        /// Set resource amount directly (for loading saves, cheats, etc.)
        /// </summary>
        void SetResourceAmount(ResourceType type, int amount);
    }
}
