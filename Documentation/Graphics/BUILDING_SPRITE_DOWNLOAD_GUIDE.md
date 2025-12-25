# ?? Building Sprite Download Guide

## Overview
This guide helps you download the emoji sprites needed for the detailed building rendering system.

---

## ?? **Recommended Sprite Sources**

### **Option 1: Twemoji (Twitter Emoji) - RECOMMENDED** ?
**Source:** https://github.com/twitter/twemoji  
**License:** CC-BY 4.0 (Free for commercial use with attribution)  
**Resolution:** 72x72 PNG files  
**Quality:** ?????

#### Download Steps:
1. Visit: https://github.com/twitter/twemoji
2. Navigate to `assets/72x72/` folder
3. Download the PNG files you need (see list below)
4. Place them in: `Assets/sprites/emojis/`

#### Quick Download Script (PowerShell):
```powershell
# Save as: download_building_sprites.ps1

$baseUrl = "https://raw.githubusercontent.com/twitter/twemoji/master/assets/72x72"
$outputDir = "Assets/sprites/emojis"

# Create directory if it doesn't exist
New-Item -ItemType Directory -Force -Path $outputDir

# Building icons
$sprites = @{
    # Full building icons
    "1f3e0.png" = "?? House"
    "1f33e.png" = "?? Farm"
    "1f3e6.png" = "?? Warehouse"
    "1f3ed.png" = "?? Workshop"
    "26cf.png"  = "?? Mine"
    "1fab5.png" = "?? Lumberyard"
    "1f3ea.png" = "?? Market"
    "1f6b0.png" = "?? Well"
    "1f3db.png" = "??? Town Hall"
    
    # Component sprites
    "1f9f1.png" = "?? Brick"
    "1faa8.png" = "?? Stone"
    "1f7eb.png" = "?? Brown square (carpet/dirt)"
    "2b1c.png"  = "? White square (plaster)"
    "1f6aa.png" = "?? Door"
    "1f6b6.png" = "?? Person (open door)"
    "1f512.png" = "?? Lock"
    "1fa9f.png" = "?? Window"
    "1f319.png" = "?? Moon (night window)"
    "1f525.png" = "?? Fire"
    "1f53a.png" = "?? Triangle (roof)"
    "1f7e9.png" = "?? Green square (shingles)"
    "1f6a7.png" = "?? Construction"
    "1f4cb.png" = "?? Clipboard (sign)"
}

foreach ($file in $sprites.Keys) {
    $url = "$baseUrl/$file"
    $output = Join-Path $outputDir $file
    $description = $sprites[$file]
    
    Write-Host "Downloading $description..." -ForegroundColor Cyan
    try {
        Invoke-WebRequest -Uri $url -OutFile $output -ErrorAction Stop
        Write-Host "  ? Downloaded: $file" -ForegroundColor Green
    }
    catch {
        Write-Host "  ? Failed: $file - $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n? Download complete! Sprites saved to: $outputDir" -ForegroundColor Green
```

---

### **Option 2: Noto Emoji (Google)**
**Source:** https://github.com/googlefonts/noto-emoji  
**License:** Apache 2.0 (Free)  
**Resolution:** Multiple sizes available  
**Quality:** ?????

#### Download Steps:
1. Visit: https://github.com/googlefonts/noto-emoji
2. Navigate to `png/` folder
3. Choose resolution (128, 512 recommended)
4. Download needed emoji files

---

### **Option 3: OpenMoji**
**Source:** https://openmoji.org/  
**License:** CC-BY-SA 4.0 (Free with attribution)  
**Resolution:** Scalable SVG + PNG  
**Quality:** ????

#### Download Steps:
1. Visit: https://openmoji.org/library/
2. Search for each emoji by keyword
3. Download as PNG (72x72 or 618x618)
4. Rename to Unicode codepoint (e.g., `1f3e0.png`)

---

## ?? **Required Sprite List**

### **Full Building Icons (9 sprites)**
| Emoji | Unicode | File Name | Building Type |
|-------|---------|-----------|---------------|
| ?? | U+1F3E0 | 1f3e0.png | House |
| ?? | U+1F33E | 1f33e.png | Farm |
| ?? | U+1F3E6 | 1f3e6.png | Warehouse |
| ?? | U+1F3ED | 1f3ed.png | Workshop |
| ?? | U+26CF | 26cf.png | Mine |
| ?? | U+1FAB5 | 1fab5.png | Lumberyard |
| ?? | U+1F3EA | 1f3ea.png | Market |
| ?? | U+1F6B0 | 1f6b0.png | Well |
| ??? | U+1F3DB | 1f3db.png | Town Hall |

### **Component Sprites (26 sprites - Optional for Detailed Mode)**

#### **Floors (4 sprites)**
| Emoji | Unicode | File Name | Component |
|-------|---------|-----------|-----------|
| ?? | U+1FAB5 | 1fab5.png | Wood floor |
| ?? | U+1FAA8 | 1faa8.png | Stone floor |
| ?? | U+1F7EB | 1f7eb.png | Carpet |
| ?? | U+1F7EB | 1f7eb.png | Dirt floor |

#### **Walls (4 sprites)**
| Emoji | Unicode | File Name | Component |
|-------|---------|-----------|-----------|
| ?? | U+1F9F1 | 1f9f1.png | Brick wall |
| ?? | U+1FAA8 | 1faa8.png | Stone wall |
| ?? | U+1FAB5 | 1fab5.png | Wood wall |
| ? | U+2B1C | 2b1c.png | Plaster wall |

