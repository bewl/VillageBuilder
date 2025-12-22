// Example: How to use the new Day/Night Cycle features

using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Commands.FamilyCommands;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Entities;

namespace VillageBuilder.Game.Examples
{
    /// <summary>
    /// Example usage of Day/Night Cycle features
    /// This is a reference guide, not executable code
    /// </summary>
    public static class DayNightCycleUsageExamples
    {
        // ============================================
        // 1. Check Time of Day
        // ============================================
        public static void CheckTimeOfDay(GameEngine engine)
        {
            // Check what time it is
            if (engine.Time.IsNight())
            {
                System.Console.WriteLine("It's nighttime!");
            }

            if (engine.Time.IsWorkHours())
            {
                System.Console.WriteLine("People should be working");
            }

            if (engine.Time.IsSleepTime())
            {
                System.Console.WriteLine("People should be sleeping");
            }

            // Get time of day
            var timeOfDay = engine.Time.GetTimeOfDay();
            System.Console.WriteLine($"Time of day: {timeOfDay}"); // Morning, Afternoon, Evening, Night

            // Get darkness for visual effects
            float darkness = engine.Time.GetDarknessFactor(); // 0.0 to 1.0
            System.Console.WriteLine($"Darkness: {darkness}");
        }

        // ============================================
        // 2. Assign Families to Houses
        // ============================================
        public static void AssignFamilyToHouse(GameEngine engine)
        {
            // After building a house, assign a family to it
            var family = engine.Families[0];
            var house = engine.Buildings.FirstOrDefault(b => b.Type == BuildingType.House);

            if (house != null && house.IsConstructed)
            {
                var command = new AssignFamilyHomeCommand(
                    playerId: 1,
                    targetTick: engine.CurrentTick,
                    familyId: family.Id,
                    buildingId: house.Id
                );

                engine.SubmitCommand(command);
            }
        }

        // ============================================
        // 3. Check Person Status
        // ============================================
        public static void CheckPersonStatus(GameEngine engine)
        {
            var family = engine.Families[0];
            var person = family.Members[0];

            // Check if person is sleeping
            if (person.IsSleeping)
            {
                System.Console.WriteLine($"{person.FirstName} is sleeping");
            }

            // Check person's location
            if (person.IsAtHome())
            {
                System.Console.WriteLine($"{person.FirstName} is at home");
            }

            if (person.IsAtWork())
            {
                System.Console.WriteLine($"{person.FirstName} is at work");
            }

            // Check person's task
            switch (person.CurrentTask)
            {
                case PersonTask.Sleeping:
                    System.Console.WriteLine($"{person.FirstName} is sleeping");
                    break;
                case PersonTask.GoingHome:
                    System.Console.WriteLine($"{person.FirstName} is going home");
                    break;
                case PersonTask.GoingToWork:
                    System.Console.WriteLine($"{person.FirstName} is going to work");
                    break;
                case PersonTask.WorkingAtBuilding:
                    System.Console.WriteLine($"{person.FirstName} is working");
                    break;
            }

            // Check energy (will be higher after sleeping)
            System.Console.WriteLine($"Energy: {person.Energy}/100");
        }

        // ============================================
        // 4. Check Building Occupancy
        // ============================================
        public static void CheckBuildingOccupancy(GameEngine engine)
        {
            var house = engine.Buildings.FirstOrDefault(b => b.Type == BuildingType.House);

            if (house != null)
            {
                // Check if house is occupied
                bool occupied = house.IsOccupied();
                System.Console.WriteLine($"House occupied: {occupied}");

                // Check if lights should show (for rendering)
                bool showLights = house.ShouldShowLights(engine.Time);
                System.Console.WriteLine($"Show lights: {showLights}");

                // Get residents
                System.Console.WriteLine($"Residents: {house.Residents.Count}");
                foreach (var resident in house.Residents)
                {
                    System.Console.WriteLine($"  - {resident.FullName}");
                }
            }
        }

        // ============================================
        // 5. Observe Daily Routines (Automatic)
        // ============================================
        public static void ObserveDailyRoutines(GameEngine engine)
        {
            // The game engine automatically handles daily routines:
            // - 6 AM: People wake up and go to work
            // - 6 PM: People return home
            // - 10 PM: People go to sleep

            // Just run the simulation and watch!
            for (int i = 0; i < 100; i++)
            {
                engine.SimulateTick();

                // Check for interesting events
                if (engine.Time.Hour == GameTime.WorkStartHour)
                {
                    System.Console.WriteLine("Work day begins!");
                }

                if (engine.Time.Hour == GameTime.WorkEndHour)
                {
                    System.Console.WriteLine("Work day ends!");
                }

                if (engine.Time.Hour == GameTime.SleepStartHour)
                {
                    System.Console.WriteLine("Time for sleep!");
                }
            }
        }

        // ============================================
        // 6. Visual Effects (in MapRenderer)
        // ============================================
        // The MapRenderer automatically applies visual effects:
        // - Darkens all tiles at night
        // - Adds warm glow to occupied buildings
        // - Shows time of day icon in status bar
        //
        // No additional code needed - just render normally:
        // mapRenderer.Render(engine, camera, selectionManager);
    }
}
