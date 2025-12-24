# Game Engine Architecture

## Overview

The **GameEngine** is the heart of VillageBuilder - a deterministic, tick-based simulation engine that manages all game entities, processes commands, and drives the main game loop.

---

## Architecture

### Core Responsibilities

1. **Simulation Loop** - Advances game state tick-by-tick
2. **Entity Management** - Tracks families, people, buildings
3. **Command Processing** - Executes queued player commands
4. **Time Management** - Handles day/night cycles, seasons
5. **Event System** - Logs and broadcasts game events

### Class Structure

```csharp
public class GameEngine
{
    // Core State
    public GameTime Time { get; private set; }
    public int CurrentTick { get; private set; }
    public Weather Weather { get; private set; }
    
    // Entities
    public List<Family> Families { get; private set; }
    public List<Building> Buildings { get; private set; }
    public Grid Grid { get; private set; }
    
    // Systems
    private CommandQueue _commandQueue;
    private readonly int _tickRate = 60; // Target ticks per second
}
```

---

## Simulation Loop

### Tick-Based Execution

The engine runs at a fixed tick rate (60 ticks/second by default):

```csharp
public void SimulateTick()
{
    CurrentTick++;
    
    // 1. Advance time (hour, day, season)
    Time.AdvanceHour();
    
    // 2. Update weather based on season
    if (Time.Hour == 0)
    {
        Weather.UpdateWeather(Time.CurrentSeason, Time.DayOfSeason);
    }
    
    // 3. Process queued commands
    ProcessCommands();
    
    // 4. Handle daily routines (wake up, work, sleep)
    HandleDailyRoutines();
    
    // 5. Update entities (people stats, movement)
    UpdatePeopleAndBuildings();
    
    // 6. Clean up (clear tile registrations)
    PrepareForNextTick();
}
```

### Determinism

**Critical:** The engine is **deterministic** - given the same initial state and commands, it will always produce the same result. This enables:
- Save/load functionality
- Multiplayer synchronization (future)
- Replay systems
- Debugging reproducibility

**Rules for Determinism:**
- ? No `DateTime.Now` or `Random()` without seed
- ? Use `GameTime` and seeded `Random`
- ? Process commands in order
- ? Fixed tick rate

---

## Entity Management

### People & Families

**Families** contain **People** with relationships:

```csharp
public class Family
{
    public int Id { get; set; }
    public string FamilyName { get; set; }
    public List<Person> Members { get; set; }
    public Vector2Int? HomePosition { get; set; }
    public Building? HomeBuilding { get; set; }
}

public class Person
{
    public int Id { get; set; }
    public string FirstName, LastName { get; set; }
    public int Age { get; set; }
    public Gender Gender { get; set; }
    
    // Relationships
    public Person? Spouse { get; set; }
    public List<Person> Children { get; set; }
    public Family? Family { get; set; }
    
    // Simulation State
    public Vector2Int Position { get; set; }
    public PersonTask CurrentTask { get; set; }
    public Building? AssignedBuilding { get; set; }
    public int Energy, Hunger, Health { get; set; }
}
```

### Buildings

Buildings are placed on the grid and have workers/residents:

```csharp
public class Building
{
    public BuildingType Type { get; set; }
    public int X, Y { get; set; }
    public bool IsConstructed { get; set; }
    
    // Workers
    public List<Person> Workers { get; set; }
    public List<Person> ConstructionWorkers { get; set; }
    
    // Residents (for houses)
    public List<Person> Residents { get; set; }
}
```

---

## Command System

### Command Pattern

All player actions are **commands** that can be queued and executed at specific ticks:

```csharp
public interface ICommand
{
    int TargetTick { get; }
    CommandResult Execute(GameEngine engine);
}
```

### Command Queue

Commands are processed in order each tick:

```csharp
private void ProcessCommands()
{
    var commands = _commandQueue.GetCommandsForTick(CurrentTick);
    
    foreach (var command in commands)
    {
        var result = command.Execute(this);
        
        if (!result.Success)
        {
            EventLog.Instance.AddMessage(
                $"Command failed: {result.Message}", 
                LogLevel.Error
            );
        }
    }
}
```

### Built-in Commands

- **ConstructBuildingCommand** - Place new building
- **AssignConstructionWorkersCommand** - Assign workers to construction
- **AssignFamilyJobCommand** - Assign family to work at building
- **AssignFamilyHomeCommand** - Assign family to live in house
- **TransferResourceCommand** - Move resources between buildings

---

## Daily Routines

### Scheduled Activities

The engine automatically handles daily activities:

