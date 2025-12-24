# Time & Seasons System

## Overview

The **GameTime** and **Weather** systems work together to create a dynamic day/night cycle with seasonal changes that affect gameplay, visuals, and NPC behavior.

---

## GameTime Class

### Core Structure

```csharp
public class GameTime
{
    // Current time state
    public int Hour { get; private set; }           // 0-23
    public int Day { get; private set; }            // Days since year start
    public int DayOfSeason { get; private set; }    // Day within current season
    public int Year { get; private set; }
    public Season CurrentSeason { get; private set; }
    
    // Constants - Daily schedule
    public const int WakeUpHour = 6;      // People wake up
    public const int WorkStartHour = 6;   // Work day begins
    public const int WorkEndHour = 18;    // Work day ends (6 PM)
    public const int SleepStartHour = 22; // Bedtime (10 PM)
    
    // Season configuration
    public const int DaysPerSeason = 30;
    public const int SeasonsPerYear = 4;
    public const int DaysPerYear = 120;
}
```

### Time Progression

Time advances through `AdvanceHour()` called each game tick:

```csharp
public void AdvanceHour()
{
    Hour++;
    
    if (Hour >= 24)
    {
        Hour = 0;
        Day++;
        DayOfSeason++;
        
        if (DayOfSeason >= DaysPerSeason)
        {
            DayOfSeason = 0;
            CurrentSeason = (Season)(((int)CurrentSeason + 1) % SeasonsPerYear);
            
            if (CurrentSeason == Season.Spring)
            {
                Year++;
            }
        }
    }
}
```

**Tick Rate:**
- Game runs at 60 ticks/second by default
- 1 game hour = 60 ticks = 1 real second (at 1.0x speed)
- 1 game day = 24 hours = 24 real seconds

---

## Seasons

### Season Enum

```csharp
public enum Season
{
    Spring,  // Days 1-30
    Summer,  // Days 31-60
    Fall,    // Days 61-90
    Winter   // Days 91-120
}
```

### Season Effects

**Spring (Days 1-30)**
- Temperature: 10-20°C
- Weather: Frequent rain (30% chance)
- Visuals: Green grass, moderate lighting
- Gameplay: Moderate growing season

**Summer (Days 31-60)**
- Temperature: 20-35°C
- Weather: Mostly clear (60% clear weather)
- Visuals: Bright, warm lighting
- Gameplay: Peak growing season, higher food production

**Fall (Days 61-90)**
- Temperature: 5-15°C
- Weather: Variable (rain and clouds common)
- Visuals: Autumn colors (future enhancement)
- Gameplay: Harvest time, prepare for winter

**Winter (Days 91-120)**
- Temperature: -15 to 5°C
- Weather: Snow and blizzards (below 0°C)
- Visuals: Snow particles, darker lighting
- Gameplay: Reduced outdoor work, higher resource consumption

---

## Daily Schedule

### Time-Based Events

The game engine automatically triggers events at specific hours:

```csharp
// In GameEngine.HandleDailyRoutines()

if (Time.Hour == GameTime.WakeUpHour)  // 6 AM
{
    // Wake up all sleeping people
    foreach (var person in AllPeople.Where(p => p.IsSleeping))
    {
        person.WakeUp();
    }
}

if (Time.Hour == GameTime.WorkStartHour)  // 6 AM
{
    // Send workers to their jobs
    // Send construction workers to sites
}

if (Time.Hour == GameTime.WorkEndHour)  // 6 PM
{
    // Send everyone home
}

if (Time.Hour == GameTime.SleepStartHour)  // 10 PM
{
    // Put people at home to sleep
}
```

### Daily Cycle Visualization

```
Hour 0-6:   Night     (Dark, people sleeping)
Hour 6-12:  Morning   (Work begins, bright)
Hour 12-18: Afternoon (Peak activity)
Hour 18-22: Evening   (Going home, dimming)
Hour 22-24: Night     (Sleeping, dark)
```

---

## Day/Night Cycle

### Visual Changes

**Lighting:**
```csharp
public float GetDarknessFactor()
{
    if (Hour >= 6 && Hour < 18)
    {
        return 0.0f;  // Daytime - no darkness
    }
    else if (Hour >= 18 && Hour < 22)
    {
        // Evening - gradual darkening
        return (Hour - 18) / 4.0f;  // 0.0 ? 1.0
    }
    else if (Hour >= 22 || Hour < 4)
    {
        return 1.0f;  // Night - full darkness
    }
    else
    {
        // Dawn - gradual lightening (4-6 AM)
        return 1.0f - ((Hour - 4) / 2.0f);  // 1.0 ? 0.0
    }
}
```

**Visual Effects by Time:**
- **Night (6 PM - 6 AM)**: Darkness overlay, chimney smoke visible
- **Day (6 AM - 6 PM)**: Normal lighting
- **Dusk/Dawn**: Gradual transitions

