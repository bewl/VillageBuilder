using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.World;

namespace VillageBuilder.Engine.Entities
{
    public enum Gender
    {
        Male,
        Female
    }

    public enum PersonTask
    {
        Idle,
        WorkingAtBuilding,
        Constructing,
        Gathering,
        Hunting,           // NEW: Hunting wildlife
        MovingToLocation,
        Resting,
        Sleeping,
        Eating,
        GoingHome,
        GoingToWork
    }

    public class Person
    {
        // Identity
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        
        // Relationships
        public Person? Spouse { get; set; }
        public List<Person> Children { get; set; }
        public Family? Family { get; set; }
        
        // Position and Navigation
        public Vector2Int Position { get; set; }
        public Vector2Int? TargetPosition { get; set; }
        public List<Vector2Int> CurrentPath { get; private set; }
        public int PathIndex { get; private set; }
        
        // Task Assignment
        public PersonTask CurrentTask { get; set; }
        public Building? AssignedBuilding { get; set; }
        public bool HasArrivedAtBuilding { get; set; } // Track if arrival has been logged
        
        // Stats
        public int Energy { get; set; } // 0-100
        public int Hunger { get; set; } // 0-100 (higher = more hungry)
        public int Health { get; set; } // 0-100
        public bool IsAlive { get; set; }
        public bool IsSleeping { get; set; } // Track if person is currently sleeping
        public bool IsSick { get; set; }
        public int DaysSick { get; private set; }
        public Building? HomeBuilding { get; set; } // The house this person lives in
        
        public Person(int id, string firstName, string lastName, int age, Gender gender)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Age = age;
            Gender = gender;
            Children = new List<Person>();
            CurrentPath = new List<Vector2Int>();
            IsAlive = true;
            Energy = 100;
            Hunger = 0;
            Health = 100;
            IsSick = false;
            DaysSick = 0;
            CurrentTask = PersonTask.Idle;
        }

        public string FullName => $"{FirstName} {LastName}";

        public void MarryTo(Person spouse)
        {
            Spouse = spouse;
            spouse.Spouse = this;
        }

        public void AddChild(Person child)
        {
            Children.Add(child);
            child.Family = this.Family;
        }

        /// <summary>
        /// Set a path for the person to follow
        /// </summary>
        public void SetPath(List<Vector2Int> path)
        {
            CurrentPath = path;
            PathIndex = 0;
            
            // Skip first position if we're already there
            if (path.Count > 0 && path[0].X == Position.X && path[0].Y == Position.Y)
            {
                PathIndex = 1;
            }
            
            if (PathIndex < path.Count)
            {
                TargetPosition = path[PathIndex];
                CurrentTask = PersonTask.MovingToLocation;
                Console.WriteLine($"[MOVE] {FirstName} starting journey: {path.Count} steps from ({Position.X},{Position.Y}) to ({path[path.Count-1].X},{path[path.Count-1].Y})");
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
        /// Update movement along path
        /// </summary>
        public bool MoveAlongPath()
        {
            if (CurrentPath.Count == 0 || PathIndex >= CurrentPath.Count)
            {
                TargetPosition = null;
                return false; // Path complete
            }

            var target = CurrentPath[PathIndex];
            
            // Simple movement - move one tile at a time
            if (Position.X != target.X || Position.Y != target.Y)
            {
                Position = target;
                PathIndex++;
                
                if (PathIndex < CurrentPath.Count)
                {
                    TargetPosition = CurrentPath[PathIndex];
                }
                else
                {
                    TargetPosition = null;
                    Console.WriteLine($"[MOVE] {FirstName} reached destination at ({Position.X},{Position.Y})");
                    return false; // Path complete
                }
            }
            
            return true; // Still moving
        }

        /// <summary>
        /// Try to move along path, avoiding occupied tiles unless necessary
        /// Returns true if still moving, false if reached destination or blocked
        /// </summary>
        public bool TryMoveAlongPath(VillageGrid grid)
        {
            if (CurrentPath.Count == 0 || PathIndex >= CurrentPath.Count)
            {
                TargetPosition = null;
                return false; // Path complete
            }

            var target = CurrentPath[PathIndex];

            // Check if we're already at the target position
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
                    Console.WriteLine($"[MOVE] {FirstName} reached destination at ({Position.X},{Position.Y})");
                    return false; // Path complete
                }
            }

            // REMOVED: Collision detection - people can now pass through each other
            // This prevents families from getting stuck when moving in opposite directions
            // People on the same tile can still be selected and cycled through in the UI

            // Move to target
            var oldPosition = Position;
            Position = target;

            // Update tile registrations immediately for proper tile tracking
            if (oldPosition.X != target.X || oldPosition.Y != target.Y)
            {
                var oldTile = grid.GetTile(oldPosition.X, oldPosition.Y);
                var newTile = grid.GetTile(target.X, target.Y);

                if (oldTile != null && oldTile.PeopleOnTile.Contains(this))
                {
                    oldTile.PeopleOnTile.Remove(this);
                }

                if (newTile != null && !newTile.PeopleOnTile.Contains(this))
                {
                    newTile.PeopleOnTile.Add(this);
                }
            }

            PathIndex++;

            if (PathIndex < CurrentPath.Count)
            {
                TargetPosition = CurrentPath[PathIndex];
            }
            else
            {
                TargetPosition = null;
                Console.WriteLine($"[MOVE] {FirstName} reached destination at ({Position.X},{Position.Y})");
                return false; // Path complete
            }

            return true; // Still moving
        }

