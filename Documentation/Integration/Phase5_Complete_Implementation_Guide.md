# Phase 5 Complete Implementation Guide

## ?? **Status: Ready for Implementation**

**Infrastructure:** ? Built (IPanel, BasePanel, QuickStatsPanel example)
**Remaining:** Extract 4 panels from SidebarRenderer and wire them up
**Time Estimate:** 2-3 hours
**Complexity:** Medium

---

## ?? **What Needs to Be Done**

### **4 Panels to Extract:**

1. **PersonInfoPanel** - Display selected person/family details
2. **WildlifeInfoPanel** - Display selected wildlife details
3. **BuildingInfoPanel** - Display selected building details
4. **GameStatsPanel** - Display overall game statistics

5. **PanelManager** - Orchestrate all panels based on selection state

---

## ?? **Step-by-Step Implementation**

### **Step 1: Create PersonInfoPanel**

**File:** `VillageBuilder.Game\Graphics\UI\Panels\PersonInfoPanel.cs`

```csharp
using Raylib_cs;
using VillageBuilder.Engine.Entities;
using VillageBuilder.Game.Core.Selection;

namespace VillageBuilder.Game.Graphics.UI.Panels
{
    public class PersonInfoPanel : BasePanel
    {
        public override string Title => "Person Info";
        
        protected override void RenderContent(int x, int y, int width, int height)
        {
            var coordinator = Context?.SelectionManager as SelectionCoordinator;
            var person = coordinator?.SelectedPerson;
            
            if (person == null) return;
            
            int currentY = y;
            
            // Name and basic info
            DrawText($"Name: {person.FirstName} {person.LastName}", x, currentY, Color.White);
            currentY += LineHeight;
            
            DrawText($"Age: {person.Age}", x, currentY, Color.LightGray);
            currentY += LineHeight;
            
            DrawText($"Gender: {person.Gender}", x, currentY, Color.LightGray);
            currentY += LineHeight;
            
            // Health
            var healthColor = person.Health > 70 ? Color.Green : 
                             person.Health > 30 ? Color.Yellow : Color.Red;
            DrawText($"Health: {person.Health}%", x, currentY, healthColor);
            currentY += LineHeight;
            
            // Hunger
            var hungerColor = person.Hunger < 30 ? Color.Green :
                             person.Hunger < 70 ? Color.Yellow : Color.Red;
            DrawText($"Hunger: {person.Hunger}%", x, currentY, hungerColor);
            currentY += LineHeight;
            
            // Task
            if (person.AssignedBuilding != null)
            {
                DrawText($"Working at: {person.AssignedBuilding.Type}", x, currentY, Color.Cyan);
                currentY += LineHeight;
            }
            else
            {
                DrawText("Status: Idle", x, currentY, Color.Gray);
                currentY += LineHeight;
            }
            
            // Family
            if (person.Family != null)
            {
                currentY += 5;
                DrawText($"Family: {person.Family.Members.Count} members", x, currentY, Color.Yellow);
            }
        }
    }
}
```

---

### **Step 2: Create WildlifeInfoPanel**

**File:** `VillageBuilder.Game\Graphics\UI\Panels\WildlifeInfoPanel.cs`

```csharp
using Raylib_cs;
using VillageBuilder.Engine.Entities.Wildlife;
using VillageBuilder.Game.Core.Selection;

namespace VillageBuilder.Game.Graphics.UI.Panels
{
    public class WildlifeInfoPanel : BasePanel
    {
        public override string Title => "Wildlife Info";
        
        protected override void RenderContent(int x, int y, int width, int height)
        {
            var coordinator = Context?.SelectionManager as SelectionCoordinator;
            var wildlife = coordinator?.SelectedWildlife;
            
            if (wildlife == null) return;
            
            int currentY = y;
            
            // Type and basic info
            DrawText($"Type: {wildlife.Type}", x, currentY, Color.White);
            currentY += LineHeight;
            
            DrawText($"Age: {wildlife.Age} days", x, currentY, Color.LightGray);
            currentY += LineHeight;
            
            // Health
            var healthColor = wildlife.Health > 70 ? Color.Green : 
                             wildlife.Health > 30 ? Color.Yellow : Color.Red;
            DrawText($"Health: {wildlife.Health}%", x, currentY, healthColor);
            currentY += LineHeight;
            
            // Hunger
            var hungerColor = wildlife.Hunger < 30 ? Color.Green :
                             wildlife.Hunger < 70 ? Color.Yellow : Color.Red;
            DrawText($"Hunger: {wildlife.Hunger}%", x, currentY, hungerColor);
            currentY += LineHeight;
            
            // Behavior
            var behaviorColor = wildlife.CurrentBehavior == WildlifeBehavior.Hunting ? Color.Red :
                               wildlife.CurrentBehavior == WildlifeBehavior.Fleeing ? Color.Yellow :
                               Color.Green;
            DrawText($"Behavior: {wildlife.CurrentBehavior}", x, currentY, behaviorColor);
            currentY += LineHeight;
            
            // Predator status
            if (wildlife.IsPredator)
            {
                DrawText("? Predator", x, currentY, Color.Orange);
            }
        }
    }
}
```

