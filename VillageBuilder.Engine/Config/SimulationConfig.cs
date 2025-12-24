namespace VillageBuilder.Engine.Config
{
    /// <summary>
    /// Configuration for simulation timing, speed, and game rules.
    /// </summary>
    public class SimulationConfig
    {
        // Time & Speed
        public float DefaultTimeScale { get; set; } = 1.0f;
        public float MinTimeScale { get; set; } = 0.0f;
        public float MaxTimeScale { get; set; } = 10.0f;
        
        // Day/Night Cycle
        public int TicksPerHour { get; set; } = 60;             // 1 hour = 60 ticks
        public int HoursPerDay { get; set; } = 24;
        public int DaysPerSeason { get; set; } = 30;
        public int DaysPerYear { get; set; } = 120;             // 4 seasons × 30 days
        
        // Night Darkness
        public float MaxDarknessFactor { get; set; } = 0.35f;   // Reduced from 0.6 for visibility
        public int NightStartHour { get; set; } = 20;
        public int NightEndHour { get; set; } = 6;
        
        // Auto-save
        public bool AutoSaveEnabled { get; set; } = true;
        public int AutoSaveIntervalMinutes { get; set; } = 5;
        
        // Performance
        public bool EnableMultithreading { get; set; } = true;
        public int MaxPathfindingOperationsPerTick { get; set; } = 10;
    }
}
