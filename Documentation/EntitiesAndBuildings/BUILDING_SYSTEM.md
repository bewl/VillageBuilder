# Building System

## Overview

The **Building System** manages structures that people can work in, live in, and construct. Buildings are central to gameplay - they provide housing, work, storage, and services.

---

## Building Class

### Core Structure

```csharp
public class Building
{
    // Identity
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BuildingType Type { get; set; }
    
    // Position
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 1;
    public int Height { get; set; } = 1;
    
    // Construction state
    public bool IsConstructed { get; set; } = false;
    public int ConstructionProgress { get; set; } = 0;
    public List<Person> ConstructionWorkers { get; set; } = new();
    
    // Occupancy
    public List<Person> Workers { get; set; } = new();
    public List<Person> Residents { get; set; } = new();
    
    // Resources (for storage buildings)
    public ResourceInventory Inventory { get; set; } = new();
}
```

---

## BuildingType Enum

### Available Types

```csharp
public enum BuildingType
{
    // Residential
    House,          // Provides housing for families
    
    // Production
    Farm,           // Produces food
    Lumberyard,     // Produces wood
    Quarry,         // Produces stone
    Mine,           // Produces ore/metal
    
    // Storage
    Warehouse,      // Stores resources
    Granary,        // Stores food (specialized warehouse)
    
    // Services
    TownHall,       // Administrative center
    Market,         // Trade hub (future)
    Hospital,       // Healthcare (future)
    School,         // Education (future)
    
    // Defense (future)
    Watchtower,     // Defensive structure
    Barracks        // Military training
}
```

---

## BuildingDefinition Class

### Static Building Data

```csharp
public class BuildingDefinition
{
    public BuildingType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    // Size
    public int Width { get; set; }
    public int Height { get; set; }
    
    // Construction
    public int ConstructionTime { get; set; }  // Ticks to build
    public Dictionary<ResourceType, int> ConstructionCost { get; set; }
    
    // Capacity
    public int WorkerCapacity { get; set; }
    public int ResidentCapacity { get; set; }
    public int StorageCapacity { get; set; }
    
    // Production (if applicable)
    public ResourceType? ProducedResource { get; set; }
    public int ProductionRate { get; set; }  // Per hour
    
    // Visual
    public char Glyph { get; set; }
    public Color Color { get; set; }
}
```

### Default Definitions

```csharp
public static class BuildingDefinitions
{
    public static Dictionary<BuildingType, BuildingDefinition> All = new()
    {
        [BuildingType.House] = new BuildingDefinition
        {
            Type = BuildingType.House,
            Name = "House",
            Description = "Provides housing for one family",
            Width = 2,
            Height = 2,
            ConstructionTime = 3600,  // 1 hour (3600 ticks at 60 TPS)
            ConstructionCost = new Dictionary<ResourceType, int>
            {
                [ResourceType.Wood] = 50,
                [ResourceType.Stone] = 20
            },
            WorkerCapacity = 0,
            ResidentCapacity = 6,
            StorageCapacity = 0,
            Glyph = '?',
            Color = new Color(210, 105, 30, 255)
        },
        
        [BuildingType.Farm] = new BuildingDefinition
        {
            Type = BuildingType.Farm,
            Name = "Farm",
            Description = "Produces food for the village",
            Width = 3,
            Height = 3,
            ConstructionTime = 2400,  // 40 minutes
            ConstructionCost = new Dictionary<ResourceType, int>
            {
                [ResourceType.Wood] = 30,
                [ResourceType.Stone] = 10
            },
            WorkerCapacity = 4,
            ResidentCapacity = 0,
            StorageCapacity = 100,
            ProducedResource = ResourceType.Food,
            ProductionRate = 10,  // 10 food per hour
            Glyph = '?',
            Color = new Color(255, 215, 0, 255)
        },
        
        [BuildingType.Warehouse] = new BuildingDefinition
        {
            Type = BuildingType.Warehouse,
            Name = "Warehouse",
            Description = "Stores resources",
            Width = 3,
            Height = 3,
            ConstructionTime = 2400,
            ConstructionCost = new Dictionary<ResourceType, int>
            {
                [ResourceType.Wood] = 40,
                [ResourceType.Stone] = 30
            },
            WorkerCapacity = 2,
            ResidentCapacity = 0,
            StorageCapacity = 500,
            Glyph = '?',
            Color = new Color(128, 128, 128, 255)
        },
        
        [BuildingType.TownHall] = new BuildingDefinition
        {
            Type = BuildingType.TownHall,
            Name = "Town Hall",
            Description = "Administrative center of the village",
            Width = 4,
            Height = 4,
            ConstructionTime = 7200,  // 2 hours
            ConstructionCost = new Dictionary<ResourceType, int>
            {
                [ResourceType.Wood] = 100,
                [ResourceType.Stone] = 80
            },
            WorkerCapacity = 3,
            ResidentCapacity = 0,
            StorageCapacity = 0,
            Glyph = '?',
            Color = new Color(150, 75, 0, 255)
        }
    };
    
    public static BuildingDefinition Get(BuildingType type)
    {
        return All.TryGetValue(type, out var def) ? def : null;
    }
}
```

