# Construction System Implementation

## Overview
Buildings now require construction workers and progress through visible stages before becoming operational.

## Key Features

### 1. **Construction Work Calculation**
- Base work: **50 work units per tile**
- Complexity multipliers:
  - Farm: 0.8x (simple structures)
  - House: 1.0x (standard)
  - Lumberyard: 0.9x
  - Warehouse: 1.2x (larger, sturdier)
  - Workshop: 1.3x (complex equipment)
  - Market: 1.4x
  - Mine: 1.5x (requires excavation)
  - Well: 0.7x (small structure)
  - **TownHall: 2.0x** (grand building)

### 2. **Construction Stages**
Buildings progress through 4 visual stages:

| Stage | Progress | Visual | Glyph | Description |
|-------|----------|--------|-------|-------------|
| **Foundation** | 0-25% | Dark brown | `?` | Ground work and foundation |
| **Framing** | 25-50% | Medium brown | `?` | Structural framework |
| **Walls** | 50-75% | Light brown | `?` | Walls and exterior |
| **Finishing** | 75-100% | Almost complete | `?` | Interior and details |

### 3. **Worker System**
- **Only idle families** can be assigned to construction
- Each construction worker contributes **1 work per tick**
- Workers must reach the construction site
- Building tracks workers in `ConstructionWorkers` list
- Workers are freed when construction completes

### 4. **Visual Feedback**

#### Map View
- Different colors for each construction stage
- Progress percentage displayed on center tile
- Worker count shown as `??{count}` icon
- Smooth progression from dark to light colors

#### Sidebar (Building Info)
- Construction status: "Under Construction"
- Current stage name (Foundation/Framing/Walls/Finishing)
- Progress bar with color coding:
  - Brown shades during construction
  - Green when near completion
- List of builder families working on site
- Warning if no builders assigned

### 5. **Gameplay Flow**

```
1. Player places building (H/F/W/L/M/K/E/T keys)
   ?
2. Resources deducted, building appears in Foundation stage
   ?
3. Building is non-functional, shows progress 0%
   ?
4. Player clicks building, selects idle family from sidebar
   ?
5. Family members walk to construction site
   ?
6. Construction progress advances 1 per worker per tick
   ?
7. Visual stage updates: Foundation ? Framing ? Walls ? Finishing
   ?
8. At 100%, building becomes operational
   ?
9. Workers are released, building ready for regular workers
   ?
10. (Houses auto-assign to homeless families)
```

## Code Architecture

### New Classes
- `AssignConstructionWorkersCommand` - Assigns idle family to construction

### Modified Classes

#### Building.cs
- Added `ConstructionStage` enum
- Added `ConstructionWorkRequired` property
- Added `ConstructionWorkers` list
- Added `CalculateConstructionWorkRequired()` method
- Added `GetConstructionStage()` method
- Added `GetConstructionProgressPercent()` method

#### GameEngine.cs
- Added `ProcessConstruction()` method - called every tick
- Added `CompleteConstruction()` method
- Added `AutoAssignHouseToFamily()` helper
- Modified `SimulateTick()` to call construction processor

#### ConstructBuildingCommand.cs
- Buildings now start with `IsConstructed = false`
- Removed instant completion
- Added construction work requirement to success message

#### MapRenderer.cs
- Added `DrawConstructionStages()` method
- Different glyphs and colors per stage
- Progress percentage overlay
- Worker count display

#### SidebarRenderer.cs
- Added construction info panel with:
  - Stage name
  - Progress bar
  - Builder list
- Modified assignment buttons to use `AssignConstructionWorkersCommand` for unfinished buildings

## Player Experience

### Before Construction System
1. Press H to place house
2. House instantly built and operational
3. No worker involvement

### With Construction System
1. Press H to place house
2. House appears in Foundation stage (?) at 0%
3. Message: "assign an idle family to build"
4. Click house in sidebar
5. Click idle family button
6. Watch workers travel to site
7. See progress bar fill: 10%... 25%... 50%...
8. Watch building evolve through stages
9. At 100%: "Construction complete" message
10. House now operational

## Example Construction Times

At 3 workers (typical family):

| Building | Area | Work Required | Time (ticks) | Time (seconds @ 20 TPS) |
|----------|------|---------------|--------------|-------------------------|
| Well (2x2) | 4 | 140 (4×50×0.7) | ~47 | ~2.3s |
| House (4x4) | 16 | 800 (16×50×1.0) | ~267 | ~13s |
| Farm (5x5) | 25 | 1000 (25×50×0.8) | ~333 | ~17s |
| Warehouse (6x6) | 36 | 2160 (36×50×1.2) | ~720 | ~36s |
| TownHall (8x8) | 64 | 6400 (64×50×2.0) | ~2133 | ~107s |

*Times assume continuous work by all 3 workers*

## Future Enhancements
- Construction skill progression (faster building over time)
- Weather delays (can't build in storms)
- Resource delivery mechanics (bring materials to site)
- Building damage requiring repairs
- Upgradeable buildings with additional construction phases
- Construction accidents/delays
- Multiple families working together on large projects

## Technical Notes

### Performance
- Construction processing is O(n) where n = buildings under construction
- Typically 0-5 buildings constructing simultaneously
- Negligible performance impact

### Multiplayer Ready
- Uses command queue system (`AssignConstructionWorkersCommand`)
- Deterministic worker contribution (1 work/tick)
- No client-side prediction needed

### Save/Load Compatible
- All construction state in `Building` properties:
  - `IsConstructed`
  - `ConstructionProgress`
  - `ConstructionWorkRequired`
  - `ConstructionWorkers` list
- Can save mid-construction and resume

## Testing Checklist
- [x] Buildings start unfinished after placement
- [x] Visual stages display correctly
- [x] Progress bar updates smoothly
- [x] Worker assignment command works
- [x] Workers pathfind to construction site
- [x] Progress advances 1 per worker per tick
- [x] Building completes at 100%
- [x] Workers are freed on completion
- [x] Houses auto-assign to families
- [x] Production buildings require separate worker assignment after construction
- [x] Sidebar shows construction info correctly
- [x] Build system compiles without errors

## Known Limitations
- Workers don't show "building" animation (use existing movement)
- No visual distinction between construction worker types
- Can't cancel construction (building remains forever)
- No partial resource refund if abandoned
- Workers don't consume food during construction (uses existing hunger system)

## Success Metrics
? Buildings feel more alive and purposeful
? Player agency increased (must manage construction)
? Visual feedback clear and satisfying
? Construction times balanced for gameplay
? System integrates seamlessly with existing mechanics
