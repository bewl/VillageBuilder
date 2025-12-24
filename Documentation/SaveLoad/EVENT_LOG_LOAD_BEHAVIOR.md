# Event Log Management on Save/Load

## Overview

This document discusses how the event log is handled during save and load operations, including the implemented solution and potential future enhancements.

---

## Current Implementation: Clear Event Log on Load

### Decision

**When a game is loaded, the event log is cleared** to provide a clean slate for the loaded game session.

### Rationale

**Why clear the log:**
1. **Context Separation** - Events from the previous game session (before save) are not relevant to the loaded game state
2. **Clarity** - Players see only events that occur after loading
3. **No Confusion** - Mixing pre-load and post-load events could be confusing
4. **Simple Implementation** - No serialization complexity

**Example scenario:**
```
Session A (before save):
- Built 5 houses
- Assigned 10 workers
- [SAVE GAME as "MyVillage"]

Session B (after restart):
- Load different save "OtherVillage"
- Event log from "MyVillage" is irrelevant
- Clear log ?
```

---

## Implementation Details

### Code Changes

**File:** `VillageBuilder.Game/Core/SaveLoadService.cs`
**Method:** `LoadGame(string saveName)`

```csharp
public static LoadResult LoadGame(string saveName)
{
    try
    {
        var filePath = GetSaveFilePath(saveName);
        
        if (!File.Exists(filePath))
        {
            return new LoadResult 
            { 
                Success = false, 
                Message = $"Save file not found: {saveName}" 
            };
        }
        
        var engine = SaveLoadManager.LoadGame(filePath);
        
        // Clear the event log on successful load to provide a clean slate
        EventLog.Instance.Clear();
        
        // Add a load notification to the now-clean event log
        EventLog.Instance.AddMessage($"Game loaded from save: {saveName}", LogLevel.Success);
        
        return new LoadResult 
        { 
            Success = true, 
            Message = $"Game loaded from: {saveName}",
            Engine = engine
        };
    }
    catch (Exception ex)
    {
        return new LoadResult 
        { 
            Success = false, 
            Message = $"Load failed: {ex.Message}" 
        };
    }
}
```

### Behavior

**Before fix:**
- Load game ? Event log retains old session events ? Confusing

**After fix:**
- Load game ? Event log cleared ? Load notification added ? Clean state ?

---

## Alternative Approach: Save and Restore Event Log

### Concept

**Save the event log as part of the game state** and restore it when loading.

### Pros

? **Historical Context** - Players can see what happened in the saved game session  
? **Continuity** - Event log represents the game's history  
? **Debugging** - Saved logs useful for troubleshooting issues  
? **Storytelling** - Event log becomes part of the saved narrative  

### Cons

? **Serialization Complexity** - Need to serialize/deserialize event log  
? **Save File Size** - Event log adds ~5-20KB per save  
? **Timestamp Issues** - Timestamps become stale (saved yesterday, loaded today)  
? **Log Spam** - If you save after many events, log is cluttered  

### Implementation Considerations

If implementing saved event logs:

#### 1. EventLog Serialization

**Current EventLog (not serializable):**
```csharp
public class EventLog
{
    private static EventLog? _instance;
    private readonly List<LogEntry> _entries = new();
    
    // Singleton - problematic for serialization
}
```

**Would need:**
```csharp
[Serializable]
public class EventLogData
{
    public List<LogEntry> Entries { get; set; } = new();
}

// Separate from singleton
public class EventLog
{
    // ... singleton for runtime use
    
    public EventLogData GetSerializableData()
    {
        return new EventLogData { Entries = _entries.ToList() };
    }
    
    public void RestoreFromData(EventLogData data)
    {
        _entries.Clear();
        _entries.AddRange(data.Entries);
    }
}
```

#### 2. Save/Load Integration

**Save operation:**
```csharp
public static void SaveGame(GameEngine engine, string filePath)
{
    var saveData = new SaveData
    {
        Engine = engine,
        EventLog = EventLog.Instance.GetSerializableData(), // NEW
        SaveTimestamp = DateTime.Now
    };
    
    // Serialize and save...
}
```

**Load operation:**
```csharp
public static GameEngine LoadGame(string filePath)
{
    var saveData = DeserializeSaveData(filePath);
    
    // Restore event log
    EventLog.Instance.Clear();
    EventLog.Instance.RestoreFromData(saveData.EventLog);
    
    return saveData.Engine;
}
```

#### 3. Timestamp Adjustment

**Problem:** Saved events have old timestamps

**Solution:** Add relative time or adjust timestamps

