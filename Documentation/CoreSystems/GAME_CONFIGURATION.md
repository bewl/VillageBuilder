# Game Configuration

## Overview

**GameConfiguration** defines all tunable parameters for game simulation, including tick rates, time scales, population settings, and gameplay balance values.

---

## GameConfiguration Class

### Structure

```csharp
public class GameConfiguration
{
    // Simulation
    public int TickRate { get; set; } = 60;              // Ticks per second
    public float DefaultTimeScale { get; set; } = 1.0f;  // Game speed multiplier
    
    // Time System
    public int HoursPerDay { get; set; } = 24;
    public int DaysPerSeason { get; set; } = 30;
    public int SeasonsPerYear { get; set; } = 4;
    
    // Starting Conditions
    public int StartingFamilies { get; set; } = 1;
    public int StartingBuildings { get; set; } = 3;
    public Vector2Int StartingPosition { get; set; } = new(50, 50);
    
    // Map Generation
    public int MapWidth { get; set; } = 100;
    public int MapHeight { get; set; } = 100;
    public int WorldSeed { get; set; } = 12345;
    
    // Gameplay Balance
    public float EnergyConsumptionRate { get; set; } = 0.2f;
    public float HungerIncreaseRate { get; set; } = 0.1f;
    public int ConstructionProgressPerWorker { get; set; } = 1;
    
    // Performance
    public bool EnablePathCaching { get; set; } = true;
    public int MaxCachedPaths { get; set; } = 100;
    
    // Debug
    public bool EnableDebugLogging { get; set; } = false;
    public bool EnablePerformanceMetrics { get; set; } = false;
}
```

---

## Default Configuration

### Default Instance

```csharp
public static class GameConfiguration
{
    public static GameConfiguration Default => new()
    {
        // Simulation
        TickRate = 60,
        DefaultTimeScale = 1.0f,
        
        // Time
        HoursPerDay = 24,
        DaysPerSeason = 30,
        SeasonsPerYear = 4,
        
        // Starting Conditions
        StartingFamilies = 1,
        StartingBuildings = 3,
        StartingPosition = new Vector2Int(50, 50),
        
        // Map
        MapWidth = 100,
        MapHeight = 100,
        WorldSeed = 12345,
        
        // Balance
        EnergyConsumptionRate = 0.2f,
        HungerIncreaseRate = 0.1f,
        ConstructionProgressPerWorker = 1,
        
        // Performance
        EnablePathCaching = true,
        MaxCachedPaths = 100,
        
        // Debug
        EnableDebugLogging = false,
        EnablePerformanceMetrics = false
    };
}
```

---

## Configuration Categories

### Simulation Settings

**TickRate** (default: 60)
- Simulation ticks per real-world second
- Higher = faster simulation processing
- Lower = reduced CPU usage
- Recommended: 60 (smooth gameplay)

**DefaultTimeScale** (default: 1.0)
- Game speed multiplier
- 1.0 = normal speed (1 game hour = 1 real second)
- 2.0 = double speed
- 0.5 = half speed
- Can be changed at runtime

```csharp
// Apply time scale
float tickDuration = 1.0f / config.TickRate;
float scaledDuration = tickDuration * timeScale;
```

---

### Time System Settings

**HoursPerDay** (default: 24)
- Hours in a game day
- Must match daily schedule constants

**DaysPerSeason** (default: 30)
- Days per season
- Total year = DaysPerSeason × SeasonsPerYear
- Default: 120 days per year

**SeasonsPerYear** (default: 4)
- Number of seasons (Spring, Summer, Fall, Winter)
- Affects weather patterns

**Daily Schedule:**
```csharp
public const int WakeUpHour = 6;      // 6 AM
public const int WorkStartHour = 6;   // 6 AM
public const int WorkEndHour = 18;    // 6 PM
public const int SleepStartHour = 22; // 10 PM
```

---

### Starting Conditions

**StartingFamilies** (default: 1)
- Number of families at game start
- Each family: 2 adults + 0-4 children

