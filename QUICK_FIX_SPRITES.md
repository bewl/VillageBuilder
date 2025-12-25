# Quick Manual Fix for Building Sprites

## Issue 1: Auto-download removed from game ?
Already fixed in Program.cs - auto-download call removed.

## Issue 2: Add B key to sidebar commands

**File:** `VillageBuilder.Game\Graphics\UI\SidebarRenderer.cs`

**Line ~266-268:** Replace these lines:

```csharp
("V", "Toggle Heat Map", new Color(100, 200, 255, 255)),
("", "", Color.White), // Spacer
("Q", "Quit Game", new Color(255, 100, 100, 255))
```

**With:**

```csharp
("V", "Toggle Heat Map", new Color(100, 200, 255, 255)),
("B", "Building Graphics", new Color(150, 200, 255, 255)),
("", "", Color.White), // Spacer
("Q", "Quit Game", new Color(255, 100, 100, 255))
```

## Manual Sprite Download Instructions

Since you don't want auto-download in the game, here's how to get the sprites manually:

### Option 1: Run the PowerShell script directly

```powershell
cd C:\Users\usarm\source\repos\bewl\VillageBuilder
.\Assets\sprites\download_building_sprites.ps1
```

### Option 2: Copy sprite files manually

The game already has these building sprite files that need to be downloaded:

**Building Icons (9 files):**
- 1f3e0.png (?? House)
- 1f33e.png (?? Farm) 
- 1f3e6.png (?? Warehouse)
- 1f3ed.png (?? Workshop)
- 26cf.png (?? Mine)
- 1fab5.png (?? Lumberyard)
- 1f3ea.png (?? Market)
- 1f6b0.png (?? Well)
- 1f3db.png (??? Town Hall)

**Component Sprites (14 files):**
- 1f9f1.png (?? Brick)
- 1faa8.png (?? Stone)
- 1f7eb.png (?? Brown square)
- 2b1c.png (? White square)
- 1f6aa.png (?? Door)
- 1f6b6.png (?? Person)
- 1f512.png (?? Lock)
- 1fa9f.png (?? Window)
- 1f319.png (?? Moon)
- 1f525.png (?? Fire)
- 1f53a.png (?? Triangle)
- 1f7e9.png (?? Green square)
- 1f6a7.png (?? Construction)
- 1f4cb.png (?? Clipboard)

**Source:** https://github.com/twitter/twemoji/tree/master/assets/72x72

**Target Directory:** `Assets/sprites/emojis/`

## Once sprites are in place:

The game will automatically:
1. Load all building sprites on startup
2. Use them in IconSprite mode (default)
3. Press **B** to cycle through: ASCII ? Icon ? Detailed ? ASCII
4. See current mode in top-right status bar: `[?] ICON`

## Building Render Modes:

| Key | Mode | Sprites Used | Description |
|-----|------|--------------|-------------|
| B (cycle) | ASCII | 0 | Retro text graphics |
| B (cycle) | IconSprite | 9 | Single emoji per building ? |
| B (cycle) | Detailed | 23 | Per-tile component sprites |
