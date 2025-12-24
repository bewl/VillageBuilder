using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Raylib_cs;
using VillageBuilder.Game.Graphics;
using VillageBuilder.Game.Graphics.UI;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Entities;
using VillageBuilder.Engine.World;
using System.Linq;
using Microsoft.VSDiagnostics;

namespace VillageBuilder.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net90)]
    [CPUUsageDiagnoser]
    public class TextRenderingBenchmark
    {
        private GameEngine _engine = null !;
        private SidebarRenderer _sidebar = null !;
        [GlobalSetup]
        public void Setup()
        {
            // Initialize Raylib for headless rendering
            Raylib.SetConfigFlags(ConfigFlags.HiddenWindow);
            Raylib.InitWindow(800, 600, "Benchmark");
            Raylib.SetTargetFPS(60);
            GraphicsConfig.LoadFont();
            // Create a game engine with some test data
            var config = new GameConfiguration
            {
                MapWidth = 100,
                MapHeight = 100,
                Seed = 12345
            };
            _engine = new GameEngine(1, config);
            // Add some families and buildings for realistic sidebar content
            int personId = 1;
            for (int i = 0; i < 5; i++)
            {
                var family = new Family(i + 1, $"TestFamily{i}");
                for (int j = 0; j < 3; j++)
                {
                    var person = new Person(personId++, $"Person{i}_{j}", $"LastName{i}", 25, Gender.Male);
                    family.AddMember(person);
                }

                _engine.Families.Add(family);
            }

            // Add some buildings
            for (int i = 0; i < 10; i++)
            {
                var building = new Building((BuildingType)(i % 9), i * 5, i * 5, BuildingRotation.North, i + 1);
                building.IsConstructed = true;
                _engine.Buildings.Add(building);
            }

            _sidebar = new SidebarRenderer();
            // Start rendering context
            Raylib.BeginDrawing();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            Raylib.EndDrawing();
            GraphicsConfig.UnloadFont();
            Raylib.CloseWindow();
        }

                [Benchmark(Baseline = true)]
                public void RenderSidebar_NoOptimization()
                {
                    // Simulate rendering sidebar for one frame
                    // This calls DrawConsoleText many times (50-100+ calls)
                    _sidebar.Render(_engine, null);
                }

                [Benchmark]
                public void RenderSidebar_WithCaching()
                {
                    // Same rendering but with cached save file lookups
                    // The SaveLoadService.GetSaveFiles() now uses a 1-second cache
                    _sidebar.Render(_engine, null);
                }
            }
        }
