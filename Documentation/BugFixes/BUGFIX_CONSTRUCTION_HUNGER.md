# Bug Fixes - Construction Workers & Hunger System

## Overview

This document describes bug fixes implemented to resolve issues with construction workers not returning to sites and overtuned hunger mechanics.

---

## Issue #1: Construction Workers Not Returning to Sites

### Problem Description

**Reported Behavior:**
- Construction workers go home at end of work day (6 PM)
- Next morning (6 AM), they don't automatically return to construction sites
- Construction progress halts indefinitely
- Workers remain idle at home despite being assigned to construction

### Root Cause

The daily routine system (`HandleDailyRoutines`) only sent people to work if they had `person.AssignedBuilding != null`. However, construction workers are tracked in two places:

1. `building.ConstructionWorkers` list (definitely tracked)
2. `person.AssignedBuilding` reference (may not be set consistently for construction workers)

Construction workers assigned via `AssignConstructionWorkersCommand` were added to the building's `ConstructionWorkers` list, but the daily routine didn't check this list when sending people back to work in the morning.

### Solution Implemented

Added explicit handling for construction workers in the daily routine system:

**File:** `VillageBuilder.Engine/Core/GameEngine.cs`
**Method:** `HandleDailyRoutines()`

```csharp
// Send workers to their jobs at work start time
if (Time.Hour == GameTime.WorkStartHour)
{
    // Send regular workers
    foreach (var family in Families)
    {
        foreach (var person in family.Members.Where(p => p.IsAlive && p.AssignedBuilding != null))
        {
            if (!person.IsAtWork() && person.CurrentTask != PersonTask.MovingToLocation)
            {
                SendPersonToWork(person);
            }
        }
    }

    // NEW: Send construction workers back to construction sites
    foreach (var building in Buildings.Where(b => !b.IsConstructed))
    {
        foreach (var worker in building.ConstructionWorkers.ToList())
        {
            if (worker.IsAlive && !worker.IsAtConstructionSite(building) && worker.CurrentTask != PersonTask.MovingToLocation)
            {
                SendPersonToConstruction(worker, building);
            }
        }
    }
}
```

**New Helper Method:**

```csharp
/// <summary>
/// Send a construction worker back to their construction site
/// </summary>
private void SendPersonToConstruction(Person person, Building building)
{
    // Get a position near the construction site (door or occupied tile)
    var doorPositions = building.GetDoorPositions();
    Vector2Int targetPosition;

    if (doorPositions.Count > 0)
    {
        targetPosition = doorPositions[0];
    }
    else
    {
        // No door yet (building not complete), use building center
        targetPosition = new Vector2Int(building.X, building.Y);
    }

    var path = Pathfinding.FindPath(person.Position, targetPosition, Grid);

    if (path != null && path.Count > 0)
    {
        person.SetPath(path);
        person.CurrentTask = PersonTask.Constructing;
    }
}
```

### How It Works Now

**6 AM (Work Start Hour):**
1. Regular workers with `AssignedBuilding` are sent to work via `SendPersonToWork()`
2. **NEW:** All unfinished buildings are checked for construction workers
3. Each construction worker not already at the site is sent back via `SendPersonToConstruction()`
4. Workers travel to construction site and resume work

**6 PM (Work End Hour):**
1. All workers (including construction workers) go home
2. Construction workers remain in the `ConstructionWorkers` list

**Next Day:**
- Cycle repeats - construction workers return each morning

### Expected Behavior After Fix

? Construction workers go home at 6 PM  
? Construction workers return to sites at 6 AM automatically  
? Construction progress resumes daily  
? No manual re-assignment needed  

---

## Issue #2: Hunger System Overtuned

### Problem Description

**Reported Behavior:**
- People become extremely hungry very quickly
- Health deteriorates rapidly from hunger
- Game balance feels too harsh
- Difficult to keep population fed

### Root Cause Analysis

**Previous Values:**
```csharp
if (IsSleeping)
{
    Hunger = Math.Min(100, Hunger + 1);  // +1 per hour while sleeping
}
else
{
    Hunger = Math.Min(100, Hunger + 2);  // +2 per hour while awake
}
```

**Calculation:**
- Awake hours per day: ~16 hours (6 AM - 10 PM)
- Sleep hours per day: ~8 hours (10 PM - 6 AM)
- Daily hunger increase: (16 × 2) + (8 × 1) = 40 points/day
- **Time to full hunger**: 100 ÷ 40 = **2.5 days**

**Problems:**
1. Too fast - people go from fed to starving in 2.5 days
2. No automatic eating mechanism exists yet
3. Sickness threshold at 80 hunger means only ~2 days before illness
4. Health loss at 60+ hunger means constant health drain
5. Makes game frustrating rather than challenging

### Solution Implemented

Reduced hunger rate to match sleeping rate:

**File:** `VillageBuilder.Engine/Entities/Person.cs`
**Method:** `UpdateStats()`

