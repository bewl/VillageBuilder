using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Entities;
using VillageBuilder.Engine.Resources;
using VillageBuilder.Engine.World;

namespace VillageBuilder.Engine.Core
{
    /// <summary>
    /// Manages saving and loading game state
    /// </summary>
    public class SaveLoadManager
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Save the current game state to a file
        /// </summary>
        public static void SaveGame(GameEngine engine, string filePath)
        {
            var gameState = ExtractGameState(engine);
            gameState.SavedAt = DateTime.Now;
            
            var json = JsonSerializer.Serialize(gameState, JsonOptions);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Load a game state from a file and create a new GameEngine
        /// </summary>
        public static GameEngine LoadGame(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var gameState = JsonSerializer.Deserialize<GameState>(json, JsonOptions);
            
            if (gameState == null)
                throw new InvalidDataException("Failed to deserialize game state");
            
            return RestoreGameState(gameState);
        }

        /// <summary>
        /// Extract complete game state from engine
        /// </summary>
        private static GameState ExtractGameState(GameEngine engine)
        {
            var state = new GameState
            {
                GameId = engine.GameId,
                Seed = engine.Configuration.Seed ?? 0,
                CurrentTick = engine.CurrentTick,
                NextBuildingId = GetNextBuildingId(engine),
                Configuration = engine.Configuration,
                
                // Time
                Time = new GameTimeState
                {
                    Hour = engine.Time.Hour,
                    Day = engine.Time.DayOfSeason,
                    Month = (int)engine.Time.CurrentSeason, // Store season as month
                    Year = engine.Time.Year,
                    CurrentSeason = engine.Time.CurrentSeason
                },

                // Weather
                Weather = new WeatherState
                {
                    CurrentWeather = engine.Weather.Condition,
                    Seed = engine.Configuration.Seed ?? 0
                },
                
                // Grid (just metadata, regenerate from seed)
                Grid = new GridState
                {
                    Width = engine.Grid.Width,
                    Height = engine.Grid.Height,
                    Seed = engine.Configuration.Seed ?? 0
                },
                
                // Resources
                VillageResources = ExtractResources(engine.VillageResources),
                
                // Families and People
                Families = engine.Families.Select(ExtractFamily).ToList(),
                
                // Buildings
                Buildings = engine.Buildings.Select(b => ExtractBuilding(b, engine)).ToList()
            };
            
            // Calculate next IDs for new entities
            state.NextPersonId = engine.Families
                .SelectMany(f => f.Members)
                .Select(p => p.Id)
                .DefaultIfEmpty(0)
                .Max() + 1;
                
            state.NextFamilyId = engine.Families
                .Select(f => f.Id)
                .DefaultIfEmpty(0)
                .Max() + 1;
            
            return state;
        }

        private static int GetNextBuildingId(GameEngine engine)
        {
            return engine.Buildings.Select(b => b.Id).DefaultIfEmpty(0).Max() + 1;
        }

        private static Dictionary<ResourceType, int> ExtractResources(ResourceInventory inventory)
        {
            var resources = new Dictionary<ResourceType, int>();
            foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
            {
                var amount = inventory.Get(resourceType);
                if (amount > 0)
                {
                    resources[resourceType] = amount;
                }
            }
            return resources;
        }

        private static FamilyState ExtractFamily(Family family)
        {
            return new FamilyState
            {
                Id = family.Id,
                FamilyName = family.FamilyName,
                HomePosition = family.HomePosition.HasValue 
                    ? new Vector2IntState(family.HomePosition.Value.X, family.HomePosition.Value.Y)
                    : null,
                Members = family.Members.Select(ExtractPerson).ToList()
            };
        }

        private static PersonState ExtractPerson(Person person)
        {
            return new PersonState
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Age = person.Age,
                Gender = person.Gender,
                
                SpouseId = person.Spouse?.Id,
                ChildIds = person.Children.Select(c => c.Id).ToList(),
                FamilyId = person.Family?.Id,
                
                Position = new Vector2IntState(person.Position.X, person.Position.Y),
                TargetPosition = person.TargetPosition.HasValue
                    ? new Vector2IntState(person.TargetPosition.Value.X, person.TargetPosition.Value.Y)
                    : null,
                CurrentPath = person.CurrentPath.Select(v => new Vector2IntState(v.X, v.Y)).ToList(),
                PathIndex = person.PathIndex,
                
                CurrentTask = person.CurrentTask,
                AssignedBuildingId = person.AssignedBuilding?.Id,
                HasArrivedAtBuilding = person.HasArrivedAtBuilding,
                HomeBuildingId = person.HomeBuilding?.Id,
                
                Energy = person.Energy,
                Hunger = person.Hunger,
                Health = person.Health,
                IsAlive = person.IsAlive,
                IsSleeping = person.IsSleeping,
                IsSick = person.IsSick,
                DaysSick = 0 // This property isn't publicly accessible, would need to add getter
            };
        }

