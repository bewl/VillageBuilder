# Quick Start Guide

Get VillageBuilder running in **5 minutes**!

---

## Prerequisites

? **Windows 10/11** (primary platform)  
? **.NET 9 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)  
? **Visual Studio 2022** or **Visual Studio Code** (optional but recommended)  
? **Git** - [Download here](https://git-scm.com/)  

---

## Installation

### Step 1: Clone Repository

```bash
git clone https://github.com/bewl/VillageBuilder.git
cd VillageBuilder
```

### Step 2: Open Solution

**Option A: Visual Studio**
```bash
start VillageBuilder.sln
```

**Option B: Command Line**
```bash
code .  # If using VS Code
# or
dotnet restore
```

### Step 3: Build & Run

**Visual Studio:**
1. Set `VillageBuilder.Game` as startup project (right-click ? Set as Startup Project)
2. Press `F5` or click "Start"

**Command Line:**
```bash
cd VillageBuilder.Game
dotnet run
```

---

## First Launch

### Window Opens

The game launches in **borderless fullscreen** mode automatically.

**You'll see:**
```
??????????????????????????????????????????????????????
?                                     ? Quick Stats  ?
?                                     ????????????????
?                                     ? Families: 1  ?
?                                     ? Population: 4?
?         Green Map Area              ? Buildings: 3 ?
?         (Terrain + River)           ?              ?
?                                     ? COMMANDS     ?
?                                     ? [P] Pause    ?
?                                     ? [+/-] Speed  ?
?                                     ?              ?
?                                     ? EVENT LOG    ?
?                                     ? > Game start ?
??????????????????????????????????????????????????????
      Status Bar: Year 1, Spring Day 1
```

### Initial Village

**Starting Conditions:**
- 1 Family (4 people: 2 adults, 2 children)
- 3 Buildings (Town Hall, House, Farm)
- Small procedurally-generated map

---

## Basic Controls

### Camera

| Control | Action |
|---------|--------|
| `Arrow Keys` | Pan map |
| `Mouse Wheel` | Zoom in/out |
| `Middle Mouse + Drag` | Pan map (alternative) |

### Game Control

| Key | Action |
|-----|--------|
| `P` | Pause/Resume |
| `+` / `=` | Speed up time |
| `-` | Slow down time |
| `ESC` | Clear selection / Return to map |

### Selection

| Action | How |
|--------|-----|
| **Select tile** | `Left Click` on map |
| **Select person** | `Left Click` on person |
| **Select building** | `Left Click` on building |
| **Cycle people on tile** | `Arrow Keys` or `Tab` |

### Building

| Key | Action |
|-----|--------|
| `H` | Build House |
| `F` | Build Farm |
| `W` | Build Warehouse |

**Building Steps:**
1. Press building key (e.g., `H` for house)
2. Dialog appears asking for X/Y coordinates
3. Enter position (e.g., X: 60, Y: 60)
4. Press Enter to confirm

---

## First Actions

### 1. Observe Daily Life

**Watch your villagers:**
- **6 AM** - People wake up and go to work
- **6 PM** - People go home from work
- **10 PM** - People go to sleep

**Check stats:**
- Click on a person to see Energy, Hunger, Health
- Watch task changes (Idle ? GoingToWork ? Working)

### 2. Build Your First House

```
1. Press [H] for House
2. Enter X: 55, Y: 55
3. Press Enter
4. Click the building site
5. Assign construction workers from sidebar
```

**Construction Progress:**
- Workers travel to site
- Construction progresses in stages:
  - Foundation (0-25%)
  - Framing (25-50%)
  - Walls (50-75%)
  - Finishing (75-100%)

### 3. Assign Jobs

```
1. Click on a completed building (e.g., Farm)
2. Sidebar shows "BUILDING INFO"
3. See list of families
4. Click a family name to assign workers
```

**Workers will:**
- Go to work at 6 AM
- Work until 6 PM
- Return home automatically

---

## Understanding the UI

### Status Bar (Top)

```
Year 1 | Spring Day 1 | ? Clear | Temp: 15°C | Zoom: 1.00x
Population: 4 | Families: 1 | Buildings: 3 | [Run] x1.0
```

**Shows:**
- Current date and season
- Weather condition and temperature
- Zoom level
- Population statistics
- Game speed

### Map Viewport (Left - 70%)

**Colors:**
- **Green** - Grass (walkable)
- **Dark Green** - Forest
- **Blue** - Water (not walkable)
- **Gray** - Mountain
- **Brown** - Dirt roads

**Symbols:**
- `?` `?` - People (animated)
- `???` - Smoke from chimneys (night)
- Building glyphs (varies by type)

### Sidebar (Right - 30%)

**Sections:**
1. **Quick Stats** - Population, buildings, resources
2. **Selection Info** - Details of selected entity
3. **Commands** - Available actions
4. **Event Log** - Recent game events

**When Person Selected:**
```
???????????????????????
? PERSON INFO         ?
???????????????????????
? John Smith          ?
? Age: 25 | Male      ?
? Family: Smith       ?
? Status: Working     ?
? Energy: 85/100      ?
? Hunger: 20/100      ?
???????????????????????
? COMMANDS            ?
? [ESC] Back to Map   ?
???????????????????????
```

**When Building Selected:**
```
???????????????????????
? BUILDING INFO       ?
???????????????????????
? Farm                ?
? Type: Farm          ?
? Status: Operational ?
? Workers: 2          ?
?   • Smith (2)       ?
???????????????????????
? ASSIGN WORKERS      ?
? [Click family name] ?
???????????????????????
```

---

## Common Tasks

### Pause the Game

```
Press [P]
```

**When to pause:**
- Reading information
- Planning building placement
- Assigning workers

### Speed Up Time

```
Press [+] to increase speed
Press [-] to decrease speed
```

**Time scales:**
- 0.25x (very slow)
- 0.5x (slow)
- 1.0x (normal)
- 2.0x (fast)
- 4.0x (very fast)

### Save Your Game

```
Press [F5] for Quick Save
```

**Save location:**
```
VillageBuilder.Game/Saves/quicksave_YYYYMMDD_HHMMSS.vbsave
```

### Load Your Game

```
Press [F9] for Quick Load
```

Loads the most recent quicksave.

---

## Tips for New Players

### ?? Housing
- Build houses before population grows
- Each house holds 1 family (4-6 people)
- Assign families to houses via building menu

### ?? Food Production
- Build farms early
- Assign workers to farms
- Monitor hunger levels (sidebar when selecting person)

### ?? Construction
- Construction requires workers
- More workers = faster construction
- Workers return daily automatically (6 AM)

### ? Time Management
- People work 6 AM - 6 PM
- People sleep 10 PM - 6 AM
- Plan construction during work hours

### ?? Monitor Stats
- Check Energy: <50 means people need rest
- Check Hunger: >60 means people need food
- Check Health: <70 means sickness risk

---

## Troubleshooting

### Game Won't Start

**Error:** "Could not find .NET 9 runtime"
- **Solution:** Install .NET 9 SDK from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/9.0)

