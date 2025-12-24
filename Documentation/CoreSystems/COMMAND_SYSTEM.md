# Command System

## Overview

VillageBuilder uses the **Command Pattern** to handle all player actions. Commands are queued, validated, and executed deterministically, enabling save/load, multiplayer synchronization, and replay systems.

---

## Architecture

### Core Concepts

**Command Pattern Benefits:**
1. **Determinism** - Same commands + same state = same result
2. **Queueing** - Commands execute at specific future ticks
3. **Validation** - Check preconditions before execution
4. **Undo/Redo** - Potentially reversible (future feature)
5. **Networking** - Commands can be serialized and sent over network
6. **Replays** - Record command stream for playback

### Class Hierarchy

```
ICommand (interface)
    ??? GameCommand (base class)
    ?   ??? ConstructBuildingCommand
    ?   ??? AssignConstructionWorkersCommand
    ?   ??? AssignFamilyJobCommand
    ?   ??? AssignFamilyHomeCommand
    ?   ??? AssignWorkerCommand
    ?   ??? TransferResourceCommand
    ??? (future commands)
```

---

## ICommand Interface

### Definition

```csharp
public interface ICommand
{
    /// <summary>
    /// The tick at which this command should execute
    /// </summary>
    int TargetTick { get; }
    
    /// <summary>
    /// Execute the command on the game engine
    /// </summary>
    /// <param name="engine">Game engine instance</param>
    /// <returns>Result indicating success/failure</returns>
    CommandResult Execute(GameEngine engine);
}
```

### CommandResult

```csharp
public class CommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }  // Optional result data
}
```

---

## GameCommand Base Class

### Implementation

```csharp
[Serializable]
public abstract class GameCommand : ICommand
{
    public int PlayerId { get; set; }
    public int TargetTick { get; set; }
    
    protected GameCommand(int playerId, int targetTick)
    {
        PlayerId = playerId;
        TargetTick = targetTick;
    }
    
    public abstract CommandResult Execute(GameEngine engine);
    
    protected CommandResult Success(string message, object? data = null)
    {
        return new CommandResult 
        { 
            Success = true, 
            Message = message,
            Data = data
        };
    }
    
    protected CommandResult Failure(string message)
    {
        return new CommandResult 
        { 
            Success = false, 
            Message = message 
        };
    }
}
```

---

## CommandQueue

### Purpose

Manages pending commands and executes them at the right tick.

### Implementation

```csharp
public class CommandQueue
{
    private readonly SortedDictionary<int, List<ICommand>> _commands = new();
    
    public void Enqueue(ICommand command)
    {
        if (!_commands.ContainsKey(command.TargetTick))
        {
            _commands[command.TargetTick] = new List<ICommand>();
        }
        
        _commands[command.TargetTick].Add(command);
    }
    
    public List<ICommand> GetCommandsForTick(int tick)
    {
        if (_commands.TryGetValue(tick, out var commands))
        {
            _commands.Remove(tick);
            return commands;
        }
        
        return new List<ICommand>();
    }
    
    public void Clear()
    {
        _commands.Clear();
    }
}
```

### Usage in GameEngine

```csharp
public void SimulateTick()
{
    CurrentTick++;
    
    // ... other simulation logic ...
    
    // Process commands for this tick
    ProcessCommands();
}

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
        else if (!string.IsNullOrEmpty(result.Message))
        {
            EventLog.Instance.AddMessage(
                result.Message, 
                LogLevel.Info
            );
        }
    }
}
```

---

## Built-in Commands

### 1. ConstructBuildingCommand

**Purpose:** Place a new building on the map

