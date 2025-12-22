# Worker Assignment UI Improvements - Summary

## Enhancement: Show Family Work Status When Assigning Workers

### Problem
When clicking on a building to assign workers, the UI didn't show:
- Which families are already working at OTHER buildings
- Which building those families are working at
- How many workers from each family are available
- Visual distinction between available and unavailable families

This made it confusing to know which families could be assigned and required memorizing who worked where.

---

## Solution: Enhanced Worker Assignment UI

### Visual Status Indicators

The UI now shows three distinct states for each family:

| Status | Button Color | Text | Meaning |
|--------|--------------|------|---------|
| **Working Here** | ?? Green | "Smith (2 working here)" | Family is already assigned to THIS building |
| **Working Elsewhere** | ?? Dark Yellow | "Smith (at Farm)" | Family is working at a DIFFERENT building |
| **Available** | ? White/Gray | "Smith (2 available)" | Family has workers available to assign |
| **No Workers** | ? Dark Gray | "Smith (no workers available)" | All adults are busy (grayed out) |

### Color Legend

A helpful legend is shown at the top of the worker list:
```
?? ASSIGN WORKERS ?????????????
? Legend:                      ?
? Green = Working here         ?
? Yellow = Working elsewhere   ?
? White = Available            ?
```

---

## UI Examples

### Before (No Status Info)
```
?? ASSIGN WORKERS ?????????????
? Smith (2 adults)             ?  ? Are they available?
? Johnson (3 adults)           ?  ? Are they working?
? Williams (2 adults)          ?  ? Where are they?
????????????????????????????????
```

### After (Full Status Info)
```
?? ASSIGN WORKERS ?????????????
? Legend:                      ?
? Green = Working here         ?
? Yellow = Working elsewhere   ?
? White = Available            ?
?                              ?
? ?? Smith (2 working here)    ?  ? Already working!
? ?? Johnson (at Farm)         ?  ? Shows where!
? ? Williams (2 available)    ?  ? Can assign!
? ? Brown (no workers available) ? All busy!
????????????????????????????????
```

---

## Detailed Status Breakdown

### 1. Working Here (Green)
**Shows**: "FamilyName (X working here)"
- ? Family is already assigned to THIS building
- ? Shows exact number of workers from this family
- ?? Cannot reassign (already here)
- Color: Green background + Green text

**Example**: When viewing a Farm where Smith family works:
```
?? Smith (2 working here)
```

### 2. Working Elsewhere (Yellow)
**Shows**: "FamilyName (at BuildingType)"
- ? Family is working at a DIFFERENT building
- ? Shows which building they're at
- ?? Cannot assign (already working elsewhere)
- Color: Dark yellow background + Yellow text

**Example**: When viewing a Mine but Smith family works at Farm:
```
?? Smith (at Farm)
```

### 3. Available Workers (White)
**Shows**: "FamilyName (X available)"
- ? Family has workers available
- ? Shows exact number of available workers
- ? Can click to assign
- Color: Normal background + White text

**Example**: Johnson family has 2 adults, neither working:
```
? Johnson (2 available)
```

### 4. No Workers Available (Gray)
**Shows**: "FamilyName (no workers available)"
- ?? All adults in family are already working
- ?? Cannot assign (no one available)
- Color: Normal background + Gray text

**Example**: Williams family adults all working at other buildings:
```
? Williams (no workers available)
```

---

## How It Works

### Detection Logic

```csharp
// Check if working at THIS building
var isAssignedHere = building.Workers.Count(w => w.Family?.Id == family.Id) > 0;

// Check if working at ANOTHER building
var workingElsewhere = family.GetAdults()
    .Where(p => p.IsAlive && p.AssignedBuilding != null)
    .Where(p => p.AssignedBuilding?.Id != building.Id)
    .ToList();

// Count available workers (not assigned anywhere)
var availableCount = family.GetAdults()
    .Count(p => p.IsAlive && p.AssignedBuilding == null);
```

### Click Behavior

