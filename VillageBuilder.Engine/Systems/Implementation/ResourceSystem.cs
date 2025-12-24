using VillageBuilder.Engine.Resources;
using VillageBuilder.Engine.Systems.Interfaces;

namespace VillageBuilder.Engine.Systems.Implementation
{
    /// <summary>
    /// Default implementation of IResourceSystem.
    /// Manages village resource stockpile.
    /// </summary>
    public class ResourceSystem : IResourceSystem
    {
        public Dictionary<ResourceType, int> Resources { get; private set; }
        
        public ResourceSystem()
        {
            Resources = new Dictionary<ResourceType, int>();
            
            // Initialize all resource types to 0
            foreach (ResourceType type in Enum.GetValues<ResourceType>())
            {
                Resources[type] = 0;
            }
        }
        
        public bool TryConsumeResources(Dictionary<ResourceType, int> cost)
        {
            // Check if we have enough of all resources
            if (!HasResources(cost))
            {
                return false;
            }
            
            // Consume resources
            foreach (var kvp in cost)
            {
                Resources[kvp.Key] -= kvp.Value;
            }
            
            return true;
        }
        
        public void AddResources(ResourceType type, int amount)
        {
            if (!Resources.ContainsKey(type))
            {
                Resources[type] = 0;
            }
            
            Resources[type] += amount;
        }
        
        public void AddResources(Dictionary<ResourceType, int> resources)
        {
            foreach (var kvp in resources)
            {
                AddResources(kvp.Key, kvp.Value);
            }
        }
        
        public bool HasResources(Dictionary<ResourceType, int> cost)
        {
            foreach (var kvp in cost)
            {
                if (GetResourceAmount(kvp.Key) < kvp.Value)
                {
                    return false;
                }
            }
            return true;
        }
        
        public int GetResourceAmount(ResourceType type)
        {
            return Resources.TryGetValue(type, out int amount) ? amount : 0;
        }
        
        public void SetResourceAmount(ResourceType type, int amount)
        {
            Resources[type] = Math.Max(0, amount);
        }
    }
}
