# Refactoring Review & Testing Strategy

## ?? **What We've Built: 5 Major Phases**

### **Phase 1: Configuration System** ?
**Status:** Complete, Building Successfully
**Integration Status:** ?? Partially Integrated

#### What Works:
- ? All config classes created (GameConfig, TerrainConfig, etc.)
- ? JSON serialization implemented
- ? TerrainGenerator uses TerrainConfig

#### What's Not Integrated Yet:
- ?? GameEngine doesn't use GameConfig
- ?? WildlifeManager doesn't use WildlifeConfig
- ?? Renderers don't use RenderConfig colors

#### Test Strategy:
```csharp
// Manual Test 1: Config Save/Load
var config = GameConfig.Instance;
config.Terrain.GrassTuftDensity = 0.01f; // Very low
config.SaveToFile("test_config.json");

var loaded = GameConfig.LoadFromFile("test_config.json");
// Start game and verify grass tufts are sparse

// Manual Test 2: Terrain Density
config.Terrain.GlobalDecorationMultiplier = 0.0f; // No decorations
// Start game and verify no decorations spawn
```

---

### **Phase 2: Subsystem Architecture** ?
**Status:** Complete, Building Successfully
**Integration Status:** ?? Not Integrated

#### What Works:
- ? All interfaces defined (ISimulationSystem, IResourceSystem, etc.)
- ? All adapters created and compiling
- ? Backward-compatible APIs

#### What's Not Integrated Yet:
- ?? GameEngine still uses old direct fields
- ?? No code uses the new subsystem interfaces
- ?? Subsystems are ready but not wired up

#### Test Strategy:
```csharp
// Unit Test: SelectionManager<T>
[Test]
public void TestSelectionManager()
{
    var manager = new SelectionManager<PersonSelectable>();
    var people = GetTestPeople();
    
    manager.SelectMultiple(people.Select(p => new PersonSelectable(p)).ToList());
    Assert.AreEqual(people[0], manager.SelectedEntity.Person);
    
    manager.CycleNext();
    Assert.AreEqual(people[1], manager.SelectedEntity.Person);
}

// Integration Test: Create subsystems
var grid = new VillageGrid(100, 100, 12345);
var wildlifeSystem = new WildlifeSystemAdapter(grid, 12345);
wildlifeSystem.InitializeWildlife(grid);
Assert.IsTrue(wildlifeSystem.GetTotalPopulation() > 0);
```

---

### **Phase 3: Rendering Architecture** ?
**Status:** Complete, Building Successfully
**Integration Status:** ?? Not Integrated

#### What Works:
- ? All renderers created (TerrainRenderer, PersonRenderer, etc.)
- ? RenderContext encapsulates state
- ? RenderHelpers eliminate duplication
- ? ColorPalette centralizes colors

#### What's Not Integrated Yet:
- ?? MapRenderer still uses old monolithic code
- ?? New renderers not called anywhere
- ?? CompositeMapRenderer not created

#### Test Strategy:
```csharp
// Visual Test: Render a single tile
var renderer = new TerrainRenderer();
var context = new RenderContext
{
    TileSize = 32,
    DarknessFactor = 0.0f,
    UseSpriteMode = false
};

var tile = new Tile(0, 0, TileType.Grass);
renderer.Render(tile, context);
// Manually verify tile renders correctly

// Comparison Test: Old vs New
// 1. Take screenshot with old MapRenderer
// 2. Switch to new renderers
// 3. Take screenshot again
// 4. Compare visually - should be identical
```

---

### **Phase 4: Selection System** ?
**Status:** Complete, Building Successfully
**Integration Status:** ?? Not Integrated

#### What Works:
- ? Generic SelectionManager<T> working
- ? SelectionCoordinator provides backward-compatible API
- ? ISelectable interface defined

#### What's Not Integrated Yet:
- ?? InputHandler still uses old SelectionManager
- ?? Renderers still use old SelectionManager
- ?? SidebarRenderer still uses old SelectionManager

#### Test Strategy:
```csharp
// Unit Test: Selection Cycling
var coordinator = new SelectionCoordinator();
var people = new List<Person> { person1, person2, person3 };

coordinator.SelectPeopleAtTile(people);
Assert.AreEqual(person1, coordinator.SelectedPerson);

coordinator.CycleNextPerson();
Assert.AreEqual(person2, coordinator.SelectedPerson);

coordinator.CyclePreviousPerson();
Assert.AreEqual(person1, coordinator.SelectedPerson);

// Integration Test: Replace old with new
// 1. Replace all `SelectionManager` references with `SelectionCoordinator`
// 2. Run game
// 3. Test clicking entities
// 4. Test Tab cycling
// 5. Verify selection highlighting works
```

---

### **Phase 5: UI Panel System** ?
**Status:** Complete, Building Successfully
**Integration Status:** ?? Not Integrated

#### What Works:
- ? IPanel interface defined
- ? BasePanel utilities created
- ? QuickStatsPanel example working

#### What's Not Integrated Yet:
- ?? SidebarRenderer doesn't use panels
- ?? Only QuickStatsPanel created (need PersonInfo, Wildlife, Building panels)

