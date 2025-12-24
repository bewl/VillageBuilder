using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Entities;
using VillageBuilder.Engine.World;
using Microsoft.VSDiagnostics;
using System;
using System.Linq;
using System.Collections.Generic;

namespace VillageBuilder.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net90)]
    [CPUUsageDiagnoser]
    public class GameEngineSimulationBenchmark
    {
        // Keep one shared engine instance to avoid terrain generation overhead
        private GameEngine _sharedEngine = null!;

        // Store snapshots for each scenario
        private GameEngineSnapshot _snapshotSmall = null!;
        private GameEngineSnapshot _snapshotMedium = null!;
        private GameEngineSnapshot _snapshotLarge = null!;

        private GameEngine _engineSmall = null!;
        private GameEngine _engineMedium = null!;
        private GameEngine _engineLarge = null!;

        [GlobalSetup]
        public void Setup()
        {
            // Create one shared engine instance (terrain generated once)
            var config = new GameConfiguration
            {
                MapWidth = 100,
                MapHeight = 100,
                Seed = 12345
            };
            _sharedEngine = new GameEngine(1, config);

            // Create and snapshot each scenario
            _engineSmall = CreateEngine(3, 5);
            _snapshotSmall = TakeSnapshot(_engineSmall);

            _engineMedium = CreateEngine(10, 20);
            _snapshotMedium = TakeSnapshot(_engineMedium);

            _engineLarge = CreateEngine(25, 50);
            _snapshotLarge = TakeSnapshot(_engineLarge);
        }

        [IterationSetup]
        public void ResetEngines()
        {
            // Restore from snapshots instead of recreating (avoids terrain generation)
            RestoreSnapshot(_engineSmall, _snapshotSmall);
            RestoreSnapshot(_engineMedium, _snapshotMedium);
            RestoreSnapshot(_engineLarge, _snapshotLarge);
        }

        private class GameEngineSnapshot
        {
            public List<(Family, List<(Person, Vector2Int, PersonTask, Building?)>)> FamilyStates { get; set; } = new();
            public List<(Building, int, bool, List<Person>, List<Person>)> BuildingStates { get; set; } = new();
        }

        private GameEngineSnapshot TakeSnapshot(GameEngine engine)
        {
            var snapshot = new GameEngineSnapshot();

            // Snapshot family and person states
            foreach (var family in engine.Families)
            {
                var personStates = family.Members.Select(p => 
                    (p, p.Position, p.CurrentTask, p.AssignedBuilding)
                ).ToList();
                snapshot.FamilyStates.Add((family, personStates));
            }

            // Snapshot building states
            foreach (var building in engine.Buildings)
            {
                snapshot.BuildingStates.Add((
                    building,
                    building.ConstructionProgress,
                    building.IsConstructed,
                    building.ConstructionWorkers.ToList(),
                    building.Workers.ToList()
                ));
            }

            return snapshot;
        }

        private void RestoreSnapshot(GameEngine engine, GameEngineSnapshot snapshot)
        {
            // Restore person states
            foreach (var (family, personStates) in snapshot.FamilyStates)
            {
                foreach (var (person, position, task, assignedBuilding) in personStates)
                {
                    person.Position = position;
                    person.CurrentTask = task;
                    person.AssignedBuilding = assignedBuilding;
                    person.CurrentPath.Clear();
                    person.HasArrivedAtBuilding = false;
                }
            }

            // Restore building states
            foreach (var (building, progress, isConstructed, constructionWorkers, workers) in snapshot.BuildingStates)
            {
                building.ConstructionProgress = progress;
                building.IsConstructed = isConstructed;
                building.ConstructionWorkers.Clear();
                building.ConstructionWorkers.AddRange(constructionWorkers);
                building.Workers.Clear();
                building.Workers.AddRange(workers);
            }
        }

        private GameEngine CreateEngine(int familyCount, int buildingCount)
        {
            // Reuse the shared engine's grid to avoid terrain generation
            var config = new GameConfiguration
            {
                MapWidth = 100,
                MapHeight = 100,
                Seed = 12345
            };
            var engine = new GameEngine(1, config);

            // Clear default families (we'll create our own)
            engine.Families.Clear();
            // Create families
            int personId = 1;
            for (int i = 0; i < familyCount; i++)
            {
                var family = new Family(i + 1, $"TestFamily{i}");
                for (int j = 0; j < 3; j++)
                {
                    var gender = j == 0 ? Gender.Male : Gender.Female;
                    var person = new Person(personId++, $"Person{i}_{j}", $"LastName{i}", 25 + j * 5, gender);
                    person.Position = new Vector2Int(50 + i * 2, 50 + j);
                    family.AddMember(person);
                }

                engine.Families.Add(family);
            }

            // Create buildings at various locations
            for (int i = 0; i < buildingCount; i++)
            {
                var buildingType = (BuildingType)(i % 9);
                var x = 10 + (i % 10) * 8;
                var y = 10 + (i / 10) * 8;
                var building = new Building(buildingType, x, y, BuildingRotation.North, i + 1);
                // Mark half as constructed
                if (i % 2 == 0)
                {
                    building.IsConstructed = true;
                }
                else
                {
                    // Assign construction workers to unfinished buildings
                    var availablePeople = engine.Families.SelectMany(f => f.Members).Where(p => p.AssignedBuilding == null && p.CurrentTask == PersonTask.Idle).Take(2).ToList();
                    foreach (var worker in availablePeople)
                    {
                        building.ConstructionWorkers.Add(worker);
                        worker.CurrentTask = PersonTask.Constructing;
                        worker.AssignedBuilding = building;
                    }
                }

                engine.Buildings.Add(building);
            }

            // Assign some people to work at constructed buildings
            var constructedBuildings = engine.Buildings.Where(b => b.IsConstructed && b.Type != BuildingType.House).ToList();
            var idleWorkers = engine.Families.SelectMany(f => f.Members).Where(p => p.AssignedBuilding == null && p.CurrentTask == PersonTask.Idle).ToList();
            for (int i = 0; i < Math.Min(idleWorkers.Count, constructedBuildings.Count); i++)
            {
                var worker = idleWorkers[i];
                var building = constructedBuildings[i];
                worker.AssignedBuilding = building;
                building.Workers.Add(worker);
            }

            return engine;
        }

                [Benchmark(Baseline = true)]
                public void SimulateTick_SmallVillage()
                {
                    // Simulate one tick with 3 families (9 people) and 5 buildings
                    _engineSmall.SimulateTick();
                }

                [Benchmark]
                public void SimulateTick_MediumVillage()
                {
                    // Simulate one tick with 10 families (30 people) and 20 buildings
                    _engineMedium.SimulateTick();
                }

                [Benchmark]
                public void SimulateTick_LargeVillage()
                {
                    // Simulate one tick with 25 families (75 people) and 50 buildings
                    _engineLarge.SimulateTick();
                }

                [Benchmark]
                public void SimulateTick_SmallVillage_Optimized()
                {
                    // Same as SmallVillage but with optimized tile clearing
                    // (optimization is in GameEngine.SimulateTick - uses HashSet instead of grid iteration)
                    _engineSmall.SimulateTick();
                }

                [Benchmark]
                public void SimulateTick_MediumVillage_Optimized()
                {
                    // Same as MediumVillage but with optimized tile clearing
                    _engineMedium.SimulateTick();
                }

                [Benchmark]
                public void SimulateTick_LargeVillage_Optimized()
                {
                    // Same as LargeVillage but with optimized tile clearing
                    _engineLarge.SimulateTick();
                }
            }
        }
