# People & Families System

## Overview

The **Person** and **Family** systems are the core of VillageBuilder's simulation. People have stats, daily routines, relationships, and autonomous behavior. Families group related people and manage housing/work assignments together.

---

## Person Class

### Core Structure

```csharp
public class Person
{
    // Identity
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public bool IsAlive { get; set; } = true;
    
    // Relationships
    public Person? Spouse { get; set; }
    public List<Person> Children { get; set; } = new();
    public Family? Family { get; set; }
    
    // Position & Navigation
    public Vector2Int Position { get; set; }
    public Vector2Int? TargetPosition { get; set; }
    public List<Vector2Int> CurrentPath { get; set; } = new();
    public int PathIndex { get; set; }
    
    // Stats (0-100)
    public int Energy { get; set; } = 100;
    public int Hunger { get; set; } = 0;
    public int Health { get; set; } = 100;
    
    // Work & Tasks
    public PersonTask CurrentTask { get; set; } = PersonTask.Idle;
    public Building? AssignedBuilding { get; set; }
    public Building? HomeBuilding { get; set; }
    
    // History tracking
    public PersonHistory History { get; set; } = new();
}
```

### Gender Enum

```csharp
public enum Gender
{
    Male,
    Female,
    NonBinary  // Future support
}
```

---

## Stats System

### Energy (0-100)

**What it represents:** Physical and mental stamina

**Consumption:**
- Walking: -0.1 per tick
- Working: -0.2 per tick  
- Constructing: -0.3 per tick

**Recovery:**
- Sleeping: +1.0 per tick
- Resting at home: +0.5 per tick

**Effects:**
- <30: Low energy, slower movement
- <10: Cannot work
- 0: Exhaustion, health decreases

**Code:**
```csharp
public void UpdateEnergy(float deltaTime)
{
    if (CurrentTask == PersonTask.Sleeping)
    {
        Energy = Math.Min(100, Energy + 1);
    }
    else if (CurrentTask == PersonTask.Working)
    {
        Energy = Math.Max(0, Energy - 1);
    }
    else if (CurrentTask == PersonTask.Constructing)
    {
        Energy = Math.Max(0, Energy - 1);
    }
}
```

### Hunger (0-100)

**What it represents:** Need for food (higher = more hungry)

**Increase:**
- Passive: +0.1 per tick
- Working: +0.2 per tick

**Decrease:**
- Eating: -20 per meal

**Effects:**
- >60: Hungry, work efficiency reduced
- >80: Very hungry, health decreases
- >95: Starving, rapid health loss

**Code:**
```csharp
public void UpdateHunger(float deltaTime)
{
    // Hunger increases over time
    Hunger = Math.Min(100, Hunger + 1);
    
    // If very hungry, health decreases
    if (Hunger > 80)
    {
        Health = Math.Max(0, Health - 1);
    }
}
```

### Health (0-100)

**What it represents:** Physical well-being

**Decrease:**
- Hunger >80: -0.5 per tick
- Energy = 0: -0.3 per tick
- Exposure (future): -0.2 per tick

**Recovery:**
- Resting: +0.2 per tick
- Medicine (future): +5 per dose

**Effects:**
- <50: Sick, cannot work
- <20: Bedridden
- 0: Death

---

## PersonTask Enum

### Task States

```csharp
public enum PersonTask
{
    Idle,                   // Standing around
    Sleeping,              // In bed at home
    Resting,               // Relaxing at home
    GoingHome,             // Walking to house
    GoingToWork,           // Walking to workplace
    WorkingAtBuilding,     // Performing job
    Constructing,          // Building construction
    MovingToLocation,      // Walking to arbitrary location
    Eating                 // Consuming food (future)
}
```

### Task Transitions

