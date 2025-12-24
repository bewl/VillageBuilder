using VillageBuilder.Engine.Entities.Wildlife;

namespace VillageBuilder.Engine.Config
{
    /// <summary>
    /// Configuration for wildlife spawning, behavior, and population management.
    /// Controls ecosystem balance and wildlife AI parameters.
    /// </summary>
    public class WildlifeConfig
    {
        // Initial Population (spawn at world generation)
        public Dictionary<WildlifeType, int> InitialPopulation { get; set; } = new()
        {
            { WildlifeType.Rabbit, 15 },    // Prey
            { WildlifeType.Deer, 8 },       // Prey
            { WildlifeType.Boar, 4 },       // Prey
            { WildlifeType.Bird, 10 },      // Neutral
            { WildlifeType.Duck, 3 },       // Neutral
            { WildlifeType.Turkey, 2 },     // Neutral
            { WildlifeType.Fox, 3 },        // Predator
            { WildlifeType.Wolf, 2 },       // Predator
            { WildlifeType.Bear, 1 }        // Predator
        };
        
        // Population Limits
        public int MaxPopulationPerType { get; set; } = 50;
        public int MaxTotalPopulation { get; set; } = 200;
        public float PopulationCullThreshold { get; set; } = 0.9f;  // Cull at 90% of max
        
        // Hunting & Combat
        public float BaseHuntingSuccessRate { get; set; } = 0.3f;
        public float HuntingDamagePerAttempt { get; set; } = 25f;
        public float FleeSpeedMultiplier { get; set; } = 1.5f;
        
        // Behavior Timings (in ticks)
        public int MinGrazingTime { get; set; } = 5;
        public int MaxGrazingTime { get; set; } = 15;
        public int MinRestingTime { get; set; } = 10;
        public int MaxRestingTime { get; set; } = 30;
        public int MinWanderingTime { get; set; } = 3;
        public int MaxWanderingTime { get; set; } = 10;
        
        // Needs Thresholds
        public int HungerThreshold { get; set; } = 70;          // Start looking for food at 70 hunger
        public int StarvationThreshold { get; set; } = 95;      // Die if hunger reaches 95
        public int EnergyThreshold { get; set; } = 30;          // Rest when energy below 30
        public int FearThreshold { get; set; } = 50;            // Flee when fear above 50
        
        // Needs Changes Per Tick
        public float HungerIncreasePerTick { get; set; } = 0.1f;
        public float EnergyDecreasePerTick { get; set; } = 0.1f;
        public float EnergyRestorePerTick { get; set; } = 1.0f;
        public float FearDecreasePerTick { get; set; } = 2.0f;
        
        // Breeding
        public bool BreedingEnabled { get; set; } = false;      // Future feature
        public int MinAgeForBreeding { get; set; } = 100;
        public float BreedingProbability { get; set; } = 0.1f;
        
        // Movement
        public int VisionRange { get; set; } = 5;               // Tiles an animal can "see"
        public int FleeDistance { get; set; } = 10;             // How far to run when fleeing
        public int HuntingRange { get; set; } = 8;              // Max distance predator will chase
        
        // Resource Drops (multipliers for base values)
        public float MeatDropMultiplier { get; set; } = 1.0f;
        public float FurDropMultiplier { get; set; } = 1.0f;
    }
}