```csharp
public class ConstructBuildingCommand : GameCommand
{
    public BuildingType BuildingType { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    
    public ConstructBuildingCommand(int playerId, int targetTick, 
        BuildingType buildingType, int x, int y)
        : base(playerId, targetTick)
    {
        BuildingType = buildingType;
        X = x;
        Y = y;
    }
    
    public override CommandResult Execute(GameEngine engine)
    {
        // Validate position
        if (!engine.Grid.IsValidPosition(X, Y))
        {
            return Failure($"Invalid position: ({X}, {Y})");
        }
        
        // Check if tile is occupied
        var tile = engine.Grid.GetTile(X, Y);
        if (tile?.Building != null)
        {
            return Failure($"Tile ({X}, {Y}) already has a building");
        }
        
        // Check terrain walkability
        if (!tile.IsWalkable)
        {
            return Failure($"Cannot build on {tile.Type} terrain");
        }
        
        // Create building (starts under construction)
        var building = new Building(BuildingType, X, Y)
        {
            IsConstructed = false,
            ConstructionProgress = 0
        };
        
        engine.Buildings.Add(building);
        tile.Building = building;
        
        return Success($"{BuildingType} construction started at ({X}, {Y})");
    }
}
```

**Usage:**
```csharp
var command = new ConstructBuildingCommand(
    playerId: 0,
    targetTick: engine.CurrentTick + 1,
    buildingType: BuildingType.House,
    x: 50,
    y: 50
);

engine.SubmitCommand(command);
```

---

### 2. AssignConstructionWorkersCommand

**Purpose:** Assign family members to work on building construction

```csharp
public class AssignConstructionWorkersCommand : GameCommand
{
    public int FamilyId { get; set; }
    public int BuildingId { get; set; }
    
    public override CommandResult Execute(GameEngine engine)
    {
        // Find family
        var family = engine.Families.FirstOrDefault(f => f.Id == FamilyId);
        if (family == null)
        {
            return Failure($"Family {FamilyId} not found");
        }
        
        // Find building
        var building = engine.Buildings.FirstOrDefault(b => b.Id == BuildingId);
        if (building == null)
        {
            return Failure($"Building {BuildingId} not found");
        }
        
        // Check if building is under construction
        if (building.IsConstructed)
        {
            return Failure($"Building is already constructed");
        }
        
        // Get adult family members (age >= 18)
        var workers = family.Members
            .Where(p => p.Age >= 18 && p.IsAlive)
            .ToList();
        
        if (workers.Count == 0)
        {
            return Failure($"No available workers in {family.FamilyName} family");
        }
        
        // Assign workers
        int assignedCount = 0;
        foreach (var worker in workers)
        {
            // Remove from previous construction site
            var previousSite = engine.Buildings
                .FirstOrDefault(b => b.ConstructionWorkers.Contains(worker));
            
            if (previousSite != null)
            {
                previousSite.ConstructionWorkers.Remove(worker);
            }
            
            // Assign to new site
            building.ConstructionWorkers.Add(worker);
            worker.CurrentTask = PersonTask.Constructing;
            assignedCount++;
        }
        
        return Success(
            $"{family.FamilyName} family ({assignedCount} workers) assigned to construct {building.Type}",
            data: assignedCount
        );
    }
}
```

---

### 3. AssignFamilyJobCommand

**Purpose:** Assign family to work at a building

```csharp
public class AssignFamilyJobCommand : GameCommand
{
    public int FamilyId { get; set; }
    public int BuildingId { get; set; }
    
    public override CommandResult Execute(GameEngine engine)
    {
        var family = engine.Families.FirstOrDefault(f => f.Id == FamilyId);
        var building = engine.Buildings.FirstOrDefault(b => b.Id == BuildingId);
        
        if (family == null || building == null)
        {
            return Failure("Family or building not found");
        }
        
        if (!building.IsConstructed)
        {
            return Failure("Building is not yet constructed");
        }
        
        // Get working-age adults
        var workers = family.Members
            .Where(p => p.Age >= 18 && p.IsAlive)
            .ToList();
        
        if (workers.Count == 0)
        {
            return Failure("No available workers");
        }
        
        // Unassign from previous jobs
        foreach (var worker in workers)
        {
            if (worker.AssignedBuilding != null)
            {
                worker.AssignedBuilding.Workers.Remove(worker);
            }
        }
        
        // Assign to new building
        int assignedCount = 0;
        foreach (var worker in workers)
        {
            worker.AssignToBuilding(building, logEvent: false);
            assignedCount++;
        }
        
        return Success(
            $"{family.FamilyName} family ({assignedCount} workers) assigned to {building.Type}",
            data: assignedCount
        );
    }
}
```

---

### 4. AssignFamilyHomeCommand

**Purpose:** Assign family to live in a house

