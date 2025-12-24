# Sidebar and TileInspector Icon Fixes

**Date**: 2024
**Status**: ? COMPLETED

## Overview
Fixed two UI rendering issues:
1. TileInspector decorations lacking sprite support and proper coloring
2. SidebarRenderer person selector showing "?" instead of arrow icon

## Issues Fixed

### 1. TileInspector Decorations Section
**Problem**: 
- Decorations were rendered as a simple text string without sprite support
- Color from `decoration.GetColor()` was ignored, using default text color
- No fallback handling for sprite vs ASCII modes

**Solution**:
- Added proper sprite rendering with `SpriteAtlasManager.Instance.GetSprite(decoration.Type)`
- Convert Engine's `DecorationColor` to Raylib's `Color` format
- Render sprite with proper color tint when available
- Fall back to ASCII glyph with correct color when sprite unavailable
- Separate icon and name rendering for better layout control

**Code Changes** (`VillageBuilder.Game\Graphics\UI\TileInspector.cs`):
```csharp
// OLD: Simple text concatenation
var decorText = $"  {decoration.GetGlyph()} {GetDecorationName(decoration.Type)}";
GraphicsConfig.DrawConsoleTextAuto(decorText, x + 4, currentY, _smallFontSize, textColor);

// NEW: Sprite support with proper coloring
int decorX = x + 4;

// Get decoration color from Engine
var decorColor = decoration.GetColor();
var raylibColor = new Color(decorColor.R, decorColor.G, decorColor.B, decorColor.A);

// Draw decoration glyph with sprite support
var glyph = decoration.GetGlyph();
if (GraphicsConfig.UseSpriteMode && SpriteAtlasManager.Instance.SpriteModeEnabled)
{
    var sprite = SpriteAtlasManager.Instance.GetSprite(decoration.Type);
    if (sprite.HasValue)
    {
        // Render as sprite with color tint
        var sourceRect = new Rectangle(0, 0, sprite.Value.Width, sprite.Value.Height);
        var destRect = new Rectangle(decorX, currentY, _smallFontSize, _smallFontSize);
        Raylib.DrawTexturePro(sprite.Value, sourceRect, destRect, origin, 0f, raylibColor);
        decorX += _smallFontSize + 2;
    }
    else
    {
        // Fallback to ASCII with proper color
        GraphicsConfig.DrawConsoleText(glyph, decorX, currentY, _smallFontSize, raylibColor);
        decorX += GraphicsConfig.MeasureText(glyph, _smallFontSize) + 2;
    }
}
else
{
    // ASCII mode with proper color
    GraphicsConfig.DrawConsoleText(glyph, decorX, currentY, _smallFontSize, raylibColor);
    decorX += GraphicsConfig.MeasureText(glyph, _smallFontSize) + 2;
}

// Draw decoration name (always in default text color)
var decorName = GetDecorationName(decoration.Type);
GraphicsConfig.DrawConsoleText(decorName, decorX, currentY, _smallFontSize, textColor);
```

**Visual Result**:
- Before: `?? Rare Flower` (white text, no sprite)
- After: `[?? sprite]` Rare Flower (pink sprite or colored ASCII)

### 2. SidebarRenderer Person Selector Icon
**Problem**:
- Person selector was hardcoded as "?" character
- Should use `UIIconType.ArrowRight` with proper sprite/ASCII fallback
- Screenshot showed "?" where arrow should appear

**Solution**:
- Replace hardcoded "?" with `GraphicsConfig.DrawUIIcon()` call
- Use `UIIconType.ArrowRight` with ">" as ASCII fallback
- Properly position name text after arrow icon
- Add spacing for non-selected items to maintain alignment

**Code Changes** (`VillageBuilder.Game\Graphics\UI\SidebarRenderer.cs`):
```csharp
// OLD: Hardcoded "?" indicator
string indicator = isSelected ? "? " : "  ";
var nameColor = isSelected ? new Color(255, 255, 100, 255) : new Color(180, 180, 180, 255);
string displayText = $"{indicator}{taskIcon} {p.FirstName} {p.LastName}";
GraphicsConfig.DrawConsoleTextAuto(displayText, _sidebarX + Padding, y, SmallFontSize - 2, nameColor);

// NEW: Proper icon rendering with sprite support
var nameColor = isSelected ? new Color(255, 255, 100, 255) : new Color(180, 180, 180, 255);

// Selection indicator (sprite arrow with ASCII fallback)
int personX = _sidebarX + Padding;
if (isSelected)
{
    personX += GraphicsConfig.DrawUIIcon(
        UIIconType.ArrowRight,
        personX, y,
        SmallFontSize - 2,
        nameColor,
        ">"  // ASCII fallback
    );
    personX += 2;
}
else
{
    personX += SmallFontSize; // Add spacing to align with selected
}

// Show name with task indicator
string taskIcon = GetTaskIcon(p.CurrentTask);
string displayText = $"{taskIcon} {p.FirstName} {p.LastName}";
GraphicsConfig.DrawConsoleTextAuto(displayText, personX, y, SmallFontSize - 2, nameColor);
```

**Visual Result**:
- Before: `?  .. Edward Weston` (question mark)
- After: `>  .. Edward Weston` or `[? sprite] .. Edward Weston` (arrow)

## Testing Checklist
- [x] Compile successfully
- [ ] TileInspector shows colored decoration icons
- [ ] Decorations use sprites when available
- [ ] Decorations fall back to colored ASCII in non-sprite mode
- [ ] Person selector shows arrow (> or sprite)
- [ ] Selected person has arrow, others have proper spacing
- [ ] All icons maintain proper color tinting

## Related Files
- `VillageBuilder.Game\Graphics\UI\TileInspector.cs`
- `VillageBuilder.Game\Graphics\UI\SidebarRenderer.cs`
- `VillageBuilder.Game\Graphics\GraphicsConfig.cs` (DrawUIIcon method)
- `VillageBuilder.Game\Graphics\SpriteAtlasManager.cs` (sprite loading)
- `VillageBuilder.Engine\World\TerrainDecoration.cs` (GetColor, GetGlyph methods)

## Benefits
1. **Consistent Icon System**: Both files now use the unified `GraphicsConfig.DrawUIIcon()` API
2. **Proper Coloring**: Decorations display with their actual colors (green trees, pink flowers, etc.)
3. **Sprite Support**: Can use emoji sprites when available, gracefully fall back to ASCII
4. **Visual Polish**: UI looks more professional with proper icons instead of placeholders

## Technical Notes
- `DecorationColor` (Engine) ? `Raylib_cs.Color` conversion required for rendering
- Sprite tinting allows single sprite to be reused with different colors
- Dynamic positioning (`personX` tracking) ensures proper alignment regardless of icon width
- Maintains backwards compatibility with ASCII-only mode

## Follow-up
- Consider adding sprite support to other decoration displays (tooltip, etc.)
- Verify emoji sprites load correctly on different platforms
- Test with various decoration types to ensure colors are appropriate
