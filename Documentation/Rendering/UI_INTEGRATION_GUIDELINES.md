# Interactive Systems UI Integration Guidelines

## Overview
When adding new interactive systems to VillageBuilder, always ensure they're properly integrated into the sidebar UI for discoverability and ease of use.

## Checklist for New Interactive Systems

### 1. **Keyboard Shortcuts**
? Add keyboard shortcuts in `GameRenderer.HandleInput()`
- Choose intuitive keys (F-keys for save/load, letters for building types)
- Document all shortcuts
- Return appropriate flags for main game loop handling

### 2. **Sidebar Integration**
? Update `SidebarRenderer.cs` to show:
- **Commands section**: Add keyboard shortcut with description
- **Quick Stats section**: Add status/info if relevant
- Use color coding for visual hierarchy
- Group related commands together with spacers

### 3. **Event Log Feedback**
? Provide user feedback via EventLog
- Success messages (green)
- Error messages (red)
- Info messages (white/gray)
- Keep messages concise and actionable

### 4. **Documentation**
? Create comprehensive documentation in `Documentation/` folder
- System overview
- Usage examples
- Architecture details
- Troubleshooting guide

## Example: Save/Load System Integration

### GameRenderer.cs - Input Handling
```csharp
// Save/Load controls
// F5 = Quick Save, F9 = Quick Load
if (Raylib.IsKeyPressed(KeyboardKey.F5))
{
    saveRequested = true;
}
if (Raylib.IsKeyPressed(KeyboardKey.F9))
{
    loadRequested = true;
}
```

### SidebarRenderer.cs - Commands Section
```csharp
var controls = new[]
{
    // ... other controls
    ("F5", "Quick Save", new Color(255, 200, 50, 255)),
    ("F9", "Quick Load", new Color(255, 200, 50, 255)),
    // ... more controls
};
```

### SidebarRenderer.cs - Status Section
```csharp
// Save/Load status
var quicksaves = GetSaveFiles().Where(s => s.StartsWith("quicksave_"));
if (quicksaves.Any())
{
    var timeSince = DateTime.Now - lastSaveTime;
    GraphicsConfig.DrawConsoleText($"? ? Last Save: {timeSince} ago", ...);
}
```

### Program.cs - Main Loop Integration
```csharp
if (saveRequested)
{
    var result = SaveLoadService.QuickSave(engine);
    EventLog.Instance.AddMessage(result.Message, 
        result.Success ? LogLevel.Success : LogLevel.Error);
}
```

## UI Design Principles

### Color Coding
- **Green** (100, 255, 100): Game control actions (pause, resume)
- **Yellow** (255, 255, 100): Speed/time controls
- **Orange** (255, 200, 50): Save/load operations
- **Orange/Brown** (255, 180, 100): Building placement
- **Blue** (150, 200, 255): View/display controls
- **Light Blue** (100, 200, 255): Map controls
- **Red** (255, 100, 100): Destructive/quit actions

### Sidebar Layout
```
???????????????????????????
? QUICK STATS             ? <- Always visible
?  Population, buildings  ?
?  System status          ?
???????????????????????????
? COMMANDS                ? <- Keyboard reference
?  Game controls          ?
?  Save/Load (F5/F9)      ?
?  Building hotkeys       ?
?  View controls          ?
???????????????????????????
? EVENT LOG               ? <- Feedback area
?  Recent actions         ?
?  Success/error messages ?
???????????????????????????
```

## Common Patterns

### Status Indicators
Show current state and keyboard hint together:
```csharp
// Pattern: [Icon] Label: Value (Hotkey)
"? ? Last Save: 5 min ago"
"? ? Population: 6"
"? ? Buildings: 3/5"
```

### Command Format
Consistent command list format:
```csharp
("KEY", "Action Description", Color)
```

Group commands by category with empty spacer rows:
```csharp
("", "", Color.White), // Spacer between groups
```

### Dynamic Status
Show relevant info based on system state:
```csharp
if (systemActive)
{
    // Show active status
}
else
{
    // Show inactive with hint to activate
    GraphicsConfig.DrawConsoleText("? No saves yet (F5)", ...);
}
```

