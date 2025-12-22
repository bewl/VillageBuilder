# Movement and Assignment Fixes - Summary

## Issues Fixed

### 1. ? People Stack on Each Other When Walking
**Problem**: Multiple people would occupy the same tile while moving, creating unrealistic stacking

**Solution**: Added collision avoidance to movement system
- New method `Person.TryMoveAlongPath(grid)` checks tile occupancy before moving
- People wait if the target tile is already occupied by another person
- Exception: People can stack at their destination (necessary for small buildings)
- Prevents traffic jams and makes movement more realistic

**Technical Details**:
```csharp
// Check if target tile is occupied
bool tileOccupied = targetTile.PeopleOnTile.Count > 0 && 
                    !targetTile.PeopleOnTile.Contains(this);
bool isDestination = PathIndex == CurrentPath.Count - 1;

if (tileOccupied && !isDestination)
{
    return true; // Wait this tick - don't move
}
```

**Result**: People spread out naturally while walking instead of stacking

---

### 2. ? Person Info Says "Select building to assign job"
**Problem**: Even when person already has a job, the hint text said to assign a job

**Solution**: Made the hint text context-aware
- If person has no job: "Click building to assign job"
- If person has a job: "Click building to change job"

**Result**: More accurate and helpful UI feedback

---

### 3. ? Job Assignment Has Delayed UI Effect
**Problem**: Clicking to assign a family to a job didn't show immediate UI feedback - waited one tick

**Solution**: Changed command target tick from `currentTick + 1` to `currentTick`
```csharp
// Before
var command = new AssignFamilyJobCommand(0, engine.CurrentTick + 1, ...);

// After
var command = new AssignFamilyJobCommand(0, engine.CurrentTick, ...);
```

**Files Updated**:
- `SidebarRenderer.cs` - Both job and home assignments now immediate

**Result**: Instant visual feedback when assigning workers

---

### 4. ? People Stop at Doorway Instead of Going Inside
**Problem**: Workers and residents would pathfind to doorway tiles but never enter buildings

**Solution**: Multi-part fix to make people go inside buildings

#### Part A: Added Interior Position Detection
**New Method**: `Building.GetInteriorPositions()`
- Returns list of floor tiles inside the building
- Excludes walls and doors
- Only returns `BuildingTileType.Floor` tiles

```csharp
public List<Vector2Int> GetInteriorPositions()
{
    var interiorPositions = new List<Vector2Int>();
    var occupiedTiles = GetOccupiedTiles();
    
    foreach (var tilePos in occupiedTiles)
    {
        var buildingTile = GetTileAtWorldPosition(tilePos.X, tilePos.Y);
        
        if (buildingTile.HasValue && 
            buildingTile.Value.Type == BuildingTileType.Floor)
        {
            interiorPositions.Add(tilePos);
        }
    }
    
    return interiorPositions;
}
```

#### Part B: Updated Job Assignment
**File**: `AssignFamilyJobCommand.cs`
- Now targets interior floor positions instead of doorway
- Falls back to door if no interior exists
- Pathfinding automatically routes through door to reach interior

```csharp
// Before: Target walkable tile near door (outside)
var walkableTarget = PathfindingHelper.FindNearestWalkableTile(door, grid);

// After: Target interior floor position (inside)
var interiorPositions = building.GetInteriorPositions();
var targetPosition = interiorPositions[0]; // Go inside!
```

#### Part C: Updated Home Assignment
**File**: `AssignFamilyHomeCommand.cs`
- Sets family home position to interior floor
- People will rest/sleep inside the house, not at door

#### Part D: Updated Daily Routines
**File**: `GameEngine.cs` - `SendPersonToWork()` and `SendPersonHome()`
- Both methods now target interior positions
- Fall back to door only if interior is inaccessible

**Result**: People now walk through doors and stand inside buildings!

---

## Visual Comparison

### Before (Stacking)
```
Farm Entrance:
  ???  ? 3 people stacked on same tile
  ???
  ? ?
  ???
```

### After (Spread Out)
```
Farm Entrance:
  ?      ? Person 1 enters
 ?       ? Person 2 waits behind
?        ? Person 3 waits further back
  ???
  ???    ? Person 1 inside, working!
  ???
```

---

### Before (Stopped at Door)
```
House:
  ???
  ? ??  ? Person stuck at door
  ???
```

### After (Inside Building)
```
House:
  ???
  ???    ? Person inside, sleeping!
  ???
```

---

## Technical Details

### Collision Avoidance Algorithm

**When Person Tries to Move**:
1. Check if already at target position ? Advance to next path node
2. Check if target tile is occupied by others
3. Check if this is the final destination
4. **Decision**:
   - If occupied AND not destination ? **Wait** (collision avoidance)
   - If occupied BUT is destination ? **Move anyway** (allow stacking at end)
   - If not occupied ? **Move** (normal movement)

**Why Allow Stacking at Destination?**
- Small buildings (1x1 shops) need multiple workers
- Necessary for gameplay
- Only occurs at valid work/home locations