---

## Construction System

### Construction Stages

Buildings progress through stages during construction:

```csharp
public enum ConstructionStage
{
    Foundation,     // 0-25%
    Framing,        // 25-50%
    Walls,          // 50-75%
    Finishing,      // 75-100%
    Complete        // 100%
}

public ConstructionStage GetConstructionStage()
{
    if (ConstructionProgress >= 100) return ConstructionStage.Complete;
    if (ConstructionProgress >= 75) return ConstructionStage.Finishing;
    if (ConstructionProgress >= 50) return ConstructionStage.Walls;
    if (ConstructionProgress >= 25) return ConstructionStage.Framing;
    return ConstructionStage.Foundation;
}
```

### Construction Progress

Progress advances based on worker count:

```csharp
public void UpdateConstruction()
{
    if (IsConstructed) return;
    
    // Each worker contributes to construction
    int workerCount = ConstructionWorkers.Count;
    if (workerCount == 0) return;
    
    // Base progress per tick per worker
    float progressPerTick = 0.1f * workerCount;
    
    ConstructionProgress += (int)progressPerTick;
    
    // Cap at 100%
    if (ConstructionProgress >= 100)
    {
        ConstructionProgress = 100;
        CompleteConstruction();
    }
}

private void CompleteConstruction()
{
    IsConstructed = true;
    
    // Release construction workers
    foreach (var worker in ConstructionWorkers.ToList())
    {
        worker.CurrentTask = PersonTask.Idle;
    }
    ConstructionWorkers.Clear();
    
    EventLog.Instance.AddMessage(
        $"{Type} construction completed at ({X}, {Y})",
        LogLevel.Success
    );
}
```

### Construction Progress Percentage

```csharp
public int GetConstructionProgressPercent()
{
    return Math.Min(100, ConstructionProgress);
}
```

**See:** [CONSTRUCTION_SYSTEM.md](CONSTRUCTION_SYSTEM.md) for detailed construction mechanics

---

## Building Placement

### Validation

```csharp
public static bool CanPlaceBuilding(
    Grid grid, 
    BuildingType type, 
    int x, 
    int y)
{
    var definition = BuildingDefinitions.Get(type);
    if (definition == null) return false;
    
    // Check bounds
    if (x < 0 || y < 0 || 
        x + definition.Width > grid.Width || 
        y + definition.Height > grid.Height)
    {
        return false;
    }
    
    // Check all tiles in footprint
    for (int dx = 0; dx < definition.Width; dx++)
    {
        for (int dy = 0; dy < definition.Height; dy++)
        {
            var tile = grid.GetTile(x + dx, y + dy);
            
            if (tile == null) return false;
            
            // Must be walkable terrain
            if (!tile.IsWalkable) return false;
            
            // Must not have existing building
            if (tile.Building != null) return false;
        }
    }
    
    return true;
}
```

### Placement

```csharp
public static Building PlaceBuilding(
    Grid grid, 
    BuildingType type, 
    int x, 
    int y)
{
    if (!CanPlaceBuilding(grid, type, x, y))
    {
        return null;
    }
    
    var definition = BuildingDefinitions.Get(type);
    
    var building = new Building
    {
        Id = GetNextBuildingId(),
        Type = type,
        Name = definition.Name,
        X = x,
        Y = y,
        Width = definition.Width,
        Height = definition.Height,
        IsConstructed = false,
        ConstructionProgress = 0
    };
    
    // Mark tiles as occupied
    for (int dx = 0; dx < definition.Width; dx++)
    {
        for (int dy = 0; dy < definition.Height; dy++)
        {
            var tile = grid.GetTile(x + dx, y + dy);
            tile.Building = building;
        }
    }
    
    return building;
}
```