```
         [6 AM Wake Up]
Sleeping ?????????????? Idle
                          ?
         [6 AM Work Start]?
                          ?
                      GoingToWork
                          ?
         [Arrive at work] ?
                          ?
                   WorkingAtBuilding
                          ?
         [6 PM Work End]  ?
                          ?
                      GoingHome
                          ?
         [Arrive home]    ?
                          ?
                       Resting
                          ?
         [10 PM Sleep]    ?
                          ?
                       Sleeping
```

---

## Movement & Pathfinding

### Position System

**Vector2Int:**
```csharp
public struct Vector2Int
{
    public int X { get; set; }
    public int Y { get; set; }
}
```

**Current position:** Where person actually is  
**Target position:** Where person is trying to reach  
**Path:** List of tiles to walk through

### Movement Execution

```csharp
public bool TryMoveAlongPath(Grid grid)
{
    if (CurrentPath.Count == 0 || PathIndex >= CurrentPath.Count)
    {
        TargetPosition = null;
        return false; // Path complete
    }

    var target = CurrentPath[PathIndex];
    
    // Check if already at target
    if (Position.X == target.X && Position.Y == target.Y)
    {
        PathIndex++;
        if (PathIndex < CurrentPath.Count)
        {
            TargetPosition = CurrentPath[PathIndex];
            return true;
        }
        else
        {
            TargetPosition = null;
            return false; // Path complete
        }
    }
    
    // Move to target (no collision - people can pass through each other)
    var oldPosition = Position;
    Position = target;

    // Update tile registrations
    var oldTile = grid.GetTile(oldPosition.X, oldPosition.Y);
    var newTile = grid.GetTile(target.X, target.Y);

    if (oldTile != null && oldTile.PeopleOnTile.Contains(this))
    {
        oldTile.PeopleOnTile.Remove(this);
    }

    if (newTile != null && !newTile.PeopleOnTile.Contains(this))
    {
        newTile.PeopleOnTile.Add(this);
    }

    PathIndex++;
    if (PathIndex < CurrentTick.Count)
    {
        TargetPosition = CurrentPath[PathIndex];
    }
    else
    {
        TargetPosition = null;
    }
    
    return true; // Still moving
}
```

### Pathfinding Integration

```csharp
public void SetPath(List<Vector2Int> path)
{
    CurrentPath = path;
    PathIndex = 0;
    
    if (path.Count > 0)
    {
        TargetPosition = path[0];
    }
}
```

**See:** [PATHFINDING.md](../../WorldAndSimulation/PATHFINDING.md) for A* algorithm details

---

## Daily Routines

### Autonomous Behavior

People follow daily schedules automatically:

**6 AM - Wake Up**
```csharp
public void WakeUp()
{
    CurrentTask = PersonTask.Idle;
    Energy = Math.Min(100, Energy + 50); // Partial energy restore
}
```

**6 AM - Go to Work** (if has job)
```csharp
public void SendToWork(Building workplace, Grid grid)
{
    CurrentTask = PersonTask.GoingToWork;
    
    var path = Pathfinding.FindPath(
        Position, 
        new Vector2Int(workplace.X, workplace.Y), 
        grid
    );
    
    SetPath(path);
}
```

**6 PM - Go Home**
```csharp
public void SendHome(Grid grid)
{
    CurrentTask = PersonTask.GoingHome;
    
    if (HomeBuilding != null)
    {
        var path = Pathfinding.FindPath(
            Position,
            new Vector2Int(HomeBuilding.X, HomeBuilding.Y),
            grid
        );
        
        SetPath(path);
    }
}
```

**10 PM - Sleep**
```csharp
public void GoToSleep()
{
    if (IsAtHome())
    {
        CurrentTask = PersonTask.Sleeping;
        Energy = Math.Min(100, Energy + 20); // Start restoring
    }
}
```

### Location Checks

