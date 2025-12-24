namespace VillageBuilder.Engine.Config
{
    /// <summary>
    /// Centralized configuration for terrain generation and decoration placement.
    /// Controls visual density and variety of the generated world.
    /// </summary>
    public class TerrainConfig
    {
        // Grass Decorations (reduced from defaults for visual clarity)
        public float GrassTuftDensity { get; set; } = 0.08f;        // Was: 0.3 (30%)
        public float WildflowerDensity { get; set; } = 0.05f;       // Was: 0.15 (15%)
        public float RareFlowerDensity { get; set; } = 0.03f;       // Unchanged
        
        // Natural Features
        public float RockDensity { get; set; } = 0.02f;             // Was: 0.05 (5%)
        public float TallGrassDensity { get; set; } = 0.04f;        // Was: 0.12 (12%)
        public float BushDensity { get; set; } = 0.08f;             // Unchanged
        
        // Forest Decorations
        public float ForestTreeDensity { get; set; } = 0.7f;
        public float ForestBushDensity { get; set; } = 0.15f;
        public float ForestMushroomDensity { get; set; } = 0.05f;
        public float ForestFernDensity { get; set; } = 0.2f;
        
        // Water Decorations
        public float ReedDensity { get; set; } = 0.3f;
        public float LilyPadDensity { get; set; } = 0.1f;
        
        // Wildlife Decorations (static - note: these are now handled by WildlifeManager)
        public float StaticRabbitDensity { get; set; } = 0.01f;     // Legacy, mostly filtered out
        public float StaticDeerDensity { get; set; } = 0.005f;      // Legacy, mostly filtered out
        public float ButterflyDensity { get; set; } = 0.08f;
        
        // Moisture thresholds for decoration placement
        public float MinMoistureForFlowers { get; set; } = 0.5f;
        public float MinMoistureForTallGrass { get; set; } = 0.6f;
        public float MinMoistureForBushes { get; set; } = 0.55f;
        
        /// <summary>
        /// Global multiplier for all decoration densities.
        /// 1.0 = normal, 0.5 = half density, 0.0 = no decorations
        /// </summary>
        public float GlobalDecorationMultiplier { get; set; } = 1.0f;
        
        /// <summary>
        /// Apply the global multiplier to a base density value
        /// </summary>
        public float ApplyMultiplier(float baseDensity) => baseDensity * GlobalDecorationMultiplier;
    }
}
