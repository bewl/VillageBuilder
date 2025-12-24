using System.Text.Json;

namespace VillageBuilder.Engine.Config
{
    /// <summary>
    /// Master configuration container for all game systems.
    /// Provides centralized access to all configuration values.
    /// Supports loading/saving configurations to JSON files.
    /// </summary>
    public class GameConfig
    {
        private static GameConfig? _instance;
        
        /// <summary>
        /// Singleton instance for global access.
        /// Can be replaced with dependency injection in the future.
        /// </summary>
        public static GameConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameConfig();
                }
                return _instance;
            }
        }
        
        // Configuration Categories
        public TerrainConfig Terrain { get; set; } = new();
        public WildlifeConfig Wildlife { get; set; } = new();
        public SimulationConfig Simulation { get; set; } = new();
        public RenderConfig Rendering { get; set; } = new();
        
        /// <summary>
        /// Reset to default configuration values
        /// </summary>
        public void ResetToDefaults()
        {
            Terrain = new TerrainConfig();
            Wildlife = new WildlifeConfig();
            Simulation = new SimulationConfig();
            Rendering = new RenderConfig();
        }
        
        /// <summary>
        /// Save configuration to JSON file
        /// </summary>
        public void SaveToFile(string filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = false
            };
            
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(filePath, json);
        }
        
        /// <summary>
        /// Load configuration from JSON file
        /// </summary>
        public static GameConfig LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Configuration file not found: {filePath}");
            }
            
            var json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<GameConfig>(json);
            
            if (config == null)
            {
                throw new InvalidDataException("Failed to deserialize configuration");
            }
            
            _instance = config;
            return config;
        }
        
        /// <summary>
        /// Try to load configuration from file, or use defaults if file doesn't exist
        /// </summary>
        public static GameConfig LoadOrDefault(string filePath)
        {
            try
            {
                return LoadFromFile(filePath);
            }
            catch
            {
                Console.WriteLine($"Could not load config from {filePath}, using defaults");
                return Instance; // Returns default instance
            }
        }
    }
}
