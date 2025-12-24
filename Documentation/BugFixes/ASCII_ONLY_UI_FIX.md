# ASCII-Only UI Fix - Complete Solution

## Problem

The game is still showing `?` question marks because:
1. ? **Terrain decorations** - FIXED (ASCII-only mode active)
2. ? **Sidebar UI symbols** - BROKEN (uses special Unicode/emojis)
3. ? **Person task icons** - BROKEN (uses emojis)
4. ? **Building icons** - BROKEN (uses special Unicode)

**Root cause:** Cascadia Code font doesn't support these extended Unicode characters and emojis, even though we added the codepoints.

---

## Solution: Replace ALL Special Characters with ASCII

### Files to Modify

**File:** `VillageBuilder.Game/Graphics/UI/SidebarRenderer.cs`

### Changes Needed

#### 1. GetBuildingAsciiIcon() - Line 273

**Replace:**
```csharp
private string GetBuildingAsciiIcon(BuildingType type)
{
    return type switch
    {
        BuildingType.House => "?",           // Block
        BuildingType.Farm => "?",            // Spade
        BuildingType.Warehouse => "?",       // Square
        BuildingType.Mine => "?",            // Double cross
        BuildingType.Lumberyard => "?",      // Box drawing
        BuildingType.Workshop => "?",        // Box drawing
        BuildingType.Market => "?",          // Box drawing
        BuildingType.Well => "?",            // Circle
        BuildingType.TownHall => "?",        // Diamond
        _ => "?"
    };
}
```

**With:**
```csharp
private string GetBuildingAsciiIcon(BuildingType type)
{
    return type switch
    {
        BuildingType.House => "H",           // House
        BuildingType.Farm => "F",            // Farm
        BuildingType.Warehouse => "W",       // Warehouse
        BuildingType.Mine => "M",            // Mine
        BuildingType.Lumberyard => "L",      // Lumberyard
        BuildingType.Workshop => "K",        // worKshop
        BuildingType.Market => "$",          // Market (dollar sign)
        BuildingType.Well => "O",            // well (circular)
        BuildingType.TownHall => "T",        // Town hall
        _ => "?"
    };
}
```

#### 2. GetLogPrefix() - Line 290

**Replace:**
```csharp
private string GetLogPrefix(LogLevel level)
{
    return level switch
    {
        LogLevel.Info => "·",         // Middle dot
        LogLevel.Warning => "!",
        LogLevel.Error => "×",        // Multiplication sign
        LogLevel.Success => "?",      // Check mark
        _ => "·"
    };
}
```

**With:**
```csharp
private string GetLogPrefix(LogLevel level)
{
    return level switch
    {
        LogLevel.Info => ".",         // Period
        LogLevel.Warning => "!",
        LogLevel.Error => "X",        // Letter X
        LogLevel.Success => "+",      // Plus sign
        _ => "."
    };
}
```

#### 3. GetTaskIcon() - Around line 430

**Replace:**
```csharp
private string GetTaskIcon(VillageBuilder.Engine.Entities.PersonTask task)
{
    return task switch
    {
        VillageBuilder.Engine.Entities.PersonTask.Sleeping => "??",          // Zzz emoji
        VillageBuilder.Engine.Entities.PersonTask.GoingHome => "??",         // House emoji
        VillageBuilder.Engine.Entities.PersonTask.GoingToWork => "?",       // Arrow
        VillageBuilder.Engine.Entities.PersonTask.WorkingAtBuilding => "?", // Hammer/pick
        VillageBuilder.Engine.Entities.PersonTask.Constructing => "??",     // Hammer emoji
        VillageBuilder.Engine.Entities.PersonTask.Resting => "?",           // Smiley
        VillageBuilder.Engine.Entities.PersonTask.MovingToLocation => "?",  // Left-right arrow
        VillageBuilder.Engine.Entities.PersonTask.Idle => "?",              // Circle
        _ => "·"
    };
}
```

**With:**
```csharp
private string GetTaskIcon(VillageBuilder.Engine.Entities.PersonTask task)
{
    return task switch
    {
        VillageBuilder.Engine.Entities.PersonTask.Sleeping => "Z",          // Sleeping (Zzz)
        VillageBuilder.Engine.Entities.PersonTask.GoingHome => "H",         // Home
        VillageBuilder.Engine.Entities.PersonTask.GoingToWork => ">",       // Arrow right
        VillageBuilder.Engine.Entities.PersonTask.WorkingAtBuilding => "W", // Working
        VillageBuilder.Engine.Entities.PersonTask.Constructing => "C",      // Constructing
        VillageBuilder.Engine.Entities.PersonTask.Resting => "R",           // Resting
        VillageBuilder.Engine.Entities.PersonTask.MovingToLocation => "M",  // Moving
        VillageBuilder.Engine.Entities.PersonTask.Idle => "I",              // Idle
        _ => "."
    };
}
```

