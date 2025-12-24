# Optimization Changelog

This file tracks all performance optimizations made to VillageBuilder, including dates, changes, and measured impact.

## Format

Each entry includes:
- **Date**: When the optimization was implemented
- **Component**: What was optimized
- **Issue**: What problem was identified
- **Solution**: How it was fixed
- **Impact**: Measured performance improvement
- **Files Changed**: List of modified files
- **Benchmark**: Link to benchmark results

---

## 2024-01-XX - Game Loop Tile Clearing Optimization

### Component
`GameEngine.SimulateTick()` - Tile person registration system

### Issue Identified
The game loop was clearing all 10,000 tiles (100×100 grid) every tick, regardless of how many tiles actually had people on them.

**Profiler Data:**
- 100% of tiles cleared every tick
- 99.91% wasted iterations for small villages (9 people)
- 99.25% wasted iterations for large villages (75 people)
- Time complexity: O(Width × Height)

**Code Before:**
```csharp
for (int x = 0; x < Grid.Width; x++)
{
    for (int y = 0; y < Grid.Height; y++)
    {
        var tile = Grid.GetTile(x, y);
        if (tile != null)
        {
            tile.PeopleOnTile.Clear();
        }
    }
}
```

### Solution Implemented
Added a `HashSet<(int x, int y)>` to track only tiles that have people on them. Changed the clearing logic to iterate only occupied tiles.

**Code After:**
```csharp
private readonly HashSet<(int x, int y)> _occupiedTiles = new HashSet<(int x, int y)>();

// Clear only occupied tiles
foreach (var (x, y) in _occupiedTiles)
{
    var tile = Grid.GetTile(x, y);
    if (tile != null)
    {
        tile.PeopleOnTile.Clear();
    }
}
_occupiedTiles.Clear();

// Track occupied tiles during registration
foreach (var family in Families)
{
    foreach (var person in family.Members)
    {
        if (!person.IsAlive) continue;
        
        var tile = Grid.GetTile(person.Position.X, person.Position.Y);
        if (tile != null)
        {
            tile.PeopleOnTile.Add(person);
            _occupiedTiles.Add((person.Position.X, person.Position.Y));
        }
    }
}
```

### Performance Impact

| Village Size | Before | After | Improvement |
|--------------|--------|-------|-------------|
| Small (9 people) | 126.2 ?s | 40.38 ?s | **-68.0%** (-85.82 ?s) |
| Medium (30 people) | 171.3 ?s | 95.76 ?s | **-44.1%** (-75.54 ?s) |
| Large (75 people) | 258.4 ?s | 178.69 ?s | **-30.8%** (-79.71 ?s) |

**Key Metrics:**
- Time complexity changed from O(Width × Height) to O(Number of People)
- Performance now scales linearly with entity count, not map size
- HashSet operations are O(1) for insert and iterate

### Files Changed
- `VillageBuilder.Engine/Core/GameEngine.cs`
  - Added `_occupiedTiles` field (line ~24)
  - Modified `SimulateTick()` method (lines ~144-170)

### Benchmark
See: `BenchmarkSuite1/GameEngineSimulationBenchmark.cs`
- `SimulateTick_SmallVillage`
- `SimulateTick_MediumVillage`
- `SimulateTick_LargeVillage`

### Additional Notes
- Also removed LINQ `.Where()` allocation in person iteration loop
- Changed from `family.Members.Where(p => p.IsAlive)` to direct iteration with `if (!person.IsAlive) continue`
- This eliminated unnecessary enumerator allocations

---

## 2024-01-XX - Save File Listing Cache

### Component
`SaveLoadService.GetSaveFiles()` - Filesystem save file enumeration

### Issue Identified
The save file listing was scanning the filesystem 60 times per second (every frame) to display save status in the sidebar UI.

**Profiler Data:**
- `Directory.Exists()`: 5.66% CPU per frame
- `Directory.GetFiles()`: 3.92% CPU per frame
- `List<T>.ctor()`: 4.88% CPU per frame
- **Total**: 14.56% CPU spent on filesystem I/O every frame

**Code Before:**
```csharp
public static string[] GetSaveFiles()
{
    InitializeSaveDirectory();
    var files = Directory.GetFiles(SaveDirectory, $"*{SaveExtension}");
    return Array.ConvertAll(files, Path.GetFileNameWithoutExtension);
}
```

### Solution Implemented
Added a 1-second cache for save file listings. The cache is automatically invalidated when saves are created or deleted.

**Code After:**
```csharp
private static string[]? _cachedSaveFiles = null;
private static DateTime _cacheTimestamp = DateTime.MinValue;
private static readonly TimeSpan CacheExpiration = TimeSpan.FromSeconds(1);

public static string[] GetSaveFiles()
{
    // Check if cache is valid
    if (_cachedSaveFiles != null && (DateTime.Now - _cacheTimestamp) < CacheExpiration)
    {
        return _cachedSaveFiles;
    }
    
    // Refresh cache
    InitializeSaveDirectory();
    var files = Directory.GetFiles(SaveDirectory, $"*{SaveExtension}");
    _cachedSaveFiles = Array.ConvertAll(files, Path.GetFileNameWithoutExtension);
    _cacheTimestamp = DateTime.Now;
    
    return _cachedSaveFiles;
}

public static void InvalidateCache()
{
    _cachedSaveFiles = null;
    _cacheTimestamp = DateTime.MinValue;
}
```

Cache invalidation added to:
- `SaveGame()` - after successful save
- `DeleteSave()` - after file deletion

### Performance Impact

