# VillageBuilder Documentation

Welcome to the VillageBuilder documentation! This guide will help you understand the game's architecture, systems, and how to work with the codebase.

## ?? Documentation Structure

### ?? Getting Started
New to VillageBuilder? Start here!
- [Quick Start Guide](GettingStarted/QUICKSTART.md) - Get up and running in 5 minutes
- [Building from Source](GettingStarted/BUILDING_FROM_SOURCE.md) - Compilation and setup
- [Your First Game](GettingStarted/FIRST_GAME.md) - Play through tutorial

### ?? Core Systems
Learn about the fundamental engine architecture:
- [Game Engine](CoreSystems/GAME_ENGINE.md) - Main simulation loop and architecture
- [Game Configuration](CoreSystems/GAME_CONFIGURATION.md) - Settings and parameters
- [Time & Seasons](CoreSystems/TIME_AND_SEASONS.md) - Day/night cycle, seasons, weather
- [Event Log](CoreSystems/EVENT_LOG.md) - Game event tracking and notifications
- [Command System](CoreSystems/COMMAND_SYSTEM.md) - Command pattern for actions

### ?? World & Simulation
Understand world generation and simulation:
- [Terrain Generation](WorldAndSimulation/TERRAIN_GENERATION.md) - Procedural world creation
- [Weather System](WorldAndSimulation/WEATHER_SYSTEM.md) - Weather conditions and effects
- [Grid & Tiles](WorldAndSimulation/GRID_AND_TILES.md) - Map structure and tile system
- [Pathfinding](WorldAndSimulation/PATHFINDING.md) - A* pathfinding algorithm

### ?? Entities & Buildings
Learn about people, families, and structures:
- [People & Families](EntitiesAndBuildings/PEOPLE_AND_FAMILIES.md) - Person/Family system
- [Building System](EntitiesAndBuildings/BUILDING_SYSTEM.md) - Buildings and definitions
- [Construction System](EntitiesAndBuildings/CONSTRUCTION_SYSTEM.md) - Building construction
- [Resource Management](EntitiesAndBuildings/RESOURCE_MANAGEMENT.md) - Resources and inventory

### ?? Rendering & Graphics
Visual systems and UI:
- [Graphics Pipeline](Rendering/GRAPHICS_PIPELINE.md) - Rendering architecture
- [Camera System](Rendering/CAMERA_SYSTEM.md) - Camera controls and viewport
- [Particle Effects](Rendering/PARTICLE_EFFECTS.md) - Smoke, rain, snow particles
- [Visual Enhancements](Rendering/VISUAL_ENHANCEMENTS.md) - Weather effects, smoke, animations
- [UI Integration](Rendering/UI_INTEGRATION_GUIDELINES.md) - UI patterns and conventions

### ?? Save/Load
Persistence systems:
- [Save/Load System](SaveLoad/SAVE_LOAD_SYSTEM.md) - Game state serialization
- [Event Log Behavior](SaveLoad/EVENT_LOG_LOAD_BEHAVIOR.md) - Event log on load

### ? Performance
Optimization guides and benchmarks:
- [Performance Optimizations](Performance/PERFORMANCE_OPTIMIZATIONS.md) - Major optimizations
- [Optimization Changelog](Performance/OPTIMIZATION_CHANGELOG.md) - Optimization history
- [Benchmark Guide](Performance/BENCHMARK_GUIDE.md) - Creating and running benchmarks

### ?? Bug Fixes
Historical bug fix documentation:
- [All Bug Fixes Index](BugFixes/README.md) - Complete list of resolved issues

---

## ?? Quick Reference

### Common Tasks
- **Add a new building type** ? [Building System](EntitiesAndBuildings/BUILDING_SYSTEM.md)
- **Modify terrain generation** ? [Terrain Generation](WorldAndSimulation/TERRAIN_GENERATION.md)
- **Change day/night cycle** ? [Time & Seasons](CoreSystems/TIME_AND_SEASONS.md)
- **Add visual effects** ? [Particle Effects](Rendering/PARTICLE_EFFECTS.md)
- **Optimize performance** ? [Performance Guide](Performance/PERFORMANCE_OPTIMIZATIONS.md)

### Architecture Overview

```
???????????????????????????????????????????
?          GameEngine (Core)              ?
?  - Simulation loop                      ?
?  - Entity management                    ?
?  - Command processing                   ?
???????????????????????????????????????????
           ?
     ?????????????
     ?           ?
??????????? ??????????????
? World   ? ?  Entities  ?
? Systems ? ?  & Logic   ?
??????????? ??????????????
? Terrain ? ? People     ?
? Weather ? ? Families   ?
? Grid    ? ? Buildings  ?
??????????? ??????????????
           ?
           ?
    ????????????????
    ?   Rendering  ?
    ????????????????
    ? Graphics     ?
    ? UI           ?
    ? Particles    ?
    ????????????????
```

---

## ?? Learning Path

**For New Developers:**
1. Read [Quick Start](GettingStarted/QUICKSTART.md)
2. Understand [Game Engine](CoreSystems/GAME_ENGINE.md)
3. Explore [People & Families](EntitiesAndBuildings/PEOPLE_AND_FAMILIES.md)
4. Learn [Command System](CoreSystems/COMMAND_SYSTEM.md)

**For Content Creators:**
1. Learn [Building System](EntitiesAndBuildings/BUILDING_SYSTEM.md)
2. Understand [Resource Management](EntitiesAndBuildings/RESOURCE_MANAGEMENT.md)
3. Explore [Terrain Generation](WorldAndSimulation/TERRAIN_GENERATION.md)

**For Graphics/UI Developers:**
1. Read [Graphics Pipeline](Rendering/GRAPHICS_PIPELINE.md)
2. Study [UI Integration](Rendering/UI_INTEGRATION_GUIDELINES.md)
3. Explore [Particle Effects](Rendering/PARTICLE_EFFECTS.md)

**For Performance Engineers:**
1. Review [Performance Optimizations](Performance/PERFORMANCE_OPTIMIZATIONS.md)
2. Learn [Benchmark Guide](Performance/BENCHMARK_GUIDE.md)
3. Study [Optimization Changelog](Performance/OPTIMIZATION_CHANGELOG.md)

---

## ?? Contributing

### Documentation Standards
- Use Markdown format
- Include code examples where relevant
- Add diagrams for complex systems (ASCII or Mermaid)
- Keep technical terms consistent
- Update index when adding new docs

### Documentation Template
See [TEMPLATE.md](TEMPLATE.md) for the standard documentation structure.

---

## ?? External Resources

- **Raylib Documentation**: https://www.raylib.com/
- **.NET 9 Documentation**: https://learn.microsoft.com/en-us/dotnet/
- **BenchmarkDotNet**: https://benchmarkdotnet.org/

---

**Last Updated:** 2024-01-XX  
**Version:** 1.0  
**Maintained By:** VillageBuilder Development Team
