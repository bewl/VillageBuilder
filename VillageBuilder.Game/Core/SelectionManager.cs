using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Entities;
using VillageBuilder.Engine.World;

namespace VillageBuilder.Game.Core
{
    public enum SelectionType
    {
        None,
        Person,
        Building,
        Tile  // NEW: Tile selection type
    }

    public class SelectionManager
    {
        public SelectionType CurrentSelectionType { get; private set; }
        public Person? SelectedPerson { get; private set; }
        public Building? SelectedBuilding { get; private set; }
        public Tile? SelectedTile { get; private set; }  // NEW: Selected tile

        // For paging through multiple people on same tile
        public int SelectedPersonIndex { get; private set; }
        public List<Person>? PeopleAtSelectedTile { get; private set; }

        public void SelectPerson(Person person)
        {
            CurrentSelectionType = SelectionType.Person;
            SelectedPerson = person;
            SelectedBuilding = null;
            SelectedTile = null;  // Clear tile selection
            SelectedPersonIndex = 0;
            PeopleAtSelectedTile = null;
        }

        public void SelectPeopleAtTile(List<Person> peopleAtTile, int initialIndex = 0)
        {
            if (peopleAtTile == null || peopleAtTile.Count == 0) return;

            // Create a defensive copy to prevent external modifications
            var peopleCopy = new List<Person>(peopleAtTile);

            CurrentSelectionType = SelectionType.Person;
            PeopleAtSelectedTile = peopleCopy;
            SelectedPersonIndex = Math.Clamp(initialIndex, 0, peopleCopy.Count - 1);
            SelectedPerson = peopleCopy[SelectedPersonIndex];
            SelectedBuilding = null;
            SelectedTile = null;  // Clear tile selection
        }
        
        public void CycleNextPerson()
        {
            if (PeopleAtSelectedTile != null && PeopleAtSelectedTile.Count > 1)
            {
                SelectedPersonIndex = (SelectedPersonIndex + 1) % PeopleAtSelectedTile.Count;
                SelectedPerson = PeopleAtSelectedTile[SelectedPersonIndex];
            }
        }
        
        public void CyclePreviousPerson()
        {
            if (PeopleAtSelectedTile != null && PeopleAtSelectedTile.Count > 1)
            {
                SelectedPersonIndex--;
                if (SelectedPersonIndex < 0) SelectedPersonIndex = PeopleAtSelectedTile.Count - 1;
                SelectedPerson = PeopleAtSelectedTile[SelectedPersonIndex];
            }
        }

        /// <summary>
        /// Select a specific person by index from the current tile's people list
        /// </summary>
        public void SelectPersonByIndex(int index)
        {
            if (PeopleAtSelectedTile != null && index >= 0 && index < PeopleAtSelectedTile.Count)
            {
                SelectedPersonIndex = index;
                SelectedPerson = PeopleAtSelectedTile[index];
            }
        }

        public void SelectBuilding(Building building)
        {
            CurrentSelectionType = SelectionType.Building;
            SelectedBuilding = building;
            SelectedPerson = null;
            SelectedTile = null;  // Clear tile selection
            SelectedPersonIndex = 0;
            PeopleAtSelectedTile = null;
        }

        /// <summary>
        /// Select a tile for inspection (NEW)
        /// </summary>
        public void SelectTile(Tile tile)
        {
            CurrentSelectionType = SelectionType.Tile;
            SelectedTile = tile;
            SelectedPerson = null;
            SelectedBuilding = null;
            SelectedPersonIndex = 0;
            PeopleAtSelectedTile = null;
        }

        public void ClearSelection()
        {
            CurrentSelectionType = SelectionType.None;
            SelectedPerson = null;
            SelectedBuilding = null;
            SelectedTile = null;  // Clear tile selection
            SelectedPersonIndex = 0;
            PeopleAtSelectedTile = null;
        }

        public bool HasSelection()
        {
            return CurrentSelectionType != SelectionType.None;
        }
        
        public bool HasMultiplePeople()
        {
            return PeopleAtSelectedTile != null && PeopleAtSelectedTile.Count > 1;
        }
    }
}