**StartingBuildings** (default: 3)
- Pre-placed buildings:
  - 1 Town Hall
  - 1 House
  - 1 Farm

**StartingPosition** (default: 50, 50)
- Center point for starting settlement
- Buildings placed near this position

```csharp
// Initialize starting village
var config = GameConfiguration.Default;

for (int i = 0; i < config.StartingFamilies; i++)
{
    var family = FamilyGenerator.CreateFamily(random);
    engine.Families.Add(family);
}

// Place starting buildings near StartingPosition
var townHall = new Building(BuildingType.TownHall, 
    config.StartingPosition.X, 
    config.StartingPosition.Y);
```

---

### Map Generation Settings

**MapWidth** (default: 100)
- Map width in tiles
- Affects world size and pathfinding cost

**MapHeight** (default: 100)
- Map height in tiles
- Total tiles = MapWidth × MapHeight (10,000 default)

**WorldSeed** (default: 12345)
- Random seed for terrain generation
- Same seed = same map
- Enables deterministic world generation

```csharp
var grid = TerrainGenerator.GenerateTerrain(
    config.MapWidth, 
    config.MapHeight, 
    config.WorldSeed
);
```

**Map Size Impact:**
| Size | Tiles | Pathfinding | Memory | Performance |
|------|-------|-------------|---------|-------------|
| 50×50 | 2,500 | Fast | Low | High |
| 100×100 | 10,000 | Good | Medium | Good |
| 200×200 | 40,000 | Slower | High | Medium |
| 500×500 | 250,000 | Slow | Very High | Low |

---

### Gameplay Balance

**EnergyConsumptionRate** (default: 0.2)
- Energy lost per tick while working
- Higher = people tire faster
- Affects work efficiency

**HungerIncreaseRate** (default: 0.1)
- Hunger increase per tick
- Higher = people need food more often
- Affects resource consumption

**ConstructionProgressPerWorker** (default: 1)
- Progress units per worker per tick
- Higher = faster construction
- Scales with worker count

```csharp
// In Person.UpdateStats()
if (CurrentTask == PersonTask.Working)
{
    Energy -= config.EnergyConsumptionRate;
    Hunger += config.HungerIncreaseRate;
}

// In Building.UpdateConstruction()
int progressGain = ConstructionWorkers.Count * 
    config.ConstructionProgressPerWorker;
ConstructionProgress += progressGain;
```

---

### Performance Settings

**EnablePathCaching** (default: true)
- Cache computed paths for reuse
- Significant performance improvement
- Minimal memory cost (~40 bytes/path)

**MaxCachedPaths** (default: 100)
- Maximum paths to cache
- Higher = more memory, fewer recalculations
- Lower = less memory, more recalculations

```csharp
if (config.EnablePathCaching)
{
    var cachedPath = PathfindingHelper.GetPath(start, end, grid);
}
else
{
    var path = Pathfinding.FindPath(start, end, grid);
}
```

---

### Debug Settings

**EnableDebugLogging** (default: false)
- Enable verbose console logging
- Logs pathfinding, commands, state changes
- Performance impact: ~5-10%

**EnablePerformanceMetrics** (default: false)
- Track and display performance stats
- Shows frame time, tick time, entity counts
- Performance impact: ~1-2%

```csharp
if (config.EnableDebugLogging)
{
    Console.WriteLine($"[DEBUG] Person {person.Id} moving to {target}");
}

if (config.EnablePerformanceMetrics)
{
    var tickTime = stopwatch.ElapsedMilliseconds;
    Console.WriteLine($"Tick time: {tickTime}ms");
}
```

---

## Custom Configurations

### Creating Custom Config

