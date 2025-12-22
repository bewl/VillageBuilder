# Day/Night Cycle - Visual Guide

## Visual Changes Throughout the Day

### Morning (6 AM - 12 PM)
- **Lighting**: Full brightness, normal colors
- **Status Bar**: ? Sun icon in bright yellow
- **People**: Waking up, traveling to work
- **Buildings**: Normal appearance

### Afternoon (12 PM - 6 PM)
- **Lighting**: Full brightness
- **Status Bar**: ? Sun icon in orange-yellow
- **People**: Working at buildings
- **Buildings**: Normal appearance

### Evening (6 PM - 10 PM)
- **Lighting**: Gradual darkening (darkens by 25% every hour)
- **Status Bar**: ? Crescent moon in light blue
- **People**: Returning home
- **Buildings**: Start to show warm glow if occupied
- **Darkness Factor**: 0.0 ? 1.0 (gradual)

### Night (10 PM - 4 AM)
- **Lighting**: Full darkness (70% darker)
- **Status Bar**: ? Full moon in dark blue
- **People**: Sleeping at home
- **Buildings**: 
  - **Occupied houses**: Warm orange/yellow glow from windows
  - **Empty houses**: Dark, no glow
  - **All buildings**: Significantly darker

### Dawn (4 AM - 6 AM)
- **Lighting**: Gradual lightening
- **Status Bar**: ? Moon icon in dark blue
- **People**: Still sleeping
- **Buildings**: Lights fading as dawn approaches
- **Darkness Factor**: 1.0 ? 0.0 (gradual)

## Color Changes

### Daytime Colors (Examples)
```
Grass:     RGB(80, 180, 80)   - Bright green
Forest:    RGB(60, 150, 60)   - Green
Houses:    RGB(100, 80, 60)   - Brown walls
```

### Nighttime Colors (70% darker)
```
Grass:     RGB(24, 54, 24)    - Dark green
Forest:    RGB(18, 45, 18)    - Very dark green
Houses:    RGB(30, 24, 18)    - Dark brown
```

### Occupied House at Night
```
Floor:     RGB(120, 94, 70) + Warm Glow
           RGB(200, 134, 70)  - Orange/yellow glow
Walls:     RGB(30, 24, 18)    - Still dark
Glyphs:    RGB(255, 255, 200) - Bright yellow-white
```

## Status Bar Time Display

```
???????????????????????????????????????????????????????
? Y1 | SPR D45 | ? 08:00 | ? Clear 15° | ? RUN x1.0 ?
? Wood: 850  Stone: 450  Tools: 89  Grain: 1240      ?
???????????????????????????????????????????????????????
```

Time of Day Icons:
- ? (Morning/Afternoon) - Bright, work hours
- ? (Evening) - Transitioning to night
- ? (Night) - Dark, sleep time

## People Indicators

### Visual States
- **Blue background**: Male person
- **Pink background**: Female person
- **Yellow corner dot**: Has assigned job
- **Yellow path line**: Currently traveling

### Task-based Behavior
- **Morning (6 AM)**: Yellow path lines as people travel to work
- **Daytime**: People clustered at work buildings
- **Evening (6 PM)**: Yellow path lines as people return home
- **Night**: People clustered at houses

## Building Indicators

### House Lighting
```
DAYTIME HOUSE:           NIGHT EMPTY HOUSE:      NIGHT OCCUPIED HOUSE:
?????????                ?????????               ?????????
? ? ? ? ? Normal         ? ? ? ? ? Dark          ? ? ? ? ? Dark walls
? ?   ? ? Colors         ? ?   ? ? No glow       ? ? ? ? ? WARM GLOW
? ? D ? ?                ? ? D ? ?               ? ? D ? ? inside!
?????????                ?????????               ?????????
```

Legend:
- `?` = Wall (dark at night)
- `D` = Door (always visible)
- ` ` = Floor (normal at day, glows when occupied at night)
- `?` = Person sleeping inside (visible through glow)

## Energy Recovery Visualization

People's energy recovers at different rates:

```
Activity          Energy Change per Hour
?????????????????????????????????????????
Working           -1 (depleting)
Idle              +2 (slow recovery)
Sleeping          +3 (fast recovery) ?
```

## Event Log Messages

The game will show these messages throughout the day:

```
[6:00 AM]  "The Smith family (2 workers) begins their work day"
[6:00 PM]  "The Smith family returns home for the evening"
[10:00 PM] "The village settles in for the night"
[Building] "The Smith family has moved into their new home"
```

## Gameplay Impact

### Strategic Considerations

1. **Work Efficiency**
   - Only 12 hours of work per day (6 AM - 6 PM)
   - Plan construction and jobs around work hours
   
2. **Energy Management**
   - Workers need homes to sleep and recover energy
   - Without homes, energy recovery is slower
   - Tired workers are less productive

3. **Building Placement**
   - Build houses near work buildings
   - Reduces travel time
   - More time spent working

4. **Visual Monitoring**
   - Glowing houses = families are home
   - Dark houses = empty or no residents assigned
   - Easy to spot at night

## Performance Notes

- Darkness overlay is efficient (just color multiplication)
- Lighting glow is per-tile calculation
- No impact on game logic speed
- Visual effects only affect rendering
