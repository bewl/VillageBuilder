# Save/Load System

## Overview
Comprehensive save/load system for VillageBuilder with quick save/load functionality and autosave support.

## Features

### Quick Save/Load
- **F5** - Quick Save (creates timestamped save)
- **F9** - Quick Load (loads latest quicksave)

### File Format
- JSON-based save files (`.vbsave` extension)
- Human-readable and editable
- Version-tracked for future compatibility

### Save Location
- Saves stored in `Saves/` directory
- Automatically created if doesn't exist
- Organized by timestamp

### What Gets Saved
- ? **Game Metadata**: Game ID, tick count, seed
- ? **Time & Weather**: Current time, season, weather conditions
- ? **Resources**: All village resources and building storage
- ? **Families**: Family structure, positions, relationships
- ? **People**: Stats, tasks, assignments, paths, health
- ? **Buildings**: Type, position, construction state, workers
- ? **Configuration**: Map size, tick rate, initial settings

### Auto-Save
- Keeps last 3 autosaves
- Automatically cleans up old saves
- Timestamped for easy identification

## Usage

### Code Example - Save
```csharp
// Quick save
var result = SaveLoadService.QuickSave(engine);
if (result.Success)
{
    Console.WriteLine(result.Message); // "Game saved to: quicksave_20231223_143022"
}

// Quick load (loads most recent quicksave)
var result = SaveLoadService.QuickLoad();
if (result.Success && result.Engine != null)
{
    engine = result.Engine;
}

// Named save
var result = SaveLoadService.SaveGame(engine, "my_village");
```

### Code Example - Load
```csharp
// Quick load (most recent quicksave)
var result = SaveLoadService.QuickLoad();
if (result.Success && result.Engine != null)
{
    engine = result.Engine;
    // Restart game loop with new engine
}

// Load specific save
var result = SaveLoadService.LoadGame("my_village");
if (result.Success && result.Engine != null)
{
    engine = result.Engine;
    // Restart game loop with new engine
}

// Get all save files
string[] saves = SaveLoadService.GetSaveFiles();
foreach (var save in saves)
{
    var info = SaveLoadService.GetSaveInfo(save);
    Console.WriteLine($"{info.Name} - {info.SavedAt} - {info.FileSize} bytes");
}
```

### File Structure
```
Saves/
??? quicksave_20231223_143022.vbsave
??? autosave_20231223_143500.vbsave
??? autosave_20231223_144000.vbsave
??? autosave_20231223_144500.vbsave
??? my_village.vbsave
```

## Architecture

### Core Classes

**GameState.cs** - Serializable state container
- GameState: Root state object
- GameTimeState: Time/season data
- WeatherState: Weather conditions
- FamilyState: Family data with members
- PersonState: Individual person data
- BuildingState: Building data with workers
- Vector2IntState: Serializable position data

**SaveLoadManager.cs** - Serialization engine
- ExtractGameState: Convert GameEngine to GameState
- RestoreGameState: Convert GameState to GameEngine
- Uses JSON serialization with System.Text.Json
- Handles object references (spouses, children, buildings)

**SaveLoadService.cs** - UI-friendly save/load API
- QuickSave: F5 quick save
- AutoSave: Periodic saves with cleanup
- SaveGame: Named saves
- LoadGame: Load from file
- GetSaveFiles: List all saves
- DeleteSave: Remove save file

### Serialization Flow

**Save:**
```
GameEngine
  ?
ExtractGameState()
  ?
GameState (object graph)
  ?
JSON Serialize
  ?
.vbsave file
```

**Load:**
```
.vbsave file
  ?
JSON Deserialize
  ?
GameState (object graph)
  ?
RestoreGameState()
  ?
GameEngine
```

## Technical Details

### Reference Resolution
The system handles complex object graphs:
1. **First Pass**: Create all objects (people, families, buildings)
2. **Grid Registration**: Register buildings on grid tiles (critical for rendering!)
3. **Second Pass**: Restore references (spouse, children, assigned buildings)
4. **Third Pass**: Restore collections (workers, residents)

**Important**: Buildings must be registered on their occupied tiles via `tile.Building = building` or they won't render. This is automatically handled during load.

### Reflection Usage
Used for setting private properties/fields:
- GameTime properties (Year, Season, Day, Hour)
- GameEngine internal counters (_currentTick, _nextBuildingId)

### World Regeneration
- Terrain is regenerated from seed (not serialized tile-by-tile)
- Reduces save file size
- Ensures deterministic world generation
- Custom tile modifications can be added if needed

### Version Compatibility
- SaveVersion field for future migration
- JSON format allows adding new fields
- Missing fields get default values

## Best Practices

### When to Save
- Before risky decisions
- After major achievements
- Periodic auto-saves (every 5-10 minutes)
- Before game exit

### Save File Management
- Use descriptive names for important saves
- Quicksaves for experimentation
- Auto-saves for safety net
- Delete old saves to save disk space

### Error Handling
```csharp
var result = SaveLoadService.SaveGame(engine, saveName);
if (!result.Success)
{
    // Show error to player
    EventLog.Instance.AddMessage(result.Message, LogLevel.Error);
}
```

## Future Enhancements

### Planned Features
- [ ] Save file browser UI
- [ ] Save thumbnails/screenshots
- [ ] Compressed saves (GZip)
- [ ] Cloud save support
- [ ] Save file validation
- [ ] Migration system for version updates
- [ ] Save file metadata (village name, population, year)
- [ ] Multiple save slots with preview

### Potential Optimizations
- Delta saves (only changes since last save)
- Binary serialization option
- Async save/load
- Background auto-save

## Troubleshooting

### Save Fails
- Check disk space
- Verify write permissions on Saves/ directory
- Check for invalid characters in save name

### Load Fails
- Verify .vbsave file exists
- Check JSON syntax if manually edited
- Ensure save version compatibility
- Try loading in a fresh game

### Missing Data After Load
- Check if all object references resolved
- Verify serialization includes all needed data
- Check reflection access to private members

## Console Output
When saving/loading:
```
[INFO] Game saved to: quicksave_20231223_143022
[SUCCESS] Game loaded from: my_village
[ERROR] Save file not found: nonexistent_save
```

## Performance
- Save time: ~50-200ms (depends on world size)
- Load time: ~100-500ms (depends on world size)
- File size: ~50KB-5MB (typical game: ~500KB)

## Integration
The save/load system is integrated into:
- **Program.cs**: Main game loop (F5/F9 handling)
- **GameRenderer.cs**: Keyboard input detection
- **EventLog**: Save/load feedback messages