```csharp
private void HandleDailyRoutines()
{
    // 6 AM - Wake up
    if (Time.Hour == GameTime.WakeUpHour)
    {
        foreach (var person in AllPeople.Where(p => p.IsSleeping))
        {
            person.WakeUp();
        }
    }
    
    // 6 AM - Go to work
    if (Time.Hour == GameTime.WorkStartHour)
    {
        foreach (var person in AllPeople.Where(p => p.AssignedBuilding != null))
        {
            SendPersonToWork(person);
        }
        
        // Send construction workers back to sites
        foreach (var building in Buildings.Where(b => !b.IsConstructed))
        {
            foreach (var worker in building.ConstructionWorkers)
            {
                SendPersonToConstruction(worker, building);
            }
        }
    }
    
    // 6 PM - Go home
    if (Time.Hour == GameTime.WorkEndHour)
    {
        foreach (var person in AllPeople)
        {
            SendPersonHome(person);
        }
    }
    
    // 10 PM - Go to sleep
    if (Time.Hour == GameTime.SleepStartHour)
    {
        foreach (var person in AllPeople.Where(p => p.IsAtHome()))
        {
            person.GoToSleep();
        }
    }
}
```

---

## Performance Optimizations

### Tile Registration Optimization

**Problem:** Originally cleared all 10,000 tiles (100×100) every tick.

**Solution:** Track only occupied tiles using HashSet:

```csharp
private readonly HashSet<(int x, int y)> _occupiedTiles = new();

// Clear only occupied tiles
foreach (var (x, y) in _occupiedTiles)
{
    var tile = Grid.GetTile(x, y);
    if (tile != null)
    {
        tile.PeopleOnTile.Clear();
    }
}
_occupiedTiles.Clear();

// Register people and track occupied tiles
foreach (var person in AllPeople)
{
    var tile = Grid.GetTile(person.Position.X, person.Position.Y);
    if (tile != null)
    {
        tile.PeopleOnTile.Add(person);
        _occupiedTiles.Add((person.Position.X, person.Position.Y));
    }
}
```

**Result:** 30.8% faster for large villages (see [PERFORMANCE_OPTIMIZATIONS.md](../Performance/PERFORMANCE_OPTIMIZATIONS.md))

---

## Initialization

### Creating a New Game

```csharp
// 1. Create engine with configuration
var config = GameConfiguration.Default;
var engine = new GameEngine(config);

// 2. Generate world
engine.Grid = TerrainGenerator.GenerateTerrain(100, 100, seed: 12345);

// 3. Initialize starting population
var startingFamily = FamilyGenerator.CreateFamily("Smith", 4);
engine.Families.Add(startingFamily);

// 4. Place starting building
var townHall = new Building(BuildingType.TownHall, 50, 50);
townHall.IsConstructed = true;
engine.Buildings.Add(townHall);

// 5. Start simulation
while (gameRunning)
{
    engine.SimulateTick();
    Thread.Sleep(1000 / 60); // 60 FPS
}
```

---

## Save/Load

### Serialization

The engine state is fully serializable:

```csharp
// Save
SaveLoadManager.SaveGame(engine, "savegame.vbsave");

// Load
var engine = SaveLoadManager.LoadGame("savegame.vbsave");
```

**What's Saved:**
- Current tick, time, weather
- All families and people
- All buildings and construction state
- Grid and terrain
- Pending commands in queue

**What's NOT Saved:**
- Event log (cleared on load)
- UI state (camera position, selections)
- Graphics/rendering state

See: [SAVE_LOAD_SYSTEM.md](../SaveLoad/SAVE_LOAD_SYSTEM.md)

---

## Event System

### Event Log

The engine logs important events:

```csharp
EventLog.Instance.AddMessage(
    "Family Smith assigned to work at Farm", 
    LogLevel.Info
);
```

**Log Levels:**
- `Info` - Normal events (assignments, construction)
- `Success` - Achievements (building completed)
- `Warning` - Issues (worker sick, low resources)
- `Error` - Failures (command failed, death)

---

## Extension Points

### Adding New Systems

**1. Create System Class:**
```csharp
public class MySystem
{
    public void Update(GameEngine engine)
    {
        // Your logic
    }
}
```

**2. Integrate into SimulateTick:**
```csharp
public void SimulateTick()
{
    // ... existing code ...
    
    _mySystem.Update(this); // Add your system
}
```

**3. Add Save/Load Support:**
```csharp
// In SaveLoadManager
public class SaveData
{
    // ... existing fields ...
    public MySystemData MySystem { get; set; } // Add your data
}
```

---

## Best Practices

### ? Do
- Use commands for all player actions
- Keep simulation logic deterministic
- Profile before optimizing (use BenchmarkDotNet)
- Add events for important game occurrences
- Document complex logic

### ? Don't
- Access UI directly from engine
- Use non-deterministic random numbers
- Perform expensive operations every tick
- Modify entities during iteration
- Break save/load compatibility without migration

---

## Related Documentation

- [Game Configuration](GAME_CONFIGURATION.md) - Engine settings
- [Time & Seasons](TIME_AND_SEASONS.md) - Time system
- [Command System](COMMAND_SYSTEM.md) - Commands in detail
- [Performance Optimizations](../Performance/PERFORMANCE_OPTIMIZATIONS.md) - Optimization guide

---

**Maintained By:** VillageBuilder Development Team  
**Last Updated:** 2024-01-XX