### Time of Day Detection

```csharp
public TimeOfDay GetTimeOfDay()
{
    if (Hour >= 0 && Hour < 6)   return TimeOfDay.Night;
    if (Hour >= 6 && Hour < 12)  return TimeOfDay.Morning;
    if (Hour >= 12 && Hour < 18) return TimeOfDay.Afternoon;
    return TimeOfDay.Evening;
}

public bool IsNight()
{
    return Hour >= 18 || Hour < 6;
}

public bool IsWorkHours()
{
    return Hour >= WorkStartHour && Hour < WorkEndHour;
}

public bool IsSleepTime()
{
    return Hour >= SleepStartHour || Hour < WakeUpHour;
}
```

---

## Weather System

### Weather Class

```csharp
public class Weather
{
    public WeatherCondition Condition { get; private set; }
    public int Temperature { get; private set; }  // Celsius
    public double Precipitation { get; private set; }  // mm
    
    private Random _random;
    
    public Weather(int seed)
    {
        _random = new Random(seed);  // Deterministic
        Condition = WeatherCondition.Clear;
        Temperature = 20;
    }
}
```

### Weather Conditions

```csharp
public enum WeatherCondition
{
    Clear,     // Sunny, no effects
    Cloudy,    // Overcast, reduced visibility
    Rain,      // Rain particles, wet ground
    Snow,      // Snow particles (winter)
    Storm,     // Heavy rain, visual effects
    Blizzard   // Heavy snow (winter)
}
```

### Weather Updates

Weather updates once per day (at midnight):

```csharp
public void UpdateWeather(Season season, int dayOfSeason)
{
    // Temperature based on season
    Temperature = season switch
    {
        Season.Spring => _random.Next(10, 20),
        Season.Summer => _random.Next(20, 35),
        Season.Fall   => _random.Next(5, 15),
        Season.Winter => _random.Next(-15, 5),
        _ => 15
    };
    
    // Weather probability based on season and temperature
    int weatherRoll = _random.Next(100);
    
    if (season == Season.Winter && Temperature < 0)
    {
        // Winter weather (snow/blizzard likely)
        Condition = weatherRoll switch
        {
            < 20 => WeatherCondition.Blizzard,
            < 50 => WeatherCondition.Snow,
            < 70 => WeatherCondition.Cloudy,
            _ => WeatherCondition.Clear
        };
    }
    else if (season == Season.Spring || season == Season.Fall)
    {
        // Spring/Fall weather (rain common)
        Condition = weatherRoll switch
        {
            < 30 => WeatherCondition.Rain,
            < 60 => WeatherCondition.Cloudy,
            _ => WeatherCondition.Clear
        };
    }
    else
    {
        // Summer weather (mostly clear)
        Condition = weatherRoll switch
        {
            < 10 => WeatherCondition.Storm,
            < 20 => WeatherCondition.Rain,
            < 40 => WeatherCondition.Cloudy,
            _ => WeatherCondition.Clear
        };
    }
    
    // Calculate precipitation
    Precipitation = Condition switch
    {
        WeatherCondition.Rain => _random.NextDouble() * 10,
        WeatherCondition.Storm => _random.NextDouble() * 20,
        WeatherCondition.Snow => _random.NextDouble() * 5,
        WeatherCondition.Blizzard => _random.NextDouble() * 15,
        _ => 0
    };
}
```

### Weather Probabilities by Season

| Season | Clear | Cloudy | Rain | Snow | Storm/Blizzard |
|--------|-------|--------|------|------|----------------|
| Spring | 40% | 30% | 30% | - | - |
| Summer | 60% | 20% | 10% | - | 10% |
| Fall | 40% | 30% | 30% | - | - |
| Winter | 30% | 20% | - | 30% | 20% |

### Gameplay Effects

```csharp
public bool IsWorkable()
{
    return Condition != WeatherCondition.Storm 
        && Condition != WeatherCondition.Blizzard;
}
```

**Weather impacts:**
- **Storm/Blizzard**: Outdoor work impossible
- **Rain/Snow**: Visual particle effects
- **Temperature**: Affects resource consumption (future)

---

## Visual Integration

### Particle Effects

Weather conditions trigger particle systems:

```csharp
// In GameRenderer
switch (weather.Condition)
{
    case WeatherCondition.Rain:
    case WeatherCondition.Storm:
        _particleSystem.EmitWeatherParticles(
            ParticleType.Rain, 
            screenWidth, 
            screenHeight, 
            camera
        );
        break;
        
    case WeatherCondition.Snow:
    case WeatherCondition.Blizzard:
        _particleSystem.EmitWeatherParticles(
            ParticleType.Snow, 
            screenWidth, 
            screenHeight, 
            camera
        );
        break;
}
```

### Lighting Overlay

Night darkness is rendered as a semi-transparent overlay:

