# Wildlife Sprite Rendering - Diagnostic Guide

## Problem
Fox (??), Wolf (??), Bear (??), and Boar (??) are not rendering with sprites even after:
- Sprites downloaded successfully
- Code updated to map wildlife types to decoration types
- Color tinting fixed
- Game restarted

## Diagnostic Logging Added

### Changes Made to `SpriteAtlasManager.cs`:

1. **Working Directory Logging** - Shows where the game is looking for sprites
2. **Absolute Path Logging** - Confirms the full path being checked
3. **Directory Existence Check** - Verifies the sprites folder exists
4. **Per-Sprite Logging** - Shows which predator sprites load successfully
5. **Post-Load Verification** - Confirms predator sprites are in memory

### Expected Console Output on Game Start:

```
=== Emoji Sprite Loading ===
Working Directory: C:\Users\usarm\source\repos\bewl\VillageBuilder\VillageBuilder.Game\bin\Debug\net9.0
Loading emoji sprites from: Assets/sprites/emojis/
Absolute path: C:\Users\usarm\source\repos\bewl\VillageBuilder\VillageBuilder.Game\bin\Debug\net9.0\Assets\sprites\emojis
Directory exists: True

  ? Loaded FoxHunting: 1f98a.png (ID: 123, Size: 72x72)
  ? Loaded WolfPack: 1f43a.png (ID: 124, Size: 72x72)
  ? Loaded BearGrizzly: 1f43b.png (ID: 125, Size: 72x72)
  ? Loaded BoarWild: 1f417.png (ID: 126, Size: 72x72)

? Loaded 22 emoji sprites successfully!
  Sprite mode: ENABLED
  Terrain will render with colorful emoji sprites

Predator Sprite Check:
  Fox (FoxHunting): ? LOADED
  Wolf (WolfPack): ? LOADED
  Bear (BearGrizzly): ? LOADED
  Boar (BoarWild): ? LOADED
```

## Potential Issues & Solutions

### Issue 1: Wrong Working Directory
**Symptom:** Console shows wrong absolute path or "Directory exists: False"

**Solution:** 
- Game is running from wrong directory (e.g., solution root instead of bin folder)
- Sprites need to be copied to build output directory
- Add to `.csproj`:
```xml
<ItemGroup>
  <None Include="Assets\sprites\emojis\**\*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

### Issue 2: Sprites Not Loading
**Symptom:** Console shows "? MISSING FoxHunting" or failed load count > 0

**Solution:**
- Check sprite files exist in source: `VillageBuilder.Game\Assets\sprites\emojis\`
- Re-run download script: `.\VillageBuilder.Game\Assets\sprites\download_twemoji.ps1`
- Verify files aren't corrupted (should be ~1KB each)

### Issue 3: Sprites Load But Don't Render
**Symptom:** Console shows all sprites loaded, but animals still appear as text/wrong

**Possible Causes:**
1. **Sprite Mode Disabled**
   - Check console for "UseSpriteMode: false"
   - Should see "? Sprite mode enabled" message

2. **Wrong Decoration Type Returned**
   - `GetWildlifeSpriteType()` returns null or wrong type
   - Check mapping in `MapRenderer.cs` line 630-633

3. **Sprite Not Found in Atlas**
   - `SpriteAtlasManager.Instance.GetSprite()` returns null
   - Even though loading succeeded, lookup fails
   - Check if DecorationType enum values match between Engine and Game projects

4. **Rendering Issue**
   - Sprites render but are invisible/white
   - Check if `Color.White` is being used (not a tint)
   - Verify texture has valid ID and dimensions

### Issue 4: Enum Mismatch
**Symptom:** Sprites load, GetWildlifeSpriteType() returns a value, but GetSprite() returns null

**Root Cause:** The `DecorationType` enum in `VillageBuilder.Engine` might not match what's expected in `VillageBuilder.Game`

**Solution:**
1. Rebuild entire solution (not just one project)
2. Verify enum values:
```csharp
// In VillageBuilder.Engine\World\TerrainDecoration.cs
FoxHunting,      // Must exist
WolfPack,        // Must exist
BearGrizzly,     // Must exist
BoarWild         // Must exist
```

## Debugging Steps

### Step 1: Check Console Output
Run the game and check for the diagnostic messages above.

### Step 2: Verify File Existence
```powershell
cd VillageBuilder.Game\Assets\sprites\emojis
ls | Where-Object { $_.Name -in @("1f98a.png","1f43a.png","1f43b.png","1f417.png") }
```

Should show 4 files, each ~1KB.

### Step 3: Check Build Output
```powershell
cd VillageBuilder.Game\bin\Debug\net9.0\Assets\sprites\emojis
ls 1f98a.png, 1f43a.png, 1f43b.png, 1f417.png
```

If files don't exist here, sprites aren't being copied to output directory.

### Step 4: Verify Enum
```powershell
Select-String -Path "VillageBuilder.Engine\World\TerrainDecoration.cs" -Pattern "FoxHunting|WolfPack|BearGrizzly|BoarWild"
```

Should find all 4 entries in the DecorationType enum.

### Step 5: Test One Animal
Add temporary debug logging to `MapRenderer.cs` in `RenderWildlife()`:

```csharp
if (wildlife.Type == WildlifeType.Fox)
{
    Console.WriteLine($"Rendering Fox at ({wildlife.Position.X},{wildlife.Position.Y})");
    Console.WriteLine($"  SpriteType: {spriteType}");
    Console.WriteLine($"  UseSpriteMode: {GraphicsConfig.UseSpriteMode}");
    Console.WriteLine($"  Sprite exists: {sprite.HasValue}");
    if (sprite.HasValue)
    {
        Console.WriteLine($"  Sprite ID: {sprite.Value.Id}, Size: {sprite.Value.Width}x{sprite.Value.Height}");
    }
}
```

## Quick Fix Checklist

- [ ] Sprites downloaded (22 files including 4 predators)
- [ ] Sprites exist in `VillageBuilder.Game\Assets\sprites\emojis\`
- [ ] DecorationType enum has FoxHunting, WolfPack, BearGrizzly, BoarWild
- [ ] GetWildlifeSpriteType() maps Fox?FoxHunting, Wolf?WolfPack, etc.
- [ ] Full solution rebuild completed
- [ ] Game restarted (not just hot reload)
- [ ] Console shows "Predator Sprite Check: ? LOADED" for all 4
- [ ] UseSpriteMode is true
- [ ] Sprites render with Color.White (no tint)

## Next Steps

**Run the game now** and check the console output. Copy the console messages here and we can diagnose exactly what's happening.

The new logging will tell us:
1. Are sprites loading at all?
2. Are the predator sprites specifically loading?
3. What's the working directory?
4. Are the files being found?

Once we see the console output, we'll know exactly what's wrong.
