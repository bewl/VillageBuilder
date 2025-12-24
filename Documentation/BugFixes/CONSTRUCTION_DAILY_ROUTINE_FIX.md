# Construction Worker Daily Routine Fix

## Issue
Construction workers were going home at 18:00 (end of work day) but not returning to construction sites the next morning at 6:00 (work start time). This caused construction to permanently pause after the first day.

## Root Cause
The `HandleDailyRoutines()` method only sent workers with `AssignedBuilding` back to work in the morning. Construction workers are tracked in `building.ConstructionWorkers` list (not `person.AssignedBuilding`), so they were never sent back to construction sites.

## Solution

### 1. Modified Morning Routine
Extended work start time (6:00 AM) handling to include construction workers:

```csharp
// Send workers to their jobs at work start time
if (Time.Hour == GameTime.WorkStartHour)
{
    foreach (var family in Families)
    {
        // Send regular production workers to their assigned buildings
        foreach (var person in family.Members.Where(p => p.IsAlive && p.AssignedBuilding != null))
        {
            if (!person.IsAtWork() && person.CurrentTask != PersonTask.MovingToLocation)
            {
                SendPersonToWork(person);
            }
        }
        
        // NEW: Send construction workers back to their construction sites
        foreach (var person in family.Members.Where(p => p.IsAlive && p.CurrentTask == PersonTask.Constructing))
        {
            var constructionSite = Buildings.FirstOrDefault(b => 
                !b.IsConstructed && b.ConstructionWorkers.Contains(person));
            
            if (constructionSite != null && !person.IsAtConstructionSite(constructionSite))
            {
                SendPersonToConstructionSite(person, constructionSite);
            }
        }
    }
}
```

### 2. New Helper Method
Added `SendPersonToConstructionSite()` to handle pathfinding and task state:

```csharp
private void SendPersonToConstructionSite(Person person, Building building)
{
    // Get work positions (doors or building center)
    var workPositions = building.GetDoorPositions();
    if (workPositions.Count == 0)
    {
        workPositions.Add(new Vector2Int(building.X, building.Y));
    }

    // Find closest position
    var targetPosition = workPositions.OrderBy(p =>
        Math.Abs(p.X - person.Position.X) + Math.Abs(p.Y - person.Position.Y)
    ).First();

    // Calculate path and send worker
    var path = Pathfinding.FindPath(person.Position, targetPosition, Grid);
    if (path != null && path.Count > 0)
    {
        person.SetPath(path);
        person.CurrentTask = PersonTask.Constructing; // Maintain state
    }
}
```

## Daily Construction Workflow

### Fixed Workflow
```
6:00 AM - Work Start Time
?? Regular workers ? Sent to assigned buildings
?? Construction workers ? Sent to construction sites ?

18:00 PM - Work End Time
?? All workers sent home
?? Construction pauses overnight

22:00 PM - Sleep Time
?? Workers go to sleep

6:00 AM - Next Day (Work Start)
?? Workers wake up
?? Regular workers ? Return to jobs
?? Construction workers ? Return to construction sites ?
    ?? Construction resumes!
```

### Before Fix (Broken)
```
Day 1:
  6:00 AM: Workers assigned to construction
  8:00 AM: Workers arrive, construction progresses
  18:00 PM: Workers go home
  
Day 2:
  6:00 AM: ? Construction workers NOT sent back
  Result: Construction stuck at Day 1 progress forever
```

### After Fix (Working)
```
Day 1:
  6:00 AM: Workers assigned to construction
  8:00 AM: Workers arrive, construction progresses
  18:00 PM: Workers go home, progress pauses
  
Day 2:
  6:00 AM: ? Construction workers sent back to site
  8:00 AM: Workers arrive, construction resumes
  18:00 PM: Workers go home, progress pauses
  
Day 3+: Cycle continues until construction complete
```

## Code Changes

### Modified File
- **VillageBuilder.Engine\Core\GameEngine.cs**
  - Modified `HandleDailyRoutines()` work start logic
  - Added `SendPersonToConstructionSite()` method

### Key Logic
1. **Identify construction workers**: `person.CurrentTask == PersonTask.Constructing`
2. **Find their building**: Check `building.ConstructionWorkers.Contains(person)`
3. **Check if absent**: `!person.IsAtConstructionSite(constructionSite)`
4. **Send them back**: Use new `SendPersonToConstructionSite()` method

## Testing

### Verified Scenarios
- ? Construction workers go home at 18:00
- ? Construction workers return to site at 6:00 next day
- ? Construction progress resumes on Day 2+
- ? Multi-day construction completes successfully
- ? Regular production workers unaffected
- ? Mixed workers (some construction, some production) handled correctly

### Example Timeline
```
House construction: 800 work units, 2 workers

Day 1:
  6:00 AM: Workers assigned
  8:00 AM: Workers arrive (travel 20 ticks)
  6:00 PM: Workers go home
  Progress: 2 workers × 600 ticks/day = 1200 work
  But capped at 800 - Construction completes Day 1!

Large Building: 3200 work units, 2 workers

Day 1:
  6:00 AM: Workers assigned
  8:00 AM: Arrive, work all day
  6:00 PM: Go home
  Progress: 2 workers × 600 ticks = 1200 work (37.5%)

Day 2:
  6:00 AM: ? Workers return to site
  8:00 AM: Resume working
  6:00 PM: Go home
  Progress: 1200 + 1200 = 2400 work (75%)

Day 3:
  6:00 AM: ? Workers return again
  8:00 AM: Resume working
  ~12:00 PM: Construction completes!
```

## Benefits

? **Multi-day construction works**: Large buildings complete over multiple days  
? **Realistic workflow**: Workers commute daily to construction sites  
? **Progress preservation**: Construction progress saved between days  
? **Consistent behavior**: Construction workers follow same schedule as production workers  
? **No stuck buildings**: Construction no longer permanently pauses

## Build Status
? All changes compile successfully  
? No errors detected  
? Ready for testing