```csharp
float darkness = gameTime.GetDarknessFactor();
if (darkness > 0)
{
    var overlay = new Color(0, 0, 0, (byte)(darkness * 128));
    Raylib.DrawRectangle(0, 0, screenWidth, screenHeight, overlay);
}
```

---

## Time Scale Control

### Adjusting Game Speed

Time can be sped up or slowed down via time scale:

```csharp
// In game loop
float timeScale = 1.0f;  // Normal speed

// Speed up
timeScale = 2.0f;  // 2x speed (2 hours per second)
timeScale = 4.0f;  // 4x speed (4 hours per second)

// Slow down
timeScale = 0.5f;  // Half speed (30 minutes per second)

// Pause
timeScale = 0.0f;  // Time frozen
```

**Implementation:**
```csharp
// In GameLoopService
float _timeAccumulator = 0f;

void Update(float deltaTime)
{
    _timeAccumulator += deltaTime * timeScale;
    
    while (_timeAccumulator >= tickDuration)
    {
        engine.SimulateTick();
        _timeAccumulator -= tickDuration;
    }
}
```

---

## Initialization

### Default Starting Time

```csharp
var gameTime = new GameTime
{
    Hour = 6,              // Start at 6 AM (morning)
    Day = 1,               // First day
    DayOfSeason = 1,       // First day of spring
    Year = 1,              // Year 1
    CurrentSeason = Season.Spring
};

var weather = new Weather(seed: 12345);
weather.UpdateWeather(Season.Spring, 1);
```

---

## UI Display

### Time Display Format

```csharp
public string GetTimeString()
{
    return $"Year {Year}, {CurrentSeason} Day {DayOfSeason}, Hour {Hour}:00";
}

// Example: "Year 1, Spring Day 5, Hour 14:00"
```

### Status Bar

```
Year 1 | Spring Day 5 | ? Clear | Temp: 18°C
```

**Weather Icons:**
- ? Clear
- ? Cloudy
- ?? Rain
- ? Snow
- ? Storm
- ?? Blizzard

---

## Save/Load

Both GameTime and Weather are fully serializable:

```csharp
[Serializable]
public class SaveData
{
    public GameTime Time { get; set; }
    public Weather Weather { get; set; }
    // ... other state
}
```

**Preserved on save:**
- Current hour, day, season, year
- Weather condition, temperature, precipitation
- Random seed for deterministic weather generation

---

## Performance Considerations

### Efficiency

- Time advancement: O(1) - simple arithmetic
- Weather update: O(1) - called once per day
- No performance impact on game loop

### Determinism

**Critical for multiplayer/replays:**
- Weather uses seeded Random instance
- Time progression is deterministic
- No reliance on real-world time (`DateTime.Now`)

---

## Future Enhancements

### Planned Features

1. **Month/Calendar System**
   - Named months (not just day numbers)
   - Holidays and festivals
   - Season transition events

2. **Weather Forecasting**
   - 3-day weather prediction
   - UI element showing forecast
   - Strategic planning opportunities

3. **Climate Zones**
   - Different regions with different weather patterns
   - Desert: hot, rarely rains
   - Coastal: moderate, frequent rain
   - Mountain: cold, snow common

4. **Temperature Effects**
   - Crop growth rate affected by temperature
   - People consume more resources in winter
   - Building heating requirements

5. **Natural Disasters**
   - Droughts (multiple days without rain)
   - Floods (excessive rain)
   - Harsh winters (extended blizzards)

---

## Related Documentation

- [Game Engine](GAME_ENGINE.md) - How time integrates with simulation
- [Visual Enhancements](../../Rendering/VISUAL_ENHANCEMENTS.md) - Weather particle effects
- [People & Families](../../EntitiesAndBuildings/PEOPLE_AND_FAMILIES.md) - Daily routines

---

## Example Usage

### Check if Work Hours

```csharp
if (gameTime.IsWorkHours())
{
    // Send people to work
}
```

### Display Weather

```csharp
string weatherText = weather.Condition switch
{
    WeatherCondition.Clear => "? Clear",
    WeatherCondition.Cloudy => "? Cloudy",
    WeatherCondition.Rain => "?? Rain",
    WeatherCondition.Snow => "? Snow",
    WeatherCondition.Storm => "? Storm",
    WeatherCondition.Blizzard => "?? Blizzard",
    _ => "?"
};

DrawText($"{weatherText} | Temp: {weather.Temperature}°C");
```

### Check Season for Gameplay

```csharp
float growthRate = gameTime.CurrentSeason switch
{
    Season.Spring => 1.0f,
    Season.Summer => 1.5f,  // Faster growth
    Season.Fall => 0.8f,
    Season.Winter => 0.2f,  // Very slow growth
};
```

---

**Maintained By:** VillageBuilder Development Team  
**Last Updated:** 2024-01-XX
