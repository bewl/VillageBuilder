using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Entities;
using VillageBuilder.Engine.Entities.Wildlife;
using VillageBuilder.Engine.World;

namespace VillageBuilder.Game.Core.Selection
{
    /// <summary>
    /// Coordinates selection across different entity types (Person, Wildlife, Building, Tile).
    /// Manages multiple SelectionManager instances and ensures only one type is selected at a time.
    /// Provides backward-compatible API matching the original SelectionManager.
    /// </summary>
    public class SelectionCoordinator
    {
        private readonly SelectionManager<PersonSelectable> _personManager;
        private readonly SelectionManager<WildlifeSelectable> _wildlifeManager;
        private readonly SelectionManager<BuildingSelectable> _buildingManager;
        private readonly SelectionManager<TileSelectable> _tileManager;
        
        private SelectionType _currentSelectionType = SelectionType.None;
        
        // Backward-compatible properties
        public SelectionType CurrentSelectionType => _currentSelectionType;
        public Person? SelectedPerson => _personManager.SelectedEntity?.Person;
        public WildlifeEntity? SelectedWildlife => _wildlifeManager.SelectedEntity?.Wildlife;
        public Building? SelectedBuilding => _buildingManager.SelectedEntity?.Building;
        public Tile? SelectedTile => _tileManager.SelectedEntity?.Tile;
        
        // Multi-selection support
        public bool HasMultiplePeople() => _personManager.HasMultipleEntities;
        public bool HasMultipleWildlife() => _wildlifeManager.HasMultipleEntities;
        public int SelectedPersonIndex => _personManager.SelectedIndex;
        public int SelectedWildlifeIndex => _wildlifeManager.SelectedIndex;

        // Expose lists for backward compatibility with SidebarRenderer
        public IReadOnlyList<Person>? PeopleAtSelectedTile => 
            _personManager.EntitiesAtLocation?.Select(s => s.Person).ToList();
        public IReadOnlyList<WildlifeEntity>? WildlifeAtSelectedTile => 
            _wildlifeManager.EntitiesAtLocation?.Select(s => s.Wildlife).ToList();
        
        public SelectionCoordinator()
        {
            _personManager = new SelectionManager<PersonSelectable>();
            _wildlifeManager = new SelectionManager<WildlifeSelectable>();
            _buildingManager = new SelectionManager<BuildingSelectable>();
            _tileManager = new SelectionManager<TileSelectable>();
        }
        
        // Person selection methods
        public void SelectPerson(Person person)
        {
            ClearAll();
            _personManager.Select(new PersonSelectable(person));
            _currentSelectionType = SelectionType.Person;
        }
        
        public void SelectPeopleAtTile(List<Person> peopleAtTile, int initialIndex = 0)
        {
            if (peopleAtTile == null || peopleAtTile.Count == 0) return;
            
            ClearAll();
            var selectables = peopleAtTile.Select(p => new PersonSelectable(p)).ToList();
            _personManager.SelectMultiple(selectables, initialIndex);
            _currentSelectionType = SelectionType.Person;
        }
        
        public void CycleNextPerson() => _personManager.CycleNext();
        public void CyclePreviousPerson() => _personManager.CyclePrevious();
        public void SelectPersonByIndex(int index) => _personManager.SelectByIndex(index);
        
        // Wildlife selection methods
        public void SelectWildlife(WildlifeEntity wildlife)
        {
            ClearAll();
            _wildlifeManager.Select(new WildlifeSelectable(wildlife));
            _currentSelectionType = SelectionType.Wildlife;
        }
        
        public void SelectWildlifeAtTile(List<WildlifeEntity> wildlifeAtTile, int initialIndex = 0)
        {
            if (wildlifeAtTile == null || wildlifeAtTile.Count == 0) return;
            
            ClearAll();
            var selectables = wildlifeAtTile.Select(w => new WildlifeSelectable(w)).ToList();
            _wildlifeManager.SelectMultiple(selectables, initialIndex);
            _currentSelectionType = SelectionType.Wildlife;
        }
        
        public void CycleNextWildlife() => _wildlifeManager.CycleNext();
        public void CyclePreviousWildlife() => _wildlifeManager.CyclePrevious();
        public void SelectWildlifeByIndex(int index) => _wildlifeManager.SelectByIndex(index);
        
        // Building selection methods
        public void SelectBuilding(Building building)
        {
            ClearAll();
            _buildingManager.Select(new BuildingSelectable(building));
            _currentSelectionType = SelectionType.Building;
        }
        
        // Tile selection methods
        public void SelectTile(Tile tile)
        {
            ClearAll();
            _tileManager.Select(new TileSelectable(tile));
            _currentSelectionType = SelectionType.Tile;
        }
        
        // General methods
        public void ClearSelection()
        {
            ClearAll();
            _currentSelectionType = SelectionType.None;
        }
        
        public bool HasSelection() => _currentSelectionType != SelectionType.None;
        
        private void ClearAll()
        {
            _personManager.Clear();
            _wildlifeManager.Clear();
            _buildingManager.Clear();
            _tileManager.Clear();
        }
    }
    
    // Wrapper classes to adapt existing entities to ISelectable
    
    public class PersonSelectable : ISelectable
    {
        public Person Person { get; }
        
        public PersonSelectable(Person person)
        {
            Person = person ?? throw new ArgumentNullException(nameof(person));
        }
        
        public string GetDisplayName() => $"{Person.FirstName} {Person.LastName}";
        public Vector2Int GetPosition() => new Vector2Int(Person.Position.X, Person.Position.Y);
        public SelectionType GetSelectionType() => SelectionType.Person;
        public int GetId() => Person.Id;
    }
    
    public class WildlifeSelectable : ISelectable
    {
        public WildlifeEntity Wildlife { get; }
        
        public WildlifeSelectable(WildlifeEntity wildlife)
        {
            Wildlife = wildlife ?? throw new ArgumentNullException(nameof(wildlife));
        }
        
        public string GetDisplayName() => Wildlife.Name;
        public Vector2Int GetPosition() => new Vector2Int(Wildlife.Position.X, Wildlife.Position.Y);
        public SelectionType GetSelectionType() => SelectionType.Wildlife;
        public int GetId() => Wildlife.Id;
    }
    
    public class BuildingSelectable : ISelectable
    {
        public Building Building { get; }
        
        public BuildingSelectable(Building building)
        {
            Building = building ?? throw new ArgumentNullException(nameof(building));
        }
        
        public string GetDisplayName() => Building.Name;
        public Vector2Int GetPosition() => new Vector2Int(Building.X, Building.Y);
        public SelectionType GetSelectionType() => SelectionType.Building;
        public int GetId() => Building.Id;
    }
    
    public class TileSelectable : ISelectable
    {
        public Tile Tile { get; }
        
        public TileSelectable(Tile tile)
        {
            Tile = tile ?? throw new ArgumentNullException(nameof(tile));
        }
        
        public string GetDisplayName() => $"Tile ({Tile.X}, {Tile.Y})";
        public Vector2Int GetPosition() => new Vector2Int(Tile.X, Tile.Y);
        public SelectionType GetSelectionType() => SelectionType.Tile;
        public int GetId() => Tile.X * 10000 + Tile.Y; // Simple unique ID from coordinates
    }
}
