# VillageBuilder Architecture Refactoring Plan

## Current Architecture Analysis

### ?? Issues Identified

#### 1. **Tight Coupling & God Objects**
- `GameEngine` has too many responsibilities (simulation, wildlife, families, buildings, resources)
- `MapRenderer` mixes rendering logic for multiple entity types
- `SidebarRenderer` handles all UI panel types in one class
- Direct dependencies between layers (Game ? Engine ? specific implementations)

#### 2. **Violation of Single Responsibility Principle (SRP)**
- `MapRenderer`: Renders tiles, buildings, people, wildlife, decorations, paths, selection
- `GameEngine`: Manages time, resources, families, buildings, wildlife, pathfinding
- `SidebarRenderer`: Handles person info, wildlife info, building info, tile inspection
- `TerrainGenerator`: Generation + decoration placement mixed

#### 3. **Configuration Scattered Throughout Code**
- Decoration spawn rates hardcoded in `TerrainGenerator`
- Wildlife population numbers in `WildlifeManager`
- Visual settings spread across multiple renderer classes
- No central configuration system

#### 4. **Code Duplication (DRY Violations)**
- Person cycling logic duplicated for wildlife
- Selection highlighting code repeated in multiple places
- Stat bar rendering duplicated
- Color management duplicated across renderers

#### 5. **Open/Closed Principle Violations**
- Adding new entity types requires modifying multiple classes
- Renderer classes not extensible without modification
- Selection system tightly coupled to specific entity types

---

## ?? Refactoring Strategy

### Phase 1: Extract Configuration System ???
**Priority: HIGH | Effort: LOW | Impact: HIGH**

Create centralized configuration management:

```csharp
// VillageBuilder.Engine/Config/GameConfig.cs
public class GameConfig
{
    // Singleton or DI
    public TerrainConfig Terrain { get; set; }
    public WildlifeConfig Wildlife { get; set; }
    public RenderConfig Rendering { get; set; }
    public SimulationConfig Simulation { get; set; }
}

// VillageBuilder.Engine/Config/TerrainConfig.cs
public class TerrainConfig
{
    public float GrassTuftDensity { get; set; } = 0.08f;
    public float WildflowerDensity { get; set; } = 0.05f;
    public float RockDensity { get; set; } = 0.02f;
    // ... etc
}

// VillageBuilder.Engine/Config/WildlifeConfig.cs
public class WildlifeConfig
{
    public Dictionary<WildlifeType, int> InitialPopulation { get; set; }
    public float HuntingSuccessRate { get; set; } = 0.3f;
    public int MaxPopulationPerType { get; set; } = 50;
}
```

**Benefits:**
- ? Single source of truth for all settings
- ? Easy to save/load configurations
- ? Runtime tuning without recompilation
- ? Unit testing with different configs

---

### Phase 2: Split GameEngine into Subsystems ???
**Priority: HIGH | Effort: MEDIUM | Impact: HIGH**

Apply **Facade Pattern** and **Subsystem Pattern**:

```csharp
// VillageBuilder.Engine/Core/GameEngine.cs (refactored)
public class GameEngine
{
    // Subsystems (composition over god object)
    public ISimulationSystem Simulation { get; private set; }
    public IResourceSystem Resources { get; private set; }
    public IPopulationSystem Population { get; private set; }
    public IWildlifeSystem Wildlife { get; private set; }
    public IBuildingSystem Buildings { get; private set; }
    public IWorldSystem World { get; private set; }
    
    public void SimulateTick()
    {
        Simulation.Tick();
        // Each subsystem manages itself
    }
}

// VillageBuilder.Engine/Systems/ISimulationSystem.cs
public interface ISimulationSystem
{
    GameTime Time { get; }
    void Tick();
    void SetTimeScale(float scale);
}

// VillageBuilder.Engine/Systems/IResourceSystem.cs
public interface IResourceSystem
{
    Dictionary<ResourceType, int> Resources { get; }
    bool TryConsumeResources(Dictionary<ResourceType, int> cost);
    void AddResources(ResourceType type, int amount);
}
```

**Benefits:**
- ? Clear separation of concerns
- ? Each system can be tested independently
- ? Systems can be swapped/mocked
- ? Easier to understand and maintain

---

### Phase 3: Refactor Rendering Architecture ???
**Priority: HIGH | Effort: MEDIUM | Impact: MEDIUM**

Apply **Strategy Pattern** and **Renderer Pattern**:

```csharp
// VillageBuilder.Game/Graphics/Rendering/IRenderer.cs
public interface IRenderer<T>
{
    void Render(T entity, RenderContext context);
    bool ShouldRender(T entity, RenderContext context);
}

// VillageBuilder.Game/Graphics/Rendering/RenderContext.cs
public class RenderContext
{
    public Camera2D Camera { get; set; }
    public int TileSize { get; set; }
    public float DarknessFactor { get; set; }
    public Rectangle ViewBounds { get; set; }
    public SelectionManager Selection { get; set; }
}

// Specific renderers
public class WildlifeRenderer : IRenderer<WildlifeEntity>
{
    public void Render(WildlifeEntity wildlife, RenderContext context) 
    { 
        // Wildlife-specific rendering logic
    }
}

public class PersonRenderer : IRenderer<Person> { ... }
public class BuildingRenderer : IRenderer<Building> { ... }
public class TerrainRenderer : IRenderer<Tile> { ... }

// VillageBuilder.Game/Graphics/Rendering/CompositeRenderer.cs
public class CompositeRenderer
{
    private readonly List<IRenderer<object>> _renderers = new();
    
    public void RegisterRenderer<T>(IRenderer<T> renderer) { ... }
    
    public void RenderAll(RenderContext context)
    {
        foreach (var renderer in _renderers)
        {
            // Render entities with their specific renderer
        }
    }
}
```

**Benefits:**
- ? Each entity type has its own renderer
- ? Easy to add new entity types
- ? Rendering logic encapsulated
- ? Can enable/disable renderers dynamically

---

### Phase 4: Extract Selection System ??
**Priority: MEDIUM | Effort: LOW | Impact: MEDIUM**

Current `SelectionManager` violates SRP by handling multiple entity types:

```csharp
// VillageBuilder.Game/Core/Selection/ISelectable.cs
public interface ISelectable
{
    string GetDisplayName();
    Vector2Int GetPosition();
    SelectionType GetSelectionType();
}

// VillageBuilder.Game/Core/Selection/SelectionManager.cs (refactored)
public class SelectionManager<T> where T : ISelectable
{
    private T? _selected;
    private List<T>? _multiSelection;
    private int _selectedIndex;
    
    public void Select(T item) { ... }
    public void SelectMultiple(List<T> items) { ... }
    public void CycleNext() { ... }
    public void CyclePrevious() { ... }
}

// Specific managers
public class PersonSelectionManager : SelectionManager<Person> { }
public class WildlifeSelectionManager : SelectionManager<WildlifeEntity> { }
```

**Benefits:**
- ? DRY - no duplicated cycling logic
- ? Generic implementation for all selectable types
- ? Easy to add new selectable entities

---

### Phase 5: Extract UI Panel System ??
**Priority: MEDIUM | Effort: MEDIUM | Impact: MEDIUM**

Split `SidebarRenderer` into individual panels:

```csharp
// VillageBuilder.Game/Graphics/UI/Panels/IPanel.cs
public interface IPanel
{
    bool CanRender(SelectionManager selection);
    int Render(int startY, int width, RenderContext context);
}

// VillageBuilder.Game/Graphics/UI/Panels/PersonInfoPanel.cs
public class PersonInfoPanel : IPanel
{
    public bool CanRender(SelectionManager selection) 
        => selection.SelectedPerson != null;
    
    public int Render(int startY, int width, RenderContext context)
    {
        // Person-specific rendering
        return nextY;
    }
}

// Similar for:
// - WildlifeInfoPanel
// - BuildingInfoPanel
// - TileInspectorPanel
// - QuickStatsPanel

// VillageBuilder.Game/Graphics/UI/SidebarRenderer.cs (refactored)
public class SidebarRenderer
{
    private readonly List<IPanel> _panels = new();
    
    public void RegisterPanel(IPanel panel) => _panels.Add(panel);
    
    public void Render(GameEngine engine, SelectionManager selection)
    {
        int currentY = _sidebarY + Padding;
        
        // Find first panel that can render
        foreach (var panel in _panels)
        {
            if (panel.CanRender(selection))
            {
                currentY = panel.Render(currentY, _sidebarWidth, context);
                break;
            }
        }
        
        RenderEventLog(currentY);
    }
}
```

**Benefits:**
- ? Each panel is self-contained
- ? Easy to add new panel types
- ? Panels can be reordered or toggled
- ? Better testability

---

### Phase 6: Create Decoration System ??
**Priority: MEDIUM | Effort: LOW | Impact: MEDIUM**

Extract decoration placement logic:

```csharp
// VillageBuilder.Engine/World/Decorations/IDecorationPlacer.cs
public interface IDecorationPlacer
{
    bool CanPlace(Tile tile, TerrainConfig config);
    TerrainDecoration CreateDecoration(Tile tile);
}

// VillageBuilder.Engine/World/Decorations/GrassTuftPlacer.cs
public class GrassTuftPlacer : IDecorationPlacer
{
    public bool CanPlace(Tile tile, TerrainConfig config)
    {
        return tile.Type == TileType.Grass && 
               Random.Shared.NextDouble() < config.GrassTuftDensity;
    }
    
    public TerrainDecoration CreateDecoration(Tile tile)
    {
        return new TerrainDecoration(
            DecorationType.GrassTuft,
            tile.X, tile.Y,
            Random.Shared.Next(4)
        );
    }
}

// VillageBuilder.Engine/World/DecorationSystem.cs
public class DecorationSystem
{
    private readonly List<IDecorationPlacer> _placers = new();
    
    public void RegisterPlacer(IDecorationPlacer placer) => _placers.Add(placer);
    
    public void PlaceDecorations(Tile tile, TerrainConfig config)
    {
        foreach (var placer in _placers)
        {
            if (placer.CanPlace(tile, config))
            {
                tile.Decorations.Add(placer.CreateDecoration(tile));
            }
        }
    }
}
```

