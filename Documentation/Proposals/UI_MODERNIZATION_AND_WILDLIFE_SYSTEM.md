# UI Modernization & Wildlife System - Design Proposal

## Overview

Two major improvements identified for VillageBuilder's visual and gameplay systems:

1. **UI Modernization** - Replace ASCII box-drawing with emoji-cohesive symbols
2. **Wildlife System** - Extract animals from decorations into proper AI entities

---

## Part 1: UI Modernization (Quick Win - 20 minutes)

### Current Problem

**Sidebar uses ASCII box-drawing** that clashes with beautiful emoji terrain:
- `?` `?` `?` `?` `?` `?` `?` `?` = Old-school terminal aesthetic
- Inconsistent with `??????` emoji sprite terrain

### Proposed Visual Updates

#### **Section Headers**
**Before:**
```
?? QUICK STATS ?????????????
? ?? Families: 3
?   Population: 12
?????????????????????????????
```

**After:**
```
? QUICK STATS
  ?? Families: 3
     Population: 12
  ???????????????????????
```

#### **Building Icons**
**Before (ASCII):**
```
?   ? House        2
?   ? Farm         1
?   ? Warehouse    1
```

**After (Emoji):**
```
  ?? House        2
  ?? Farm         1
  ?? Warehouse    1
```

#### **Status Indicators**
**Before:**
```
? ? Last Save: 5 min ago
```

**After:**
```
  ?? Last Save: 5 min ago
```

### Icon Mapping

| Element | ASCII | Emoji | Rationale |
|---------|-------|-------|-----------|
| **Buildings** |
| House | `?` | ?? | Universal house symbol |
| Farm | `?` | ?? | Wheat represents farming |
| Warehouse | `?` | ?? | Box for storage |
| Mine | `+` | ?? | Pickaxe for mining |
| Lumberyard | `+` | ?? | Axe for lumber |
| Workshop | `+` | ?? | Hammer for crafting |
| Market | `+` | ?? | Store front |
| Well | `?` | ?? | Water droplet |
| Town Hall | `?` | ??? | Government building |
| **UI Elements** |
| Families | `?` | ?? | People group |
| Save status | `?` | ?? | Floppy disk |
| Task icons | Various | ?????? | Activity-specific |
| **Borders** |
| Vertical | `?` | (remove/indent) | Clean spacing |
| Horizontal | `?` | `?` | Block line |
| Section start | `??` | `?` | Modern arrow |

### Benefits

? **Visual Cohesion** - Matches emoji terrain aesthetic  
? **Modern Look** - Appeals to broader audience  
? **Better Readability** - Emoji icons are more intuitive  
? **Consistent Theme** - Unified visual language  

### Implementation Approach

Replace in `SidebarRenderer.cs`:
1. `DrawBorder()` - Remove or simplify
2. `DrawSectionHeader()` - Use `?` instead of `??`
3. `GetBuildingAsciiIcon()` - Return emoji instead of ASCII
4. All `?` prefixes - Replace with clean indent
5. All `?` separators - Replace with `?`

---

## Part 2: Wildlife System (Major Feature - 2-4 hours)

### Current Problem

**Animals are "decorations"** which is architecturally wrong:
```
TerrainDecoration
??? Trees ?? (static) ? Correct
??? Rocks ?? (static) ? Correct
??? Animals ?????? (dynamic) ? WRONG
```

**Issues:**
- Animals don't move realistically (just bob/hover)
- Can't hunt, flee, reproduce, or die
- No ecosystem dynamics
- No prey/predator relationships
- Essentially "animated scenery"

### Proposed Architecture

#### **Phase 2A: Extract Wildlife Entity** (1 hour)

Create separate `Wildlife` system:

```
1. TerrainDecoration (Static/Semi-static)
   ??? Trees ????
   ??? Rocks ??
   ??? Flowers ????
   ??? Mushrooms ??
   ??? Grass Tufts ??

2. Wildlife (Dynamic Entities)
   ??? WildlifeEntity base class
   ??? Rendering on tiles (like People)
   ??? Basic movement AI
```

**WildlifeEntity Class:**
```csharp
public class WildlifeEntity
{
    public int Id { get; }
    public WildlifeType Type { get; } // Rabbit, Deer, Bird, etc.
    public Vector2Int Position { get; set; }
    public WildlifeBehavior Behavior { get; set; } // Grazing, Fleeing, Hunting
    public int Health { get; set; }
    public float Speed { get; }
    public bool IsAlive { get; set; }
    
    // AI state
    public WildlifeTask CurrentTask { get; set; }
    public List<Vector2Int> CurrentPath { get; set; }
    public Vector2Int? HomeTerritory { get; set; }
    
    public void Update(GameEngine engine);
    public void Move(Vector2Int target);
    public void Flee(Vector2Int dangerSource);
}
```