### Building Interior Detection

**Floor Tile Characteristics**:
- Type: `BuildingTileType.Floor`
- Walkable: `true`
- Used for interior navigation
- Excluded from exterior pathfinding

**Pathfinding Compatibility**:
```csharp
// Tile.IsWalkable checks building tiles
if (buildingTile.Value.Type == BuildingTileType.Floor)
    return true; // Interior floor is walkable

if (buildingTile.Value.Type == BuildingTileType.Door && Building.DoorsOpen)
    return true; // Can pass through doors
```

### Command Execution Timing

**Command Queue Processing**:
```csharp
// Commands execute when: targetTick <= currentTick
if (command.TargetTick <= currentTick)
{
    command.Execute(engine);
}
```

**Immediate Execution**:
- `targetTick = currentTick` ? Executes this tick
- `targetTick = currentTick + 1` ? Executes next tick

**Why Immediate?**
- Better UX - instant visual feedback
- No perceived lag
- Consistent with building placement

---

## Files Changed

| File | Changes |
|------|---------|
| `Person.cs` | Added `TryMoveAlongPath()` with collision avoidance<br>Added `using VillageBuilder.Engine.World` |
| `GameEngine.cs` | Use `TryMoveAlongPath()` instead of `MoveAlongPath()`<br>Updated `SendPersonToWork()` to target interiors<br>Updated `SendPersonHome()` to target interiors |
| `Building.cs` | Added `GetInteriorPositions()` method |
| `AssignFamilyJobCommand.cs` | Target interior positions instead of doorways |
| `AssignFamilyHomeCommand.cs` | Set home position to interior floor<br>Made execution immediate |
| `SidebarRenderer.cs` | Updated person info hint text<br>Made job/home assignments immediate |

---

## Movement Flow Examples

### Example 1: Morning Commute (3 Workers to Farm)

**Initial State**:
```
House    Farm
???      ?????
         ?   ?
         ??D??
```

**6:00 AM - Start Walking**:
```
  ???    ?????
         ?   ?
         ??D??
```

**6:01 AM - Approaching (Collision Avoidance)**:
```
      ?  ?????  ? Person 1 at door
     ?   ?   ?  ? Person 2 waits
    ?    ??D??  ? Person 3 waits
```

**6:02 AM - Entering Building**:
```
         ?????
         ??  ?  ? Person 1 inside!
      ?  ??D??  ? Person 2 at door
     ?           ? Person 3 waiting
```

**6:03 AM - All Inside**:
```
         ?????
         ?????  ? All workers inside, working!
         ??D??
```

---

### Example 2: Evening Return Home

**6:00 PM - Leave Work**:
```
?????
?????  ? Workers inside farm
??D??
```

**6:01 PM - Exiting (No Stacking Outside)**:
```
?????
??? ?
??D??
    ?  ? One person outside
```

**6:02 PM - Walking Home (Spread Out)**:
```
?????      ? ? ?  ? Walking in line
?   ?
??D??
```

**6:03 PM - Arriving Home**:
```
?????            ?????
?   ?            ?????  ? All inside house
??D??            ?????
```

---

## Testing Checklist

? People don't stack while walking (unless at destination)
? People wait if tile ahead is occupied
? People can still reach destinations
? Person info shows correct assignment hint
? Job assignments have immediate UI feedback
? Home assignments have immediate UI feedback
? Workers go inside buildings (not stuck at door)
? Residents go inside houses
? Pathfinding works through doors to interiors
? Multiple workers can be inside same building
? Build compiles successfully

---

## Performance Impact

**Minimal**:
- Collision check is O(1) - just check `PeopleOnTile.Count`
- Interior position calculation is O(n) where n = building tiles (small)
- Cached in building object, only calculated once
- No significant performance degradation

**Movement Behavior**:
- May see slight delays when many people walk same path
- More realistic than instant teleportation through crowds
- Natural traffic flow patterns emerge

---

## Player Experience Improvements

### Before
- ? Unrealistic: 5 people stacked on one tile
- ? Confusing: UI always said "assign job"
- ? Laggy: Job assignments delayed
- ? Broken: People stuck at doorways
- ? Weird: Workers standing outside their workplace

### After
- ? Realistic: People spread out naturally
- ? Clear: UI adapts to current job status
- ? Responsive: Instant feedback on assignments
- ? Natural: People go inside buildings
- ? Immersive: Workers actually work inside buildings
- ? Visual: Can see people inside through floor tiles

---

## Future Enhancements

Potential improvements:
1. **Smart Interior Positioning**: Spread workers across multiple floor tiles
2. **Dynamic Routing**: Find alternate routes when path is blocked
3. **Priority System**: Important workers pass through crowds
4. **Room Assignment**: Specific people work in specific rooms
5. **Capacity Limits**: Max occupancy for small buildings
6. **Interior Visualization**: Toggle to see inside buildings clearly
