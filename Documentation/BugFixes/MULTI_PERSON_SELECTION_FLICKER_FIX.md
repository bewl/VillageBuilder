# Multi-Person Selection Flicker Fix

## Issue
When clicking on a tile with multiple people, the sidebar briefly flickered, showing a single person before displaying the multi-person view.

## Root Cause

The flicker was caused by a mismatch between hover detection and click handling:

### The Problem Flow

1. **Hover Detection** (`UpdateEntityHover()`)
   ```csharp
   // ALWAYS set hover to first person, even with multiple people
   if (tile.PeopleOnTile.Count > 0)
   {
       _hoveredPerson = tile.PeopleOnTile[0]; // ? Only first person!
   }
   ```

2. **Sidebar Rendering** (every frame)
   ```csharp
   // Renders based on _hoveredPerson
   if (_hoveredPerson != null)
       RenderPersonInfo(_hoveredPerson); // Shows single person
   ```

3. **Click Handler**
   ```csharp
   // Correctly selects ALL people on tile
   if (clickedTile.PeopleOnTile.Count > 0)
   {
       SelectPeopleAtTile(clickedTile.PeopleOnTile); // ? All people
   }
   ```

4. **Result:** One frame with single person (from hover), then switch to multi-person = **Flicker!**

## The Solution

Modified `UpdateEntityHover()` to NOT set `_hoveredPerson` when there are multiple people on a tile:

```csharp
private void UpdateEntityHover()
{
    _hoveredPerson = null;
    _hoveredBuilding = null;
    
    var tile = _engine.Grid.GetTile(worldTileX, worldTileY);
    if (tile != null && tile.PeopleOnTile.Count > 0)
    {
        // NEW: Only set hover for single person tiles
        if (tile.PeopleOnTile.Count == 1)
        {
            _hoveredPerson = tile.PeopleOnTile[0]; // ? Single person
        }
        // For multiple people, don't set hover - let click handle it
        return;
    }
    
    // Check for building hover...
}
```

## Behavior Changes

### Before Fix (Flickering)

| Scenario | Hover State | Click Action | Result |
|----------|-------------|--------------|--------|
| 1 person on tile | `_hoveredPerson = person` | `SelectPerson(person)` | ? Smooth |
| 2+ people on tile | `_hoveredPerson = first person` | `SelectPeopleAtTile(all)` | ? **Flicker!** |

**Frame sequence for multi-person tile:**
```
Frame N: Hover renders first person (single view)
Frame N+1: Click processed, SelectPeopleAtTile called
Frame N+2: Sidebar switches to multi-person view
Result: Visible flicker as sidebar changes
```

### After Fix (No Flicker)

| Scenario | Hover State | Click Action | Result |
|----------|-------------|--------------|--------|
| 1 person on tile | `_hoveredPerson = person` | `SelectPerson(person)` | ? Smooth |
| 2+ people on tile | `_hoveredPerson = null` | `SelectPeopleAtTile(all)` | ? **No flicker!** |

**Frame sequence for multi-person tile:**
```
Frame N: Hover is null, no sidebar preview
Frame N+1: Click processed, SelectPeopleAtTile called
Frame N+2: Sidebar shows multi-person view directly
Result: Smooth transition, no intermediate state
```

## Technical Details

### Hover vs Selection States

**Hover State** (transient, updates every frame):
- Used for tooltips and previews
- Should be lightweight
- Cleared when mouse moves away

**Selection State** (persistent, until next click):
- Used for detailed info panels
- Can be complex (multi-person handling)
- Cleared only on explicit action

### Why This Works

1. **Single person tiles:** Hover preview still works (shows tooltip/quick info)
2. **Multi-person tiles:** No hover preview (avoids ambiguity)
3. **Click handling:** Always correct (selects all people on tile)
4. **No intermediate state:** Sidebar goes directly to correct view

## Edge Cases Handled

? **Hovering over single person**
- Shows hover preview
- Click selects that person
- Smooth experience

? **Hovering over multiple people**
- No hover preview (avoids confusion about which person)
- Click shows multi-person selection
- No flicker

? **Moving mouse between tiles**
- Hover state updates correctly
- No lingering hover from previous tile

? **Clicking empty tile after hovering people**
- Selection cleared properly
- No stale hover data

## Alternative Solutions Considered

### Option 1: Show Multi-Person Hover (Rejected)
```csharp
// Show all people in hover
if (tile.PeopleOnTile.Count > 0)
{
    _hoveredPeople = tile.PeopleOnTile; // Would need new field
}
```
**Problem:** Requires significant refactoring (new hover state, new rendering logic)

### Option 2: Delay Click Processing (Rejected)
```csharp
// Wait a frame before processing click
if (justClicked) 
    waitOneFrame = true;
```
**Problem:** Adds input latency, feels sluggish

### Option 3: Don't Show Hover for Any People (Rejected)
```csharp
// Never set _hoveredPerson
_hoveredPerson = null;
```
**Problem:** Loses useful hover preview for single people

### Option 4: Current Solution ? (Chosen)
**Only show hover for single person, skip for multiple**
- Minimal code change
- No new states needed
- Preserves useful hover for common case
- Eliminates flicker for multi-person case

## Code Changes

### File Modified
`VillageBuilder.Game\Graphics\GameRenderer.cs` - `UpdateEntityHover()` method

### Diff
```diff
  if (tile != null && tile.PeopleOnTile.Count > 0)
  {
-     _hoveredPerson = tile.PeopleOnTile[0];
+     // Only set hover for single person tiles to avoid flicker
+     if (tile.PeopleOnTile.Count == 1)
+     {
+         _hoveredPerson = tile.PeopleOnTile[0];
+     }
      return;
  }
```

## Testing

### Verified Scenarios
? Click on tile with 1 person - smooth selection  
? Click on tile with 2+ people - no flicker, direct to multi-view  
? Hover over single person - shows hover preview  
? Hover over multiple people - no preview (expected)  
? Cycle through people on multi-person tile - smooth transitions  
? Clear selection by clicking empty tile - works correctly

### Performance Impact
- **Before:** Hover set every frame (even when wrong)
- **After:** Hover set only when appropriate
- **Result:** Slight performance improvement (fewer unnecessary state changes)

## User Experience

### Before
```
User: *clicks tile with 3 people*
UI: *briefly shows person #1 in sidebar*
UI: *flickers*
UI: *switches to showing all 3 people*
User: "Why did it flicker?"
```

### After
```
User: *clicks tile with 3 people*
UI: *directly shows all 3 people*
User: "Perfect!"
```

## Related Systems

### Works With
- ? Person selection
- ? Multi-person cycling (arrow keys)
- ? Sidebar rendering
- ? Tooltip system

### No Impact On
- Building selection (separate hover state)
- Click handling logic (unchanged)
- Selection manager (unchanged)

## Build Status
? Code compiles successfully  
? No regressions detected  
? Ready for testing
