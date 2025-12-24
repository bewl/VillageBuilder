# Automatic Construction Worker Assignment

## Overview
The system now automatically assigns idle families to buildings under construction, eliminating the need for manual worker assignment. When multiple buildings need construction, families are distributed evenly across projects.

## Key Features

### 1. **Immediate Auto-Assignment**
When a building is placed:
- System immediately scans for idle families
- Assigns available workers to the new construction site
- Shows log message: "The [Family] family (X workers) automatically assigned to construct the [BuildingType]"

### 2. **Round-Robin Distribution**
When multiple buildings need workers:
- Families are distributed across buildings in round-robin fashion
- Example: 3 idle families + 3 buildings = 1 family per building
- Ensures all projects progress simultaneously
- Oldest buildings (by ID) get priority

### 3. **Continuous Monitoring**
- System checks every hour (game time) for idle families
- Automatically assigns them to buildings without workers
- No manual intervention needed

### 4. **Worker Reallocation**
When construction completes:
- Workers are freed (set to Idle status)
- System immediately checks for other buildings needing workers
- Freed workers automatically move to next construction project
- Creates a natural workflow: Build 1 ? Build 2 ? Build 3

### 5. **Smart Assignment Logic**
Only assigns workers when:
- Family has idle adults (age 18+)
- Building has no construction workers assigned
- Path to building is accessible
- Person can reach the construction site

## Implementation Details

### Auto-Assignment Triggers

1. **Building Placement** (immediate)
   ```
   Player places building ? ConstructBuildingCommand.Execute() 
   ? engine.TriggerAutoConstructionAssignment()
   ```

2. **Hourly Check** (during game tick)
   ```
   Time advances 1 hour ? SimulateTick() 
   ? AutoAssignConstructionWorkers()
   ```

3. **Construction Completion** (immediate)
   ```
   Building completes ? CompleteConstruction() 
   ? Workers freed ? AutoAssignConstructionWorkers()
   ```

### Distribution Algorithm

```csharp
// Pseudocode
buildingsNeedingWorkers = Buildings without workers (oldest first)
idleFamilies = Families with idle adults

buildingIndex = 0
foreach family in idleFamilies:
    if no more buildings need workers: break
    
    building = buildingsNeedingWorkers[buildingIndex]
    assign family to building
    
    buildingIndex++ // Move to next building (round-robin)
```

### Priority System
Buildings are assigned workers in order of:
1. **Building ID** (oldest first) - ensures oldest projects complete first
2. **Distance** (path calculation) - workers only assigned if reachable

## Player Experience

### Before Auto-Assignment
```
1. Place house
2. Open sidebar
3. Click building
4. Click idle family
5. Repeat for each building
```

### With Auto-Assignment
```
1. Place house
2. Workers automatically assigned!
3. (Optional) Manually reassign if desired
```

### Multiple Buildings Scenario

**Example:** Player places 3 houses with 2 idle families

```
Tick 0:
- Player places House #1
- Family A assigned to House #1
- Log: "Smith family (2 workers) automatically assigned to construct House"

Tick 5:
- Player places House #2
- Family B assigned to House #2
- Log: "Johnson family (2 workers) automatically assigned to construct House"

Tick 10:
- Player places House #3
- No idle families available
- House #3 waits for workers

Tick 500:
- House #1 construction completes
- Family A freed (now idle)
- Family A automatically assigned to House #3
- Log: "Smith family (2 workers) automatically assigned to construct House"
```

## Configuration

### Timing
- **Immediate**: Building placement, construction completion
- **Hourly**: Regular check for idle families
- **On-demand**: Manual trigger via `engine.TriggerAutoConstructionAssignment()`

### Constants (in AutoAssignConstructionWorkers)
```csharp
// Buildings needing workers (filter)
!building.IsConstructed && building.ConstructionWorkers.Count == 0

// Family qualification (filter)
family.GetIdleMembers().Any() && person.Age >= 18

// Assignment order
.OrderBy(b => b.Id) // Oldest buildings first
```

## Manual Override

Players can still manually assign families:
1. Click building in sidebar
2. Click family button
3. Manual assignment takes precedence
4. Auto-assignment skips buildings with workers

## Edge Cases Handled

### No Idle Families
- System silently returns (no spam)
- Buildings remain in queue for next check

### All Buildings Have Workers
- System returns immediately
- No unnecessary processing