#### Test Strategy:
```csharp
// Unit Test: Panel Can Render
var panel = new QuickStatsPanel();
var context = new PanelRenderContext
{
    SelectionManager = null, // Nothing selected
    Engine = testEngine
};

Assert.IsTrue(panel.CanRender(context));

context.SelectionManager = new SelectionManager { SelectedPerson = testPerson };
Assert.IsFalse(panel.CanRender(context)); // Should not render when something selected

// Visual Test: Render panel
int nextY = panel.Render(context);
// Manually verify QuickStats displays correctly
```

---

## ?? **Integration Status Summary**

| Phase | Built | Compiles | Integrated | Tested | Status |
|-------|-------|----------|------------|--------|--------|
| Phase 1: Config | ? | ? | ?? Partial | ? | Ready for integration |
| Phase 2: Subsystems | ? | ? | ? | ? | Ready for integration |
| Phase 3: Rendering | ? | ? | ? | ? | Ready for integration |
| Phase 4: Selection | ? | ? | ? | ? | Ready for integration |
| Phase 5: Panels | ? | ? | ?? Partial | ? | Ready for integration |

---

## ?? **Testing Strategy: 3 Levels**

### **Level 1: Unit Tests (Isolated Components)**

Create test project with unit tests for each system:

```csharp
// VillageBuilder.Tests/SelectionManagerTests.cs
[TestFixture]
public class SelectionManagerTests
{
    [Test]
    public void SelectMultiple_WithValidList_SelectsFirst()
    {
        var manager = new SelectionManager<PersonSelectable>();
        var selectables = CreateTestSelectables(3);
        
        manager.SelectMultiple(selectables);
        
        Assert.NotNull(manager.SelectedEntity);
        Assert.AreEqual(selectables[0], manager.SelectedEntity);
        Assert.AreEqual(0, manager.SelectedIndex);
    }
    
    [Test]
    public void CycleNext_WithMultipleEntities_CyclesCorrectly()
    {
        var manager = new SelectionManager<PersonSelectable>();
        var selectables = CreateTestSelectables(3);
        manager.SelectMultiple(selectables);
        
        manager.CycleNext();
        Assert.AreEqual(selectables[1], manager.SelectedEntity);
        
        manager.CycleNext();
        Assert.AreEqual(selectables[2], manager.SelectedEntity);
        
        manager.CycleNext(); // Wrap around
        Assert.AreEqual(selectables[0], manager.SelectedEntity);
    }
    
    [Test]
    public void CyclePrevious_WithMultipleEntities_CyclesBackward()
    {
        var manager = new SelectionManager<PersonSelectable>();
        var selectables = CreateTestSelectables(3);
        manager.SelectMultiple(selectables);
        
        manager.CyclePrevious(); // Wrap to end
        Assert.AreEqual(selectables[2], manager.SelectedEntity);
    }
}

// VillageBuilder.Tests/TerrainRendererTests.cs
[TestFixture]
public class TerrainRendererTests
{
    [Test]
    public void ShouldRender_WithBuilding_ReturnsFalse()
    {
        var renderer = new TerrainRenderer();
        var tile = new Tile(0, 0, TileType.Grass);
        tile.Building = new Building(...);
        
        var context = CreateTestContext();
        
        Assert.IsFalse(renderer.ShouldRender(tile, context));
    }
    
    [Test]
    public void ShouldRender_OutOfBounds_ReturnsFalse()
    {
        var renderer = new TerrainRenderer();
        var tile = new Tile(1000, 1000, TileType.Grass); // Way outside
        
        var context = CreateTestContext();
        context.ViewBounds = new Rectangle(0, 0, 10, 10);
        
        Assert.IsFalse(renderer.ShouldRender(tile, context));
    }
}
```

### **Level 2: Integration Tests (System Interaction)**

Test how systems work together:

```csharp
// VillageBuilder.Tests/IntegrationTests.cs
[TestFixture]
public class IntegrationTests
{
    [Test]
    public void NewSelectionSystem_WorksWithOldCode()
    {
        var coordinator = new SelectionCoordinator();
        var engine = CreateTestEngine();
        var person = engine.Families[0].Members[0];
        
        // Test backward compatibility
        coordinator.SelectPerson(person);
        Assert.AreEqual(person, coordinator.SelectedPerson);
        Assert.AreEqual(SelectionType.Person, coordinator.CurrentSelectionType);
    }
    
    [Test]
    public void ConfigSystem_LoadsAndApplies()
    {
        var config = new GameConfig();
        config.Terrain.GrassTuftDensity = 0.05f;
        
        var generator = new TerrainGenerator(100, 100, 12345, config.Terrain);
        var tiles = generator.Generate();
        
        // Count grass tufts
        int tuftCount = tiles.Cast<Tile>()
            .Sum(t => t.Decorations.Count(d => d.Type == DecorationType.GrassTuft));
        
        Assert.Greater(tuftCount, 0);
        Assert.Less(tuftCount, 1000); // Reasonable range
    }
}
```

### **Level 3: Manual/Visual Tests (User Experience)**

Create a test checklist for manual verification:

```markdown
## Manual Test Checklist

### Configuration System
- [ ] Create test_config.json with extreme values
- [ ] Load config and start game
- [ ] Verify terrain looks different (more/fewer decorations)
- [ ] Save game with custom config
- [ ] Load save and verify config persists

### Selection System
- [ ] Click on a person
- [ ] Verify selection highlights correctly
- [ ] Press Tab - verify cycles to next person
- [ ] Press Shift+Tab - verify cycles backward
- [ ] Click on wildlife
- [ ] Verify wildlife selection works
- [ ] Tab through multiple wildlife on same tile

### Rendering System
- [ ] Game renders without errors
- [ ] Terrain tiles render correctly
- [ ] People render correctly
- [ ] Wildlife renders correctly
- [ ] Buildings render correctly
- [ ] Selection highlighting visible
- [ ] Day/night cycle works (darkness effect)

### Panel System
- [ ] With nothing selected, QuickStats panel shows
- [ ] Shows families count
- [ ] Shows buildings count
- [ ] Click person, verify panel changes
- [ ] Click wildlife, verify panel changes
- [ ] ESC to deselect, verify returns to QuickStats
```

---

## ?? **Recommended Testing Approach**

### **Phase A: Verify Current State (Baseline)**
1. ? Run build - should succeed
2. ? Run game - should work as before
3. ? Test all major features still work
4. ? Take screenshots of current UI

### **Phase B: Unit Test New Systems**
1. Create `VillageBuilder.Tests` project
2. Add tests for Phase 4 (SelectionManager) - easiest to test
3. Add tests for Phase 3 (Renderers) - verify ShouldRender logic
4. Add tests for Phase 5 (Panels) - verify CanRender logic
5. Run tests - should all pass

### **Phase C: Integration Testing**
1. Wire up one system at a time
2. Start with Phase 4 (Selection) - lowest risk
   - Replace `SelectionManager` with `SelectionCoordinator` in InputHandler
   - Test clicking and cycling
3. Then Phase 1 (Config)
   - Use configs in GameEngine constructor
   - Verify configurable behavior
4. Then Phase 3 (Rendering)
   - Create CompositeMapRenderer
   - Switch MapRenderer to use it
   - Compare screenshots (before/after)

### **Phase D: Performance Testing**
1. Measure FPS before changes
2. Integrate new systems
3. Measure FPS after
4. Should be same or better (culling optimizations)

---

## ?? **What Should We Test First?**

### **Priority 1: Selection System (Phase 4)** ???
**Why:** 
- Easiest to integrate
- Backward compatible API
- Clear success criteria (selection works)
- Low risk

**How:**
1. Create unit tests for SelectionManager<T>
2. Test cycling forward/backward
3. Test with different entity types
4. Wire up SelectionCoordinator in InputHandler
5. Manual test in-game

**Expected Time:** 1-2 hours

---

### **Priority 2: Configuration System (Phase 1)** ??
**Why:**
- Already partially integrated (TerrainGenerator)
- Easy to verify (change config, see visual difference)
- Good for gameplay tuning

**How:**
1. Create test config file with extreme values
2. Load in GameEngine
3. Start game and verify terrain looks different
4. Document config options

**Expected Time:** 1 hour

---

### **Priority 3: Panel System (Phase 5)** ?
**Why:**
- Only infrastructure built
- Need to extract actual panels
- Lower priority (UI enhancement)

**How:**
1. Extract PersonInfoPanel from SidebarRenderer
2. Test it renders correctly
3. Extract WildlifeInfoPanel
4. Wire up panel system

**Expected Time:** 2-3 hours

---

## ?? **Recommended Next Steps**

### **Option A: Quick Validation (30 min)**
```bash
# 1. Build everything
dotnet build

# 2. Run game
dotnet run --project VillageBuilder.Game

# 3. Test these scenarios:
- Start game
- Click person - selection works?
- Press Tab - cycling works?
- Generate world - decorations spawn?
- Check event log - no errors?
```

### **Option B: Create Test Project (2 hours)**
```bash
# 1. Create test project
dotnet new nunit -o VillageBuilder.Tests
cd VillageBuilder.Tests
dotnet add reference ../VillageBuilder.Engine/VillageBuilder.Engine.csproj
dotnet add reference ../VillageBuilder.Game/VillageBuilder.Game.csproj

# 2. Add selection tests
# 3. Add renderer tests
# 4. Run tests
dotnet test
```

### **Option C: Integration Test (1-2 hours)**
1. Pick one system (e.g., Selection)
2. Wire it up in real code
3. Test in-game
4. Verify works as before
5. Commit if successful

---

## ? **What Do You Want to Do?**

1. **Quick validation** - Just verify game still runs?
2. **Create unit tests** - Professional testing approach?
3. **Integration test** - Wire up Phase 4 (Selection) and test?
4. **Visual comparison** - Test rendering changes?
5. **Config testing** - Test different configurations?

**I recommend Option 3 (Integration Test for Phase 4)** because:
- ? Lowest risk
- ? Clear success criteria
- ? See immediate benefit
- ? Validates architecture works in practice

What would you like to do? ??