        /// <summary>
        /// Assign person to work at a building
        /// </summary>
        public void AssignToBuilding(Building building, bool logEvent = true)
        {
            // Remove from previous building
            if (AssignedBuilding != null && AssignedBuilding != building)
            {
                AssignedBuilding.Workers.Remove(this);
            }
            
            AssignedBuilding = building;
            CurrentTask = PersonTask.WorkingAtBuilding;
            HasArrivedAtBuilding = false; // Reset arrival flag when assigning new job
            
            if (!building.Workers.Contains(this))
            {
                building.Workers.Add(this);
            }
            
            // Only log if requested (to avoid spam when assigning whole family)
            if (logEvent)
            {
                EventLog.Instance.AddMessage(
                    $"{FirstName} {LastName} begins work at the {building.Type}", 
                    LogLevel.Info);
            }
        }

        /// <summary>
        /// Unassign from current building
        /// </summary>
        public void UnassignFromBuilding()
        {
            if (AssignedBuilding != null)
            {
                AssignedBuilding.Workers.Remove(this);
                AssignedBuilding = null;
            }
            CurrentTask = PersonTask.Idle;
        }

        /// <summary>
        /// Update person's stats (called each game tick)
        /// </summary>
        public void UpdateStats()
        {
            if (!IsAlive) return;

            // Increase hunger over time (slower when sleeping, doesn't increase when eating)
            if (CurrentTask == PersonTask.Eating)
            {
                // No hunger increase while eating
            }
            else if (IsSleeping)
            {
                Hunger = Math.Min(100, Hunger + 1);
            }
            else
            {
                Hunger = Math.Min(100, Hunger + 1); // Steady hunger rate (was +2, too fast)
            }

            // Health effects from hunger
            if (Hunger >= 80)
            {
                // Very hungry - lose health
                Health = Math.Max(0, Health - 2);
                if (!IsSick && Health < 70)
                {
                    IsSick = true;
                    EventLog.Instance.AddMessage(
                        $"{FirstName} {LastName} has fallen ill from hunger", 
                        LogLevel.Warning);
                }
            }
            else if (Hunger >= 60)
            {
                // Hungry - slow health loss
                Health = Math.Max(0, Health - 1);
            }
            else if (Health < 100 && Hunger < 40)
            {
                // Recovering - gain health when well-fed
                Health = Math.Min(100, Health + 1);
                if (Health >= 70 && IsSick)
                {
                    IsSick = false;
                    DaysSick = 0;
                    EventLog.Instance.AddMessage(
                        $"{FirstName} {LastName} has recovered from illness", 
                        LogLevel.Info);
                }
            }

            // Track days sick
            if (IsSick)
            {
                DaysSick++;
            }

            // Decrease energy when working (more when sick or hungry)
            if (CurrentTask == PersonTask.WorkingAtBuilding || CurrentTask == PersonTask.Constructing)
            {
                int energyLoss = 1;
                if (IsSick) energyLoss += 1;
                if (Hunger >= 60) energyLoss += 1;
                Energy = Math.Max(0, Energy - energyLoss);
            }

            // Recover energy when resting or sleeping (slower when sick)
            int energyGain = IsSick ? 1 : 2;
            if (CurrentTask == PersonTask.Resting || CurrentTask == PersonTask.Idle)
            {
                Energy = Math.Min(100, Energy + energyGain);
            }
            else if (CurrentTask == PersonTask.Sleeping || IsSleeping)
            {
                Energy = Math.Min(100, Energy + (energyGain + 1)); // Sleep recovers energy faster
            }
            else if (CurrentTask == PersonTask.Eating)
            {
                Energy = Math.Min(100, Energy + 5); // Eating gives a small energy boost
            }

            // Death from health reaching zero
            if (Health <= 0)
            {
                IsAlive = false;
                EventLog.Instance.AddMessage(
                    $"{FirstName} {LastName} has died from starvation and illness", 
                    LogLevel.Error);
            }
        }

