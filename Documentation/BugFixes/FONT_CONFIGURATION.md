# Font Configuration Guide

## Overview

VillageBuilder uses monospace console fonts with comprehensive ASCII/Unicode support for rendering text, box-drawing characters, shaded blocks, and special symbols (smoke effects, task icons, etc.).

---

## Recommended Fonts

### ? **Best Choice: Cascadia Code / Cascadia Mono**

**Why it's best:**
- **Comprehensive Unicode support** - All box-drawing, shaded blocks, arrows, bullets
- **Modern and clean** - Designed by Microsoft for terminals and code
- **Excellent readability** - Optimized for on-screen reading
- **Free and open-source**

**Supports:**
- ? All smoke characters: `????·??`
- ? All box-drawing: `????????????`
- ? All task icons: `???????`
- ? Shaded blocks: `????`
- ? Emojis (if needed)

**How to get it:**
1. **Windows 11**: Already installed (included with Windows Terminal)
2. **Windows 10**: Download from [GitHub Releases](https://github.com/microsoft/cascadia-code/releases)
3. **Install**: Download `.ttf` files, right-click ? Install

---

### ? **Excellent Alternative: DejaVu Sans Mono**

**Why it's excellent:**
- **Complete Unicode coverage** - One of the most comprehensive free fonts
- **Open-source** - Used widely in Linux distributions
- **Highly compatible** - Works on all platforms

**Supports:**
- ? All ASCII/Unicode characters
- ? All smoke and particle effects
- ? All box-drawing and symbols

**How to get it:**
1. Download from [DejaVu Fonts](https://dejavu-fonts.github.io/)
2. Extract and install `DejaVuSansMono.ttf`

---

### ?? **Good Alternatives**

#### **JetBrains Mono**
- Modern, designed for developers
- Great Unicode support
- Download: [JetBrains Mono](https://www.jetbrains.com/lp/mono/)

#### **IBM Plex Mono**
- Professional, clean design
- Good Unicode coverage
- Download: [IBM Plex](https://github.com/IBM/plex)

---

## Current Font Priority (in GraphicsConfig.cs)

The game tries fonts in this order:

```csharp
1. Cascadia Mono (C:\Windows\Fonts\CascadiaMono.ttf)         ? BEST
2. Cascadia Code (C:\Windows\Fonts\CascadiaCode.ttf)
3. DejaVu Sans Mono (C:\Windows\Fonts\DejaVuSansMono.ttf)   ? EXCELLENT
4. JetBrains Mono (C:\Windows\Fonts\JetBrainsMono-Regular.ttf)
5. IBM Plex Mono (C:\Windows\Fonts\IBMPlexMono-Regular.ttf)
6. Consolas (C:\Windows\Fonts\consola.ttf)                   ?? Limited Unicode
7. Lucida Console (C:\Windows\Fonts\lucon.ttf)               ?? Limited Unicode
8. DOS VGA fonts (assets/fonts/Px437_IBM_VGA_8x16.ttf)       ?? Retro style
```

**Note:** If no font is found, the game falls back to Raylib's default font.

---

## Installing Fonts (Windows)

### Method 1: System-Wide Installation (Recommended)

1. Download the font `.ttf` file(s)
2. Right-click the `.ttf` file
3. Select **"Install for all users"**
4. Font is installed to `C:\Windows\Fonts\`

### Method 2: User Installation

1. Download the font `.ttf` file(s)
2. Right-click the `.ttf` file
3. Select **"Install"**
4. Font is installed to `%LOCALAPPDATA%\Microsoft\Windows\Fonts\`

### Method 3: Bundled Fonts (Alternative)

Place font files in your project:
```
VillageBuilder.Game/
??? assets/
    ??? fonts/
        ??? CascadiaMono.ttf
        ??? CascadiaCode.ttf
        ??? DejaVuSansMono.ttf
        ??? JetBrainsMono.ttf
        ??? IBMPlexMono.ttf
```

**Note:** Remember to set these files to **"Copy to Output Directory"** in Visual Studio.

---

## Character Support Verification

### Critical Characters for VillageBuilder

**Box-Drawing:**
```
?????  ?????
? ? ?  ? ? ?
?????  ?????
? ? ?  ? ? ?
?????  ?????
```

**Shaded Blocks (for smoke):**
```
? Light shade
? Medium shade
? Dark shade
? Full block
```

**Smoke Characters:**
```
? Bullet operator
· Middle dot
? Dot above
? White circle
? White bullet
```

**Task Icons:**
```
? Hammer and pick (working)
?? Hammer (constructing)
?? House (going home)
? Arrow (going to work)
? Left-right arrow (traveling)
?? Sleeping
```

---

## Testing Your Font

### In-Game Test
1. Run the game
2. Check console output for font loading message:
   ```
   ? Font loaded: CascadiaMono.ttf
     Font size: 20px
     Texture filter: Point (nearest neighbor) - CRISP MODE
   ```
3. Look for smoke effects from chimneys at night
4. Check person selection UI for task icons
5. Verify box-drawing characters in UI borders

### Missing Character Detection

If you see **square boxes (?)** or **question marks (?)** instead of characters:
- ? Your current font doesn't support those characters
- ? Install Cascadia Mono or DejaVu Sans Mono

---

## Font Configuration in Code

**Location:** `VillageBuilder.Game/Graphics/GraphicsConfig.cs`

### Key Settings

```csharp
// Font size (adjustable)
public const int ConsoleFontSize = 20;
public const int SmallConsoleFontSize = 18;

// Character set loaded
int[] codepoints = new int[512]; // Extended to include all needed characters
```

### Loaded Character Ranges

1. **Basic ASCII** (0-127) - All standard characters
2. **Extended ASCII** (128-255) - Code Page 437 characters
3. **Box-Drawing** (0x2500-0x256C) - All box styles
4. **Block Elements** (0x2580-0x2593) - Shaded blocks
5. **Geometric Shapes** (0x25A0-0x25CF) - Circles, squares, triangles
6. **Symbols** (0x2190-0x2713) - Arrows, checkmarks, bullets
7. **Emojis** (0x1F3E0-0x1F4A4) - Task icons (if supported)

### Adding More Characters

To add custom characters, edit `LoadFont()`:

```csharp
// Add after line 161
codepoints[idx++] = 0xXXXX; // Your Unicode character
```

Find Unicode codes: [Unicode Table](https://unicode-table.com/)

---

## Troubleshooting

### Issue: "No font with Unicode support found"

**Solution:**
1. Install Cascadia Mono or DejaVu Sans Mono
2. Verify installation: Check `C:\Windows\Fonts\` for `CascadiaMono.ttf`
3. Restart the game

### Issue: Smoke characters appear as squares

**Symptoms:** Smoke shows as `???` instead of `???`

**Solution:**
1. Current font (probably Consolas) has limited Unicode
2. Install Cascadia Mono (has all block shading characters)
3. Verify in-game: Smoke should show as organic shaded characters

### Issue: Emoji task icons don't appear

**Symptoms:** Task icons show as boxes or missing

**Solution:**
1. Not all monospace fonts support emojis
2. Cascadia Code supports color emojis on Windows 11
3. Fallback: Use ASCII alternatives in `GetTaskIcon()` method:
   ```csharp
   PersonTask.Sleeping => "Z", // Instead of ??
   PersonTask.GoingHome => "H", // Instead of ??
   ```

### Issue: Font looks blurry

**Cause:** Texture filter is not set to Point (nearest neighbor)

**Solution:** This is already handled in code:
```csharp
Raylib.SetTextureFilter(ConsoleFont.Texture, TextureFilter.Point);
```

If still blurry, check font size vs screen resolution.

---

## Font Performance

### Current Performance
- **Font loading**: Once at startup (~10-50ms)
- **Text rendering**: Highly optimized by Raylib
- **Character cache**: All glyphs pre-rendered to texture atlas

### Optimization Notes
- Using `TextureFilter.Point` for crisp pixel-perfect rendering
- Font atlas contains only characters we use (512 codepoints)
- No runtime font loading or dynamic character generation

**Performance Impact:** Negligible (<0.1% frame time)

---

## Quick Setup Guide

### For Best Results (5 minutes)

1. **Download Cascadia Mono:**
   - Visit: https://github.com/microsoft/cascadia-code/releases
   - Download latest `CascadiaCode-*.zip`
   - Extract and find `CascadiaMono.ttf`

2. **Install the font:**
   - Right-click `CascadiaMono.ttf`
   - Select "Install for all users"

3. **Run the game:**
   - Should automatically detect and load Cascadia Mono
   - Check console for: `? Font loaded: CascadiaMono.ttf`

4. **Verify:**
   - Smoke effects should be organic and wispy (`????·`)
   - Task icons should appear correctly (`?????`)
   - Box-drawing should be crisp (`??????`)

---

## Advanced: Bundling Fonts with Game

To distribute the game with fonts included:

### 1. Create Assets Folder Structure
```
VillageBuilder.Game/
??? assets/
    ??? fonts/
        ??? CascadiaMono.ttf  (copy here)
```

### 2. Set File Properties
- In Visual Studio, right-click `CascadiaMono.ttf`
- Set **"Copy to Output Directory"** ? "Copy if newer"

### 3. Update Build Configuration
Font will be copied to output directory automatically:
```
bin/Debug/net9.0/assets/fonts/CascadiaMono.ttf
```

### 4. Distribution
When distributing your game, include the `assets/fonts/` folder.

**License Note:** Check font license before distribution:
- ? Cascadia Code: SIL Open Font License (free to distribute)
- ? DejaVu: Free to distribute
- ? IBM Plex: SIL Open Font License
- ? JetBrains Mono: SIL Open Font License

---

## Related Documentation

- [VISUAL_ENHANCEMENTS.md](./VISUAL_ENHANCEMENTS.md) - ASCII smoke and particle effects
- [UI_INTEGRATION_GUIDELINES.md](./UI_INTEGRATION_GUIDELINES.md) - UI rendering guidelines

---

## Changelog

### 2024-01-XX - Font Configuration Overhaul
- ? Prioritized Cascadia Mono/Code for best Unicode support
- ? Added DejaVu Sans Mono as excellent alternative
- ? Expanded codepoint list to include smoke characters (?·??)
- ? Added arrow symbols for task icons (??????)
- ? Improved font fallback order
- ? Comprehensive documentation

---

**Maintained By:** VillageBuilder Development Team  
**Last Updated:** 2024-01-XX