```csharp
public bool IsAtHome()
{
    if (HomeBuilding == null) return false;
    
    return Position.X == HomeBuilding.X && 
           Position.Y == HomeBuilding.Y;
}

public bool IsAtWork()
{
    if (AssignedBuilding == null) return false;
    
    return Position.X == AssignedBuilding.X && 
           Position.Y == AssignedBuilding.Y;
}
```

---

## Family Class

### Structure

```csharp
public class Family
{
    public int Id { get; set; }
    public string FamilyName { get; set; } = string.Empty;
    public List<Person> Members { get; set; } = new();
    
    // Housing
    public Vector2Int? HomePosition { get; set; }
    public Building? HomeBuilding { get; set; }
    
    // Helpers
    public Person? GetHead() => Members.FirstOrDefault(p => p.Age >= 18);
    
    public int GetAdultCount() => Members.Count(p => p.Age >= 18);
    
    public int GetChildCount() => Members.Count(p => p.Age < 18);
}
```

### Family Relationships

**Nuclear Family Structure:**
- 2 adults (married couple)
- 0-4 children
- Children age and eventually leave (future)

**Example:**
```
Smith Family (ID: 1)
??? John Smith (35, Male) - Father, Head
??? Mary Smith (33, Female) - Mother
??? Alice Smith (10, Female) - Daughter
??? Bob Smith (7, Male) - Son
```

### Family-Wide Operations

**Assign Home:**
```csharp
public void AssignHome(Building house)
{
    HomeBuilding = house;
    HomePosition = new Vector2Int(house.X, house.Y);
    
    foreach (var member in Members)
    {
        member.HomeBuilding = house;
        house.Residents.Add(member);
    }
}
```

**Assign Job:**
```csharp
public void AssignJob(Building workplace)
{
    var adults = Members.Where(p => p.Age >= 18 && p.IsAlive);
    
    foreach (var adult in adults)
    {
        adult.AssignToBuilding(workplace);
    }
}
```

---

## Name Generation

### NameGenerator Class

```csharp
public static class NameGenerator
{
    private static readonly string[] FirstNamesMale = 
    {
        "John", "James", "William", "Thomas", "Edward",
        "Henry", "Charles", "George", "Robert", "Samuel"
    };
    
    private static readonly string[] FirstNamesFemale = 
    {
        "Mary", "Elizabeth", "Margaret", "Sarah", "Emma",
        "Alice", "Catherine", "Jane", "Anne", "Emily"
    };
    
    private static readonly string[] LastNames = 
    {
        "Smith", "Johnson", "Williams", "Brown", "Jones",
        "Miller", "Davis", "Wilson", "Moore", "Taylor",
        "Anderson", "Thomas", "Jackson", "White", "Harris"
    };
    
    public static string GenerateFirstName(Gender gender, Random random)
    {
        return gender == Gender.Male
            ? FirstNamesMale[random.Next(FirstNamesMale.Length)]
            : FirstNamesFemale[random.Next(FirstNamesFemale.Length)];
    }
    
    public static string GenerateLastName(Random random)
    {
        return LastNames[random.Next(LastNames.Length)];
    }
}
```

### Creating a Person

```csharp
public static Person CreatePerson(Gender gender, int age, Random random)
{
    return new Person
    {
        Id = GetNextId(),
        FirstName = NameGenerator.GenerateFirstName(gender, random),
        LastName = NameGenerator.GenerateLastName(random),
        Age = age,
        Gender = gender,
        Energy = 100,
        Hunger = 0,
        Health = 100,
        IsAlive = true
    };
}
```

### Creating a Family