#### 4. RenderQuickStats() - Around line 85-135

Find and replace these lines:

**Line ~95:** `"? ? Families:` ? `"? # Families:`  
**Line ~105:** `"? ? No families` ? `"? # No families`  
**Line ~111:** `"? ? Buildings:` ? `"? [] Buildings:`  
**Line ~138:** `"? ? Last Save:` ? `"? * Last Save:`  
**Line ~148:** `"? ? No saves` ? `"? * No saves`

---

## How to Apply Changes

### Method 1: Manual Edit (Recommended)

1. Open `VillageBuilder.Game/Graphics/UI/SidebarRenderer.cs`
2. Find each method listed above
3. Replace the Unicode characters with the ASCII alternatives
4. **Build** the project (Ctrl+Shift+B)
5. **Restart** the game

### Method 2: Find & Replace (Quick)

In Visual Studio:
1. Press `Ctrl+H` (Find and Replace)
2. Search scope: Current Document (`SidebarRenderer.cs`)
3. Perform these replacements:

```
Find: "?"     Replace: "H"
Find: "?"     Replace: "F"
Find: "?"     Replace: "W"
Find: "?"     Replace: "M"
Find: "?"     Replace: "L"
Find: "?"     Replace: "K"
Find: "?"     Replace: "O"
Find: "?"     Replace: "T"
Find: "·"     Replace: "."
Find: "×"     Replace: "X"
Find: "?"     Replace: "+"
Find: "??"    Replace: "Z"
Find: "??"    Replace: "H"
Find: "?"     Replace: ">"
Find: "?"     Replace: "W"
Find: "??"    Replace: "C"
Find: "?"     Replace: "R" (in GetTaskIcon) / "#" (in RenderQuickStats)
Find: "?"     Replace: "M"
Find: "?"     Replace: "[]"
Find: "?"     Replace: "*"
```

**Note:** Be careful with replacements that might appear in multiple contexts (like "?" which means "families" in one place and "resting" in another).

---

## Expected Result

**Before:**
```
? ? Families:    3           <- Question marks
?   Population:  6
? ? Buildings:   0/0
? ? Last Save:  < 1 min ago
```

**After:**
```
? # Families:    3           <- Clean ASCII!
?   Population:  6
? [] Buildings:  0/0
? * Last Save:  < 1 min ago
```

**Person Info:**
```
Before: ? ? Henry Brewer     (? = task icon)
After:  ? I Henry Brewer     (I = Idle)
```

**Building List:**
```
Before: ?   ? House        1   (? symbols)
After:  ?   H House        1   (H = House)
```

---

## Why This Works

1. **ASCII characters (0-127)** are universally supported by ALL fonts
2. **Cascadia Code** renders ASCII perfectly
3. **Letters and numbers** are always safe
4. **Symbols like `#`, `*`, `+`, `>`, `[]`** work in all monospace fonts

---

## Legend for New Icons

### Buildings
- `H` = House
- `F` = Farm  
- `W` = Warehouse
- `M` = Mine
- `L` = Lumberyard
- `K` = worKshop
- `$` = Market
- `O` = well (circle shape)
- `T` = Town hall

### Person Tasks
- `Z` = Sleeping (Zzz)
- `H` = going Home
- `>` = going to Work (arrow)
- `W` = Working
- `C` = Constructing
- `R` = Resting
- `M` = Moving
- `I` = Idle

### Log Levels
- `.` = Info
- `!` = Warning
- `X` = Error
- `+` = Success

### Misc
- `#` = Families (people/population)
- `[]` = Buildings
- `*` = Save status

---

## Status

? **Not yet applied** - Manual changes required  
? **Awaiting**: Edit `SidebarRenderer.cs` with ASCII replacements  
? **Once done**: Build ? Restart ? All UI will render correctly!

---

## Future: Emoji Font Support

If you want the full emoji/symbol experience later:

1. Add `Segoe UI Emoji` font to project
2. Remove ASCII-only overrides
3. Font system will auto-detect emoji support
4. Game will render beautiful Unicode symbols

But for now, **ASCII-only is the reliable solution** that works everywhere!

---

**Related Documentation:**
- `TERRAIN_EMOJI_RENDERING_FIX.md` - Terrain decoration fix (already applied)
- `FONT_CONFIGURATION.md` - Font system overview

**Files Modified:**
- `VillageBuilder.Game/Graphics/UI/SidebarRenderer.cs` - UI symbol replacements

---

**Last Updated:** 2024-01-XX  
**Status:** Ready to apply