        /// <summary>
        /// Person eats food
        /// </summary>
        public void Eat(int amount)
        {
            Hunger = Math.Max(0, Hunger - amount);
        }

        /// <summary>
        /// Check if person needs to eat (hungry enough to stop and eat)
        /// </summary>
        public bool NeedsToEat()
        {
            return IsAlive && Hunger >= 50 && CurrentTask != PersonTask.Eating && CurrentTask != PersonTask.Sleeping;
        }

        /// <summary>
        /// Start eating (person will consume food from village storage)
        /// </summary>
        public void StartEating()
        {
            CurrentTask = PersonTask.Eating;
        }

        /// <summary>
        /// Finish eating and return to previous activity
        /// </summary>
        public void FinishEating()
        {
            // Return to idle - they'll pick up their work again through daily routines
            if (CurrentTask == PersonTask.Eating)
            {
                CurrentTask = PersonTask.Idle;
            }
        }

        /// <summary>
        /// Send person to sleep at their home
        /// </summary>
        public void GoToSleep()
        {
            if (HomeBuilding != null)
            {
                CurrentTask = PersonTask.Sleeping;
                IsSleeping = true;
                // Reset work arrival flag for next day
                HasArrivedAtBuilding = false;
            }
        }

        /// <summary>
        /// Wake person up from sleep
        /// </summary>
        public void WakeUp()
        {
            IsSleeping = false;
            if (CurrentTask == PersonTask.Sleeping)
            {
                CurrentTask = PersonTask.Idle;
            }
        }

        /// <summary>
        /// Check if person is at home
        /// </summary>
        public bool IsAtHome()
        {
            if (HomeBuilding == null || Family?.HomePosition == null)
                return false;

            // Check if person is at family home position or at home building
            var homePos = Family.HomePosition.Value;
            if (Position.X == homePos.X && Position.Y == homePos.Y)
                return true;

            // Check if at home building
            var homeTiles = HomeBuilding.GetOccupiedTiles();
            return homeTiles.Any(t => t.X == Position.X && t.Y == Position.Y);
        }

                /// <summary>
                /// Check if person is at their assigned work building
                /// </summary>
                public bool IsAtWork()
                {
                    if (AssignedBuilding == null)
                        return false;

                    var workTiles = AssignedBuilding.GetOccupiedTiles();
                    return workTiles.Any(t => t.X == Position.X && t.Y == Position.Y);
                }

                /// <summary>
                /// Check if person is at a construction site (near or at the building)
                /// </summary>
                public bool IsAtConstructionSite(Building building)
                {
                    if (building == null)
                        return false;

                    // Check if person is on any of the building's tiles or adjacent to them
                    var buildingTiles = building.GetOccupiedTiles();

                    // Check if on building tile
                    if (buildingTiles.Any(t => t.X == Position.X && t.Y == Position.Y))
                        return true;

                    // Check if adjacent to building (within 1 tile)
                    foreach (var tile in buildingTiles)
                    {
                        int distance = Math.Abs(tile.X - Position.X) + Math.Abs(tile.Y - Position.Y);
                        if (distance <= 1)
                            return true;
                    }

                    return false;
                }
            }
        }