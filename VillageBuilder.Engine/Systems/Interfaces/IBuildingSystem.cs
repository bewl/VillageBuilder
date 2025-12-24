using VillageBuilder.Engine.Buildings;

namespace VillageBuilder.Engine.Systems.Interfaces
{
    /// <summary>
    /// Manages buildings, construction, and building-related systems.
    /// Handles placement, construction progress, and building operations.
    /// </summary>
    public interface IBuildingSystem
    {
        /// <summary>
        /// All buildings in the world
        /// </summary>
        List<Building> AllBuildings { get; }
        
        /// <summary>
        /// Try to place a new building. Returns the building if successful, null if blocked.
        /// </summary>
        Building? TryPlaceBuilding(BuildingType type, int x, int y, int rotation, IResourceSystem resources);
        
        /// <summary>
        /// Update all buildings (construction progress, production, etc.)
        /// </summary>
        void UpdateBuildings(Core.GameTime time);
        
        /// <summary>
        /// Get building at specific tile position, if any
        /// </summary>
        Building? GetBuildingAt(int x, int y);
        
        /// <summary>
        /// Check if a building can be placed at position
        /// </summary>
        bool CanPlaceBuilding(BuildingType type, int x, int y, int rotation, World.VillageGrid grid);

        /// <summary>
        /// Remove/demolish a building
        /// </summary>
        void RemoveBuilding(Building building, World.VillageGrid grid);
        
        /// <summary>
        /// Get all buildings of a specific type
        /// </summary>
        List<Building> GetBuildingsByType(BuildingType type);
    }
}