#### **Phase 2B: Add Wildlife AI** (2 hours)

**Prey Animals (Rabbits ??, Deer ??):**
- Graze in grasslands
- Flee from people/predators
- Breed when food abundant
- Die from starvation/hunting

**Predators (Wolves ??, Foxes ??):**
- Hunt prey animals
- Territorial behavior
- Population control mechanics

**Ambient (Birds ??, Butterflies ??, Fish ??):**
- Flock/swarm behavior
- Seasonal migration
- Environmental indicators (healthy ecosystem)

#### **Phase 2C: Hunting System** (1 hour)

**Gameplay Integration:**
```csharp
public class HuntingSystem
{
    // Villagers can hunt wildlife for food
    public bool TryHuntAnimal(Person hunter, WildlifeEntity target);
    
    // Animals drop resources
    public Dictionary<ResourceType, int> GetResourcesFromKill(WildlifeType type);
    
    // Overhunting consequences
    public void CheckEcosystemBalance();
}
```

**Benefits:**
- Food source for villagers
- Ecosystem simulation
- Dynamic world (animals move, breed, die)
- Hunting mini-game potential

### Migration Strategy

**Option A: Big Bang (Not Recommended)**
- Remove all animals from TerrainDecoration
- Add complete Wildlife system
- Risk: Breaks existing game

**Option B: Gradual Migration (Recommended)**
1. Keep animals as decorations (backward compatible)
2. Add Wildlife system alongside
3. Gradually replace decoration animals with Wildlife entities
4. Remove old decoration animals when ready

**Option C: Hybrid (Compromise)**
1. Keep simple animals (butterflies ??, birds ??) as decorations
2. Migrate complex animals (rabbits ??, deer ??) to Wildlife system
3. Add predators (wolves ??) as new Wildlife only

### Technical Considerations

**Rendering:**
- Wildlife rendered in `MapRenderer.RenderWildlife()` (after terrain, before people)
- Sprite-based like people (reuse existing sprite atlas)

**Performance:**
- Limit active wildlife entities (100-200 max)
- Cull off-screen wildlife
- Simple AI (A* pathfinding reuse)

**Save/Load:**
- Wildlife state saved in game save file
- Regenerate if save format changes (non-critical data)

---

## Comparison: Decorations vs Wildlife

| Aspect | TerrainDecoration (Current) | Wildlife System (Proposed) |
|--------|----------------------------|---------------------------|
| **Movement** | Static/Bob animation | Full pathfinding AI |
| **Behavior** | None | Graze, Flee, Hunt, Breed |
| **Interaction** | Visual only | Can hunt, affects gameplay |
| **Performance** | O(decorations per tile) | O(active wildlife entities) |
| **Complexity** | Simple | Moderate |
| **Gameplay Value** | Aesthetic | Mechanic + Aesthetic |

---

## Recommendations

### **Immediate (This Session):**
? **Implement UI Modernization** (20 min)
- Quick win, high visual impact
- Non-breaking, purely cosmetic
- Makes game look more polished

### **Near-term (Next Session):**
?? **Design Wildlife System** (1 hour discussion)
- Architectural decision
- Define scope and priorities
- Plan migration strategy

### **Long-term (Future):**
?? **Implement Wildlife System** (2-4 hours)
- Phase 2A: Extract Wildlife entities
- Phase 2B: Add basic AI
- Phase 2C: Add hunting mechanics

---

## Discussion Questions

Before implementing wildlife system, we should decide:

1. **Scope**: Which animals become Wildlife entities?
   - All animals? (big change)
   - Just mammals (rabbits, deer)? (moderate)
   - Just huntable animals? (minimal)

2. **Behavior Complexity**:
   - Simple (wander + flee)? (1 hour)
   - Moderate (graze + breed + flee)? (2 hours)
   - Complex (full ecosystem simulation)? (4+ hours)

3. **Gameplay Integration**:
   - Just visual improvement?
   - Add hunting mechanics?
   - Add ecosystem balance mechanics?

4. **Migration Strategy**:
   - Big bang replacement?
   - Gradual migration?
   - Hybrid approach?

---

## Next Steps

### **Step 1: UI Modernization** (Do Now)
I'll implement the UI modernization with emoji icons and clean borders. This is quick, safe, and makes the game look much better immediately.

### **Step 2: Wildlife Discussion** (After Step 1)
Once UI looks great, we can have a detailed discussion about wildlife system architecture and decide on the scope.

---

**Author:** VillageBuilder Development Team  
**Date:** 2025-01-XX  
**Status:** Proposal - Awaiting approval to proceed with Phase 1