#### **Doors (3 sprites)**
| Emoji | Unicode | File Name | Component |
|-------|---------|-----------|-----------|
| ?? | U+1F6AA | 1f6aa.png | Closed door |
| ?? | U+1F6B6 | 1f6b6.png | Open door |
| ?? | U+1F512 | 1f512.png | Locked door |

#### **Windows (3 sprites)**
| Emoji | Unicode | File Name | Component |
|-------|---------|-----------|-----------|
| ?? | U+1FA9F | 1fa9f.png | Window (day) |
| ?? | U+1F319 | 1f319.png | Window (night) |
| ?? | U+1F525 | 1f525.png | Broken window |

#### **Roofs (4 sprites)**
| Emoji | Unicode | File Name | Component |
|-------|---------|-----------|-----------|
| ?? | U+1F53A | 1f53a.png | Tile roof |
| ?? | U+1F33E | 1f33e.png | Thatch roof |
| ?? | U+1F7E9 | 1f7e9.png | Shingle roof |
| ?? | U+1FAB5 | 1fab5.png | Wood roof |

#### **Decorations (5 sprites)**
| Emoji | Unicode | File Name | Component |
|-------|---------|-----------|-----------|
| ?? | U+1F525 | 1f525.png | Chimney |
| ?? | U+1F6A7 | 1f6a7.png | Fence |
| ?? | U+1F4CB | 1f4cb.png | Sign |
| ?? | U+1FAA8 | 1faa8.png | Stone foundation |
| ?? | U+1FAB5 | 1fab5.png | Wood foundation |

---

## ?? **Usage**

### **Directory Structure:**
```
Assets/
??? sprites/
?   ??? emojis/          ? Place all .png files here
?   ?   ??? 1f3e0.png    (House)
?   ?   ??? 1f33e.png    (Farm)
?   ?   ??? 1f9f1.png    (Brick)
?   ?   ??? ...
?   ??? ui_icons/        (Existing UI icons)
```

### **Rendering Modes:**

#### **1. ASCII Mode** (No sprites needed)
```csharp
GraphicsConfig.BuildingDetail = GraphicsConfig.BuildingRenderMode.ASCII;
```
Traditional character grid rendering.

#### **2. Icon Sprite Mode** (9 sprites recommended) ?
```csharp
GraphicsConfig.BuildingDetail = GraphicsConfig.BuildingRenderMode.IconSprite;
```
Single centered emoji per building. **DEFAULT MODE**.

#### **3. Detailed Sprite Mode** (35 sprites recommended)
```csharp
GraphicsConfig.BuildingDetail = GraphicsConfig.BuildingRenderMode.DetailedSprite;
```
Per-tile component sprites with variants.

---

## ?? **Testing**

After downloading sprites:

1. **Verify sprites loaded:**
```
Console output on startup:
"? Loaded 35 emoji sprites successfully!"
"   Sprite mode: ENABLED"
```

2. **Toggle modes in-game:**
```csharp
// In GameRenderer or input handler
if (Raylib.IsKeyPressed(KeyboardKey.B))
{
    // Cycle through modes
    GraphicsConfig.BuildingDetail = GraphicsConfig.BuildingDetail switch
    {
        BuildingRenderMode.ASCII => BuildingRenderMode.IconSprite,
        BuildingRenderMode.IconSprite => BuildingRenderMode.DetailedSprite,
        BuildingRenderMode.DetailedSprite => BuildingRenderMode.ASCII,
        _ => BuildingRenderMode.IconSprite
    };
}
```

3. **Check fallback:**
If sprites don't load, buildings automatically render in ASCII mode.

---

## ?? **Attribution**

If using Twemoji graphics, include in your game credits:
```
Building sprites: Twemoji by Twitter
Licensed under CC-BY 4.0
https://github.com/twitter/twemoji
```

If using Noto Emoji:
```
Building sprites: Noto Emoji by Google
Licensed under Apache License 2.0
https://github.com/googlefonts/noto-emoji
```

---

## ?? **Quick Start Checklist**

- [ ] Run `download_building_sprites.ps1` script
- [ ] Verify `Assets/sprites/emojis/` has PNG files
- [ ] Start game and check console for "Sprite mode: ENABLED"
- [ ] Build a house (H key) and see ?? emoji
- [ ] Press `B` to cycle rendering modes
- [ ] Enjoy beautiful sprite buildings! ??

---

## ?? **Troubleshooting**

### **Problem:** Sprites not loading
**Solution:**
1. Check file paths match exactly: `Assets/sprites/emojis/1f3e0.png`
2. Verify PNG format (not JPG or SVG)
3. Check file permissions (readable)
4. Review console output for error messages

### **Problem:** Some sprites missing
**Solution:**
- Game will use ASCII fallback for missing sprites
- Download missing files individually
- Check unicode filename matches (lowercase, no U+)

### **Problem:** Sprites look blurry
**Solution:**
- Use 72x72 PNG minimum resolution
- Higher resolutions (128x128, 256x256) look better
- Avoid scaling down from very large files

---

## ?? **Advanced: Custom Sprites**

Want custom building art? Create your own!

### **Requirements:**
- PNG format
- Square aspect ratio (72x72, 128x128, 256x256)
- Transparent background
- Named with Unicode codepoint

### **Example:**
```
Custom house: 1f3e0.png
- 128x128 pixels
- Transparent background
- Hand-drawn or generated
```

Place in `Assets/sprites/emojis/` and the game will use it!

---

**Enjoy your beautiful sprite buildings!** ????