```csharp
public static Family CreateFamily(string lastName, Random random)
{
    var family = new Family
    {
        Id = GetNextId(),
        FamilyName = lastName
    };
    
    // Create parents
    var father = CreatePerson(Gender.Male, 25 + random.Next(15), random);
    father.LastName = lastName;
    father.Family = family;
    
    var mother = CreatePerson(Gender.Female, 25 + random.Next(15), random);
    mother.LastName = lastName;
    mother.Family = family;
    
    // Marry them
    father.Spouse = mother;
    mother.Spouse = father;
    
    family.Members.Add(father);
    family.Members.Add(mother);
    
    // Create children
    int childCount = random.Next(0, 4);
    for (int i = 0; i < childCount; i++)
    {
        var gender = random.Next(2) == 0 ? Gender.Male : Gender.Female;
        var child = CreatePerson(gender, random.Next(1, 16), random);
        child.LastName = lastName;
        child.Family = family;
        
        father.Children.Add(child);
        mother.Children.Add(child);
        
        family.Members.Add(child);
    }
    
    return family;
}
```

---

## Aging System

### Age Progression

Currently people don't age, but future system:

```csharp
public void Age()
{
    Age++;
    
    // Milestone events
    if (Age == 18)
    {
        EventLog.Instance.AddMessage(
            $"{FirstName} {LastName} has come of age!",
            LogLevel.Success
        );
    }
    
    // Death from old age
    if (Age > 70)
    {
        int deathChance = (Age - 70) * 5; // 5% per year over 70
        if (Random.Shared.Next(100) < deathChance)
        {
            Die();
        }
    }
}
```

### Death System

```csharp
public void Die()
{
    IsAlive = false;
    Health = 0;
    CurrentTask = PersonTask.Idle;
    
    // Remove from workplace
    if (AssignedBuilding != null)
    {
        AssignedBuilding.Workers.Remove(this);
        AssignedBuilding.ConstructionWorkers.Remove(this);
    }
    
    EventLog.Instance.AddMessage(
        $"{FirstName} {LastName} has died.",
        LogLevel.Error
    );
}
```

---

## Person History

### PersonHistory Class

```csharp
public class PersonHistory
{
    public DateTime BirthDate { get; set; }
    public DateTime? DeathDate { get; set; }
    public List<string> Events { get; set; } = new();
    
    public void AddEvent(string eventDescription)
    {
        Events.Add($"[Tick {CurrentTick}] {eventDescription}");
    }
}
```

**Example events:**
- "Born in Smith family"
- "Assigned to work at Farm"
- "Moved to house at (50, 50)"
- "Married to Mary Smith"
- "Child Alice born"

---

## Rendering

### Person Glyph

People are rendered as smiley faces:

```csharp
public string GetGlyph()
{
    return Gender == Gender.Male ? "?" : "?";
}

public Color GetColor()
{
    return CurrentTask switch
    {
        PersonTask.Sleeping => new Color(100, 100, 150, 255),
        PersonTask.Working => new Color(150, 255, 150, 255),
        PersonTask.Constructing => new Color(255, 200, 100, 255),
        _ => Color.White
    };
}
```

**Visual states:**
- White: Idle/walking
- Light blue: Sleeping
- Green: Working
- Yellow: Constructing

---

## Work Assignment

### Assigning to Building

```csharp
public void AssignToBuilding(Building building, bool logEvent = true)
{
    // Remove from previous job
    if (AssignedBuilding != null)
    {
        AssignedBuilding.Workers.Remove(this);
    }
    
    // Assign to new building
    AssignedBuilding = building;
    building.Workers.Add(this);
    
    if (logEvent)
    {
        EventLog.Instance.AddMessage(
            $"{FirstName} {LastName} assigned to work at {building.Type}",
            LogLevel.Info
        );
    }
}
```

### Unassigning

```csharp
public void UnassignFromBuilding()
{
    if (AssignedBuilding != null)
    {
        AssignedBuilding.Workers.Remove(this);
        AssignedBuilding = null;
        CurrentTask = PersonTask.Idle;
    }
}
```

---

## AI Behavior

### Decision Making (Future)

Currently people follow fixed schedules, but future AI system:

