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
        MovingToLocation,
        Resting,
        Sleeping,
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
        public bool IsAlive { get; set; }
        public bool IsSleeping { get; set; } // Track if person is currently sleeping
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
            
            // Check if target tile is occupied by other people
            var targetTile = grid.GetTile(target.X, target.Y);
            if (targetTile != null)
            {
                // Only avoid stacking if there's more than one person already there
                // AND we're not at the final destination
                bool isDestination = PathIndex == CurrentPath.Count - 1;
                bool tileOccupied = targetTile.PeopleOnTile.Count > 0 && !targetTile.PeopleOnTile.Contains(this);
                
                if (tileOccupied && !isDestination)
                {
                    // Wait this tick - don't move if tile is occupied (unless it's our destination)
                    return true; // Still moving (waiting)
                }
            }
            
            // Move to target
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

            // Increase hunger over time (slower when sleeping)
            if (IsSleeping)
            {
                Hunger = Math.Min(100, Hunger + 1);
            }
            else
            {
                Hunger = Math.Min(100, Hunger + 1);
            }
            
            // Decrease energy when working
            if (CurrentTask == PersonTask.WorkingAtBuilding || CurrentTask == PersonTask.Constructing)
            {
                Energy = Math.Max(0, Energy - 1);
            }
            
            // Recover energy when resting or sleeping (faster when sleeping)
            if (CurrentTask == PersonTask.Resting || CurrentTask == PersonTask.Idle)
            {
                Energy = Math.Min(100, Energy + 2);
            }
            else if (CurrentTask == PersonTask.Sleeping || IsSleeping)
            {
                Energy = Math.Min(100, Energy + 3); // Sleep recovers energy faster
            }
            
            // Death from starvation
            if (Hunger >= 100)
            {
                IsAlive = false;
                EventLog.Instance.AddMessage(
                    $"{FirstName} {LastName} has died from starvation", 
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
    }
}