using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Entities;
using VillageBuilder.Engine.Entities.Wildlife;
using VillageBuilder.Engine.Resources;
using VillageBuilder.Engine.World;
using VillageBuilder.Engine.Core;
using System;
using System.Linq;

namespace VillageBuilder.Engine.Systems
{
    /// <summary>
    /// Handles hunting mechanics - villagers hunting wildlife for food
    /// </summary>
    public class HuntingSystem
    {
        private VillageGrid _grid;
        private WildlifeManager _wildlifeManager;
        private ResourceInventory _villageResources;
        private Random _random;
        
        public HuntingSystem(VillageGrid grid, WildlifeManager wildlifeManager, ResourceInventory villageResources, Random random)
        {
            _grid = grid;
            _wildlifeManager = wildlifeManager;
            _villageResources = villageResources;
            _random = random;
        }
        
        /// <summary>
        /// Attempt to hunt a wildlife animal
        /// </summary>
        public HuntingResult TryHunt(Person hunter, WildlifeEntity target)
        {
            if (!hunter.IsAlive || !target.IsAlive)
            {
                return new HuntingResult 
                { 
                    Success = false, 
                    Message = "Hunter or target is not alive" 
                };
            }
            
            // Check if hunter is close enough
            int distance = GetDistance(hunter.Position, target.Position);
            if (distance > 1)
            {
                return new HuntingResult 
                { 
                    Success = false, 
                    Message = $"{hunter.FirstName} is too far from the {target.Type}" 
                };
            }
            
            // Calculate hunting success chance based on animal type
            int successChance = CalculateHuntingChance(target.Type);
            int roll = _random.Next(100);
            
            if (roll < successChance)
            {
                // Successful hunt - kill the animal
                target.TakeDamage(target.Health, attacker: hunter);
                
                // Collect resources from the kill
                var resources = CollectResourcesFromKill(target);
                
                // Add resources to village storage
                foreach (var kvp in resources)
                {
                    _villageResources.Add(kvp.Key, kvp.Value);
                }
                
                // Remove the dead animal
                _wildlifeManager.RemoveWildlife(target);
                
                // Log the successful hunt
                EventLog.Instance.AddMessage(
                    $"{hunter.FirstName} {hunter.LastName} successfully hunted a {target.Type} (+{resources.Sum(r => r.Value)} resources)",
                    LogLevel.Success);
                
                return new HuntingResult 
                { 
                    Success = true, 
                    Message = $"Successful hunt! Gained {FormatResources(resources)}",
                    ResourcesGained = resources
                };
            }
            else
            {
                // Failed hunt - animal escapes
                target.Fear = 100; // Maximum fear
                target.ThreatPerson = hunter;
                
                // Animal flees
                EventLog.Instance.AddMessage(
                    $"{hunter.FirstName} {hunter.LastName} failed to hunt the {target.Type} - it escaped!",
                    LogLevel.Warning);
                
                return new HuntingResult 
                { 
                    Success = false, 
                    Message = $"The {target.Type} escaped!" 
                };
            }
        }
        
        /// <summary>
        /// Calculate hunting success chance based on animal type
        /// </summary>
        private int CalculateHuntingChance(WildlifeType type)
        {
            return type switch
            {
                WildlifeType.Rabbit => 80,    // Small, easy to catch
                WildlifeType.Bird => 70,       // Quick but catchable
                WildlifeType.Duck => 75,       // Moderate difficulty
                WildlifeType.Turkey => 65,     // Bigger, harder to catch
                WildlifeType.Fox => 50,        // Fast and cunning
                WildlifeType.Deer => 45,       // Fast and alert
                WildlifeType.Boar => 40,       // Dangerous, fights back
                WildlifeType.Wolf => 25,       // Very dangerous
                WildlifeType.Bear => 15,       // Extremely dangerous
                _ => 50
            };
        }
        
        /// <summary>
        /// Collect resources from a killed animal
        /// </summary>
        private System.Collections.Generic.Dictionary<ResourceType, int> CollectResourcesFromKill(WildlifeEntity wildlife)
        {
            var resources = new System.Collections.Generic.Dictionary<ResourceType, int>();
            
            // Copy the animal's resource drops
            foreach (var kvp in wildlife.ResourceDrops)
            {
                resources[kvp.Key] = kvp.Value;
            }
            
            // Predators drop hides
            if (wildlife.IsPredator && !resources.ContainsKey(ResourceType.Fur))
            {
                resources[ResourceType.Fur] = wildlife.Type switch
                {
                    WildlifeType.Fox => 3,
                    WildlifeType.Wolf => 5,
                    WildlifeType.Bear => 8,
                    _ => 2
                };
            }
            
            return resources;
        }
        
        /// <summary>
        /// Format resources dictionary for display
        /// </summary>
        private string FormatResources(System.Collections.Generic.Dictionary<ResourceType, int> resources)
        {
            return string.Join(", ", resources.Select(r => $"{r.Value} {r.Key}"));
        }
        
        /// <summary>
        /// Get the nearest huntable wildlife to a person
        /// </summary>
        public WildlifeEntity? GetNearestHuntableWildlife(Person hunter, int maxDistance = 10)
        {
            return _wildlifeManager.Wildlife
                .Where(w => w.IsAlive)
                .Where(w => GetDistance(hunter.Position, w.Position) <= maxDistance)
                .OrderBy(w => GetDistance(hunter.Position, w.Position))
                .FirstOrDefault();
        }
        
        /// <summary>
        /// Check if overhunting is occurring and issue warning
        /// </summary>
        public void CheckOverhuntingWarning()
        {
            var stats = _wildlifeManager.GetPopulationStats();
            int totalPrey = stats.Where(s => IsPreyType(s.Key)).Sum(s => s.Value);
            
            if (totalPrey < 10) // Very low prey population
            {
                EventLog.Instance.AddMessage(
                    "Warning: Wildlife population is critically low due to overhunting!",
                    LogLevel.Error);
            }
            else if (totalPrey < 20) // Low prey population
            {
                EventLog.Instance.AddMessage(
                    "Wildlife population is declining - consider reducing hunting",
                    LogLevel.Warning);
            }
        }
        
        private bool IsPreyType(WildlifeType type)
        {
            return type == WildlifeType.Rabbit ||
                   type == WildlifeType.Deer ||
                   type == WildlifeType.Boar ||
                   type == WildlifeType.Bird ||
                   type == WildlifeType.Duck ||
                   type == WildlifeType.Turkey;
        }
        
        private int GetDistance(Vector2Int a, Vector2Int b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
    }
    
    /// <summary>
    /// Result of a hunting attempt
    /// </summary>
    public class HuntingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public System.Collections.Generic.Dictionary<ResourceType, int>? ResourcesGained { get; set; }
    }
}