```csharp
// BEFORE
else
{
    Hunger = Math.Min(100, Hunger + 2); // Faster when awake/active
}

// AFTER
else
{
    Hunger = Math.Min(100, Hunger + 1); // Steady hunger rate (was +2, too fast)
}
```

**New Calculation:**
- Awake hours per day: ~16 hours
- Sleep hours per day: ~8 hours
- Daily hunger increase: (16 × 1) + (8 × 1) = 24 points/day
- **Time to full hunger**: 100 ÷ 24 = **~4.2 days**

### Comparison: Before vs After

| Metric | Before (Overtuned) | After (Balanced) | Improvement |
|--------|-------------------|------------------|-------------|
| **Hunger rate (awake)** | +2/hour | +1/hour | 50% slower |
| **Daily hunger gain** | 40 points | 24 points | 40% reduction |
| **Days to full hunger** | 2.5 days | 4.2 days | **+68% more time** |
| **Days to sickness** | 2.0 days | 3.3 days | **+65% more time** |
| **Days to health loss** | 1.5 days | 2.5 days | **+67% more time** |

### Game Balance Impact

**Before Fix:**
- ? Frustrating - constant hunger management
- ? Punishing - little margin for error
- ? Stressful - always fighting starvation
- ? Unrealistic - people need food every 2-3 days

**After Fix:**
- ? Challenging but fair
- ? Time to strategize and manage resources
- ? More realistic hunger progression
- ? Allows focus on other gameplay aspects
- ? Still requires food management (not removed, just balanced)

### Future Considerations

**Not Yet Implemented (Future Work):**
1. Automatic eating system when food is available
2. Food preparation/cooking mechanics
3. Hunger satisfaction from meals
4. Different food types with varying satiation
5. Eating at specific times (breakfast, lunch, dinner)

**Why Not Implemented Now:**
- No food consumption mechanic exists yet
- No eating behavior implemented
- Focus on fixing broken mechanics first
- Will add eating system in future update

---

## Testing & Verification

### How to Test Construction Worker Fix

1. Start new game or load existing save
2. Assign construction workers to an unfinished building
3. Wait until 6 PM (work end time)
   - ? Workers should go home
4. Wait until next day, 6 AM (work start time)
   - ? Workers should automatically return to construction site
   - ? Construction progress should resume
5. Verify over multiple days
   - ? Workers continue daily cycle indefinitely

### How to Test Hunger Fix

1. Start new game
2. Monitor a person's hunger stat over time
3. **Before fix**: Hunger reaches 60 in ~1.5 days, 100 in ~2.5 days
4. **After fix**: Hunger reaches 60 in ~2.5 days, 100 in ~4.2 days
5. Verify people have more time before health deterioration
6. Check that gameplay feels more balanced

### Expected Behavior

**Construction Workers:**
- ? Return to construction sites every morning
- ? Resume work automatically
- ? No manual intervention needed
- ? Construction completes over time

**Hunger System:**
- ? Slower hunger progression
- ? More time to manage resources
- ? Still requires food eventually
- ? Game feels challenging, not punishing

---

## Technical Details

### Files Modified

1. **VillageBuilder.Engine/Core/GameEngine.cs**
   - Modified `HandleDailyRoutines()` method
   - Added `SendPersonToConstruction()` helper method
   - Lines ~583-610

2. **VillageBuilder.Engine/Entities/Person.cs**
   - Modified `UpdateStats()` method
   - Changed hunger rate from +2 to +1 when awake
   - Line ~316

### Performance Impact

**Construction Worker Fix:**
- Adds O(unfinished buildings × construction workers) iterations at 6 AM
- Typical: ~5 buildings × 2-4 workers = 10-20 checks
- **Impact:** Negligible (<1 ?s)

**Hunger Rate Change:**
- No performance impact (same calculation, different constant)
- **Impact:** None

### Backward Compatibility

**Save Files:**
- ? Fully compatible with existing saves
- Construction workers will start returning next morning
- Hunger will slow down immediately

**Game Balance:**
- May need to adjust food consumption rates in future
- Current change is conservative (still requires food management)

---

## Related Documentation

- [PERFORMANCE_OPTIMIZATIONS.md](../../VillageBuilder.Engine/Documentation/PERFORMANCE_OPTIMIZATIONS.md) - Performance work
- [VISUAL_ENHANCEMENTS.md](./VISUAL_ENHANCEMENTS.md) - Visual polish features
- [UI_INTEGRATION_GUIDELINES.md](./UI_INTEGRATION_GUIDELINES.md) - UI best practices

---

## Changelog

### 2024-01-XX - Bug Fixes: Construction & Hunger

#### Fixed
- ? Construction workers now return to sites each morning
- ? Hunger rate reduced from +2/hour to +1/hour when awake
- ? Game balance improved - more time before starvation

#### Added
- ? `SendPersonToConstruction()` helper method
- ? Construction worker daily routine handling
- ? Documentation for both fixes

---

**Maintained By:** VillageBuilder Development Team  
**Last Updated:** 2024-01-XX
