using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Entities;
using VillageBuilder.Engine.Entities.Wildlife;
using VillageBuilder.Engine.World;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VillageBuilder.Engine.Systems
{
    /// <summary>
    /// AI system for wildlife behavior - handles decision making and actions for animals
    /// </summary>
    public class WildlifeAI
    {
        private VillageGrid _grid;
        private WildlifeManager _wildlifeManager;
        private Random _random;
        
        public WildlifeAI(VillageGrid grid, WildlifeManager wildlifeManager, Random random)
        {
            _grid = grid;
            _wildlifeManager = wildlifeManager;
            _random = random;
        }
        
        /// <summary>
        /// Update AI for a single wildlife entity
        /// </summary>
        public void UpdateWildlifeAI(WildlifeEntity wildlife, List<Person> allPeople)
        {
            if (!wildlife.IsAlive || wildlife.CurrentBehavior == WildlifeBehavior.Dead)
                return;
            
            // Check for immediate threats
            CheckForThreats(wildlife, allPeople);
            
            // Execute behavior based on current state
            switch (wildlife.CurrentBehavior)
            {
                case WildlifeBehavior.Idle:
                    HandleIdleBehavior(wildlife);
                    break;
                    
                case WildlifeBehavior.Wandering:
                    HandleWanderingBehavior(wildlife);
                    break;
                    
                case WildlifeBehavior.Grazing:
                    HandleGrazingBehavior(wildlife);
                    break;
                    
                case WildlifeBehavior.Fleeing:
                    HandleFleeingBehavior(wildlife);
                    break;
                    
                case WildlifeBehavior.Hunting:
                    HandleHuntingBehavior(wildlife);
                    break;
                    
                case WildlifeBehavior.Eating:
                    HandleEatingBehavior(wildlife);
                    break;
                    
                case WildlifeBehavior.Resting:
                    HandleRestingBehavior(wildlife);
                    break;
                    
                case WildlifeBehavior.Breeding:
                    HandleBreedingBehavior(wildlife);
                    break;
            }
            
            // Move along path if one exists
            if (wildlife.CurrentPath.Count > 0)
            {
                wildlife.MoveAlongPath(_grid);
            }
        }
        
        /// <summary>
        /// Check for nearby threats (people or predators)
        /// </summary>
        private void CheckForThreats(WildlifeEntity wildlife, List<Person> allPeople)
        {
            // Skip if already fleeing or dead
            if (wildlife.CurrentBehavior == WildlifeBehavior.Fleeing || wildlife.CurrentBehavior == WildlifeBehavior.Dead)
                return;
            
            // Check for nearby people (threat to all animals)
            var nearbyPeople = allPeople
                .Where(p => p.IsAlive && GetDistance(wildlife.Position, p.Position) <= wildlife.DetectionRange)
                .ToList();
            
            if (nearbyPeople.Count > 0)
            {
                wildlife.ThreatPerson = nearbyPeople[0];
                wildlife.Fear = Math.Min(100, wildlife.Fear + 20);
                
                if (wildlife.IsScared())
                {
                    StartFleeing(wildlife, nearbyPeople[0].Position);
                    return;
                }
            }
            
            // Check for predators (prey animals only)
            if (wildlife.IsPrey)
            {
                var nearbyPredator = _wildlifeManager.GetNearestPredator(wildlife);
                if (nearbyPredator != null)
                {
                    wildlife.ThreatPredator = nearbyPredator;
                    wildlife.Fear = Math.Min(100, wildlife.Fear + 30);
                    
                    if (wildlife.IsScared())
                    {
                        StartFleeing(wildlife, nearbyPredator.Position);
                        return;
                    }
                }
            }
        }
        
        /// <summary>
        /// Handle idle behavior - decide what to do next
        /// </summary>
        private void HandleIdleBehavior(WildlifeEntity wildlife)
        {
            // Rest if tired
            if (wildlife.NeedsToRest())
            {
                wildlife.CurrentBehavior = WildlifeBehavior.Resting;
                return;
            }
            
            // Hunt if hungry (predators)
            if (wildlife.IsPredator && wildlife.NeedsToEat())
            {
                var prey = _wildlifeManager.GetNearestPrey(wildlife);
                if (prey != null)
                {
                    StartHunting(wildlife, prey);
                    return;
                }
            }
            
            // Graze if hungry (prey animals)
            if (wildlife.IsPrey && wildlife.NeedsToEat())
            {
                wildlife.CurrentBehavior = WildlifeBehavior.Grazing;
                return;
            }
            
            // Try to breed if conditions are right
            if (wildlife.CanBreed() && _random.Next(100) < 5) // 5% chance per tick
            {
                var mate = FindMate(wildlife);
                if (mate != null)
                {
                    StartBreeding(wildlife);
                    return;
                }
            }
            
            // Otherwise, wander around
            if (_random.Next(100) < 10) // 10% chance to start wandering
            {
                StartWandering(wildlife);
            }
        }
        
        /// <summary>
        /// Handle wandering behavior - move randomly within territory
        /// </summary>
        private void HandleWanderingBehavior(WildlifeEntity wildlife)
        {
            // If reached destination, go idle
            if (wildlife.CurrentPath.Count == 0)
            {
                wildlife.CurrentBehavior = WildlifeBehavior.Idle;
                return;
            }
            
            // Continue moving (path movement handled in UpdateWildlifeAI)
        }
        
        /// <summary>
        /// Handle grazing behavior - eat vegetation to reduce hunger
        /// </summary>
        private void HandleGrazingBehavior(WildlifeEntity wildlife)
        {
            wildlife.Eat(10); // Grazing reduces hunger
            
            // Graze for a while
            if (wildlife.Hunger < 30 || _random.Next(100) < 20) // 20% chance to stop
            {
                wildlife.CurrentBehavior = WildlifeBehavior.Idle;
            }
        }
        
        /// <summary>
        /// Handle fleeing behavior - run away from threats
        /// </summary>
        private void HandleFleeingBehavior(WildlifeEntity wildlife)
        {
            // If reached safe distance or threat is gone, stop fleeing
            if (wildlife.CurrentPath.Count == 0)
            {
                wildlife.Fear = Math.Max(0, wildlife.Fear - 10);
                
                if (!wildlife.IsScared())
                {
                    wildlife.CurrentBehavior = WildlifeBehavior.Idle;
                    wildlife.ThreatPerson = null;
                    wildlife.ThreatPredator = null;
                }
                else
                {
                    // Continue fleeing if still scared
                    Vector2Int threatPos = wildlife.ThreatPerson?.Position ?? wildlife.ThreatPredator?.Position ?? wildlife.Position;
                    StartFleeing(wildlife, threatPos);
                }
            }
        }
        
        /// <summary>
        /// Handle hunting behavior - chase and kill prey
        /// </summary>
        private void HandleHuntingBehavior(WildlifeEntity wildlife)
        {
            if (wildlife.CurrentTarget == null || !wildlife.CurrentTarget.IsAlive)
            {
                wildlife.CurrentBehavior = WildlifeBehavior.Idle;
                wildlife.CurrentTarget = null;
                return;
            }
            
            var target = wildlife.CurrentTarget;
            int distance = GetDistance(wildlife.Position, target.Position);
            
            // If close enough, attack
            if (distance <= 1)
            {
                AttackPrey(wildlife, target);
            }
            // If path is empty or target moved, recalculate path
            else if (wildlife.CurrentPath.Count == 0 || 
                     (wildlife.TargetPosition.HasValue && GetDistance(wildlife.TargetPosition.Value, target.Position) > 2))
            {
                var path = Pathfinding.FindPath(wildlife.Position, target.Position, _grid);
                if (path != null && path.Count > 0)
                {
                    wildlife.SetPath(path);
                }
                else
                {
                    // Can't reach target, give up
                    wildlife.CurrentBehavior = WildlifeBehavior.Idle;
                    wildlife.CurrentTarget = null;
                }
            }
        }
        
        /// <summary>
        /// Handle eating behavior - consume killed prey
        /// </summary>
        private void HandleEatingBehavior(WildlifeEntity wildlife)
        {
            wildlife.Eat(15); // Eating reduces hunger more than grazing
            
            // Eat for a while
            if (wildlife.Hunger < 20 || _random.Next(100) < 30) // 30% chance to stop
            {
                wildlife.CurrentBehavior = WildlifeBehavior.Idle;
            }
        }
        
        /// <summary>
        /// Handle resting behavior - recover energy
        /// </summary>
        private void HandleRestingBehavior(WildlifeEntity wildlife)
        {
            // Rest until energy is recovered
            if (wildlife.Energy >= 80)
            {
                wildlife.CurrentBehavior = WildlifeBehavior.Idle;
            }
        }
        
        /// <summary>
        /// Handle breeding behavior
        /// </summary>
        private void HandleBreedingBehavior(WildlifeEntity wildlife)
        {
            // Breeding takes time
            if (_random.Next(100) < 50) // 50% chance to finish breeding
            {
                // Spawn offspring near parent
                var offspringPos = FindNearbyWalkableTile(wildlife.Position, 2);
                if (offspringPos.HasValue)
                {
                    _wildlifeManager.SpawnWildlife(wildlife.Type, offspringPos.Value);
                }
                
                wildlife.BreedingCooldown = 100; // Cooldown before can breed again
                wildlife.CurrentBehavior = WildlifeBehavior.Idle;
            }
        }
        
        /// <summary>
        /// Start fleeing from a threat
        /// </summary>
        private void StartFleeing(WildlifeEntity wildlife, Vector2Int threatPosition)
        {
            // Calculate flee direction (away from threat)
            int dx = wildlife.Position.X - threatPosition.X;
            int dy = wildlife.Position.Y - threatPosition.Y;
            
            // Normalize and extend
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            if (length > 0)
            {
                dx = (int)(dx / length * wildlife.FleeDistance);
                dy = (int)(dy / length * wildlife.FleeDistance);
            }
            else
            {
                // Random direction if on same tile
                dx = _random.Next(-wildlife.FleeDistance, wildlife.FleeDistance);
                dy = _random.Next(-wildlife.FleeDistance, wildlife.FleeDistance);
            }
            
            Vector2Int fleeTarget = new Vector2Int(
                Math.Clamp(wildlife.Position.X + dx, 0, _grid.Width - 1),
                Math.Clamp(wildlife.Position.Y + dy, 0, _grid.Height - 1)
            );
            
            // Find nearest walkable tile to flee target
            var walkableTarget = FindNearbyWalkableTile(fleeTarget, 5);
            if (walkableTarget.HasValue)
            {
                var path = Pathfinding.FindPath(wildlife.Position, walkableTarget.Value, _grid);
                if (path != null && path.Count > 0)
                {
                    wildlife.SetPath(path);
                    wildlife.CurrentBehavior = WildlifeBehavior.Fleeing;
                }
            }
        }
        
        /// <summary>
        /// Start hunting a prey animal
        /// </summary>
        private void StartHunting(WildlifeEntity predator, WildlifeEntity prey)
        {
            predator.CurrentTarget = prey;
            predator.CurrentBehavior = WildlifeBehavior.Hunting;
            
            var path = Pathfinding.FindPath(predator.Position, prey.Position, _grid);
            if (path != null && path.Count > 0)
            {
                predator.SetPath(path);
            }
        }
        
        /// <summary>
        /// Start wandering within territory
        /// </summary>
        private void StartWandering(WildlifeEntity wildlife)
        {
            var homePos = wildlife.HomeTerritory ?? wildlife.Position;
            
            // Pick random destination within territory
            int targetX = homePos.X + _random.Next(-wildlife.TerritoryRadius, wildlife.TerritoryRadius);
            int targetY = homePos.Y + _random.Next(-wildlife.TerritoryRadius, wildlife.TerritoryRadius);
            
            targetX = Math.Clamp(targetX, 0, _grid.Width - 1);
            targetY = Math.Clamp(targetY, 0, _grid.Height - 1);
            
            var target = new Vector2Int(targetX, targetY);
            var walkableTarget = FindNearbyWalkableTile(target, 5);
            
            if (walkableTarget.HasValue)
            {
                var path = Pathfinding.FindPath(wildlife.Position, walkableTarget.Value, _grid);
                if (path != null && path.Count > 0)
                {
                    wildlife.SetPath(path);
                    wildlife.CurrentBehavior = WildlifeBehavior.Wandering;
                }
            }
        }
        
        /// <summary>
        /// Start breeding behavior
        /// </summary>
        private void StartBreeding(WildlifeEntity wildlife)
        {
            wildlife.CurrentBehavior = WildlifeBehavior.Breeding;
        }
        
        /// <summary>
        /// Predator attacks prey
        /// </summary>
        private void AttackPrey(WildlifeEntity predator, WildlifeEntity prey)
        {
            int damage = _random.Next(20, 40); // Variable damage
            prey.TakeDamage(damage, predator: predator);
            
            // If prey died, start eating
            if (!prey.IsAlive)
            {
                predator.CurrentBehavior = WildlifeBehavior.Eating;
                predator.CurrentTarget = null;
                predator.ClearPath();
            }
        }
        
        /// <summary>
        /// Find a potential mate nearby
        /// </summary>
        private WildlifeEntity? FindMate(WildlifeEntity wildlife)
        {
            return _wildlifeManager.GetWildlifeInRange(wildlife.Position, 5)
                .FirstOrDefault(w => w.Type == wildlife.Type && 
                                    w.Gender != wildlife.Gender && 
                                    w.CanBreed());
        }
        
        /// <summary>
        /// Find a nearby walkable tile
        /// </summary>
        private Vector2Int? FindNearbyWalkableTile(Vector2Int center, int searchRadius)
        {
            for (int radius = 0; radius <= searchRadius; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        if (Math.Abs(dx) != radius && Math.Abs(dy) != radius)
                            continue; // Only check perimeter
                        
                        int x = center.X + dx;
                        int y = center.Y + dy;
                        
                        if (x < 0 || x >= _grid.Width || y < 0 || y >= _grid.Height)
                            continue;
                        
                        var tile = _grid.GetTile(x, y);
                        if (tile != null && tile.IsWalkable)
                        {
                            return new Vector2Int(x, y);
                        }
                    }
                }
            }
            
            return null;
        }
        
        private int GetDistance(Vector2Int a, Vector2Int b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
    }
}
