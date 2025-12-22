using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Commands;
using VillageBuilder.Engine.Entities;
using VillageBuilder.Engine.Resources;
using VillageBuilder.Engine.World;
using VillageBuilder.Engine.Core;

namespace VillageBuilder.Engine.Core
{
    public class GameEngine
    {
        public int GameId { get; }
        public GameTime Time { get; }
        public Weather Weather { get; }
        public VillageGrid Grid { get; }
        public ResourceInventory VillageResources { get; }
        public List<Family> Families { get; }
        public List<Building> Buildings { get; }
        public CommandQueue CommandQueue { get; }
        public GameConfiguration Configuration { get; }
        
        private int _seed;
        private int _currentTick;
        private int _nextBuildingId = 1;

        public int CurrentTick => _currentTick;
        public int GetNextBuildingId() => _nextBuildingId++;

        public GameEngine(int gameId, int seed)
            : this(gameId, new GameConfiguration { Seed = seed })
        {
        }

        public GameEngine(int gameId, GameConfiguration configuration)
        {
            configuration.Validate();
            Configuration = configuration;
            
            GameId = gameId;
            _seed = configuration.Seed ?? new Random().Next();
            _currentTick = 0;
            
            Time = new GameTime();
            Weather = new Weather(_seed);
            Grid = new VillageGrid(configuration.MapWidth, configuration.MapHeight, _seed);
            VillageResources = new ResourceInventory();
            Families = new List<Family>();
            Buildings = new List<Building>();
            CommandQueue = new CommandQueue();
            
            InitializeStartingResources(configuration);
            InitializeStartingFamilies();
        }

        private void InitializeStartingResources(GameConfiguration config)
        {
            if (config.InitialResources != null)
            {
                VillageResources.Add(ResourceType.Wood, config.InitialResources.Wood);
                VillageResources.Add(ResourceType.Stone, config.InitialResources.Stone);
                VillageResources.Add(ResourceType.Tools, config.InitialResources.Tools);
                VillageResources.Add(ResourceType.Grain, config.InitialResources.Grain);
                VillageResources.Add(ResourceType.Firewood, config.InitialResources.Firewood);
            }
            else
            {
                // Default starting resources
                VillageResources.Add(ResourceType.Wood, 10000);
                VillageResources.Add(ResourceType.Stone, 5000);
                VillageResources.Add(ResourceType.Tools, 1000);
                VillageResources.Add(ResourceType.Grain, 20000);
            }
        }

        private void InitializeStartingFamilies()
        {
            var random = new Random(_seed);
            int personId = 1;
            int familyId = 1;

            // Create 3 starting families with husband and wife (all around 20 years old)
            for (int i = 0; i < 3; i++)
            {
                string lastName = NameGenerator.GetRandomLastName(random);
                var family = new Family(familyId++, lastName);

                // Create husband
                string husbandFirstName = NameGenerator.GetRandomMaleName(random);
                int husbandAge = 18 + random.Next(5); // 18-22 years old
                var husband = new Person(personId++, husbandFirstName, lastName, husbandAge, Gender.Male);

                // Create wife
                string wifeFirstName = NameGenerator.GetRandomFemaleName(random);
                int wifeAge = 18 + random.Next(5); // 18-22 years old
                var wife = new Person(personId++, wifeFirstName, lastName, wifeAge, Gender.Female);

                // Marry them
                husband.MarryTo(wife);

                // Add to family
                family.AddMember(husband);
                family.AddMember(wife);

                // Position them near the center of the map
                int centerX = Grid.Width / 2;
                int centerY = Grid.Height / 2;
                int offsetX = (i - 1) * 3; // Spread them out a bit
                int offsetY = 0;

                husband.Position = new Vector2Int(centerX + offsetX, centerY + offsetY);
                wife.Position = new Vector2Int(centerX + offsetX + 1, centerY + offsetY);

                family.HomePosition = new Vector2Int(centerX + offsetX, centerY + offsetY);

                Families.Add(family);
                
                // Log family arrival as a story event
                EventLog.Instance.AddMessage(
                    $"The {family.FamilyName} family settles in the village", 
                    LogLevel.Info);
            }

            Console.WriteLine($"\n=== STARTING FAMILIES ===");
            foreach (var family in Families)
            {
                Console.WriteLine($"Family: {family.FamilyName}");
                foreach (var member in family.Members)
                {
                    Console.WriteLine($"  - {member.FullName}, {member.Gender}, Age {member.Age}, Position ({member.Position.X}, {member.Position.Y})");
                }
            }
            Console.WriteLine($"=========================\n");
            
            // Log game start with context
            var totalPeople = Families.Sum(f => f.Members.Count);
            EventLog.Instance.AddMessage(
                $"A new village is founded with {Families.Count} families ({totalPeople} settlers)", 
                LogLevel.Success);
        }