```csharp
public class AssignFamilyHomeCommand : GameCommand
{
    public int FamilyId { get; set; }
    public int BuildingId { get; set; }
    
    public override CommandResult Execute(GameEngine engine)
    {
        var family = engine.Families.FirstOrDefault(f => f.Id == FamilyId);
        var building = engine.Buildings.FirstOrDefault(b => b.Id == BuildingId);
        
        if (family == null || building == null)
        {
            return Failure("Family or building not found");
        }
        
        if (building.Type != BuildingType.House)
        {
            return Failure("Can only assign families to houses");
        }
        
        if (!building.IsConstructed)
        {
            return Failure("House is not yet constructed");
        }
        
        // Check capacity
        if (building.Residents.Count + family.Members.Count > building.GetCapacity())
        {
            return Failure($"House is full (capacity: {building.GetCapacity()})");
        }
        
        // Remove from previous home
        if (family.HomeBuilding != null)
        {
            foreach (var member in family.Members)
            {
                family.HomeBuilding.Residents.Remove(member);
                member.HomeBuilding = null;
            }
        }
        
        // Assign to new home
        family.HomeBuilding = building;
        family.HomePosition = new Vector2Int(building.X, building.Y);
        
        foreach (var member in family.Members)
        {
            member.HomeBuilding = building;
            building.Residents.Add(member);
        }
        
        return Success(
            $"{family.FamilyName} family now lives in house at ({building.X}, {building.Y})"
        );
    }
}
```

---

### 5. TransferResourceCommand

**Purpose:** Transfer resources between buildings (future use)

```csharp
public class TransferResourceCommand : GameCommand
{
    public int SourceBuildingId { get; set; }
    public int TargetBuildingId { get; set; }
    public ResourceType ResourceType { get; set; }
    public int Amount { get; set; }
    
    public override CommandResult Execute(GameEngine engine)
    {
        // Validate buildings
        var source = engine.Buildings.FirstOrDefault(b => b.Id == SourceBuildingId);
        var target = engine.Buildings.FirstOrDefault(b => b.Id == TargetBuildingId);
        
        if (source == null || target == null)
        {
            return Failure("Source or target building not found");
        }
        
        // Check if source has enough resources
        if (source.Inventory.GetAmount(ResourceType) < Amount)
        {
            return Failure($"Insufficient {ResourceType} (has {source.Inventory.GetAmount(ResourceType)}, needs {Amount})");
        }
        
        // Check if target has capacity
        if (!target.Inventory.CanStore(ResourceType, Amount))
        {
            return Failure($"Target building cannot store {Amount} {ResourceType}");
        }
        
        // Transfer
        source.Inventory.Remove(ResourceType, Amount);
        target.Inventory.Add(ResourceType, Amount);
        
        return Success($"Transferred {Amount} {ResourceType} from {source.Type} to {target.Type}");
    }
}
```

---

## Command Submission

### Player Submits Command

```csharp
// In UI/Game code
public void OnBuildButtonClick(BuildingType type, int x, int y)
{
    var command = new ConstructBuildingCommand(
        playerId: 0,
        targetTick: engine.CurrentTick + 1,  // Execute next tick
        buildingType: type,
        x: x,
        y: y
    );
    
    engine.SubmitCommand(command);
}
```

### Engine Queues Command

```csharp
public void SubmitCommand(ICommand command)
{
    _commandQueue.Enqueue(command);
    
    EventLog.Instance.AddMessage(
        $"Command queued: {command.GetType().Name} for tick {command.TargetTick}",
        LogLevel.Info
    );
}
```

---

## Determinism

### Critical Rules

**DO:**
- ? Use `engine.CurrentTick` for time-based logic
- ? Use seeded `Random` instances
- ? Process commands in order
- ? Validate all inputs

**DON'T:**
- ? Use `DateTime.Now` or `Environment.TickCount`
- ? Use unseeded random numbers
- ? Access external state (files, network) during execution
- ? Depend on execution timing

### Example: Deterministic Building Placement

```csharp
// WRONG - non-deterministic
var x = Random.Shared.Next(100);  // Different each run!

// RIGHT - deterministic
var seededRandom = new Random(engine.CurrentTick + buildingId);
var x = seededRandom.Next(100);  // Same each run with same inputs
```

