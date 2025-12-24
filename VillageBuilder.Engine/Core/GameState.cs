using System;
using System.Collections.Generic;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Entities;
using VillageBuilder.Engine.Resources;
using VillageBuilder.Engine.World;

namespace VillageBuilder.Engine.Core
{
    /// <summary>
    /// Represents the complete serializable state of a game
    /// </summary>
    [Serializable]
    public class GameState
    {
        // Game metadata
        public int GameId { get; set; }
        public int Seed { get; set; }
        public int CurrentTick { get; set; }
        public int NextBuildingId { get; set; }
        public int NextPersonId { get; set; }
        public int NextFamilyId { get; set; }
        
        // Time and weather
        public GameTimeState Time { get; set; } = new GameTimeState();
        public WeatherState Weather { get; set; } = new WeatherState();
        
        // Configuration
        public GameConfiguration Configuration { get; set; } = new GameConfiguration();
        
        // World state
        public GridState Grid { get; set; } = new GridState();
        
        // Resources
        public Dictionary<ResourceType, int> VillageResources { get; set; } = new();
        
        // Entities
        public List<FamilyState> Families { get; set; } = new();
        public List<BuildingState> Buildings { get; set; } = new();
        
        // Metadata
        public DateTime SavedAt { get; set; }
        public string SaveVersion { get; set; } = "1.0.0";
    }
    
    /// <summary>
    /// Serializable time state
    /// </summary>
    [Serializable]
    public class GameTimeState
    {
        public int Hour { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public Season CurrentSeason { get; set; }
    }
    
    /// <summary>
    /// Serializable weather state
    /// </summary>
    [Serializable]
    public class WeatherState
    {
        public WeatherCondition CurrentWeather { get; set; }
        public int Seed { get; set; }
    }
    
    /// <summary>
    /// Serializable grid/map state
    /// </summary>
    [Serializable]
    public class GridState
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Seed { get; set; }
        // Note: We'll regenerate terrain from seed rather than serialize every tile
        // If custom modifications are needed, add: public List<TileModification> Modifications { get; set; }
    }
    
    /// <summary>
    /// Serializable family state
    /// </summary>
    [Serializable]
    public class FamilyState
    {
        public int Id { get; set; }
        public string FamilyName { get; set; } = "";
        public Vector2IntState? HomePosition { get; set; }
        public List<PersonState> Members { get; set; } = new();
    }
    
    /// <summary>
    /// Serializable person state
    /// </summary>
    [Serializable]
    public class PersonState
    {
        // Identity
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int Age { get; set; }
        public Gender Gender { get; set; }
        
        // Relationships
        public int? SpouseId { get; set; }
        public List<int> ChildIds { get; set; } = new();
        public int? FamilyId { get; set; }
        
        // Position and Navigation
        public Vector2IntState Position { get; set; } = new();
        public Vector2IntState? TargetPosition { get; set; }
        public List<Vector2IntState> CurrentPath { get; set; } = new();
        public int PathIndex { get; set; }
        
        // Task Assignment
        public PersonTask CurrentTask { get; set; }
        public int? AssignedBuildingId { get; set; }
        public bool HasArrivedAtBuilding { get; set; }
        public int? HomeBuildingId { get; set; }
        
        // Stats
        public int Energy { get; set; }
        public int Hunger { get; set; }
        public int Health { get; set; }
        public bool IsAlive { get; set; }
        public bool IsSleeping { get; set; }
        public bool IsSick { get; set; }
        public int DaysSick { get; set; }
    }
    
    /// <summary>
    /// Serializable building state
    /// </summary>
    [Serializable]
    public class BuildingState
    {
        public int Id { get; set; }
        public BuildingType Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public BuildingRotation Rotation { get; set; }
        
        public bool IsConstructed { get; set; }
        public int ConstructionProgress { get; set; }
        public int ConstructionWorkRequired { get; set; }
        
        public List<int> WorkerIds { get; set; } = new();
        public List<int> ConstructionWorkerIds { get; set; } = new();
        public List<int> ResidentIds { get; set; } = new();
        
        public Dictionary<ResourceType, int> Storage { get; set; } = new();
        public bool DoorsOpen { get; set; }
    }
    
    /// <summary>
    /// Serializable 2D vector
    /// </summary>
    [Serializable]
    public class Vector2IntState
    {
        public int X { get; set; }
        public int Y { get; set; }
        
        public Vector2IntState() { }
        
        public Vector2IntState(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