```csharp
// Easy mode - faster progress, less hunger
var easyConfig = new GameConfiguration
{
    TickRate = 60,
    DefaultTimeScale = 1.5f,  // Faster game
    
    EnergyConsumptionRate = 0.1f,  // Tire slower
    HungerIncreaseRate = 0.05f,    // Hunger slower
    ConstructionProgressPerWorker = 2,  // Build faster
    
    StartingFamilies = 2,  // More starting people
    StartingBuildings = 5  // More starting buildings
};

// Hard mode - slower progress, more needs
var hardConfig = new GameConfiguration
{
    TickRate = 60,
    DefaultTimeScale = 1.0f,
    
    EnergyConsumptionRate = 0.3f,  // Tire faster
    HungerIncreaseRate = 0.2f,     // Hunger faster
    ConstructionProgressPerWorker = 1,  // Normal speed
    
    StartingFamilies = 1,  // Minimal start
    StartingBuildings = 1  // Just town hall
};

// Performance mode - larger maps, optimized
var performanceConfig = new GameConfiguration
{
    TickRate = 30,  // Lower tick rate for big maps
    MapWidth = 200,
    MapHeight = 200,
    
    EnablePathCaching = true,
    MaxCachedPaths = 200,  // More caching for big map
    
    EnableDebugLogging = false,
    EnablePerformanceMetrics = true
};
```

---

## Configuration Loading

### From JSON File

```csharp
public static GameConfiguration LoadFromFile(string filePath)
{
    if (!File.Exists(filePath))
    {
        return GameConfiguration.Default;
    }
    
    try
    {
        var json = File.ReadAllText(filePath);
        var config = JsonSerializer.Deserialize<GameConfiguration>(json);
        return config ?? GameConfiguration.Default;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to load config: {ex.Message}");
        return GameConfiguration.Default;
    }
}

public static void SaveToFile(GameConfiguration config, string filePath)
{
    var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
    {
        WriteIndented = true
    });
    
    File.WriteAllText(filePath, json);
}
```

### Example JSON

```json
{
  "TickRate": 60,
  "DefaultTimeScale": 1.0,
  "HoursPerDay": 24,
  "DaysPerSeason": 30,
  "SeasonsPerYear": 4,
  "StartingFamilies": 1,
  "StartingBuildings": 3,
  "StartingPosition": {
    "X": 50,
    "Y": 50
  },
  "MapWidth": 100,
  "MapHeight": 100,
  "WorldSeed": 12345,
  "EnergyConsumptionRate": 0.2,
  "HungerIncreaseRate": 0.1,
  "ConstructionProgressPerWorker": 1,
  "EnablePathCaching": true,
  "MaxCachedPaths": 100,
  "EnableDebugLogging": false,
  "EnablePerformanceMetrics": false
}
```

---

## Configuration Validation

### Validation Rules

```csharp
public bool Validate(out List<string> errors)
{
    errors = new List<string>();
    
    if (TickRate <= 0 || TickRate > 120)
        errors.Add("TickRate must be between 1 and 120");
    
    if (DefaultTimeScale <= 0 || DefaultTimeScale > 10)
        errors.Add("DefaultTimeScale must be between 0 and 10");
    
    if (MapWidth < 50 || MapWidth > 500)
        errors.Add("MapWidth must be between 50 and 500");
    
    if (MapHeight < 50 || MapHeight > 500)
        errors.Add("MapHeight must be between 50 and 500");
    
    if (StartingFamilies < 1 || StartingFamilies > 10)
        errors.Add("StartingFamilies must be between 1 and 10");
    
    return errors.Count == 0;
}
```

---

## Runtime Configuration Changes

### Modifying at Runtime

```csharp
// Time scale can be changed dynamically
gameEngine.SetTimeScale(2.0f);  // Double speed

// Most other settings require restart
// Store in config for next game session
var config = gameEngine.Configuration;
config.MapWidth = 150;
config.MapHeight = 150;
GameConfiguration.SaveToFile(config, "config.json");
```

### Hot-Reloadable Settings

**Can change at runtime:**
- ? DefaultTimeScale
- ? EnableDebugLogging
- ? EnablePerformanceMetrics

