using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Entities;

namespace VillageBuilder.Game.Core
{
    public enum SelectionType
    {
        None,
        Person,
        Building
    }

    public class SelectionManager
    {
        public SelectionType CurrentSelectionType { get; private set; }
        public Person? SelectedPerson { get; private set; }
        public Building? SelectedBuilding { get; private set; }
        
        // For paging through multiple people on same tile
        public int SelectedPersonIndex { get; private set; }
        public List<Person>? PeopleAtSelectedTile { get; private set; }

        public void SelectPerson(Person person)
        {
            CurrentSelectionType = SelectionType.Person;
            SelectedPerson = person;
            SelectedBuilding = null;
            SelectedPersonIndex = 0;
            PeopleAtSelectedTile = null;
        }
        
        public void SelectPeopleAtTile(List<Person> peopleAtTile, int initialIndex = 0)
        {
            if (peopleAtTile == null || peopleAtTile.Count == 0) return;
            
            CurrentSelectionType = SelectionType.Person;
            PeopleAtSelectedTile = peopleAtTile;
            SelectedPersonIndex = Math.Clamp(initialIndex, 0, peopleAtTile.Count - 1);
            SelectedPerson = peopleAtTile[SelectedPersonIndex];
            SelectedBuilding = null;
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

        public void SelectBuilding(Building building)
        {
            CurrentSelectionType = SelectionType.Building;
            SelectedBuilding = building;
            SelectedPerson = null;
            SelectedPersonIndex = 0;
            PeopleAtSelectedTile = null;
        }

        public void ClearSelection()
        {
            CurrentSelectionType = SelectionType.None;
            SelectedPerson = null;
            SelectedBuilding = null;
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
