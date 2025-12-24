using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Commands;
using VillageBuilder.Engine.Entities;
using VillageBuilder.Engine.Entities.Wildlife;
using VillageBuilder.Engine.Resources;
using VillageBuilder.Engine.Systems;
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
        public WildlifeManager WildlifeManager { get; }

        private int _seed;
        private int _currentTick;
        private int _nextBuildingId = 1;
        private WildlifeAI _wildlifeAI;

        // Track tiles that have people on them for efficient clearing
        private readonly HashSet<(int x, int y)> _occupiedTiles = new HashSet<(int x, int y)>();

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

                // Initialize wildlife system
                WildlifeManager = new WildlifeManager(Grid, _seed);
                _wildlifeAI = new WildlifeAI(Grid, WildlifeManager, new Random(_seed));

                InitializeStartingResources(configuration);
                InitializeStartingFamilies();

                // Spawn initial wildlife population
                WildlifeManager.InitializeWildlife();
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
            // Clear only tiles that had people on them last tick (optimized from O(Width*Height) to O(people))
            foreach (var (x, y) in _occupiedTiles)
            {
                var tile = Grid.GetTile(x, y);
                if (tile != null)
                {
                    tile.PeopleOnTile.Clear();
                }
            }
            _occupiedTiles.Clear();

            // Register all people on their current tiles and track occupied tiles
            foreach (var family in Families)
            {
                foreach (var person in family.Members)
                {
                    if (!person.IsAlive) continue;

                    var tile = Grid.GetTile(person.Position.X, person.Position.Y);
                    if (tile != null)
                    {
                        tile.PeopleOnTile.Add(person);
                        _occupiedTiles.Add((person.Position.X, person.Position.Y));
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
                // Auto-assign idle families to construction (once per hour to avoid spam)
                if (hourPassed)
                {
                    AutoAssignConstructionWorkers();
                }

                // Process construction on unfinished buildings
                ProcessConstruction();

                // Process production on constructed buildings
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

            // NEW: Update terrain decoration animations (every tick for smooth animation)
            UpdateTerrainAnimations(1f / 60f); // Assuming 60 TPS

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

                    // Update wildlife system (every tick)
                    if (WildlifeManager != null && _wildlifeAI != null)
                    {
                        // Update all wildlife AI and movement
                        var allPeople = Families.SelectMany(f => f.Members).ToList();
                        foreach (var wildlife in WildlifeManager.Wildlife.Where(w => w.IsAlive))
                        {
                            _wildlifeAI.UpdateWildlifeAI(wildlife, allPeople);
                        }

                        // Update wildlife stats and tile registrations
                        WildlifeManager.UpdateWildlife();

                        // Balance ecosystem periodically (every hour)
                        if (hourPassed)
                        {
                            WildlifeManager.BalanceEcosystem();
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

            /// <summary>
            /// Process construction work on unfinished buildings
            /// </summary>
            private void ProcessConstruction()
            {
                var buildingsUnderConstruction = Buildings.Where(b => !b.IsConstructed).ToList();

                foreach (var building in buildingsUnderConstruction)
                {
                    // Only count workers who are alive AND physically present at the construction site
                    int workDone = building.ConstructionWorkers.Count(w => w.IsAlive && w.IsAtConstructionSite(building));

                    if (workDone > 0)
                    {
                        building.ConstructionProgress += workDone;

                        // Check if construction is complete
                        if (building.ConstructionProgress >= building.ConstructionWorkRequired)
                        {
                            CompleteConstruction(building);
                        }
                    }
                }
            }

            /// <summary>
            /// Complete construction of a building
            /// </summary>
            private void CompleteConstruction(Building building)
            {
                building.IsConstructed = true;
                building.ConstructionProgress = building.ConstructionWorkRequired;

                // Release construction workers
                foreach (var worker in building.ConstructionWorkers.ToList())
                {
                    worker.CurrentTask = PersonTask.Idle;
                }
                building.ConstructionWorkers.Clear();

                // Log completion
                EventLog.Instance.AddMessage(
                    $"Construction complete: a new {building.Type} is now operational",
                    LogLevel.Success);

                // Auto-assign house to a homeless family
                if (building.Type == BuildingType.House)
                {
                    AutoAssignHouseToFamily(building);
                }

                // Check if there are more buildings that need workers (freed workers can move to next project)
                AutoAssignConstructionWorkers();
            }

            /// <summary>
            /// Automatically assign a newly constructed house to a homeless family
            /// </summary>
            private void AutoAssignHouseToFamily(Building house)
            {
                // Find families without a home
                var homelessFamily = Families
                    .FirstOrDefault(f => f.Members.Any(m => m.IsAlive && m.HomeBuilding == null));

                if (homelessFamily == null)
                    return; // No homeless families

                // Assign all family members to this house
                foreach (var member in homelessFamily.Members.Where(m => m.IsAlive))
                {
                    member.HomeBuilding = house;

                    // Add to building residents if not already there
                    if (!house.Residents.Contains(member))
                    {
                        house.Residents.Add(member);
                    }
                }

                // Update family home position to the house
                var doorPositions = house.GetDoorPositions();
                if (doorPositions.Count > 0)
                {
                    homelessFamily.HomePosition = doorPositions[0];
                }
                else
                {
                    homelessFamily.HomePosition = new Vector2Int(house.X, house.Y);
                }

                EventLog.Instance.AddMessage(
                    $"The {homelessFamily.FamilyName} family has moved into their new home",
                    LogLevel.Success);
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
        /// Trigger automatic construction worker assignment (public for manual triggering)
        /// </summary>
        public void TriggerAutoConstructionAssignment()
        {
            AutoAssignConstructionWorkers();
        }

        /// <summary>
        /// Automatically assign idle families to buildings under construction
        /// </summary>
        private void AutoAssignConstructionWorkers()
        {
            // Get all buildings that need construction and don't have workers
            var buildingsNeedingWorkers = Buildings
                .Where(b => !b.IsConstructed && b.ConstructionWorkers.Count == 0)
                .OrderBy(b => b.Id) // Oldest buildings first
                .ToList();

            if (buildingsNeedingWorkers.Count == 0)
                return;

            // Get all families with idle adults
            var idleFamilies = Families
                .Where(f => f.GetIdleMembers().Any(p => p.Age >= 18))
                .ToList();

            if (idleFamilies.Count == 0)
                return;

            // Distribute families across buildings (round-robin)
            int buildingIndex = 0;
            foreach (var family in idleFamilies)
            {
                if (buildingIndex >= buildingsNeedingWorkers.Count)
                    break;

                var building = buildingsNeedingWorkers[buildingIndex];
                var idleAdults = family.GetIdleMembers().Where(p => p.Age >= 18).ToList();

                if (idleAdults.Count == 0)
                    continue;

                // Get door positions or building perimeter for construction work
                var workPositions = building.GetDoorPositions();
                if (workPositions.Count == 0)
                {
                    workPositions.Add(new Vector2Int(building.X, building.Y));
                }

                // Assign each idle adult to construction
                int assignedCount = 0;
                foreach (var person in idleAdults)
                {
                    // Find least crowded work position
                    var targetPosition = workPositions
                        .OrderBy(p => Grid.GetTile(p.X, p.Y)?.PeopleOnTile.Count ?? 0)
                        .ThenBy(p => Math.Abs(p.X - person.Position.X) + Math.Abs(p.Y - person.Position.Y))
                        .First();

                    // Calculate path to work site
                    var path = Pathfinding.FindPath(person.Position, targetPosition, Grid);

                    if (path != null && path.Count > 0)
                    {
                        person.SetPath(path);
                        person.CurrentTask = PersonTask.Constructing;

                        // Add to building's construction workers
                        if (!building.ConstructionWorkers.Contains(person))
                        {
                            building.ConstructionWorkers.Add(person);
                        }

                        assignedCount++;
                    }
                }

                if (assignedCount > 0)
                {
                    EventLog.Instance.AddMessage(
                        $"The {family.FamilyName} family ({assignedCount} workers) automatically assigned to construct the {building.Type}",
                        LogLevel.Info);

                    // Move to next building for next family (round-robin distribution)
                    buildingIndex++;
                }
            }
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
                // Send regular workers
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

                // Send construction workers back to construction sites
                foreach (var building in Buildings.Where(b => !b.IsConstructed))
                {
                    foreach (var worker in building.ConstructionWorkers.ToList())
                    {
                        if (worker.IsAlive && !worker.IsAtConstructionSite(building) && worker.CurrentTask != PersonTask.MovingToLocation)
                        {
                            SendPersonToConstruction(worker, building);
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
                    /// Update terrain decoration animations for smooth visual effects
                    /// </summary>
                    private void UpdateTerrainAnimations(float deltaTime)
                    {
                        // Update decorations on visible tiles only (optimization)
                        // For now, update all - could be optimized with viewport culling later
                        for (int x = 0; x < Grid.Width; x++)
                        {
                            for (int y = 0; y < Grid.Height; y++)
                            {
                                var tile = Grid.GetTile(x, y);
                                if (tile != null && tile.Decorations.Count > 0)
                                {
                                    foreach (var decoration in tile.Decorations)
                                    {
                                        decoration.UpdateAnimation(deltaTime);
                                    }
                                }
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

                    /// <summary>
                    /// Send a construction worker back to their construction site
                    /// </summary>
                    private void SendPersonToConstruction(Person person, Building building)
                    {
                        // Get a position near the construction site (door or occupied tile)
                        var doorPositions = building.GetDoorPositions();
                        Vector2Int targetPosition;

                        if (doorPositions.Count > 0)
                        {
                            targetPosition = doorPositions[0];
                        }
                        else
                        {
                            // No door yet (building not complete), use building center
                            targetPosition = new Vector2Int(building.X, building.Y);
                        }

                        var path = Pathfinding.FindPath(person.Position, targetPosition, Grid);

                        if (path != null && path.Count > 0)
                        {
                            person.SetPath(path);
                            person.CurrentTask = PersonTask.Constructing;
                        }
                    }
                }
            }