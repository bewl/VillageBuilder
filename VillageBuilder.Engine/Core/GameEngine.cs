using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Commands;
using VillageBuilder.Engine.Entities;
using VillageBuilder.Engine.Resources;
using VillageBuilder.Engine.World;

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

        public int CurrentTick => _currentTick;

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
                VillageResources.Add(ResourceType.Wood, 1000);
                VillageResources.Add(ResourceType.Stone, 500);
                VillageResources.Add(ResourceType.Tools, 100);
                VillageResources.Add(ResourceType.Grain, 2000);
            }
        }

        /// <summary>
        /// Main simulation tick - processes commands first, then game logic
        /// </summary>
        public List<CommandExecutionRecord> SimulateTick()
        {
            // Process all commands scheduled for this tick (multiplayer-synced)
            var commandResults = CommandQueue.ProcessTick(this);
            
            // Advance time
            Time.AdvanceHour();
            
            // Update weather each day
            if (Time.Hour == 0)
            {
                Weather.UpdateWeather(Time.CurrentSeason, Time.DayOfSeason);
            }

            // Process buildings
            if (Weather.IsWorkable())
            {
                foreach (var building in Buildings.Where(b => b.IsConstructed))
                {
                    building.Work(Time.CurrentSeason);
                }
            }

            // Consume resources (daily at midnight)
            if (Time.Hour == 0)
            {
                ConsumeResources();
            }

            _currentTick++;
            return commandResults;
        }

        private void ConsumeResources()
        {
            // Each person needs food and firewood in winter
            int totalPeople = Families.Sum(f => f.Members.Count);
            VillageResources.Remove(ResourceType.Grain, totalPeople);
            
            if (Time.CurrentSeason == Season.Winter)
            {
                VillageResources.Remove(ResourceType.Firewood, totalPeople / 2);
            }
        }

        /// <summary>
        /// Submit a command for execution (multiplayer-safe)
        /// </summary>
        public void SubmitCommand(ICommand command)
        {
            CommandQueue.EnqueueCommand(command);
        }
    }
}