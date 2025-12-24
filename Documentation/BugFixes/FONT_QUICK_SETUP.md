# Quick Font Setup - VillageBuilder

## ?? 5-Minute Setup for Best Font

### Step 1: Download Cascadia Mono

**Windows 11:** Already installed! Skip to Step 3.

**Windows 10 / Other:**
1. Visit: https://github.com/microsoft/cascadia-code/releases
2. Download latest `CascadiaCode-*.zip`
3. Extract the ZIP file

### Step 2: Install Font

1. Find `CascadiaMono.ttf` in the extracted folder (usually in `ttf/` subfolder)
2. Right-click `CascadiaMono.ttf`
3. Click **"Install for all users"** (requires admin)
4. Wait for installation to complete

### Step 3: Run the Game

1. Launch VillageBuilder
2. Check the console window for:
   ```
   ? Font loaded: CascadiaMono.ttf
   ```

### Step 4: Verify

**What to check:**
- ? Smoke effects look wispy and organic (not squares)
- ? Task icons appear correctly in person selection UI
- ? Box-drawing borders are crisp and connected

**If you see squares (?):**
- Font didn't install correctly
- Try restarting the game
- Or install DejaVu Sans Mono as alternative

---

## Alternative: DejaVu Sans Mono

If Cascadia Mono doesn't work:

1. Download: https://dejavu-fonts.github.io/
2. Extract and find `DejaVuSansMono.ttf`
3. Right-click ? "Install for all users"
4. Restart game

---

## Character Test

**When game is running, you should see:**

**Smoke (at night from chimneys):**
```
  ?
 ???
????
 ??
```

**Task Icons (in person selection):**
```
? Working
?? Constructing
?? Going Home
? Going to Work
? Idle
```

**Box Drawing (UI borders):**
```
????????????
? Person   ?
????????????
? Details  ?
????????????
```

---

## Troubleshooting

**Q: I see "No font with Unicode support found"**
A: Install Cascadia Mono or DejaVu Sans Mono, then restart game.

**Q: Smoke shows as squares (???)**
A: Current font doesn't support block shading. Install Cascadia Mono.

**Q: Font installed but game still doesn't use it**
A: Restart the game. Font loading happens at startup.

**Q: Where is the font installed?**
A: Check `C:\Windows\Fonts\` folder - you should see `CascadiaMono.ttf`

---

## Need Help?

See full documentation: `VillageBuilder.Game/Documentation/FONT_CONFIGURATION.md`