---

## Multi-Tile Buildings

### Footprint System

Buildings can occupy multiple tiles:

```
House (2x2):        Farm (3x3):
?????              ???????
?????              ???????
?????              ???????
                   ???????
```

### Anchor Point

The building's `(X, Y)` position is the **top-left corner** of its footprint.

### Door Position

Buildings have entry points for pathfinding:

```csharp
public Vector2Int GetDoorPosition()
{
    // Default: center-bottom of building
    return new Vector2Int(
        X + Width / 2,
        Y + Height - 1
    );
}
```

### Interior Check

```csharp
public bool ContainsPosition(int x, int y)
{
    return x >= X && x < X + Width &&
           y >= Y && y < Y + Height;
}
```

---

## Worker Management

### Assigning Workers

```csharp
public bool CanAssignWorker(Person person)
{
    if (!IsConstructed) return false;
    
    var definition = BuildingDefinitions.Get(Type);
    
    // Check capacity
    if (Workers.Count >= definition.WorkerCapacity)
    {
        return false;
    }
    
    // Check if person is adult
    if (person.Age < 18)
    {
        return false;
    }
    
    return true;
}

public void AssignWorker(Person person)
{
    if (!CanAssignWorker(person)) return;
    
    // Remove from previous job
    if (person.AssignedBuilding != null)
    {
        person.AssignedBuilding.Workers.Remove(person);
    }
    
    // Assign to this building
    person.AssignedBuilding = this;
    Workers.Add(person);
    
    EventLog.Instance.AddMessage(
        $"{person.FirstName} assigned to work at {Type}",
        LogLevel.Info
    );
}

public void UnassignWorker(Person person)
{
    if (Workers.Remove(person))
    {
        person.AssignedBuilding = null;
    }
}
```

### Worker Capacity

```csharp
public int GetWorkerCapacity()
{
    var definition = BuildingDefinitions.Get(Type);
    return definition?.WorkerCapacity ?? 0;
}

public int GetAvailableWorkerSlots()
{
    return GetWorkerCapacity() - Workers.Count;
}

public bool IsFull()
{
    return Workers.Count >= GetWorkerCapacity();
}
```

---

## Resident Management (Houses)

### Housing Capacity

```csharp
public int GetCapacity()
{
    var definition = BuildingDefinitions.Get(Type);
    return definition?.ResidentCapacity ?? 0;
}

public int GetAvailableSpace()
{
    return GetCapacity() - Residents.Count;
}

public bool HasSpace(int requiredSpace)
{
    return GetAvailableSpace() >= requiredSpace;
}
```

### Assigning Residents

```csharp
public bool CanAssignFamily(Family family)
{
    if (Type != BuildingType.House) return false;
    if (!IsConstructed) return false;
    
    return HasSpace(family.Members.Count);
}

public void AssignFamily(Family family)
{
    if (!CanAssignFamily(family)) return;
    
    // Remove from previous home
    if (family.HomeBuilding != null)
    {
        foreach (var member in family.Members)
        {
            family.HomeBuilding.Residents.Remove(member);
        }
    }
    
    // Assign to this house
    family.HomeBuilding = this;
    family.HomePosition = new Vector2Int(X, Y);
    
    foreach (var member in family.Members)
    {
        member.HomeBuilding = this;
        Residents.Add(member);
    }
    
    EventLog.Instance.AddMessage(
        $"{family.FamilyName} family moved into house at ({X}, {Y})",
        LogLevel.Success
    );
}
```

---

## Resource Production

### Production Buildings

Buildings like Farms produce resources when staffed:

```csharp
public void UpdateProduction(int currentTick)
{
    if (!IsConstructed) return;
    if (Workers.Count == 0) return;
    
    var definition = BuildingDefinitions.Get(Type);
    if (definition.ProducedResource == null) return;
    
    // Produce every hour (3600 ticks)
    if (currentTick % 3600 == 0)
    {
        int baseProduction = definition.ProductionRate;
        
        // Scale by worker count (efficiency)
        float workerRatio = (float)Workers.Count / definition.WorkerCapacity;
        int actualProduction = (int)(baseProduction * workerRatio);
        
        // Add to inventory
        Inventory.Add(definition.ProducedResource.Value, actualProduction);
        
        EventLog.Instance.AddMessage(
            $"{Type} produced {actualProduction} {definition.ProducedResource}",
            LogLevel.Info
        );
    }
}
```

