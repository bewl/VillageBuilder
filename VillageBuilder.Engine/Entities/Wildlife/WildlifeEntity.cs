using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Resources;
using VillageBuilder.Engine.World;
using System;
using System.Collections.Generic;

namespace VillageBuilder.Engine.Entities.Wildlife
{
    /// <summary>
    /// Represents a wildlife animal entity with AI behavior, stats, and ecosystem interactions
    /// </summary>
    public class WildlifeEntity
    {
        // Identity
        public int Id { get; set; }
        public WildlifeType Type { get; set; }
        public string Name { get; set; }
        
        // Position and Movement
        public Vector2Int Position { get; set; }
        public Vector2Int? TargetPosition { get; set; }
        public List<Vector2Int> CurrentPath { get; private set; }
        public int PathIndex { get; private set; }
        
        // AI State
        public WildlifeBehavior CurrentBehavior { get; set; }
        public Vector2Int? HomeTerritory { get; set; }
        public int TerritoryRadius { get; set; }
        
        // Stats
        public int Health { get; set; }
        public int Hunger { get; set; }
        public int Fear { get; set; }
        public int Energy { get; set; }
        public bool IsAlive { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        
        // Behavior Modifiers
        public float Speed { get; set; }
        public int FleeDistance { get; set; }
        public int DetectionRange { get; set; }
        public bool IsHerdAnimal { get; set; }
        
        // Prey/Predator relationships
        public bool IsPrey { get; set; }
        public bool IsPredator { get; set; }
        public List<WildlifeType> PreyTypes { get; set; }
        public List<WildlifeType> PredatorTypes { get; set; }
        
        // Current targets/threats
        public WildlifeEntity? CurrentTarget { get; set; }
        public Person? ThreatPerson { get; set; }
        public WildlifeEntity? ThreatPredator { get; set; }
        
        // Breeding
        public int BreedingCooldown { get; set; }
        public int MaturityAge { get; set; }
        
        // Hunting drops
        public Dictionary<ResourceType, int> ResourceDrops { get; set; }
        
        // Animation state (for rendering)
        public float AnimationTime { get; set; }
        
        private static Random _random = new Random();
        
        public WildlifeEntity(int id, WildlifeType type, Vector2Int position)
        {
            Id = id;
            Type = type;
            Position = position;
            Name = GenerateName(type);
            
            CurrentPath = new List<Vector2Int>();
            PathIndex = 0;
            IsAlive = true;
            AnimationTime = 0f;
            
            PreyTypes = new List<WildlifeType>();
            PredatorTypes = new List<WildlifeType>();
            ResourceDrops = new Dictionary<ResourceType, int>();
            
            // Initialize based on type
            InitializeStats();
        }
        
        private string GenerateName(WildlifeType type)
        {
            var colors = new[] { "Brown", "Gray", "White", "Black", "Red", "Golden", "Spotted", "Dark" };
            var color = colors[_random.Next(colors.Length)];
            return $"{color} {type}";
        }
        
        private void InitializeStats()
        {
            switch (Type)
            {
                case WildlifeType.Rabbit:
                    Health = 30;
                    Speed = 2.0f;
                    FleeDistance = 15;
                    DetectionRange = 8;
                    TerritoryRadius = 20;
                    IsPrey = true;
                    PredatorTypes = new List<WildlifeType> { WildlifeType.Wolf, WildlifeType.Fox };
                    MaturityAge = 15;
                    ResourceDrops[ResourceType.Meat] = 5;
                    break;
                    
                case WildlifeType.Deer:
                    Health = 80;
                    Speed = 1.8f;
                    FleeDistance = 20;
                    DetectionRange = 10;
                    TerritoryRadius = 30;
                    IsPrey = true;
                    IsHerdAnimal = true;
                    PredatorTypes = new List<WildlifeType> { WildlifeType.Wolf, WildlifeType.Bear };
                    MaturityAge = 30;
                    ResourceDrops[ResourceType.Meat] = 20;
                    break;
                    
                case WildlifeType.Boar:
                    Health = 100;
                    Speed = 1.5f;
                    FleeDistance = 10;
                    DetectionRange = 6;
                    TerritoryRadius = 25;
                    IsPrey = true;
                    PredatorTypes = new List<WildlifeType> { WildlifeType.Wolf, WildlifeType.Bear };
                    MaturityAge = 40;
                    ResourceDrops[ResourceType.Meat] = 30;
                    break;
                    
                case WildlifeType.Wolf:
                    Health = 100;
                    Speed = 2.5f;
                    FleeDistance = 5;
                    DetectionRange = 15;
                    TerritoryRadius = 50;
                    IsPredator = true;
                    PreyTypes = new List<WildlifeType> { WildlifeType.Rabbit, WildlifeType.Deer };
                    MaturityAge = 60;
                    break;
                    
                case WildlifeType.Fox:
                    Health = 60;
                    Speed = 2.2f;
                    FleeDistance = 12;
                    DetectionRange = 12;
                    TerritoryRadius = 40;
                    IsPredator = true;
                    PreyTypes = new List<WildlifeType> { WildlifeType.Rabbit, WildlifeType.Bird };
                    MaturityAge = 45;
                    break;
                    
                case WildlifeType.Bear:
                    Health = 150;
                    Speed = 1.3f;
                    FleeDistance = 3;
                    DetectionRange = 12;
                    TerritoryRadius = 60;
                    IsPredator = true;
                    PreyTypes = new List<WildlifeType> { WildlifeType.Deer, WildlifeType.Boar };
                    MaturityAge = 90;
                    ResourceDrops[ResourceType.Meat] = 50;
                    break;
                    
                case WildlifeType.Bird:
                case WildlifeType.Duck:
                case WildlifeType.Turkey:
                    Health = 20;
                    Speed = 1.5f;
                    FleeDistance = 10;
                    DetectionRange = 8;
                    TerritoryRadius = 15;
                    IsPrey = true;
                    PredatorTypes = new List<WildlifeType> { WildlifeType.Fox };
                    MaturityAge = 10;
                    ResourceDrops[ResourceType.Meat] = 3;
                    break;
            }
            
            Energy = 100;
            Hunger = 0;
            Fear = 0;
            Age = _random.Next(MaturityAge / 2, MaturityAge * 2);
            Gender = _random.Next(2) == 0 ? Gender.Male : Gender.Female;
            CurrentBehavior = WildlifeBehavior.Idle;
        }
        
        /// <summary>
        /// Set a path for the wildlife to follow
        /// </summary>
        public void SetPath(List<Vector2Int> path)
        {
            CurrentPath = path ?? new List<Vector2Int>();
            PathIndex = 0;
            
            if (CurrentPath.Count > 0 && CurrentPath[0].X == Position.X && CurrentPath[0].Y == Position.Y)
            {
                PathIndex = 1;
            }
            
            if (PathIndex < CurrentPath.Count)
            {
                TargetPosition = CurrentPath[PathIndex];
            }
        }
        
        /// <summary>
        /// Clear current path
        /// </summary>
        public void ClearPath()
        {
            CurrentPath.Clear();
            PathIndex = 0;
            TargetPosition = null;
        }
        
        /// <summary>
        /// Move along the current path
        /// </summary>
        public bool MoveAlongPath(VillageGrid grid)
        {
            if (CurrentPath.Count == 0 || PathIndex >= CurrentPath.Count)
            {
                TargetPosition = null;
                return false;
            }

            var target = CurrentPath[PathIndex];
            
            if (Position.X == target.X && Position.Y == target.Y)
            {
                PathIndex++;
                
                if (PathIndex < CurrentPath.Count)
                {
                    TargetPosition = CurrentPath[PathIndex];
                    return true;
                }
                else
                {
                    TargetPosition = null;
                    return false;
                }
            }

            // Update tile registrations
            var oldPosition = Position;
            Position = target;
            
            var oldTile = grid.GetTile(oldPosition.X, oldPosition.Y);
            var newTile = grid.GetTile(target.X, target.Y);
            
            if (oldTile != null && oldTile.WildlifeOnTile.Contains(this))
            {
                oldTile.WildlifeOnTile.Remove(this);
            }
            
            if (newTile != null && !newTile.WildlifeOnTile.Contains(this))
            {
                newTile.WildlifeOnTile.Add(this);
            }
            
            PathIndex++;
            
            if (PathIndex < CurrentPath.Count)
            {
                TargetPosition = CurrentPath[PathIndex];
            }
            else
            {
                TargetPosition = null;
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Update wildlife stats each tick
        /// </summary>
        public void UpdateStats()
        {
            if (!IsAlive) return;
            
            // Increase hunger over time
            Hunger = Math.Min(100, Hunger + 1);
            
            // Decrease fear gradually
            Fear = Math.Max(0, Fear - 2);
            
            // Energy management
            if (CurrentBehavior == WildlifeBehavior.Fleeing || CurrentBehavior == WildlifeBehavior.Hunting)
            {
                Energy = Math.Max(0, Energy - 3);
            }
            else if (CurrentBehavior == WildlifeBehavior.Resting || CurrentBehavior == WildlifeBehavior.Idle)
            {
                Energy = Math.Min(100, Energy + 5);
            }
            else if (CurrentBehavior == WildlifeBehavior.Grazing || CurrentBehavior == WildlifeBehavior.Eating)
            {
                Energy = Math.Min(100, Energy + 2);
            }
            
            // Health effects from hunger
            if (Hunger >= 80)
            {
                Health = Math.Max(0, Health - 2);
            }
            else if (Hunger < 30 && Health < 100)
            {
                Health = Math.Min(100, Health + 1);
            }
            
            // Death from health reaching zero
            if (Health <= 0)
            {
                Die();
            }
            
            // Breeding cooldown
            if (BreedingCooldown > 0)
            {
                BreedingCooldown--;
            }
        }
        
        /// <summary>
        /// Animal eats food (reduces hunger)
        /// </summary>
        public void Eat(int amount)
        {
            Hunger = Math.Max(0, Hunger - amount);
        }
        
        /// <summary>
        /// Animal takes damage
        /// </summary>
        public void TakeDamage(int damage, Person? attacker = null, WildlifeEntity? predator = null)
        {
            Health = Math.Max(0, Health - damage);
            Fear = Math.Min(100, Fear + 30);
            
            if (attacker != null)
            {
                ThreatPerson = attacker;
            }
            
            if (predator != null)
            {
                ThreatPredator = predator;
            }
            
            if (Health <= 0)
            {
                Die();
            }
        }
        
        /// <summary>
        /// Kill the animal
        /// </summary>
        public void Die()
        {
            IsAlive = false;
            CurrentBehavior = WildlifeBehavior.Dead;
            ClearPath();
        }
        
        /// <summary>
        /// Check if animal can breed
        /// </summary>
        public bool CanBreed()
        {
            return IsAlive && Age >= MaturityAge && BreedingCooldown == 0 && Hunger < 50 && Energy > 40;
        }
        
        /// <summary>
        /// Check if animal needs to eat
        /// </summary>
        public bool NeedsToEat()
        {
            return IsAlive && Hunger >= 60;
        }
        
        /// <summary>
        /// Check if animal is scared
        /// </summary>
        public bool IsScared()
        {
            return Fear >= 40;
        }
        
        /// <summary>
        /// Check if animal needs to rest
        /// </summary>
        public bool NeedsToRest()
        {
            return IsAlive && Energy < 30;
        }
        
        /// <summary>
        /// Update animation time (for bobbing/movement animation)
        /// </summary>
        public void UpdateAnimation(float deltaTime)
        {
            AnimationTime += deltaTime;
        }
    }
}
