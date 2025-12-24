# Phase 4 Integration - Exact Changes Needed

## MapRenderer.cs Changes

### Change 1 - Line 383
**Find this line:**
```csharp
private void RenderPeople(GameEngine engine, int tileSize, int minX, int maxX, int minY, int maxY, VillageBuilder.Game.Core.SelectionManager? selectionManager)
```

**Replace with:**
```csharp
private void RenderPeople(GameEngine engine, int tileSize, int minX, int maxX, int minY, int maxY, VillageBuilder.Game.Core.Selection.SelectionCoordinator? selectionManager)
```

---

### Change 2 - Line 463
**Find this line:**
```csharp
private void RenderWildlife(GameEngine engine, int tileSize, int minX, int maxX, int minY, int maxY, VillageBuilder.Game.Core.SelectionManager? selectionManager)
```

**Replace with:**
```csharp
private void RenderWildlife(GameEngine engine, int tileSize, int minX, int maxX, int minY, int maxY, VillageBuilder.Game.Core.Selection.SelectionCoordinator? selectionManager)
```

---

## SidebarRenderer.cs Changes

You also need to fix SidebarRenderer.cs. Use Find & Replace in that file:

**Find:** `VillageBuilder.Game.Core.SelectionManager?`
**Replace:** `VillageBuilder.Game.Core.Selection.SelectionCoordinator?`

This should find 2 occurrences in method signatures.

---

## Quick Fix Method

**In Visual Studio:**
1. Open Solution Explorer
2. Right-click on `VillageBuilder.Game` project
3. Click "Find and Replace" ? "Replace in Files"
4. Find what: `VillageBuilder.Game.Core.SelectionManager?`
5. Replace with: `VillageBuilder.Game.Core.Selection.SelectionCoordinator?`
6. Look in: `VillageBuilder.Game` project
7. Click "Replace All"

This should fix all 4 occurrences (2 in MapRenderer, 2 in SidebarRenderer).

---

After making these changes, **save all files** and **rebuild**.
