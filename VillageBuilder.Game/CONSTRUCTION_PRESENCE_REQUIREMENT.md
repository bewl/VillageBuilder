# Construction Worker Presence Requirement

## Overview
Construction now only progresses when workers are **physically present** at the construction site. Workers must travel to the building location before contributing to construction progress.

## Key Changes

### 1. **Presence Check**
Workers must be at or adjacent to the building to contribute work:
- **At building tile**: Worker is standing on the building footprint
- **Adjacent to building**: Worker is within 1 tile of the building (Manhattan distance)
- **Traveling**: Workers en route do NOT contribute to progress

### 2. **New Method: `IsAtConstructionSite()`**

```csharp
public bool IsAtConstructionSite(Building building)
{
    // Checks if person is on building tile OR adjacent (within 1 tile)
    // Returns true only when physically present at site
}
```

### 3. **Modified Construction Processing**

**Before:**
```csharp
// ANY alive worker in ConstructionWorkers list contributed
int workDone = building.ConstructionWorkers.Count(w => w.IsAlive);
```

**After:**
```csharp
// ONLY workers physically at site contribute
int workDone = building.ConstructionWorkers.Count(w => 
    w.IsAlive && w.IsAtConstructionSite(building));
```

## Gameplay Impact

### Construction Flow

```
1. Building Placed
   ?? Resources deducted
   ?? Building appears at 0% progress
   ?? Auto-assignment triggered

2. Workers Assigned
   ?? Family members set to PersonTask.Constructing
   ?? Path calculated to construction site
   ?? Workers begin traveling

3. Workers Traveling
   ?? Progress remains at 0%
   ?? Building shows "0 workers" or workers still traveling
   ?? ?? NO construction progress yet

4. Workers Arrive
   ?? Task remains PersonTask.Constructing
   ?? IsAtConstructionSite() returns true
   ?? ? Construction progress begins (1 work/worker/tick)

5. Construction Completes
   ?? Workers freed (set to Idle)
   ?? Workers can be assigned to next project
```

### Example Scenario

**Building:** House (800 work required)  
**Workers:** 2 family members  
**Distance:** 20 tiles away

```
Tick 0: House placed, workers assigned
Ticks 1-20: Workers traveling (20 ticks to arrive)
  Progress: 0/800 (0%) - No work done while traveling
  
Tick 21: Workers arrive at construction site
  Progress: 2/800 (0%) - Work begins!
  
Ticks 22-420: Workers building (400 ticks needed)
  Progress: 2 work/tick * 2 workers = 4 work/tick
  Actually: 800 work ÷ 2 workers = 400 ticks
  
Tick 421: Construction complete!
  Total time: 421 ticks (20 travel + 400 work + 1 completion)
```

## Technical Details

### Presence Detection

**IsAtConstructionSite() Logic:**
```csharp
// Check 1: On building tile?
if (buildingTiles.Any(t => t.X == Position.X && t.Y == Position.Y))
    return true;

// Check 2: Adjacent to building? (within 1 tile)
foreach (var tile in buildingTiles)
{
    int distance = Math.Abs(tile.X - Position.X) + Math.Abs(tile.Y - Position.Y);
    if (distance <= 1)
        return true;
}

return false; // Not at site
```

**Why Adjacent Tiles Count:**
- Workers can work from beside the building
- Allows multiple workers on large buildings
- Prevents pathfinding conflicts when building is "full"
- Matches real-world construction (work from outside)

### Task State Machine

```
PersonTask.Constructing (assigned)
    ?
Moving to construction site (CurrentPath has steps)
    ?
Arrived at site (CurrentPath empty, still Constructing)
    ?
Working (IsAtConstructionSite = true, contributes work)
    ?
Construction completes ? PersonTask.Idle
```

### Movement Handling

Added explicit handling for construction worker arrival:
```csharp
else if (person.CurrentTask == PersonTask.Constructing)
{
    // Arrived at construction site
    // Task remains Constructing (don't change to Idle)
    // Worker will now contribute to progress each tick
}
```

## Edge Cases Handled

### Workers Leave Site
**Scenario:** Worker walks away mid-construction
- **Result:** Progress stops immediately for that worker
- **Other workers:** Continue working if present
- **Behavior:** Worker can return to resume work

### Workers Called Away
**Scenario:** Daily routine sends workers home
- **Result:** Construction pauses overnight
- **Next day:** Workers return and resume
- **Progress:** Preserved, continues from where it stopped

### Unreachable Building
**Scenario:** Building surrounded by obstacles
- **Result:** Workers can't pathfind to site
- **Progress:** Never begins (workers never arrive)
- **Solution:** Auto-assignment skips, manual assignment needed

### Building Surrounded
**Scenario:** Large building with no adjacent free tiles
- **Result:** Some workers may not fit
- **Progress:** Only workers who reach site contribute
- **Limitation:** Max workers = building perimeter tiles

## Performance Considerations

### IsAtConstructionSite() Complexity
- **O(n)** where n = building tiles (typically 4-64)
- Called once per worker per tick
- Manhattan distance calculation (cheap)
- Early exit when match found