| Metric | Before | After | Improvement |
|--------|--------|-------|-------------|
| Filesystem calls per second | 60 | 1 | **-98.3%** |
| UI rendering overhead | 14.56% CPU | ~0.24% CPU | **-14.32%** CPU |

**Note:** The optimization primarily benefits UI rendering, not the game loop. The sidebar rendering benchmark showed elimination of filesystem overhead, though overall rendering is still dominated by Raylib text drawing (73-85% of sidebar rendering time).

### Files Changed
- `VillageBuilder.Game/Core/SaveLoadService.cs`
  - Added cache fields (lines ~17-19)
  - Added `InvalidateCache()` method
  - Modified `GetSaveFiles()` to use cache
  - Modified `SaveGame()` to invalidate cache
  - Modified `DeleteSave()` to invalidate cache

### Benchmark
See: `BenchmarkSuite1/TextRenderingBenchmark.cs`
- `RenderSidebar_NoOptimization` (baseline)
- `RenderSidebar_WithCaching` (optimized)

### Additional Notes
- Cache expiration set to 1 second to balance freshness with performance
- Cache is proactively invalidated on save/delete operations for immediate UI updates
- Users will see updated save list within 1 second maximum, typically immediately after save/load

---

## Future Optimization Candidates

### Not Yet Implemented

These optimizations have been identified but not yet implemented. They are documented here for future reference.

#### 1. Collection Reuse/Pooling
**Estimated Impact:** 10-15 ?s per tick

**Issue:**
```csharp
var arrivedFamilies = new Dictionary<Building, List<Person>>();  // Allocated every family, every tick
```

**Proposed Solution:**
- Maintain reusable collection as class field
- Clear and reuse instead of allocating new

**Complexity:** Low
**Risk:** Low

---

#### 2. Skip Empty Work
**Estimated Impact:** 15-25 ?s per tick

**Issue:**
```csharp
foreach (var person in family.Members)
{
    if (person.CurrentPath.Count > 0)  // Checked for all people, most have empty paths
    {
        // Movement logic
    }
}
```

**Proposed Solution:**
- Maintain list of "active movers"
- Only iterate people who are actually moving

**Complexity:** Medium
**Risk:** Medium (requires careful state management)

---

#### 3. Cached Building Filters
**Estimated Impact:** 20-30 ?s per tick

**Issue:**
```csharp
foreach (var building in Buildings.Where(b => b.IsConstructed))  // LINQ every tick
{
    building.Work(Time.CurrentSeason);
}
```

**Proposed Solution:**
- Cache `List<Building> _constructedBuildings`
- Invalidate when construction completes

**Complexity:** Low
**Risk:** Low

---

#### 4. Spatial Partitioning
**Estimated Impact:** Significant for large populations (100+ people)

**Issue:**
- Currently O(n) searches through all people
- No spatial indexing for collision detection

**Proposed Solution:**
- Implement quad-tree or grid-based spatial index
- Update index as people move

**Complexity:** High
**Risk:** Medium (complex data structure)

---

## Benchmarking Infrastructure

### 2024-01-XX - Initial Benchmark Suite

**Created:**
- `BenchmarkSuite1/` project
- `GameEngineSimulationBenchmark.cs` - Measures SimulateTick()
- `TextRenderingBenchmark.cs` - Measures UI rendering
- `Program.cs` - BenchmarkSwitcher runner

**Features:**
- CPU usage diagnostics
- Iteration setup for state reset
- Snapshot system for fast state restore
- Multiple village size scenarios

**Documentation:**
- [BENCHMARK_GUIDE.md](./BENCHMARK_GUIDE.md) - How to create and run benchmarks
- [PERFORMANCE_OPTIMIZATIONS.md](./PERFORMANCE_OPTIMIZATIONS.md) - Optimization details and results

---

## Metrics and Goals

### Current Performance (After Optimizations)

**Frame Budget at 60 FPS:** 16,667 ?s per frame

| Village Size | SimulateTick Time | % of Frame Budget | Status |
|--------------|-------------------|-------------------|--------|
| Small (9 people) | 40.38 ?s | 0.24% | ? Excellent |
| Medium (30 people) | 95.76 ?s | 0.57% | ? Excellent |
| Large (75 people) | 178.69 ?s | 1.07% | ? Excellent |

**Capacity Headroom:**
- Current largest village uses only 1.07% of frame budget
- Could theoretically support **~100x larger villages** before hitting 60 FPS limit
- Realistic limit considering rendering: ~500-1000 people before other bottlenecks

### Performance Goals

**Target:** Maintain <5% frame budget for SimulateTick() even with:
- 200+ people
- 100+ buildings
- Complex pathfinding
- Weather/season simulation

**Current Status:** ? On track - significant headroom remaining

---

## Review Process

Before implementing new optimizations:

1. **Profile First** - Use Visual Studio profiler to confirm bottleneck
2. **Create Benchmark** - Establish baseline measurement
3. **Document Issue** - Add entry to this changelog with "In Progress" status
4. **Implement** - Make changes to production code only
5. **Measure Impact** - Re-run benchmark
6. **Update Documentation** - Complete changelog entry with results
7. **Code Review** - Ensure no regressions

---

## References

- [PERFORMANCE_OPTIMIZATIONS.md](./PERFORMANCE_OPTIMIZATIONS.md) - Detailed optimization analysis
- [BENCHMARK_GUIDE.md](./BENCHMARK_GUIDE.md) - How to create benchmarks
- BenchmarkSuite1 project - Contains all benchmarks

---

**Last Updated:** 2024-01-XX  
**Maintained By:** VillageBuilder Development Team
