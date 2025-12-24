# Phase 4 Integration - Remaining Changes

## Files to Update

### MapRenderer.cs - Line 383
**Find:**
```csharp
private void RenderPeople(GameEngine engine, int tileSize, int minX, int maxX, int minY, int maxY, VillageBuilder.Game.Core.SelectionManager? selectionManager)
```

**Replace with:**
```csharp
private void RenderPeople(GameEngine engine, int tileSize, int minX, int maxX, int minY, int maxY, VillageBuilder.Game.Core.Selection.SelectionCoordinator? selectionManager)
```

### MapRenderer.cs - Line 463
**Find:**
```csharp
private void RenderWildlife(GameEngine engine, int tileSize, int minX, int maxX, int minY, int maxY, VillageBuilder.Game.Core.SelectionManager? selectionManager)
```

**Replace with:**
```csharp
private void RenderWildlife(GameEngine engine, int tileSize, int minX, int maxX, int minY, int maxY, VillageBuilder.Game.Core.Selection.SelectionCoordinator? selectionManager)
```

### SidebarRenderer.cs - Line 419
**Find:**
```csharp
private int RenderPersonInfo(GameEngine engine, VillageBuilder.Engine.Entities.Person person, int startY, VillageBuilder.Game.Core.SelectionManager? selectionManager = null)
```

**Replace with:**
```csharp
private int RenderPersonInfo(GameEngine engine, VillageBuilder.Engine.Entities.Person person, int startY, VillageBuilder.Game.Core.Selection.SelectionCoordinator? selectionManager = null)
```

### SidebarRenderer.cs - Line 551
**Find:**
```csharp
private int RenderWildlifeInfo(GameEngine engine, WildlifeEntity wildlife, int y, VillageBuilder.Game.Core.SelectionManager? selectionManager)
```

**Replace with:**
```csharp
private int RenderWildlifeInfo(GameEngine engine, WildlifeEntity wildlife, int y, VillageBuilder.Game.Core.Selection.SelectionCoordinator? selectionManager)
```

## Manual Steps

1. Open `MapRenderer.cs`
2. Find-Replace: `VillageBuilder.Game.Core.SelectionManager?` ? `VillageBuilder.Game.Core.Selection.SelectionCoordinator?`
3. Open `SidebarRenderer.cs`  
4. Find-Replace: `VillageBuilder.Game.Core.SelectionManager?` ? `VillageBuilder.Game.Core.Selection.SelectionCoordinator?`
5. Save both files
6. Build solution

## Alternative: Use IDE Find/Replace

Press Ctrl+H in Visual Studio:
- Find: `VillageBuilder.Game.Core.SelectionManager?`
- Replace: `VillageBuilder.Game.Core.Selection.SelectionCoordinator?`
- Scope: Current Project or Entire Solution
- Click Replace All

This should fix all 4 method signatures at once!