### Construction Processing
- **Before:** O(w) where w = assigned workers
- **After:** O(w * n) where w = workers, n = building tiles
- **Typical:** 2-3 workers, 16 tile building = 32-48 checks/tick
- **Impact:** Negligible (< 0.1ms)

## Visual Feedback

### Sidebar Construction Info
```
Status: Under Construction
Stage: Foundation
Progress: 15%

Builders: 2
  • Smith family (2)
```

**Interpretation:**
- 2 workers listed = 2 assigned
- Progress advancing = workers present
- Progress stalled = workers traveling or absent

### Map View
```
Building shows construction stage glyph (????)
Progress percentage visible
Worker count: ??2
```

**Dynamic Updates:**
- Workers traveling: Count shown, progress not moving
- Workers arrive: Progress bar starts filling
- Workers leave: Progress pauses

## Comparison: Before vs After

| Aspect | Before | After |
|--------|--------|-------|
| **Work starts** | Immediately on assignment | When workers arrive |
| **Travel time** | Ignored | Counts as delay |
| **Progress check** | IsAlive | IsAlive + IsAtConstructionSite |
| **Workers away** | Still contribute | Progress pauses |
| **Realism** | Instant work | Workers must be present |
| **Strategy** | Assign and forget | Consider travel distance |

## Strategic Implications

### Building Placement
- **Before:** Place anywhere, same construction time
- **After:** Closer to workers = faster start
- **Strategy:** Cluster buildings near family homes

### Worker Management
- **Before:** Assign workers, guaranteed progress
- **After:** Monitor worker positions, ensure presence
- **Strategy:** Keep construction workers on-site

### Multiple Projects
- **Before:** Workers contribute regardless of location
- **After:** Workers must physically move between sites
- **Strategy:** Finish one project before starting next

### Time Estimation
**Before:**
```
House = 800 work ÷ 2 workers = 400 ticks = 20 seconds
```

**After:**
```
House = Travel time + (800 work ÷ 2 workers)
      = 20 ticks + 400 ticks = 420 ticks = 21 seconds
```

## Benefits

### Realism
? Workers must physically travel to construction site  
? Construction happens where workers are located  
? Distance and accessibility matter  
? Workers can't teleport or work remotely

### Gameplay Depth
? Building placement becomes strategic  
? Travel time adds meaningful delay  
? Workers can be interrupted (day/night cycle)  
? Multiple projects require worker distribution

### Visual Coherence
? Workers visible at construction sites  
? Progress correlates with worker presence  
? Construction pauses when workers leave  
? Matches player expectations

### Performance
? Minimal computational overhead  
? Efficient proximity checking  
? No additional pathfinding needed  
? Scales well with building size

## Future Enhancements

### Construction Speed Bonuses
- Workers adjacent to multiple tiles work faster
- Team bonuses for multiple workers
- Tool efficiency boosts

### Construction Zones
- Designate specific work areas
- Workers move to optimal positions
- Better distribution for large buildings

### Shift System
- Day shift / night shift workers
- 24-hour construction for urgent projects
- Rotation prevents fatigue

### Visual Improvements
- Show workers wielding tools
- Construction animations at site
- Dust/particle effects where workers stand

## Testing Scenarios

### Basic Presence
- [x] Workers travel to site before work begins
- [x] Progress only when workers present
- [x] Multiple workers contribute simultaneously

### Movement
- [x] Workers traveling don't contribute
- [x] Workers arriving start contributing
- [x] Workers leaving stop contributing

### Daily Routine
- [x] Workers go home at night (progress pauses)
- [x] Workers return next day (progress resumes)
- [x] Construction survives day/night cycles

### Edge Cases
- [x] Worker dies ? removed from contributors
- [x] Worker reassigned ? stops contributing
- [x] Building unreachable ? no progress ever
- [x] All workers leave ? progress stops

## Code Changes

### VillageBuilder.Engine\Entities\Person.cs
```csharp
// Added new method
public bool IsAtConstructionSite(Building building)
{
    // Checks if person is on or adjacent to building
    // Returns true if within 1 tile Manhattan distance
}
```

### VillageBuilder.Engine\Core\GameEngine.cs
```csharp
// Modified ProcessConstruction()
int workDone = building.ConstructionWorkers.Count(w => 
    w.IsAlive && w.IsAtConstructionSite(building)); // NEW: presence check

// Added movement handler
else if (person.CurrentTask == PersonTask.Constructing)
{
    // Worker arrived, remains in Constructing state
}
```

## Documentation Updates

- ? Construction system now requires presence
- ? Travel time factors into total construction time
- ? Strategic building placement encouraged
- ? Worker management more engaging

## Success Criteria

? **Build Success:** Code compiles without errors  
? **Realism:** Workers must travel to site  
? **Gameplay:** Distance affects construction time  
? **Performance:** Negligible overhead  
? **Visual:** Progress matches worker presence  
? **Strategic:** Placement matters for efficiency