| Scenario | Click Result |
|----------|--------------|
| Working here | ? No action (already assigned) |
| Working elsewhere | ? No action (busy at other building) |
| Available workers | ? Assign family to this building |
| No workers available | ? No action (grayed out) |

---

## Benefits

### 1. Clear Visual Feedback
- Instant recognition of family status
- Color-coded for quick scanning
- No need to remember assignments

### 2. Prevents Mistakes
- Can't accidentally assign busy workers
- Shows exactly where families are working
- Grays out unavailable options

### 3. Better Planning
- See how many workers you have available
- Know which buildings need more workers
- Easy to redistribute workforce

### 4. Reduces Confusion
- Legend explains colors
- Shows current workplace
- Clear available count

---

## Technical Details

### Files Changed

**VillageBuilder.Game/Graphics/UI/SidebarRenderer.cs**
- Enhanced `RenderBuildingInfo()` method
- Added status detection logic
- Added color legend for work buildings
- Updated button text to show status
- Color-coded buttons based on status
- Prevents assignment when no workers available

### Key Changes

1. **Status Detection**
   ```csharp
   var isAssignedHere = building.Workers.Count(w => w.Family?.Id == family.Id) > 0;
   var workingElsewhere = family.GetAdults().Where(p => p.AssignedBuilding != null);
   var availableCount = family.GetAdults().Count(p => p.AssignedBuilding == null);
   ```

2. **Color Coding**
   ```csharp
   if (isAssignedHere)
       buttonColor = Green;
   else if (workingElsewhere.Any())
       buttonColor = DarkYellow;
   else if (availableCount > 0)
       buttonColor = Normal;
   ```

3. **Smart Text**
   ```csharp
   if (isAssignedHere)
       text = "Smith (2 working here)";
   else if (workingElsewhere.Any())
       text = "Smith (at Farm)";
   else if (availableCount > 0)
       text = "Smith (2 available)";
   ```

---

## Usage Examples

### Scenario 1: Starting Village
All families available for work:
```
?? ASSIGN WORKERS ?????????????
? ? Smith (2 available)       ?
? ? Johnson (3 available)     ?
? ? Williams (2 available)    ?
????????????????????????????????
```
**Action**: Click any family to assign to this building

### Scenario 2: Some Families Working
```
?? ASSIGN WORKERS (Mine) ??????
? ?? Smith (2 working here)   ?  ? Already mining
? ?? Johnson (at Farm)        ?  ? Farming
? ? Williams (2 available)   ?  ? Can assign
????????????????????????????????
```
**Action**: Click Williams to assign them to the Mine

### Scenario 3: Everyone Busy
```
?? ASSIGN WORKERS (Workshop) ??
? ?? Smith (at Mine)          ?
? ?? Johnson (at Farm)        ?
? ?? Williams (at Lumberyard) ?
????????????????????????????????
```
**Action**: Can't assign anyone - need more families or unassign from other buildings

---

## Future Enhancements

Potential improvements for later:

1. **Click to Unassign**
   - Click green "Working here" to remove assignment
   - Confirmation dialog

2. **Click to Reassign**
   - Click yellow "at Farm" to move them here
   - Show warning about leaving previous job

3. **Partial Assignment**
   - Family has 3 adults, 1 working elsewhere
   - Show "Johnson (2 available, 1 at Farm)"
   - Assign only the available workers

4. **Skill-Based Filtering**
   - Highlight families with relevant skills
   - "Smith (2 available) ? Farming skill"

5. **Distance Warning**
   - Show if home is far from workplace
   - "Williams (2 available) ?? Far from home"

---

## Testing Notes

? Build compiles successfully
? Shows correct status for each family
? Color coding works as expected
? Legend displays for work buildings
? Only shows legend for non-house buildings
? Prevents assignment when no workers available
? Shows which building family is working at
? Counts available workers correctly

---

## Player Experience

### Before
- Guessed which families were available
- Clicked families randomly hoping they'd assign
- No feedback about current assignments
- Had to check each building to find workers

### After
- Instantly see who's available at a glance
- Know exactly where each family is working
- Clear color coding prevents mistakes
- Legend explains what colors mean
- Can plan workforce distribution effectively