        private static BuildingState ExtractBuilding(Building building, GameEngine engine)
        {
            return new BuildingState
            {
                Id = building.Id,
                Type = building.Type,
                X = building.X,
                Y = building.Y,
                Rotation = building.Rotation,
                
                IsConstructed = building.IsConstructed,
                ConstructionProgress = building.ConstructionProgress,
                ConstructionWorkRequired = building.ConstructionWorkRequired,
                
                WorkerIds = building.Workers.Select(w => w.Id).ToList(),
                ConstructionWorkerIds = building.ConstructionWorkers.Select(w => w.Id).ToList(),
                ResidentIds = building.Residents.Select(r => r.Id).ToList(),
                
                Storage = ExtractResources(building.Storage),
                DoorsOpen = building.DoorsOpen
            };
        }

        /// <summary>
        /// Restore GameEngine from saved state
        /// </summary>
        private static GameEngine RestoreGameState(GameState state)
        {
            // Create engine with saved configuration
            var engine = new GameEngine(state.GameId, state.Configuration);
            
            // Clear default initialization
            engine.Families.Clear();
            engine.Buildings.Clear();

            // Clear resources (remove all by setting to 0)
            foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
            {
                var amount = engine.VillageResources.Get(resourceType);
                if (amount > 0)
                {
                    engine.VillageResources.Remove(resourceType, amount);
                }
            }

            // Restore time
            RestoreTime(engine.Time, state.Time);
            
            // Restore resources
            foreach (var kvp in state.VillageResources)
            {
                engine.VillageResources.Add(kvp.Key, kvp.Value);
            }
            
            // Restore families and people
            var personLookup = new Dictionary<int, Person>();
            foreach (var familyState in state.Families)
            {
                var family = RestoreFamily(familyState, personLookup);
                engine.Families.Add(family);
            }
            
            // Restore relationships after all people are created
            RestoreRelationships(state.Families, personLookup);
            
            // Restore buildings
            var buildingLookup = new Dictionary<int, Building>();
            foreach (var buildingState in state.Buildings)
            {
                var building = RestoreBuilding(buildingState);
                engine.Buildings.Add(building);
                buildingLookup[building.Id] = building;

                // CRITICAL: Register building on grid tiles so it renders
                var occupiedTiles = building.GetOccupiedTiles();
                foreach (var tilePos in occupiedTiles)
                {
                    var tile = engine.Grid.GetTile(tilePos.X, tilePos.Y);
                    if (tile != null)
                    {
                        tile.Building = building;
                    }
                }
            }

            // Restore building references in people
            RestoreBuildingReferences(state.Families, state.Buildings, personLookup, buildingLookup);
            
            // Restore internal state
            SetPrivateField(engine, "_currentTick", state.CurrentTick);
            SetPrivateField(engine, "_nextBuildingId", state.NextBuildingId);
            SetPrivateField(engine, "_seed", state.Seed);
            
            return engine;
        }

        private static void RestoreTime(GameTime time, GameTimeState state)
        {
            // Properties with private setters - need to use backing fields
            var type = time.GetType();

            type.GetProperty("Year")?.SetValue(time, state.Year);
            type.GetProperty("CurrentSeason")?.SetValue(time, state.CurrentSeason);
            type.GetProperty("DayOfSeason")?.SetValue(time, state.Day);
            type.GetProperty("Hour")?.SetValue(time, state.Hour);
        }

        private static Family RestoreFamily(FamilyState state, Dictionary<int, Person> personLookup)
        {
            var family = new Family(state.Id, state.FamilyName);
            
            if (state.HomePosition != null)
            {
                family.HomePosition = new Vector2Int(state.HomePosition.X, state.HomePosition.Y);
            }
            
            foreach (var personState in state.Members)
            {
                var person = RestorePerson(personState);
                family.AddMember(person);
                personLookup[person.Id] = person;
            }
            
            return family;
        }