### Production Efficiency

```csharp
public float GetProductionEfficiency()
{
    if (!IsConstructed) return 0f;
    
    var definition = BuildingDefinitions.Get(Type);
    if (definition.WorkerCapacity == 0) return 1f;
    
    return (float)Workers.Count / definition.WorkerCapacity;
}
```

---

## Resource Storage

### Inventory System

```csharp
public class ResourceInventory
{
    private Dictionary<ResourceType, int> _resources = new();
    
    public int GetAmount(ResourceType type)
    {
        return _resources.TryGetValue(type, out int amount) ? amount : 0;
    }
    
    public void Add(ResourceType type, int amount)
    {
        if (!_resources.ContainsKey(type))
        {
            _resources[type] = 0;
        }
        
        _resources[type] += amount;
    }
    
    public bool Remove(ResourceType type, int amount)
    {
        int current = GetAmount(type);
        if (current < amount) return false;
        
        _resources[type] = current - amount;
        return true;
    }
    
    public bool CanStore(ResourceType type, int amount)
    {
        // Check against building's storage capacity
        int total = _resources.Values.Sum() + amount;
        return total <= building.GetStorageCapacity();
    }
}
```

### Storage Capacity

```csharp
public int GetStorageCapacity()
{
    var definition = BuildingDefinitions.Get(Type);
    return definition?.StorageCapacity ?? 0;
}

public int GetUsedStorage()
{
    return Inventory.GetTotalAmount();
}

public int GetAvailableStorage()
{
    return GetStorageCapacity() - GetUsedStorage();
}
```

---

## Rendering

### Building Glyph

```csharp
public char GetGlyph()
{
    var definition = BuildingDefinitions.Get(Type);
    return definition?.Glyph ?? '?';
}

public Color GetColor()
{
    if (!IsConstructed)
    {
        // Show construction stage as progressively brighter brown
        int brightness = 80 + (ConstructionProgress * 120 / 100);
        return new Color(
            (byte)brightness, 
            (byte)(brightness / 2), 
            0, 
            255
        );
    }
    
    var definition = BuildingDefinitions.Get(Type);
    return definition?.Color ?? Color.Gray;
}
```

### Multi-Tile Rendering

```csharp
public void Render(int cameraX, int cameraY)
{
    char glyph = GetGlyph();
    Color color = GetColor();
    
    // Render each tile of the building
    for (int dx = 0; dx < Width; dx++)
    {
        for (int dy = 0; dy < Height; dy++)
        {
            int screenX = (X + dx - cameraX) * TileSize;
            int screenY = (Y + dy - cameraY) * TileSize;
            
            // Only center tile shows glyph
            if (dx == Width / 2 && dy == Height / 2)
            {
                DrawConsoleText(
                    glyph.ToString(), 
                    screenX, 
                    screenY, 
                    FontSize, 
                    color
                );
            }
            else
            {
                // Other tiles show building base
                Raylib.DrawRectangle(
                    screenX, 
                    screenY, 
                    TileSize, 
                    TileSize, 
                    new Color(color.R / 2, color.G / 2, color.B / 2, 255)
                );
            }
        }
    }
}
```

---

## Special Building Behaviors

### Chimney Smoke (Houses)

Houses emit smoke when occupied at night:

```csharp
public bool ShouldEmitSmoke(GameTime time)
{
    if (Type != BuildingType.House) return false;
    if (Residents.Count == 0) return false;
    
    // Only at evening/night (6 PM - 6 AM)
    return time.IsNight();
}

public Vector2Int GetSmokeEmissionPoint()
{
    // Top-center of building
    return new Vector2Int(
        X + Width / 2,
        Y
    );
}
```

**See:** [VISUAL_ENHANCEMENTS.md](../../Rendering/VISUAL_ENHANCEMENTS.md) for smoke rendering

---

## Building Lifecycle

### Creation

```csharp
// 1. Command issued
var command = new ConstructBuildingCommand(
    playerId: 0,
    targetTick: engine.CurrentTick + 1,
    buildingType: BuildingType.House,
    x: 50,
    y: 50
);

engine.SubmitCommand(command);

// 2. Building placed (not constructed)
var building = PlaceBuilding(grid, BuildingType.House, 50, 50);
engine.Buildings.Add(building);

// 3. Workers assigned
var family = engine.Families[0];
AssignConstructionWorkersCommand assignCmd = new(...);
engine.SubmitCommand(assignCmd);

// 4. Construction progresses
// ... over multiple ticks ...

// 5. Construction completes
building.CompleteConstruction();

// 6. Building is operational
// Can now assign residents/workers
```