        /// <summary>
        /// Main simulation tick - processes commands first, then game logic
        /// </summary>
        public List<CommandExecutionRecord> SimulateTick()
        {
            // Clear all tile person registrations at start of tick
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int y = 0; y < Grid.Height; y++)
                {
                    var tile = Grid.GetTile(x, y);
                    if (tile != null)
                    {
                        tile.PeopleOnTile.Clear();
                    }
                }
            }
            
            // Register all people on their current tiles
            foreach (var family in Families)
            {
                foreach (var person in family.Members.Where(p => p.IsAlive))
                {
                    var tile = Grid.GetTile(person.Position.X, person.Position.Y);
                    if (tile != null)
                    {
                        tile.PeopleOnTile.Add(person);
                    }
                }
            }
            
            // IMPORTANT: Pass the current tick to CommandQueue so it processes the right commands
            var commandResults = CommandQueue.ProcessTick(this, _currentTick);
            
            // Advance time (now tick-based, not every tick)
            var previousSeason = Time.CurrentSeason;
            var previousHour = Time.Hour;
            bool hourPassed = Time.AdvanceTick();
            
            // Only do hourly updates when an hour has actually passed
            if (hourPassed)
            {
                // Log season changes
                if (Time.CurrentSeason != previousSeason)
                {
                    EventLog.Instance.AddMessage(
                        $"{Time.CurrentSeason} has arrived - Day {Time.DayOfSeason + 1}", 
                        LogLevel.Info);
                }
                
                // Update weather each day
                if (Time.Hour == 0)
                {
                    Weather.UpdateWeather(Time.CurrentSeason, Time.DayOfSeason);
                }

                // Handle daily routines based on time of day
                HandleDailyRoutines();
                
                // Update people stats (hourly)
                foreach (var family in Families)
                {
                    foreach (var person in family.Members)
                    {
                        person.UpdateStats();
                    }
                }

                // Consume resources (daily at midnight)
                if (Time.Hour == 0)
                {
                    ConsumeResources();
                }
            }

            // Process buildings every tick (not just hourly)
            if (Weather.IsWorkable())
            {
                foreach (var building in Buildings.Where(b => b.IsConstructed))
                {
                    building.Work(Time.CurrentSeason);
                }
            }
            
            // Transfer building production to village storage (daily at midnight)
            if (hourPassed && Time.Hour == 0)
            {
                TransferBuildingProduction();
            }
            