```csharp
public class LogEntry
{
    public string Message { get; set; }
    public LogLevel Level { get; set; }
    public DateTime Timestamp { get; set; }
    public int GameTick { get; set; } // NEW - game time instead of real time
}
```

Or:

```csharp
// On load, adjust all timestamps
public void RestoreFromData(EventLogData data, TimeSpan adjustment)
{
    _entries.Clear();
    foreach (var entry in data.Entries)
    {
        entry.Timestamp = entry.Timestamp.Add(adjustment);
        _entries.Add(entry);
    }
}
```

#### 4. User Option

Could make it **configurable**:

```csharp
public class GameSettings
{
    public bool SaveEventLog { get; set; } = false; // Default: clear on load
}

// In LoadGame:
if (GameSettings.SaveEventLog)
{
    EventLog.Instance.RestoreFromData(saveData.EventLog);
}
else
{
    EventLog.Instance.Clear();
    EventLog.Instance.AddMessage($"Game loaded from save: {saveName}", LogLevel.Success);
}
```

---

## Recommendation

### Current Approach (Implemented): **Clear on Load**

**Recommended for:**
- ? Simplicity
- ? Clarity for players
- ? No serialization overhead
- ? No timestamp confusion

**Best for most games** - event log is a runtime log, not historical data.

---

### Alternative Approach: **Save & Restore Event Log**

**Recommended for:**
- Games where event history is critical (e.g., grand strategy)
- Debugging/testing scenarios
- Players who want full historical context

**Implementation effort:** Medium (1-2 days)

---

## Hybrid Approach: Event Log Archive

**Interesting middle ground:**

### Concept

1. **On save**: Archive current event log to save file (optional/separate)
2. **On load**: Clear current log (clean slate)
3. **Player can view**: Historical archived logs from saved game

### Benefits

? Clean current log (like current implementation)  
? Historical data preserved (like save/restore approach)  
? No confusion (separate UI for history vs current)  

### UI Mock

```
???????????????????????????????????
? EVENT LOG                       ?
???????????????????????????????????
? [Current] [History]             ? ? Tabs
???????????????????????????????????
? Current Events:                 ?
? ? Game loaded from save         ?
? ? Worker assigned to Farm       ?
? ? Construction completed        ?
?                                 ?
? [Switch to History View]        ?
???????????????????????????????????

When clicking History:
???????????????????????????????????
? EVENT LOG - HISTORY             ?
???????????????????????????????????
? [Current] [History]             ?
???????????????????????????????????
? Historical Events (from save):  ?
? ? Village founded               ?
? ? First house built             ?
? ? 10 workers assigned           ?
? ? [GAME SAVED]                  ?
?                                 ?
? [Back to Current Events]        ?
???????????????????????????????????
```

---

## Testing

### Verify Current Implementation

**Test Case 1: Load Game with Clean Log**
1. Start new game
2. Generate some events (build, assign workers)
3. Save game
4. Load that save
5. **Verify:** Event log shows only "Game loaded from save: [name]"

**Test Case 2: Load Different Game**
1. Play Game A, generate events
2. Load Game B (different save)
3. **Verify:** Event log cleared, no events from Game A

**Test Case 3: Multiple Loads**
1. Load Game A
2. Play briefly, generate events
3. Load Game B
4. **Verify:** Event log cleared again

---

## Future Enhancements (Not Implemented)

### Priority: Low (Current solution is sufficient)

If implementing saved event logs:

**Phase 1: Basic Serialization**
- Serialize event log to save file
- Restore on load
- ~2 days work

**Phase 2: Timestamp Handling**
- Use game ticks instead of real timestamps
- Or adjust timestamps on load
- ~1 day work

**Phase 3: UI Enhancements**
- History tab for archived logs
- Filter/search historical events
- ~3 days work

**Total Effort:** ~1 week

**Priority:** Low - current clear-on-load is adequate for most use cases

---

## Related Documentation

- [SAVE_LOAD_SYSTEM.md](./SAVE_LOAD_SYSTEM.md) - Save/load architecture (if exists)
- [UI_INTEGRATION_GUIDELINES.md](./UI_INTEGRATION_GUIDELINES.md) - UI patterns

---

## Changelog

### 2024-01-XX - Event Log Clear on Load

#### Added
- ? Event log is now cleared when loading a game
- ? Load notification added to clean log after loading

#### Changed
- `SaveLoadService.LoadGame()` now clears event log after successful load

#### Technical
- Calls `EventLog.Instance.Clear()` after loading game engine
- Adds "Game loaded from save" message to clean log

---

**Maintained By:** VillageBuilder Development Team  
**Last Updated:** 2024-01-XX