```csharp
public void UpdateAI(GameEngine engine)
{
    // Check needs
    if (Hunger > 80 && HasFoodAvailable())
    {
        GoEat();
    }
    else if (Energy < 20 && !IsWorkHours())
    {
        GoRest();
    }
    else if (IsWorkHours() && AssignedBuilding != null)
    {
        if (!IsAtWork())
        {
            GoToWork();
        }
    }
    else if (!IsWorkHours() && !IsAtHome())
    {
        GoHome();
    }
}
```

---

## Serialization

### Save/Load

Person state is fully serializable:

```csharp
[Serializable]
public class PersonData
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public Gender Gender { get; set; }
    
    public Vector2Int Position { get; set; }
    public int Energy, Hunger, Health { get; set; }
    public PersonTask CurrentTask { get; set; }
    
    public int? SpouseId { get; set; }
    public List<int> ChildrenIds { get; set; }
    public int? FamilyId { get; set; }
    public int? AssignedBuildingId { get; set; }
    public int? HomeBuildingId { get; set; }
}
```

**References saved as IDs** to avoid circular dependencies.

---

## Performance Considerations

### Person Update Optimization

```csharp
// DON'T: Update every person every tick
foreach (var person in allPeople)
{
    person.UpdateStats();  // Expensive!
}

// DO: Batch updates, only when needed
if (tick % 60 == 0)  // Once per second
{
    foreach (var person in allPeople)
    {
        person.UpdateStats();
    }
}
```

### Tile Registration

**See:** [PERFORMANCE_OPTIMIZATIONS.md](../../Performance/PERFORMANCE_OPTIMIZATIONS.md)

Only clear/register occupied tiles, not all 10,000 tiles.

---

## Future Enhancements

### Planned Features

1. **Aging & Lifecycle**
   - People age over years
   - Children grow into adults
   - Old age and natural death
   - Birth of new children

2. **Needs System**
   - Food consumption (reduces hunger)
   - Water requirement
   - Shelter/warmth in winter
   - Social needs (happiness)

3. **Skills & Experience**
   - Job-specific skills improve over time
   - Experienced workers more efficient
   - Skill requirements for advanced buildings

4. **Personalities**
   - Traits (Hard-worker, Lazy, Social, etc.)
   - Preferences (favorite foods, dislike cold, etc.)
   - Moods affected by life events

5. **Complex Relationships**
   - Friendships
   - Rivalries
   - Extended family (grandparents, aunts/uncles)
   - Romance and new families forming

6. **Health System**
   - Diseases and illnesses
   - Injuries from accidents
   - Medical treatment
   - Recovery time

---

## Example Usage

### Creating Starting Population

```csharp
// Create initial families
for (int i = 0; i < 3; i++)
{
    var family = FamilyGenerator.CreateFamily(
        NameGenerator.GenerateLastName(random),
        random
    );
    
    engine.Families.Add(family);
    
    // Place near town center
    foreach (var member in family.Members)
    {
        member.Position = new Vector2Int(50, 50);
    }
}
```

### Checking Person Status

```csharp
// In UI code
if (selectedPerson != null)
{
    Console.WriteLine($"Name: {selectedPerson.FullName}");
    Console.WriteLine($"Age: {selectedPerson.Age}");
    Console.WriteLine($"Task: {selectedPerson.CurrentTask}");
    Console.WriteLine($"Energy: {selectedPerson.Energy}/100");
    Console.WriteLine($"Hunger: {selectedPerson.Hunger}/100");
    Console.WriteLine($"Health: {selectedPerson.Health}/100");
}
```

---

## Related Documentation

- [Game Engine](../../CoreSystems/GAME_ENGINE.md) - How people integrate with simulation
- [Building System](BUILDING_SYSTEM.md) - Work assignments
- [Pathfinding](../../WorldAndSimulation/PATHFINDING.md) - Movement system
- [Construction System](CONSTRUCTION_SYSTEM.md) - Construction workers

---

**Maintained By:** VillageBuilder Development Team  
**Last Updated:** 2024-01-XX