---

### **Step 3: Create BuildingInfoPanel**

**File:** `VillageBuilder.Game\Graphics\UI\Panels\BuildingInfoPanel.cs`

```csharp
using Raylib_cs;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Game.Core.Selection;

namespace VillageBuilder.Game.Graphics.UI.Panels
{
    public class BuildingInfoPanel : BasePanel
    {
        public override string Title => "Building Info";
        
        protected override void RenderContent(int x, int y, int width, int height)
        {
            var coordinator = Context?.SelectionManager as SelectionCoordinator;
            var building = coordinator?.SelectedBuilding;
            
            if (building == null) return;
            
            int currentY = y;
            
            // Building type
            DrawText($"Type: {building.Type}", x, currentY, Color.White);
            currentY += LineHeight;
            
            // Construction status
            if (!building.IsConstructed)
            {
                var stage = building.GetConstructionStage();
                var progress = building.GetConstructionProgressPercent();
                
                DrawText($"Construction: {stage}", x, currentY, Color.Yellow);
                currentY += LineHeight;
                
                DrawText($"Progress: {progress}%", x, currentY, Color.Cyan);
                currentY += LineHeight;
                
                var workerCount = building.ConstructionWorkers.Count;
                DrawText($"Workers: {workerCount}", x, currentY, Color.LightGray);
                currentY += LineHeight;
            }
            else
            {
                DrawText("Status: Operational", x, currentY, Color.Green);
                currentY += LineHeight;
                
                // Workers
                if (building.AssignedWorkers.Count > 0)
                {
                    DrawText($"Workers: {building.AssignedWorkers.Count}", x, currentY, Color.Cyan);
                    currentY += LineHeight;
                }
                
                // Production (if applicable)
                if (building.Type == BuildingType.Farm || 
                    building.Type == BuildingType.Mine ||
                    building.Type == BuildingType.Lumberyard)
                {
                    currentY += 5;
                    DrawText("Production Active", x, currentY, Color.Green);
                }
            }
        }
    }
}
```

---

### **Step 4: Create GameStatsPanel**

**File:** `VillageBuilder.Game\Graphics\UI\Panels\GameStatsPanel.cs`

```csharp
using Raylib_cs;
using VillageBuilder.Engine.Core;

namespace VillageBuilder.Game.Graphics.UI.Panels
{
    public class GameStatsPanel : BasePanel
    {
        public override string Title => "Village Stats";
        
        protected override void RenderContent(int x, int y, int width, int height)
        {
            if (Context?.Engine == null) return;
            
            var engine = Context.Engine;
            int currentY = y;
            
            // Population
            var totalPeople = engine.Families.Sum(f => f.Members.Count(p => p.IsAlive));
            DrawText($"Population: {totalPeople}", x, currentY, Color.White);
            currentY += LineHeight;
            
            // Buildings
            DrawText($"Buildings: {engine.Buildings.Count}", x, currentY, Color.LightGray);
            currentY += LineHeight;
            
            // Resources (if accessible)
            if (engine.VillageResources != null)
            {
                currentY += 5;
                DrawText("Resources:", x, currentY, Color.Yellow);
                currentY += LineHeight;
                
                foreach (var resource in engine.VillageResources.GetAll())
                {
                    DrawText($"  {resource.Key}: {resource.Value}", x, currentY, Color.Cyan);
                    currentY += LineHeight;
                }
            }
            
            // Wildlife count
            if (engine.WildlifeManager != null)
            {
                currentY += 5;
                var wildlifeCount = engine.WildlifeManager.Wildlife.Count(w => w.IsAlive);
                DrawText($"Wildlife: {wildlifeCount}", x, currentY, Color.Green);
            }
        }
    }
}
```

---

### **Step 5: Create PanelManager**

**File:** `VillageBuilder.Game\Graphics\UI\Panels\PanelManager.cs`

