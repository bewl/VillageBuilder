# Wildlife System

## Overview

The Wildlife System introduces dynamic animal AI with predator-prey relationships, hunting mechanics, and ecosystem management. Animals are no longer static decorations but living entities that move, eat, breed, hunt, and flee.

## Table of Contents
- [Architecture](#architecture)
- [Wildlife Types](#wildlife-types)
- [Behavior System](#behavior-system)
- [AI System](#ai-system)
- [Hunting System](#hunting-system)
- [Ecosystem Management](#ecosystem-management)
- [Integration](#integration)
- [Usage Examples](#usage-examples)

---

## Architecture

### Core Components

```
VillageBuilder.Engine/
??? Entities/
?   ??? Wildlife/
?       ??? WildlifeType.cs          # Animal types enum
?       ??? WildlifeBehavior.cs      # Behavior states enum
?       ??? WildlifeEntity.cs        # Main wildlife entity class
??? Systems/
?   ??? WildlifeManager.cs           # Population & spawning
?   ??? WildlifeAI.cs                # AI behavior logic
?   ??? HuntingSystem.cs             # Hunting mechanics
??? World/
    ??? Tile.cs                      # Wildlife tile tracking
```

### Class Hierarchy

```
WildlifeEntity
??? Identity (Id, Type, Name, Gender, Age)
??? Position & Movement (Position, Path, Speed)
??? Stats (Health, Hunger, Fear, Energy)
??? Behavior (CurrentBehavior, HomeTerritory)
??? Relationships (PreyTypes, PredatorTypes)

WildlifeManager
??? Population Management
??? Spawning Logic
??? Ecosystem Balancing
??? Wildlife Queries

WildlifeAI
??? Behavior Decision Making
??? Pathfinding Integration
??? Threat Detection
??? Predator/Prey Interactions

HuntingSystem
??? Hunt Success Calculation
??? Resource Collection
??? Overhunting Warnings
```

---

## Wildlife Types

### Prey Animals (Herbivores)

#### ?? Rabbit
- **Health**: 30
- **Speed**: 2.0
- **Behavior**: Fast, skittish, breeds quickly
- **Habitat**: Grasslands, forests
- **Predators**: Wolf, Fox
- **Resources**: 5 Meat
- **Hunting Success**: 80%
- **Maturity Age**: 15 days

#### ?? Deer
- **Health**: 80
- **Speed**: 1.8
- **Behavior**: Herd animal, alert, fast runners
- **Habitat**: Forests, grasslands
- **Predators**: Wolf, Bear
- **Resources**: 20 Meat
- **Hunting Success**: 45%
- **Maturity Age**: 30 days

#### ?? Boar
- **Health**: 100
- **Speed**: 1.5
- **Behavior**: Less skittish, defensive
- **Habitat**: Forests
- **Predators**: Wolf, Bear
- **Resources**: 30 Meat
- **Hunting Success**: 40%
- **Maturity Age**: 40 days

#### ?? Turkey
- **Health**: 20
- **Speed**: 1.5
- **Behavior**: Ground bird, flighty
- **Habitat**: Grasslands
- **Predators**: Fox
- **Resources**: 3 Meat
- **Hunting Success**: 65%
- **Maturity Age**: 10 days

#### ?? Duck
- **Health**: 20
- **Speed**: 1.5
- **Behavior**: Water-loving bird
- **Habitat**: Water, grasslands
- **Predators**: Fox
- **Resources**: 3 Meat
- **Hunting Success**: 75%
- **Maturity Age**: 10 days

#### ?? Bird
- **Health**: 20
- **Speed**: 1.5
- **Behavior**: Small, flighty
- **Habitat**: Grasslands
- **Predators**: Fox
- **Resources**: 3 Meat
- **Hunting Success**: 70%
- **Maturity Age**: 10 days

### Predators (Carnivores)

#### ?? Fox
- **Health**: 60
- **Speed**: 2.2
- **Behavior**: Cunning, fast, solitary hunter
- **Habitat**: Forests, grasslands
- **Prey**: Rabbit, Bird
- **Resources**: 3 Fur
- **Hunting Success**: 50%
- **Detection Range**: 12 tiles
- **Maturity Age**: 45 days

#### ?? Wolf
- **Health**: 100
- **Speed**: 2.5
- **Behavior**: Pack hunter, aggressive
- **Habitat**: Forests, mountains
- **Prey**: Rabbit, Deer
- **Resources**: 5 Fur
- **Hunting Success**: 25%
- **Detection Range**: 15 tiles
- **Maturity Age**: 60 days

#### ?? Bear
- **Health**: 150
- **Speed**: 1.3
- **Behavior**: Apex predator, very dangerous
- **Habitat**: Forests, mountains
- **Prey**: Deer, Boar
- **Resources**: 50 Meat, 8 Fur
- **Hunting Success**: 15%
- **Detection Range**: 12 tiles
- **Maturity Age**: 90 days

---

## Behavior System

### Behavior States

Wildlife can be in one of nine behavior states:

```csharp
public enum WildlifeBehavior
{
    Idle,           // Standing still, deciding next action
    Grazing,        // Eating vegetation (herbivores)
    Wandering,      // Moving randomly within territory
    Fleeing,        // Running from danger
    Hunting,        // Chasing prey (predators)
    Eating,         // Consuming killed prey
    Resting,        // Recovering energy
    Breeding,       // Mating behavior
    Dead            // Animal has died
}
```

### State Transitions

```
???????????
?  Idle   ????????????????????
???????????                  ?
     ?                       ?
     ???? Grazing (if hungry & herbivore)
     ???? Hunting (if hungry & predator)
     ???? Resting (if tired)
     ???? Breeding (if conditions met)
     ???? Wandering (random)
     ?
     ?
???????????     ???????????
?Wandering??????? Fleeing ?
???????????     ???????????
     ?               ?
     ?               ?
     ????(threat)?????
```

### Stats System

Each wildlife entity tracks:

- **Health** (0-100): Dies at 0
- **Hunger** (0-100): Increases over time, causes health loss if > 80
- **Fear** (0-100): Triggers fleeing behavior if > 40
- **Energy** (0-100): Depleted by movement, recovered by resting
- **Age** (days): Determines maturity for breeding

---

## AI System

### Decision Making

The AI updates every tick and follows this priority:

1. **Threat Detection** (highest priority)
   - Check for nearby people (all animals fear humans)
   - Check for nearby predators (prey animals only)
   - If threat detected and fear > 40 ? Flee

2. **Survival Needs**
   - If energy < 30 ? Rest
   - If hunger > 60 ? Eat (herbivores graze, predators hunt)

3. **Reproduction**
   - If can breed (mature, low hunger, low energy cost) ? Seek mate
   - 5% chance per tick to attempt breeding

4. **Exploration**
   - 10% chance per tick to start wandering
   - Wander within territory radius

### Threat Detection

```csharp
// People are threats to all animals
var nearbyPeople = allPeople
    .Where(p => Distance(wildlife.Position, p.Position) <= wildlife.DetectionRange)
    .ToList();

if (nearbyPeople.Any())
{
    wildlife.Fear += 20;
    if (wildlife.Fear >= 40)
        StartFleeing(wildlife, nearbyPeople[0].Position);
}

// Predators are threats to prey
var nearbyPredator = wildlifeManager.GetNearestPredator(wildlife);
if (nearbyPredator != null)
{
    wildlife.Fear += 30;
    if (wildlife.Fear >= 40)
        StartFleeing(wildlife, nearbyPredator.Position);
}
```

### Fleeing Behavior

When fleeing, animals:
1. Calculate opposite direction from threat
2. Extend flee distance based on animal type
3. Find walkable tile in that direction
4. Use A* pathfinding to escape
5. Continue fleeing until fear < 40

### Hunting Behavior

Predators hunt when hungry:
1. Detect prey within detection range
2. Calculate path to prey
3. Chase prey (recalculate path if prey moves)
4. Attack when within 1 tile
5. Kill with damage roll (20-40 damage)
6. Enter Eating state upon successful kill

### Breeding System

Breeding requirements:
- Both animals mature (age >= MaturityAge)
- Both same species and opposite gender
- Both within 5 tiles
- Both hunger < 50
- Both energy > 40
- Both breeding cooldown = 0

When breeding occurs:
- Spawn offspring nearby (within 2 tiles)
- Offspring starts at random age (MaturityAge/2 to MaturityAge*2)
- Both parents get breeding cooldown (100 ticks)

---

## Hunting System

### Hunting Mechanics

Villagers can hunt wildlife for resources:

```csharp
var huntingSystem = new HuntingSystem(grid, wildlifeManager, villageResources, random);
var result = huntingSystem.TryHunt(hunter, targetAnimal);
```

### Success Rates

| Animal | Success Rate | Difficulty |
|--------|-------------|-----------|
| Rabbit | 80% | Easy - small and slow |
| Duck | 75% | Easy - not very alert |
| Bird | 70% | Moderate - quick |
| Turkey | 65% | Moderate - bigger bird |
| Fox | 50% | Hard - cunning and fast |
| Deer | 45% | Hard - fast and alert |
| Boar | 40% | Hard - fights back |
| Wolf | 25% | Very Hard - dangerous |
| Bear | 15% | Extremely Hard - apex predator |

### Resource Drops

Successful hunts provide:

**Meat Resources**
- Rabbit: 5 Meat
- Bird/Duck/Turkey: 3 Meat
- Deer: 20 Meat
- Boar: 30 Meat
- Bear: 50 Meat

**Fur Resources**
- Fox: 3 Fur
- Wolf: 5 Fur
- Bear: 8 Fur

### Failed Hunts

When a hunt fails:
- Animal's fear increases to 100 (maximum)
- Animal immediately flees from hunter
- No resources gained
- Event log records the escape

### Overhunting Warnings

The system monitors ecosystem balance:
- **Critical** (< 10 prey): Error-level warning
- **Low** (< 20 prey): Warning-level alert
- Automatic prey spawning if too low

---

## Ecosystem Management

### Population Limits

- **Maximum Wildlife**: 150 entities
- **Minimum Prey**: 20 animals
- **Predator/Prey Ratio**: 1:5 (1 predator per 5 prey)

### Initial Population

When a game starts, wildlife spawns:
- 30 Rabbits
- 15 Deer
- 8 Boar
- 20 Birds
- 10 Ducks/Turkeys
- 5 Foxes
- 3 Wolves
- 2 Bears

**Total**: ~93 animals

### Ecosystem Balancing

Runs every in-game hour:

**Prey Population Too Low** (< 20)
- Spawn additional rabbits
- Occurs when overhunting or predation is excessive

**Too Many Predators** (> prey / 5)
- Natural die-off of hungry predators
- Predators with hunger > 70 have chance to starve

**Corpse Decay**
- 5% chance per tick for dead wildlife to be removed
- Prevents entity buildup

### Biome-Based Spawning

Animals spawn in appropriate biomes:

| Animal | Preferred Biomes |
|--------|-----------------|
| Rabbit, Deer, Fox | Grass, Forest |
| Boar, Wolf, Bear | Forest, Mountain |
| Bird, Turkey | Grass |
| Duck | Grass, Water |

---

## Integration

### GameEngine Integration

Wildlife is updated in `GameEngine.SimulateTick()`:

```csharp
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
```

### Tile Tracking

Tiles track wildlife similar to people:

```csharp
public class Tile
{
    public List<Person> PeopleOnTile { get; }
    public List<WildlifeEntity> WildlifeOnTile { get; }  // NEW
}
```

### Selection System

Wildlife can be selected like people:

```csharp
public enum SelectionType
{
    None,
    Person,
    Building,
    Tile,
    Wildlife  // NEW
}

public class SelectionManager
{
    public WildlifeEntity? SelectedWildlife { get; }
    public List<WildlifeEntity>? WildlifeAtSelectedTile { get; }
    
    public void SelectWildlife(WildlifeEntity wildlife);
    public void SelectWildlifeAtTile(List<WildlifeEntity> wildlife, int index = 0);
    public void CycleNextWildlife();
    public void CyclePreviousWildlife();
}
```

### Resource System

New resource types added:

```csharp
public enum ResourceType
{
    // Existing resources...
    Meat,      // Already existed - food from hunting
    Fur,       // NEW - pelts from predators
}
```

### Person Tasks

New task for hunting:

```csharp
public enum PersonTask
{
    // Existing tasks...
    Hunting,   // NEW - hunting wildlife
}
```

---

## Usage Examples

### Spawning Wildlife

```csharp
// Spawn specific animal at location
var rabbit = wildlifeManager.SpawnWildlife(WildlifeType.Rabbit, new Vector2Int(50, 50));

// Spawn in suitable biome (automatic)
var deer = wildlifeManager.SpawnWildlife(WildlifeType.Deer);
```

### Hunting

```csharp
// Find nearest huntable animal
var target = huntingSystem.GetNearestHuntableWildlife(hunter, maxDistance: 10);

if (target != null && GetDistance(hunter.Position, target.Position) <= 1)
{
    var result = huntingSystem.TryHunt(hunter, target);
    
    if (result.Success)
    {
        // Resources automatically added to village storage
        Console.WriteLine($"Hunted {target.Type}! Gained {result.Message}");
    }
    else
    {
        Console.WriteLine($"Hunt failed: {result.Message}");
    }
}
```

### Querying Wildlife

```csharp
// Get all wildlife in range
var nearbyAnimals = wildlifeManager.GetWildlifeInRange(position, radius: 10);

// Get nearest specific type
var nearestRabbit = wildlifeManager.GetNearestWildlife(position, WildlifeType.Rabbit);

// Get population statistics
var stats = wildlifeManager.GetPopulationStats();
Console.WriteLine($"Rabbits: {stats[WildlifeType.Rabbit]}");
Console.WriteLine($"Wolves: {stats[WildlifeType.Wolf]}");
```

### Checking Overhunting

```csharp
// Manually check for overhunting
huntingSystem.CheckOverhuntingWarning();

// Automatically called by ecosystem balancing every hour
```

### Wildlife Selection

```csharp
// Select individual wildlife
selectionManager.SelectWildlife(wildlifeEntity);

// Select wildlife at tile (with cycling)
var tile = grid.GetTile(x, y);
if (tile.WildlifeOnTile.Count > 0)
{
    selectionManager.SelectWildlifeAtTile(tile.WildlifeOnTile);
    
    // Cycle through multiple wildlife on same tile
    selectionManager.CycleNextWildlife();
}

// Check selection
if (selectionManager.CurrentSelectionType == SelectionType.Wildlife)
{
    var selected = selectionManager.SelectedWildlife;
    Console.WriteLine($"Selected: {selected.Name} ({selected.Type})");
    Console.WriteLine($"Health: {selected.Health}, Hunger: {selected.Hunger}");
    Console.WriteLine($"Behavior: {selected.CurrentBehavior}");
}
```

---

## Performance Considerations

### Optimizations

1. **Tile Tracking**: HashSet for O(wildlife count) instead of O(grid size)
   ```csharp
   private readonly HashSet<(int x, int y)> _wildlifeOccupiedTiles;
   ```

2. **Dead Wildlife Cleanup**: 5% chance per tick for corpse removal
   ```csharp
   _wildlife.RemoveAll(w => !w.IsAlive && _random.Next(100) < 5);
   ```

3. **Population Limits**: Hard cap at 150 wildlife entities

4. **Selective Updates**: Only alive wildlife execute AI

### Benchmarking

Wildlife state is included in benchmark snapshots:

```csharp
private class GameEngineSnapshot
{
    public List<(WildlifeEntity, Vector2Int, WildlifeBehavior, int, int, int)> WildlifeStates;
}
```

This ensures consistent wildlife state across benchmark iterations.

---

## Future Enhancements

### Potential Features

1. **Pack Behavior** - Wolves hunting in coordinated packs
2. **Migration** - Seasonal animal movement
3. **Dens/Nests** - Wildlife home locations
4. **Domestication** - Taming and farming animals
5. **Animal Sounds** - Audio feedback for wildlife
6. **Predator Attacks** - Wildlife attacking villagers
7. **Animal Diseases** - Infections spreading through populations
8. **Fishing System** - Catching fish in water
9. **Traps** - Passive hunting mechanics
10. **Wildlife Events** - Animal invasions, stampedes

### AI Improvements

- **Smarter Fleeing**: Animals remember threat locations
- **Group Behavior**: Herd animals flee together
- **Territorial Fighting**: Predators defend territory
- **Hunting Cooperation**: Pack hunting mechanics
- **Learning**: Animals adapt to player behavior

---

## Debugging

### Console Logging

Wildlife AI includes detailed logging:
- Path finding: `[MOVE] Fox starting journey...`
- Hunting: `[HUNT] Wolf chasing Rabbit at (50, 50)`
- Fleeing: `[FLEE] Deer running from Person at (45, 45)`

### Stats Display

Check wildlife stats for debugging:
```csharp
Console.WriteLine($"Wildlife: {wildlife.Name}");
Console.WriteLine($"Health: {wildlife.Health}, Energy: {wildlife.Energy}");
Console.WriteLine($"Hunger: {wildlife.Hunger}, Fear: {wildlife.Fear}");
Console.WriteLine($"Behavior: {wildlife.CurrentBehavior}");
Console.WriteLine($"Position: ({wildlife.Position.X}, {wildlife.Position.Y})");
```

### Population Monitoring

```csharp
var stats = wildlifeManager.GetPopulationStats();
Console.WriteLine($"Total Wildlife: {wildlifeManager.WildlifeCount}");
foreach (var kvp in stats)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}
```

---

## Related Documentation

- [People & Families](../EntitiesAndBuildings/PEOPLE_AND_FAMILIES.md)
- [Pathfinding](../WorldAndSimulation/PATHFINDING.md)
- [Resource Management](../EntitiesAndBuildings/RESOURCE_MANAGEMENT.md)
- [Grid & Tiles](../WorldAndSimulation/GRID_AND_TILES.md)
- [Game Engine](../CoreSystems/GAME_ENGINE.md)