**Benefits:**
- ? Each decoration type is independent
- ? Easy to add new decorations
- ? Configuration-driven
- ? Can enable/disable decoration types

---

### Phase 7: Extract Common Rendering Utilities ?
**Priority: LOW | Effort: LOW | Impact: LOW**

Create shared rendering helpers:

```csharp
// VillageBuilder.Game/Graphics/Rendering/RenderHelpers.cs
public static class RenderHelpers
{
    public static void DrawStatBar(int x, int y, int width, int value, Color color)
    {
        // Shared stat bar rendering
    }
    
    public static void DrawSelectionHighlight(Rectangle bounds, Color color)
    {
        // Multi-layer selection highlighting
    }
    
    public static void DrawHealthBar(Rectangle bounds, float health)
    {
        // Standard health bar
    }
}

// VillageBuilder.Game/Graphics/Rendering/ColorPalette.cs
public static class ColorPalette
{
    public static readonly Color GrassBackground = new(35, 50, 35, 255);
    public static readonly Color ForestBackground = new(25, 40, 25, 255);
    public static readonly Color SelectionYellow = new(255, 255, 0, 255);
    // ... centralized color management
}
```

**Benefits:**
- ? DRY - no duplicated rendering code
- ? Consistent visual style
- ? Easy to change colors globally

---

## ?? Refactoring Priority Matrix

| Phase | Priority | Effort | Impact | Dependencies |
|-------|----------|--------|--------|--------------|
| 1. Configuration System | ??? HIGH | LOW | HIGH | None |
| 2. GameEngine Subsystems | ??? HIGH | MEDIUM | HIGH | Phase 1 |
| 3. Rendering Architecture | ??? HIGH | MEDIUM | MEDIUM | Phase 1 |
| 4. Selection System | ?? MEDIUM | LOW | MEDIUM | None |
| 5. UI Panel System | ?? MEDIUM | MEDIUM | MEDIUM | Phase 4 |
| 6. Decoration System | ?? MEDIUM | LOW | MEDIUM | Phase 1 |
| 7. Rendering Utilities | ? LOW | LOW | LOW | Phase 3 |

---

## ?? Recommended Implementation Order

### **Week 1: Foundation**
1. Create configuration system (Phase 1)
2. Extract rendering utilities (Phase 7)

### **Week 2: Core Refactoring**
3. Split GameEngine into subsystems (Phase 2)
4. Refactor rendering architecture (Phase 3)

### **Week 3: UI & Polish**
5. Extract selection system (Phase 4)
6. Split UI panels (Phase 5)

### **Week 4: Optional Enhancements**
7. Create decoration system (Phase 6)

---

## ?? SOLID Principles Applied

### **Single Responsibility Principle**
- ? Each system has one reason to change
- ? Renderers only handle rendering
- ? Managers only handle state

### **Open/Closed Principle**
- ? Open for extension (new renderers, panels)
- ? Closed for modification (interfaces remain stable)

### **Liskov Substitution Principle**
- ? Interfaces can be swapped
- ? Mock implementations for testing

### **Interface Segregation Principle**
- ? Small, focused interfaces
- ? Clients only depend on what they use

### **Dependency Inversion Principle**
- ? Depend on abstractions (IRenderer, IPanel)
- ? Not on concrete implementations

---

## ?? Expected Benefits

### **Maintainability**
- Code is easier to understand
- Changes are localized
- Less risk of breaking changes

### **Testability**
- Each component can be tested in isolation
- Mocking is straightforward
- Unit test coverage increases

### **Extensibility**
- New features require less modification
- Plugin-like architecture
- Community contributions easier

### **Performance**
- Systems can be optimized independently
- Rendering can be parallelized
- Easier to profile and identify bottlenecks

### **Configuration**
- Runtime configuration changes
- Different game modes with config files
- A/B testing gameplay parameters

---

## ?? Quick Wins (Do First)

1. **Extract TerrainConfig** - Move hardcoded densities to config class
2. **Create RenderHelpers** - Extract duplicate stat bar rendering
3. **Split PersonRenderer** - Extract from MapRenderer
4. **Create PersonInfoPanel** - Extract from SidebarRenderer

These provide immediate benefits with minimal risk!

---

Would you like me to start implementing any of these phases? I recommend starting with Phase 1 (Configuration System) as it's low-effort, high-impact, and doesn't require any risky refactoring.