### Destruction (Future)

```csharp
public void Destroy(Grid grid)
{
    // Remove residents/workers
    foreach (var resident in Residents.ToList())
    {
        resident.HomeBuilding = null;
    }
    Residents.Clear();
    
    foreach (var worker in Workers.ToList())
    {
        worker.AssignedBuilding = null;
    }
    Workers.Clear();
    
    // Clear tiles
    for (int dx = 0; dx < Width; dx++)
    {
        for (int dy = 0; dy < Height; dy++)
        {
            var tile = grid.GetTile(X + dx, Y + dy);
            if (tile != null)
            {
                tile.Building = null;
            }
        }
    }
    
    EventLog.Instance.AddMessage(
        $"{Type} at ({X}, {Y}) was destroyed",
        LogLevel.Warning
    );
}
```

---

## Serialization

### Save Format

```csharp
[Serializable]
public class BuildingData
{
    public int Id { get; set; }
    public BuildingType Type { get; set; }
    public int X, Y, Width, Height { get; set; }
    
    public bool IsConstructed { get; set; }
    public int ConstructionProgress { get; set; }
    
    public List<int> WorkerIds { get; set; }
    public List<int> ResidentIds { get; set; }
    public List<int> ConstructionWorkerIds { get; set; }
    
    public ResourceInventoryData Inventory { get; set; }
}
```

**Person/Resource references saved as IDs** to avoid circular dependencies.

---

## Performance Considerations

### Building Updates

```csharp
// DON'T: Update every building every tick
foreach (var building in allBuildings)
{
    building.UpdateProduction(tick);  // Expensive!
}

// DO: Update only active production buildings
if (tick % 3600 == 0)  // Once per hour
{
    foreach (var building in productionBuildings)
    {
        building.UpdateProduction(tick);
    }
}
```

### Building Lookups

```csharp
// Cache building references
private Dictionary<int, Building> _buildingCache = new();

public Building GetBuilding(int id)
{
    return _buildingCache.TryGetValue(id, out var building) 
        ? building 
        : null;
}
```

---

## Future Enhancements

### Planned Features

1. **Building Upgrades**
   - Increase capacity/production
   - Visual changes
   - Cost resources

2. **Building Maintenance**
   - Degradation over time
   - Repair requirements
   - Conditional functionality

3. **Special Buildings**
   - Markets (trade)
   - Hospitals (healthcare)
   - Schools (education/skills)
   - Defense structures

4. **Building Rotation**
   - 4 orientations (N, E, S, W)
   - Door position changes
   - Visual variety

5. **Building Destruction**
   - Natural disasters
   - Manual demolition
   - Resource recovery

---

## Related Documentation

- [Construction System](CONSTRUCTION_SYSTEM.md) - Construction mechanics in detail
- [People & Families](PEOPLE_AND_FAMILIES.md) - Worker/resident management
- [Resource Management](RESOURCE_MANAGEMENT.md) - Resource production/storage
- [Command System](../../CoreSystems/COMMAND_SYSTEM.md) - Building commands

---

## Example Usage

### Build a House

```csharp
// Place building
var command = new ConstructBuildingCommand(0, tick + 1, BuildingType.House, 50, 50);
engine.SubmitCommand(command);

// Assign construction workers
var assignCmd = new AssignConstructionWorkersCommand(0, tick + 1, familyId: 1, buildingId: newBuilding.Id);
engine.SubmitCommand(assignCmd);

// Wait for construction to complete...

// Assign family to live there
var homeCmd = new AssignFamilyHomeCommand(0, tick + 1, familyId: 1, buildingId: newBuilding.Id);
engine.SubmitCommand(homeCmd);
```

### Start a Farm

```csharp
// Build farm
var command = new ConstructBuildingCommand(0, tick + 1, BuildingType.Farm, 60, 60);
engine.SubmitCommand(command);

// After construction completes...

// Assign workers
var jobCmd = new AssignFamilyJobCommand(0, tick + 1, familyId: 2, buildingId: farm.Id);
engine.SubmitCommand(jobCmd);

// Farm produces food hourly automatically
```

---

**Maintained By:** VillageBuilder Development Team  
**Last Updated:** 2024-01-XX