```csharp
using System.Collections.Generic;
using VillageBuilder.Engine.Core;
using VillageBuilder.Game.Core.Selection;

namespace VillageBuilder.Game.Graphics.UI.Panels
{
    /// <summary>
    /// Manages and orchestrates UI panels based on selection state.
    /// Phase 5: Panel coordination and rendering.
    /// </summary>
    public class PanelManager
    {
        private readonly PersonInfoPanel _personPanel;
        private readonly WildlifeInfoPanel _wildlifePanel;
        private readonly BuildingInfoPanel _buildingPanel;
        private readonly GameStatsPanel _gameStatsPanel;
        private readonly QuickStatsPanel _quickStatsPanel;
        
        public PanelManager()
        {
            _personPanel = new PersonInfoPanel();
            _wildlifePanel = new WildlifeInfoPanel();
            _buildingPanel = new BuildingInfoPanel();
            _gameStatsPanel = new GameStatsPanel();
            _quickStatsPanel = new QuickStatsPanel();
        }
        
        /// <summary>
        /// Render appropriate panel based on current selection.
        /// </summary>
        public void RenderPanels(GameEngine engine, SelectionCoordinator? selectionManager, 
                                int x, int y, int width, int height)
        {
            // Create render context
            var context = new PanelRenderContext
            {
                Engine = engine,
                SelectionManager = selectionManager
            };
            
            int currentY = y;
            int panelSpacing = 10;
            
            // Always show quick stats at top
            _quickStatsPanel.Render(context, x, currentY, width, height / 4);
            currentY += (height / 4) + panelSpacing;
            
            // Show context-specific panel based on selection
            if (selectionManager != null && selectionManager.HasSelection())
            {
                if (selectionManager.SelectedPerson != null)
                {
                    _personPanel.Render(context, x, currentY, width, height - currentY);
                }
                else if (selectionManager.SelectedWildlife != null)
                {
                    _wildlifePanel.Render(context, x, currentY, width, height - currentY);
                }
                else if (selectionManager.SelectedBuilding != null)
                {
                    _buildingPanel.Render(context, x, currentY, width, height - currentY);
                }
                else
                {
                    // Show general stats if only tile selected
                    _gameStatsPanel.Render(context, x, currentY, width, height - currentY);
                }
            }
            else
            {
                // No selection - show general game stats
                _gameStatsPanel.Render(context, x, currentY, width, height - currentY);
            }
        }
    }
}
```

---

### **Step 6: Wire into SidebarRenderer**

**File:** `VillageBuilder.Game\Graphics\UI\SidebarRenderer.cs`

**Add field:**
```csharp
private readonly PanelManager _panelManager;
```

**In constructor:**
```csharp
public SidebarRenderer()
{
    _tileInspector = new TileInspector(FontSize, SmallFontSize);
    _panelManager = new PanelManager(); // Phase 5
}
```

**Replace rendering code in Render() method:**
```csharp
// Phase 5: Use PanelManager to orchestrate all UI panels
_panelManager.RenderPanels(engine, selectionManager, 
                          _sidebarX + Padding, 
                          currentY, 
                          _sidebarWidth - (Padding * 2), 
                          _sidebarHeight - Padding);
```

---

## ?? **Quick Implementation Steps**

1. Copy each panel code above into new files
2. Add `PanelManager` class
3. Update `SidebarRenderer` to use `PanelManager`
4. Build: `dotnet build`
5. Test: `dotnet run --project VillageBuilder.Game`

---

## ? **Benefits After Completion**

1. **Modular:** Each panel is independent and testable
2. **Maintainable:** Easy to add/modify panels
3. **Reusable:** Panels can be used elsewhere
4. **Clean:** SidebarRenderer becomes thin orchestrator
5. **SOLID:** Single Responsibility achieved

---

## ?? **Before vs After**

### Before:
```csharp
// SidebarRenderer: 800+ lines
// - Person rendering inline
// - Wildlife rendering inline
// - Building rendering inline
// - Stats rendering inline
// = Hard to test, maintain, extend
```

### After:
```csharp
// SidebarRenderer: ~100 lines (orchestrator)
// + PersonInfoPanel: ~80 lines
// + WildlifeInfoPanel: ~70 lines
// + BuildingInfoPanel: ~75 lines
// + GameStatsPanel: ~60 lines
// + PanelManager: ~50 lines
// = Easy to test, maintain, extend!
```

---

## ?? **Next Action**

**When ready to implement:**
1. Create the 4 panel files
2. Create PanelManager
3. Update SidebarRenderer
4. Build and test

**Estimated time:** 2-3 hours

**Result:** 100% complete refactoring across all 5 phases! ??

---

**You're 80% done - this is the final 20%!**
