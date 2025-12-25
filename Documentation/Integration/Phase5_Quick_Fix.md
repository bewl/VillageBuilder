# Phase 5 Quick Fix Guide

## ? Current Issue
The 4 new panels don't match BasePanel's abstract method signatures.

## ? Required Signature
```csharp
public override bool CanRender(PanelRenderContext context)
public override int Render(PanelRenderContext context)
```

## ?? Fix for Each Panel

### Pattern to Follow (from QuickStatsPanel):
```csharp
public class ExamplePanel : BasePanel
{
    public override bool CanRender(PanelRenderContext context)
    {
        // Return true if this panel should render
        return context.SelectionManager?.SelectedPerson != null;
    }
    
    public override int Render(PanelRenderContext context)
    {
        int y = context.StartY;
        int x = context.StartX;
        
        DrawSectionHeader("TITLE", x, y, context.FontSize);
        y += context.LineHeight + 5;
        
        // Draw content using context.FontSize, context.LineHeight
        GraphicsConfig.DrawConsoleText("Text", x, y, context.SmallFontSize, Color.White);
        y += context.LineHeight;
        
        return y - context.StartY; // Return height used
    }
}
```

### Quick Fixes:

**Person InfoPanel:**
1. Change `CanRender` to check: `context.SelectionManager?.SelectedPerson != null`
2. Change `Render` to use `context.StartX`, `context.StartY`, return height
3. Replace all `DrawText` with `GraphicsConfig.DrawConsoleText`
4. Use `context.FontSize` and `context.LineHeight`

**WildlifeInfoPanel:**
1. Same pattern, check for `SelectedWildlife`

**BuildingInfoPanel:**
1. Same pattern, check for `SelectedBuilding`
2. Fix `AssignedWorkers` - use proper Building property

**GameStatsPanel:**
1. `CanRender` return true always (default stats)
2. Follow same pattern

**PanelManager:**
1. Fix `SelectionManager` type in PanelRenderContext
2. Fix `Render` calls to use single parameter: `panel.Render(context)`
3. Update context creation with StartX, StartY, FontSize, LineHeight

### PanelRenderContext Fix:
Add to RenderContext or create properly:
```csharp
public class PanelRenderContext
{
    public GameEngine Engine { get; set; }
    public SelectionCoordinator? SelectionManager { get; set; }  // Fix type
    public int StartX { get; set; }
    public int StartY { get; set; }
    public int FontSize => GraphicsConfig.SmallConsoleFontSize;
    public int SmallFontSize => (int)(FontSize * 0.9f);
    public int LineHeight => FontSize + 4;
}
```

## ? Fastest Fix
Copy QuickStatsPanel.cs and modify for each panel type - it has the correct pattern!

**Estimated Time:** 30-45 minutes to fix all panels

**Result:** Phase 5 100% complete!