            // Move people along their paths every tick
            foreach (var family in Families)
            {
                // Track families that have arrived at work this tick (for logging)
                var arrivedFamilies = new Dictionary<Building, List<Person>>();
                
                foreach (var person in family.Members)
                {
                    // Move people along their paths (with collision avoidance)
                    if (person.CurrentPath.Count > 0)
                    {
                        var reachedDestination = !person.TryMoveAlongPath(Grid);
                        
                        // Handle arrival at destination
                        if (reachedDestination)
                        {
                            // Check what they were doing
                            if (person.CurrentTask == PersonTask.GoingToWork && person.AssignedBuilding != null)
                            {
                                // Arrived at work
                                person.CurrentTask = PersonTask.WorkingAtBuilding;
                                
                                if (!person.HasArrivedAtBuilding)
                                {
                                    person.HasArrivedAtBuilding = true;
                                    
                                    // Track for family-based logging
                                    if (!arrivedFamilies.ContainsKey(person.AssignedBuilding))
                                    {
                                        arrivedFamilies[person.AssignedBuilding] = new List<Person>();
                                    }
                                    arrivedFamilies[person.AssignedBuilding].Add(person);
                                }
                            }
                            else if (person.CurrentTask == PersonTask.GoingHome)
                            {
                                // Arrived at home
                                person.CurrentTask = PersonTask.Idle;
                                
                                // If it's sleep time, go to sleep immediately
                                if (Time.IsSleepTime())
                                {
                                    person.GoToSleep();
                                }
                            }
                            else if (person.CurrentTask == PersonTask.MovingToLocation)
                            {
                                // Generic movement finished, go idle
                                person.CurrentTask = PersonTask.Idle;
                            }
                        }
                    }
                }
                
                // Log family arrival events (one per building per family)
                foreach (var kvp in arrivedFamilies)
                {
                    var building = kvp.Key;
                    var workers = kvp.Value;
                    EventLog.Instance.AddMessage(
                        $"The {family.FamilyName} family ({workers.Count} workers) has arrived at the {building.Type}",
                        LogLevel.Info);
                }
            }

