namespace VillageBuilder.Game.Core.Selection
{
    /// <summary>
    /// Generic selection manager for any selectable entity type.
    /// Eliminates duplicate cycling logic for Person, Wildlife, Building, etc.
    /// Supports single selection and multi-selection with cycling.
    /// </summary>
    /// <typeparam name="T">The type of selectable entity</typeparam>
    public class SelectionManager<T> where T : class, ISelectable
    {
        private T? _selectedEntity;
        private List<T>? _entitiesAtLocation;
        private int _selectedIndex;
        
        /// <summary>
        /// Currently selected entity
        /// </summary>
        public T? SelectedEntity => _selectedEntity;
        
        /// <summary>
        /// All entities at the currently selected location (for cycling)
        /// </summary>
        public IReadOnlyList<T>? EntitiesAtLocation => _entitiesAtLocation?.AsReadOnly();
        
        /// <summary>
        /// Current index in the entities list (for cycling)
        /// </summary>
        public int SelectedIndex => _selectedIndex;
        
        /// <summary>
        /// Whether an entity is currently selected
        /// </summary>
        public bool HasSelection => _selectedEntity != null;
        
        /// <summary>
        /// Whether there are multiple entities to cycle through
        /// </summary>
        public bool HasMultipleEntities => _entitiesAtLocation != null && _entitiesAtLocation.Count > 1;
        
        /// <summary>
        /// Select a single entity
        /// </summary>
        public void Select(T entity)
        {
            _selectedEntity = entity;
            _entitiesAtLocation = null;
            _selectedIndex = 0;
        }
        
        /// <summary>
        /// Select from a list of entities at the same location (enables cycling)
        /// </summary>
        public void SelectMultiple(List<T> entities, int initialIndex = 0)
        {
            if (entities == null || entities.Count == 0)
            {
                Clear();
                return;
            }
            
            // Defensive copy
            _entitiesAtLocation = new List<T>(entities);
            _selectedIndex = Math.Clamp(initialIndex, 0, _entitiesAtLocation.Count - 1);
            _selectedEntity = _entitiesAtLocation[_selectedIndex];
        }
        
        /// <summary>
        /// Cycle to the next entity in the list
        /// </summary>
        public void CycleNext()
        {
            if (!HasMultipleEntities) return;
            
            _selectedIndex = (_selectedIndex + 1) % _entitiesAtLocation!.Count;
            _selectedEntity = _entitiesAtLocation[_selectedIndex];
        }
        
        /// <summary>
        /// Cycle to the previous entity in the list
        /// </summary>
        public void CyclePrevious()
        {
            if (!HasMultipleEntities) return;
            
            _selectedIndex--;
            if (_selectedIndex < 0)
                _selectedIndex = _entitiesAtLocation!.Count - 1;
            
            _selectedEntity = _entitiesAtLocation[_selectedIndex];
        }
        
        /// <summary>
        /// Select entity by index from the current location's list
        /// </summary>
        public void SelectByIndex(int index)
        {
            if (_entitiesAtLocation == null || 
                index < 0 || 
                index >= _entitiesAtLocation.Count)
            {
                return;
            }
            
            _selectedIndex = index;
            _selectedEntity = _entitiesAtLocation[index];
        }
        
        /// <summary>
        /// Clear the current selection
        /// </summary>
        public void Clear()
        {
            _selectedEntity = null;
            _entitiesAtLocation = null;
            _selectedIndex = 0;
        }
        
        /// <summary>
        /// Get count of entities at current location
        /// </summary>
        public int GetEntityCount()
        {
            return _entitiesAtLocation?.Count ?? 0;
        }
    }
}