## Integration Workflow

### Step-by-Step Process

1. **Design the System**
   - Define functionality
   - Choose keyboard shortcuts
   - Plan status indicators

2. **Implement Core Logic**
   - Create service/manager classes
   - Implement business logic
   - Add error handling

3. **Add Input Handling**
   - Update `GameRenderer.HandleInput()`
   - Add appropriate return parameters
   - Test keyboard shortcuts

4. **Integrate into Main Loop**
   - Update `Program.cs` to handle new flags
   - Add EventLog feedback
   - Test complete workflow

5. **Update Sidebar UI**
   - Add commands to `RenderCommands()`
   - Add status to `RenderQuickStats()` if relevant
   - Use appropriate colors and spacing

6. **Document Everything**
   - Create markdown documentation
   - Include code examples
   - Add troubleshooting section

7. **Test & Iterate**
   - Test all keyboard shortcuts
   - Verify sidebar updates correctly
   - Check EventLog messages display properly
   - Ensure status indicators update in real-time

## Real-World Examples

### Current Integrated Systems

**Save/Load System** ?
- Commands: F5 (save), F9 (load)
- Status: Last save time
- Feedback: EventLog messages
- Colors: Orange (255, 200, 50)

**Heat Map** ?
- Command: V (toggle)
- Status: N/A (visual only)
- Feedback: Visual change
- Color: Light blue (100, 200, 255)

**Building Placement** ?
- Commands: H, F, W, L, M, K, E, T
- Status: N/A
- Feedback: Visual preview
- Colors: Orange/brown (255, 180, 100)

**Game Speed** ?
- Commands: +, -, 0, SPACE
- Status: Shows in status bar
- Feedback: Immediate speed change
- Colors: Yellow (255, 255, 100), Green (100, 255, 100)

### Future Systems to Integrate

**Trade System** (Future)
- Commands: ? (TBD)
- Status: Active trades count
- Feedback: Trade notifications
- Color: ? (suggest cyan)

**Tech Tree** (Future)
- Commands: ? (TBD)
- Status: Research progress
- Feedback: Research complete
- Color: ? (suggest purple)

**Diplomacy** (Future)
- Commands: ? (TBD)
- Status: Relationship status
- Feedback: Diplomatic events
- Color: ? (suggest gold)

## Best Practices

### Do's ?
- Always add keyboard shortcuts for new interactive systems
- Update sidebar immediately when adding new features
- Use consistent color scheme across UI
- Provide clear feedback via EventLog
- Group related commands together
- Show status for stateful systems
- Document everything

### Don'ts ?
- Don't add features without UI integration
- Don't use conflicting keyboard shortcuts
- Don't skip EventLog feedback
- Don't forget to update documentation
- Don't use random colors (follow scheme)
- Don't hide important commands
- Don't make users guess how to use features

## Testing Checklist

When adding a new interactive system:

- [ ] Keyboard shortcut works correctly
- [ ] Command appears in sidebar
- [ ] Status indicator updates in real-time (if applicable)
- [ ] EventLog shows success/error messages
- [ ] Colors follow established scheme
- [ ] Documentation is complete
- [ ] No conflicting keyboard shortcuts
- [ ] System is discoverable without external help
- [ ] Feedback is clear and immediate
- [ ] Build succeeds without warnings

## Maintenance

### Regular Updates
- Review sidebar layout for clarity
- Update documentation when behavior changes
- Consolidate similar commands if UI gets crowded
- Gather user feedback on discoverability
- Refactor if patterns emerge

### When to Create New Sections
Create a new sidebar section when:
- Adding 3+ related commands
- System has persistent status to display
- Commands don't fit logically in existing sections
- UI becomes too cluttered

## Conclusion

**Key Principle**: Every interactive system must be discoverable through the sidebar UI. Users should never need to read external documentation to know how to interact with the game.

By following these guidelines, we ensure:
- Consistent user experience
- Easy discoverability
- Clear feedback
- Professional polish
- Maintainable codebase