### Pathfinding Fails
- Worker not assigned to unreachable building
- Other family members still attempt assignment
- Building remains available for next check

### Family Has Working Members
- Only idle adults (18+) are assigned
- Working family members continue their jobs
- Construction workers counted separately from production workers

## Performance Impact

### Computation Cost
- O(n) where n = idle families (typically 0-3)
- O(m) where m = buildings without workers (typically 0-5)
- Runs once per hour (game time) + on key events
- Negligible performance impact

### Memory
- No additional data structures
- Uses existing Family.GetIdleMembers()
- Uses existing Building.ConstructionWorkers list

## Multiplayer Compatibility

### Deterministic Behavior
- Uses building ID (deterministic) for ordering
- Pathfinding is deterministic
- No random assignment
- All clients see same assignments

### Command Queue Integration
- Auto-assignment happens in game engine (not command)
- Runs after command execution
- Synchronized across all clients

## Future Enhancements

### Priority Flags
- Allow player to mark buildings as "high priority"
- Auto-assignment prefers high priority buildings

### Family Preferences
- Families prefer buildings near their home
- Distance-based assignment

### Skill-Based Assignment
- Assign experienced builders first
- Track construction speed bonuses

### Construction Teams
- Allow multiple families on one large building
- Coordinate teams for faster completion

### Assignment Notifications
- Separate log level for auto-assignments
- Optional: Disable auto-assignment notifications
- Show summary: "3 families assigned to 3 buildings"

## Testing Scenarios

### Basic Assignment
- [x] Place building ? idle family assigned immediately
- [x] No idle families ? building waits
- [x] Family reaches site ? construction progresses

### Multiple Buildings
- [x] 2 buildings + 2 families = 1 family each
- [x] 3 buildings + 2 families = 2 assigned, 1 waits
- [x] Round-robin distribution works

### Worker Reallocation
- [x] Building completes ? workers freed
- [x] Freed workers ? assigned to next building
- [x] Chain construction: A?B?C works

### Edge Cases
- [x] No path to building ? skip assignment
- [x] Building already has workers ? skip
- [x] All families busy ? no assignments
- [x] Hourly check doesn't spam logs

### Manual Override
- [x] Manual assignment still works
- [x] Auto-assignment respects manual assignments
- [x] Can reassign workers between buildings

## Code Files Modified

### VillageBuilder.Engine\Core\GameEngine.cs
- Added `AutoAssignConstructionWorkers()` private method
- Added `TriggerAutoConstructionAssignment()` public method
- Modified `SimulateTick()` to call auto-assignment hourly
- Modified `CompleteConstruction()` to trigger reallocation
- Modified `ProcessConstruction()` call location

### VillageBuilder.Engine\Commands\BuildingCommands\ConstructBuildingCommand.cs
- Added call to `engine.TriggerAutoConstructionAssignment()` after building placement
- Updated log message to indicate auto-assignment

## Log Messages

### Auto-Assignment Success
```
"The [FamilyName] family (X workers) automatically assigned to construct the [BuildingType]"
Level: Info
```

### Building Placement
```
"Construction started: [BuildingType] (idle families will be automatically assigned)"
Level: Info
```

### Construction Complete
```
"Construction complete: a new [BuildingType] is now operational"
Level: Success
```

## Benefits

### Player Quality of Life
- ? Eliminates tedious micro-management
- ? Reduces clicks by ~75% for construction
- ? Automatic workflow feels natural
- ? Still allows manual control when needed

### Gameplay Balance
- ? Idle families automatically contribute
- ? Multiple projects progress simultaneously
- ? No "forgotten" buildings stuck at 0%
- ? Construction becomes fire-and-forget

### Strategic Depth
- ? Player focuses on building placement
- ? Resource management remains important
- ? Can still optimize by manual reassignment
- ? Large buildings complete faster with more families

## Known Limitations

### Current Version
- No priority system (oldest building gets workers first)
- No notification when all workers are busy
- Cannot disable auto-assignment
- No multi-family teams on single building

### Future Improvements
- Add player preference toggle
- Priority markers for buildings
- Team-based construction for large buildings
- Distance-based smart assignment

## Success Metrics
? Players no longer need to manually assign construction workers
? Multiple buildings progress simultaneously
? Workers automatically move between projects
? System is transparent and predictable
? Manual assignment still available for control
? No performance degradation
? Multiplayer-compatible and deterministic