            _currentTick++;
            return commandResults;
        }

        private void ConsumeResources()
        {
            // Each person needs food and firewood in winter
            int totalPeople = Families.Sum(f => f.Members.Count);
            
            var grainBefore = VillageResources.Get(ResourceType.Grain);
            VillageResources.Remove(ResourceType.Grain, totalPeople);
            var grainAfter = VillageResources.Get(ResourceType.Grain);
            
            // Warn if food is running low
            if (grainAfter < totalPeople * 10 && grainBefore >= totalPeople * 10)
            {
                EventLog.Instance.AddMessage(
                    "Food supplies are running low!", 
                    LogLevel.Warning);
            }
            
            if (Time.CurrentSeason == Season.Winter)
            {
                var firewoodBefore = VillageResources.Get(ResourceType.Firewood);
                VillageResources.Remove(ResourceType.Firewood, totalPeople / 2);
                var firewoodAfter = VillageResources.Get(ResourceType.Firewood);
                
                // Warn if firewood is running low in winter
                if (firewoodAfter < totalPeople * 5 && firewoodBefore >= totalPeople * 5)
                {
                    EventLog.Instance.AddMessage(
                        "Firewood supplies are dwindling in the cold winter!", 
                        LogLevel.Warning);
                }
            }
        }
        
        private void TransferBuildingProduction()
        {
            foreach (var building in Buildings.Where(b => b.IsConstructed))
            {
                // Transfer all resources from building storage to village
                foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
                {
                    var amount = building.Storage.Get(resourceType);
                    if (amount > 0)
                    {
                        VillageResources.Add(resourceType, amount);
                        building.Storage.Remove(resourceType, amount);
                        
                        // Log significant production
                        if (amount >= 10)
                        {
                            EventLog.Instance.AddMessage(
                                $"The {building.Type} produced {amount} {resourceType}",
                                LogLevel.Info);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Submit a command for execution (multiplayer-safe)
        /// </summary>
        public void SubmitCommand(ICommand command)
        {
            CommandQueue.EnqueueCommand(command);
        }

        /// <summary>
        /// Handle daily routines - people go to work in morning, home at night
        /// </summary>
        private void HandleDailyRoutines()
        {
            // Wake people up in the morning
            if (Time.Hour == GameTime.WakeUpHour)
            {
                foreach (var family in Families)
                {
                    foreach (var person in family.Members.Where(p => p.IsAlive && p.IsSleeping))
                    {
                        person.WakeUp();
                    }
                }
            }

            // Send workers to their jobs at work start time
            if (Time.Hour == GameTime.WorkStartHour)
            {
                foreach (var family in Families)
                {
                    foreach (var person in family.Members.Where(p => p.IsAlive && p.AssignedBuilding != null))
                    {
                        // Only send if not already at work and not already traveling
                        if (!person.IsAtWork() && person.CurrentTask != PersonTask.MovingToLocation)
                        {
                            SendPersonToWork(person);
                        }
                    }
                }
            }

            // Send people home at end of work day
            if (Time.Hour == GameTime.WorkEndHour)
            {
                foreach (var family in Families)
                {
                    foreach (var person in family.Members.Where(p => p.IsAlive))
                    {
                        // Send everyone home who isn't already there
                        if (!person.IsAtHome() && person.CurrentTask != PersonTask.MovingToLocation)
                        {
                            SendPersonHome(person);
                        }
                    }
                }
            }

            // Put people to sleep at sleep time
            if (Time.Hour == GameTime.SleepStartHour)
            {
                foreach (var family in Families)
                {
                    foreach (var person in family.Members.Where(p => p.IsAlive && p.IsAtHome()))
                    {
                        person.GoToSleep();
                    }
                }
            }
        }

        /// <summary>
        /// Send a person to their assigned work building
        /// </summary>
        private void SendPersonToWork(Person person)
        {
            if (person.AssignedBuilding == null) return;

            // Try to go inside the building (interior floor)
            var interiorPositions = person.AssignedBuilding.GetInteriorPositions();
            if (interiorPositions.Count > 0)
            {
                var targetPosition = interiorPositions[0]; // Use first interior position
                var path = Pathfinding.FindPath(person.Position, targetPosition, Grid);
                
                if (path != null && path.Count > 0)
                {
                    person.SetPath(path);
                    person.CurrentTask = PersonTask.GoingToWork;
                    return;
                }
            }
            
            // Fallback: go to door if interior not accessible
            var doorPositions = person.AssignedBuilding.GetDoorPositions();
            if (doorPositions.Count > 0)
            {
                var targetDoor = doorPositions[0];
                var path = Pathfinding.FindPath(person.Position, targetDoor, Grid);
                
                if (path != null && path.Count > 0)
                {
                    person.SetPath(path);
                    person.CurrentTask = PersonTask.GoingToWork;
                }
            }
        }

        /// <summary>
        /// Send a person home to their family house
        /// </summary>
        private void SendPersonHome(Person person)
        {
            Vector2Int targetPosition;

            // Try to go inside home building (interior floor)
            if (person.HomeBuilding != null && person.HomeBuilding.IsConstructed)
            {
                var interiorPositions = person.HomeBuilding.GetInteriorPositions();
                if (interiorPositions.Count > 0)
                {
                    targetPosition = interiorPositions[0];
                }
                else
                {
                    // Fallback to door
                    var doorPositions = person.HomeBuilding.GetDoorPositions();
                    if (doorPositions.Count > 0)
                    {
                        targetPosition = doorPositions[0];
                    }
                    else
                    {
                        // No door, just go to building center
                        targetPosition = new Vector2Int(person.HomeBuilding.X, person.HomeBuilding.Y);
                    }
                }
            }
            else if (person.Family?.HomePosition != null)
            {
                targetPosition = person.Family.HomePosition.Value;
            }
            else
            {
                // No home, stay where you are
                return;
            }

            var path = Pathfinding.FindPath(person.Position, targetPosition, Grid);
            
            if (path != null && path.Count > 0)
            {
                person.SetPath(path);
                person.CurrentTask = PersonTask.GoingHome;
            }
        }
    }
}