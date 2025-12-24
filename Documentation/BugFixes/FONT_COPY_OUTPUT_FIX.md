# Font Loading Fix - Assets Not Copied to Output Directory

## Problem

**Symptom:** Game console shows "? File not found" for `assets/fonts/NotoColorEmoji.ttf` even though the font exists in the source directory.

**Root Cause:** The font files were downloaded to `VillageBuilder.Game/assets/fonts/` but were **not being copied to the output directory** (`bin/Debug/net9.0/`).

When the game runs, the working directory is `bin/Debug/net9.0/`, so relative paths like `assets/fonts/NotoColorEmoji.ttf` don't resolve correctly.

---

## Solution

Updated `VillageBuilder.Game.csproj` to include **all font files** with `CopyToOutputDirectory` set to `Always`.

### Changes Made

**Modified:** `VillageBuilder.Game/VillageBuilder.Game.csproj`

Added copy configuration for new fonts:

```xml
<ItemGroup>
  <None Update="Assets\Fonts\JetBrainsMono-Regular.ttf">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </None>
  <None Update="Assets\Fonts\NotoColorEmoji.ttf">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </None>
  <None Update="Assets\Fonts\CascadiaCode.ttf">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </None>
  <None Update="Assets\Fonts\CascadiaMono.ttf">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </None>
  <None Update="Assets\Fonts\DejaVuSansMono.ttf">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

---

## Verification

After building, all fonts are now present in the output directory:

```
VillageBuilder.Game/bin/Debug/net9.0/assets/fonts/
??? JetBrainsMono-Regular.ttf   (267 KB)   ?
??? NotoColorEmoji.ttf          (10.4 MB)  ?
??? CascadiaCode.ttf            (719 KB)   ?
??? CascadiaMono.ttf            (403 KB)   ?
??? DejaVuSansMono.ttf          (327 KB)   ?
```

---

## Expected Console Output on Next Run

```
=== Font Loading Debug ===
Trying 13 font paths in priority order...
  Checking: assets/fonts/JetBrainsMono-Regular.ttf
    ? File exists! Attempting to load...
? Font loaded successfully: JetBrainsMono-Regular.ttf
  Full path: assets/fonts/JetBrainsMono-Regular.ttf
  Font size: 23px
  Texture ID: 3
  Texture filter: Point (nearest neighbor) - CRISP MODE
==========================

=== Emoji Font Loading ===
  Checking: assets/fonts/NotoColorEmoji.ttf
    ? File exists! Attempting to load...
? Emoji font loaded successfully: NotoColorEmoji.ttf
  Texture ID: 4
  ? Terrain decorations will use emoji glyphs!
==========================
```

---

## How It Works

### Build Process

1. **Source files:** Fonts stored in `VillageBuilder.Game/assets/fonts/`
2. **Build step:** MSBuild copies files to output directory
3. **Output files:** Fonts available at `bin/Debug/net9.0/assets/fonts/`
4. **Runtime:** Game loads from relative path `assets/fonts/...`

### Directory Structure

```
VillageBuilder.Game/
??? assets/fonts/              ? Source (version controlled)
?   ??? JetBrainsMono-Regular.ttf
?   ??? NotoColorEmoji.ttf
?   ??? ...
??? bin/Debug/net9.0/
    ??? VillageBuilder.Game.exe
    ??? assets/fonts/          ? Copied on build
        ??? JetBrainsMono-Regular.ttf
        ??? NotoColorEmoji.ttf
        ??? ...
```

---

## Future: Adding More Fonts

To add additional fonts to the game:

1. **Download/copy font** to `assets/fonts/`
2. **Update `.csproj`** to include the new font:
   ```xml
   <None Update="Assets\Fonts\YourFont.ttf">
     <CopyToOutputDirectory>Always</CopyToOutputDirectory>
   </None>
   ```
3. **Build project** - font will be copied automatically
4. **Update font paths** in `GraphicsConfig.cs` if needed

---

## Alternative: Use Wildcard (Not Recommended)

You could use a wildcard to copy all TTF files:

```xml
<ItemGroup>
  <None Update="Assets\Fonts\*.ttf">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

**Why explicit is better:**
- ? Clear which fonts are included
- ? Version control-friendly
- ? No accidental inclusion of test/temporary fonts

---

## Related Files

- `VillageBuilder.Game/VillageBuilder.Game.csproj` - Font copy configuration
- `VillageBuilder.Game/assets/fonts/download_fonts.ps1` - Font download script
- `VillageBuilder.Game/Graphics/GraphicsConfig.cs` - Font loading logic

---

## Status

? **Fixed** - Fonts now copy to output directory on build  
? **Build successful** - No errors  
? **Ready to run** - Restart the game to see fonts load correctly!

---

**Next Step:** **Restart the game** (stop debugging and start again) to verify both fonts load successfully!