---

## Validation Best Practices

### Pre-execution Checks

```csharp
public override CommandResult Execute(GameEngine engine)
{
    // 1. Validate entities exist
    if (building == null)
    {
        return Failure("Building not found");
    }
    
    // 2. Check preconditions
    if (!building.IsConstructed)
    {
        return Failure("Building not constructed");
    }
    
    // 3. Check resources/capacity
    if (workers.Count == 0)
    {
        return Failure("No available workers");
    }
    
    // 4. Execute action
    // ... perform state changes ...
    
    // 5. Return result
    return Success("Action completed");
}
```

---

## Serialization

### For Save/Load

Commands in queue are serialized with game state:

```csharp
[Serializable]
public class SaveData
{
    public GameEngine Engine { get; set; }
    public List<ICommand> PendingCommands { get; set; }
}
```

### For Networking (Future)

Commands can be serialized to JSON:

```csharp
public class CommandDto
{
    public string TypeName { get; set; }
    public string JsonData { get; set; }
}

// Serialize
var dto = new CommandDto
{
    TypeName = command.GetType().FullName,
    JsonData = JsonSerializer.Serialize(command)
};

// Deserialize
var type = Type.GetType(dto.TypeName);
var command = (ICommand)JsonSerializer.Deserialize(dto.JsonData, type);
```

---

## Creating Custom Commands

### Step 1: Define Command

```csharp
public class MyCustomCommand : GameCommand
{
    public int MyParameter { get; set; }
    
    public MyCustomCommand(int playerId, int targetTick, int myParameter)
        : base(playerId, targetTick)
    {
        MyParameter = myParameter;
    }
    
    public override CommandResult Execute(GameEngine engine)
    {
        // Validate
        if (MyParameter < 0)
        {
            return Failure("Invalid parameter");
        }
        
        // Execute logic
        // ... modify game state ...
        
        return Success($"Command executed with parameter: {MyParameter}");
    }
}
```

### Step 2: Submit from UI

```csharp
var command = new MyCustomCommand(
    playerId: 0,
    targetTick: engine.CurrentTick + 1,
    myParameter: 42
);

engine.SubmitCommand(command);
```

### Step 3: Add to Serialization (if saving)

```csharp
// Add [Serializable] attribute
[Serializable]
public class MyCustomCommand : GameCommand
{
    // ...
}
```

---

## Error Handling

### Failed Commands

```csharp
var result = command.Execute(engine);

if (!result.Success)
{
    // Log error
    EventLog.Instance.AddMessage(
        $"Failed: {result.Message}", 
        LogLevel.Error
    );
    
    // Optionally notify UI
    OnCommandFailed?.Invoke(result.Message);
}
```

### Common Failure Reasons

1. **Entity not found** - ID doesn't exist
2. **Invalid state** - Building not constructed
3. **Insufficient resources** - Not enough workers/materials
4. **Capacity exceeded** - Building full
5. **Invalid position** - Out of bounds or occupied

---

## Performance

### Command Queue Efficiency

- **SortedDictionary**: O(log n) insertion, O(1) retrieval by tick
- **Per-tick overhead**: Negligible (<0.1% frame time)
- **Memory**: ~40 bytes per command

### Optimization Tips

1. **Batch commands** when possible
2. **Validate early** before queueing
3. **Avoid expensive lookups** in Execute()
4. **Cache entity references** when safe

---

## Future Enhancements

### Planned Features

1. **Undo/Redo System**
   - Store command history
   - Implement `Undo()` method
   - Reverse state changes

2. **Command Macros**
   - Record sequence of commands
   - Replay as macro
   - Useful for repeated tasks

3. **Networked Multiplayer**
   - Serialize commands to packets
   - Synchronize command queues
   - Handle latency compensation

4. **Replay System**
   - Save command stream
   - Playback deterministically
   - Debug/demo tool

---

## Related Documentation

- [Game Engine](GAME_ENGINE.md) - Command processing in simulation loop
- [Save/Load System](../../SaveLoad/SAVE_LOAD_SYSTEM.md) - Command serialization

---

**Maintained By:** VillageBuilder Development Team  
**Last Updated:** 2024-01-XX
