using VillageBuilder.Engine.Buildings;

namespace VillageBuilder.Game.Core.Selection
{
    /// <summary>
    /// Types of entities that can be selected
    /// </summary>
    public enum SelectionType
    {
        None,
        Person,
        Building,
        Tile,
        Wildlife
    }
    
    /// <summary>
    /// Common interface for all selectable entities in the game.
    /// Enables generic selection management and eliminates duplicate code.
    /// </summary>
    public interface ISelectable
    {
        /// <summary>
        /// Display name for this entity (shown in UI)
        /// </summary>
        string GetDisplayName();
        
        /// <summary>
        /// Position of this entity in the world (X, Y)
        /// </summary>
        Vector2Int GetPosition();
        
        /// <summary>
        /// Type of selection for UI rendering and behavior
        /// </summary>
        SelectionType GetSelectionType();
        
        /// <summary>
        /// Unique identifier for this entity (optional, for tracking)
        /// </summary>
        int GetId();
    }
}