**Require restart:**
- ? TickRate (affects simulation loop)
- ? Map dimensions (affects world generation)
- ? Starting conditions (only apply at initialization)
- ? Balance values (require new game for fair testing)

---

## Difficulty Presets

### Preset Implementation

```csharp
public static class DifficultyPresets
{
    public static GameConfiguration Easy => new()
    {
        EnergyConsumptionRate = 0.1f,
        HungerIncreaseRate = 0.05f,
        ConstructionProgressPerWorker = 2,
        StartingFamilies = 2,
        StartingBuildings = 5
    };
    
    public static GameConfiguration Normal => GameConfiguration.Default;
    
    public static GameConfiguration Hard => new()
    {
        EnergyConsumptionRate = 0.3f,
        HungerIncreaseRate = 0.2f,
        ConstructionProgressPerWorker = 1,
        StartingFamilies = 1,
        StartingBuildings = 2
    };
    
    public static GameConfiguration Survival => new()
    {
        EnergyConsumptionRate = 0.4f,
        HungerIncreaseRate = 0.25f,
        ConstructionProgressPerWorker = 1,
        StartingFamilies = 1,
        StartingBuildings = 1,  // Only town hall
        DefaultTimeScale = 0.5f  // Slower to increase challenge
    };
}
```

---

## Performance Tuning

### Optimization Settings

**For Large Maps (200×200+):**
```csharp
var config = new GameConfiguration
{
    TickRate = 30,  // Reduce tick rate
    EnablePathCaching = true,
    MaxCachedPaths = 200,  // Increase cache
    EnableDebugLogging = false  // Disable logging
};
```

**For Many Entities (50+ people):**
```csharp
var config = new GameConfiguration
{
    TickRate = 60,  // Keep normal rate
    EnablePathCaching = true,
    MaxCachedPaths = 150,  // More cache for traffic
    EnablePerformanceMetrics = true  // Monitor performance
};
```

**For Low-End Hardware:**
```csharp
var config = new GameConfiguration
{
    TickRate = 30,  // Half tick rate
    MapWidth = 75,
    MapHeight = 75,  // Smaller map
    EnablePathCaching = true,
    MaxCachedPaths = 50,  // Less memory
    DefaultTimeScale = 1.0f  // Don't speed up
};
```

---

## Testing Configurations

### Automated Testing Config

```csharp
public static GameConfiguration Testing => new()
{
    TickRate = 120,  // Fast simulation
    DefaultTimeScale = 10.0f,  // Very fast
    
    StartingFamilies = 1,
    StartingBuildings = 1,
    
    MapWidth = 50,
    MapHeight = 50,  // Small map
    
    EnableDebugLogging = true,
    EnablePerformanceMetrics = true
};
```

---

## Future Enhancements

### Planned Settings

1. **Seasonal Difficulty**
   - Winter severity multiplier
   - Crop growth rates by season

2. **Economic Settings**
   - Resource production multipliers
   - Trade values (future)
   - Building costs

3. **Population Settings**
   - Birth rate
   - Death rate
   - Aging speed

4. **UI Settings**
   - Camera speed
   - Zoom limits
   - UI scale

---

## Related Documentation

- [Game Engine](GAME_ENGINE.md) - Engine initialization
- [Time & Seasons](TIME_AND_SEASONS.md) - Time system
- [Quick Start](../GettingStarted/QUICKSTART.md) - First launch

---

## Example Usage

### Initialize Game with Custom Config

```csharp
// Load or create config
var config = GameConfiguration.LoadFromFile("game_config.json");

// Validate
if (!config.Validate(out var errors))
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Config error: {error}");
    }
    config = GameConfiguration.Default;
}

// Create engine
var engine = new GameEngine(config);

// Generate world
engine.Grid = TerrainGenerator.GenerateTerrain(
    config.MapWidth,
    config.MapHeight,
    config.WorldSeed
);

// Initialize starting conditions
InitializeStartingConditions(engine, config);
```

---

**Maintained By:** VillageBuilder Development Team  
**Last Updated:** 2024-01-XX