        private static Person RestorePerson(PersonState state)
        {
            var person = new Person(state.Id, state.FirstName, state.LastName, state.Age, state.Gender)
            {
                Position = new Vector2Int(state.Position.X, state.Position.Y),
                CurrentTask = state.CurrentTask,
                HasArrivedAtBuilding = state.HasArrivedAtBuilding,
                Energy = state.Energy,
                Hunger = state.Hunger,
                Health = state.Health,
                IsAlive = state.IsAlive,
                IsSleeping = state.IsSleeping,
                IsSick = state.IsSick
            };
            
            // Restore target position
            if (state.TargetPosition != null)
            {
                person.TargetPosition = new Vector2Int(state.TargetPosition.X, state.TargetPosition.Y);
            }
            
            // Restore path
            if (state.CurrentPath.Any())
            {
                var path = state.CurrentPath.Select(v => new Vector2Int(v.X, v.Y)).ToList();
                person.SetPath(path);
                SetPrivateField(person, "PathIndex", state.PathIndex);
            }
            
            return person;
        }

        private static void RestoreRelationships(List<FamilyState> familyStates, Dictionary<int, Person> personLookup)
        {
            foreach (var familyState in familyStates)
            {
                foreach (var personState in familyState.Members)
                {
                    var person = personLookup[personState.Id];
                    
                    // Restore spouse
                    if (personState.SpouseId.HasValue && personLookup.ContainsKey(personState.SpouseId.Value))
                    {
                        person.Spouse = personLookup[personState.SpouseId.Value];
                    }
                    
                    // Restore children
                    foreach (var childId in personState.ChildIds)
                    {
                        if (personLookup.ContainsKey(childId))
                        {
                            person.Children.Add(personLookup[childId]);
                        }
                    }
                }
            }
        }

        private static Building RestoreBuilding(BuildingState state)
        {
            var building = new Building(state.Type, state.X, state.Y, state.Rotation, state.Id)
            {
                IsConstructed = state.IsConstructed,
                ConstructionProgress = state.ConstructionProgress,
                DoorsOpen = state.DoorsOpen
            };
            
            // Restore storage
            foreach (var kvp in state.Storage)
            {
                building.Storage.Add(kvp.Key, kvp.Value);
            }
            
            return building;
        }

        private static void RestoreBuildingReferences(
            List<FamilyState> familyStates,
            List<BuildingState> buildingStates,
            Dictionary<int, Person> personLookup,
            Dictionary<int, Building> buildingLookup)
        {
            // Restore person -> building references
            foreach (var familyState in familyStates)
            {
                foreach (var personState in familyState.Members)
                {
                    var person = personLookup[personState.Id];
                    
                    if (personState.AssignedBuildingId.HasValue && buildingLookup.ContainsKey(personState.AssignedBuildingId.Value))
                    {
                        person.AssignedBuilding = buildingLookup[personState.AssignedBuildingId.Value];
                    }
                    
                    if (personState.HomeBuildingId.HasValue && buildingLookup.ContainsKey(personState.HomeBuildingId.Value))
                    {
                        person.HomeBuilding = buildingLookup[personState.HomeBuildingId.Value];
                    }
                }
            }
            
            // Restore building -> person references
            foreach (var buildingState in buildingStates)
            {
                var building = buildingLookup[buildingState.Id];
                
                foreach (var workerId in buildingState.WorkerIds)
                {
                    if (personLookup.ContainsKey(workerId))
                    {
                        building.Workers.Add(personLookup[workerId]);
                    }
                }
                
                foreach (var workerId in buildingState.ConstructionWorkerIds)
                {
                    if (personLookup.ContainsKey(workerId))
                    {
                        building.ConstructionWorkers.Add(personLookup[workerId]);
                    }
                }
                
                foreach (var residentId in buildingState.ResidentIds)
                {
                    if (personLookup.ContainsKey(residentId))
                    {
                        building.Residents.Add(personLookup[residentId]);
                    }
                }
            }
        }

        /// <summary>
        /// Helper to set private fields or properties using reflection
        /// </summary>
        private static void SetPrivateField(object obj, string memberName, object value)
        {
            var type = obj.GetType();

            // Try field first
            var field = type.GetField(memberName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(obj, value);
                return;
            }

            // Try property
            var property = type.GetProperty(memberName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance);

            property?.SetValue(obj, value);
        }
    }
}