### Window Size Wrong

**Issue:** Window doesn't fill screen
- **Solution:** Restart game - window detection happens at startup

### Font Looks Wrong

**Issue:** Smoke shows as squares `???`
- **Solution:** Install Cascadia Code font
- See: [FONT_QUICK_SETUP.md](../BugFixes/FONT_QUICK_SETUP.md)

### People Not Working

**Issue:** People idle at home instead of working
- **Check:** Are workers assigned to building?
- **Check:** Is it work hours (6 AM - 6 PM)?
- **Check:** Is building constructed?

### Construction Not Progressing

**Issue:** Construction workers leave and don't return
- **Fixed:** Recent patch ensures workers return daily
- **Workaround:** Reassign construction workers

---

## What's Next?

### Learn More

- **[Your First Game](FIRST_GAME.md)** - Detailed gameplay tutorial
- **[Building System](../EntitiesAndBuildings/BUILDING_SYSTEM.md)** - All building types
- **[People & Families](../EntitiesAndBuildings/PEOPLE_AND_FAMILIES.md)** - Villager mechanics

### Advanced Features

- **Save/Load System** - Manual saves, multiple save slots
- **Weather Effects** - Rain, snow, seasonal changes
- **Construction Planning** - Multi-tile buildings, rotation
- **Resource Management** - Food production, storage

### Join Community

- **GitHub Issues** - Report bugs, request features
- **Discussions** - Share strategies, get help

---

## Quick Reference Card

```
???????????????????????????????????????????????????????
  VILLAGEBUILDER - QUICK REFERENCE
???????????????????????????????????????????????????????

CAMERA
  Arrows     Pan map
  Wheel      Zoom
  
CONTROL
  P          Pause/Resume
  +/-        Speed up/down
  ESC        Clear selection
  
BUILDING
  H          House
  F          Farm
  W          Warehouse
  
SAVE/LOAD
  F5         Quick Save
  F9         Quick Load
  
SELECTION
  Click      Select tile/person/building
  Arrows/Tab Cycle people on tile
  
???????????????????????????????????????????????????????
```

---

**Happy Building!** ???

**Need Help?** Check [Documentation README](../README.md) for full guides.

**Found a Bug?** Report on [GitHub Issues](https://github.com/bewl/VillageBuilder/issues)

---

**Last Updated:** 2024-01-XX  
**Version:** 1.0
