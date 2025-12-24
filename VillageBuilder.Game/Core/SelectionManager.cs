using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Entities;
using VillageBuilder.Engine.Entities.Wildlife;
using VillageBuilder.Engine.World;

namespace VillageBuilder.Game.Core
{
    public enum SelectionType
    {
        None,
        Person,
        Building,
        Tile,      // Tile selection type
        Wildlife   // NEW: Wildlife selection type
    }

    public class SelectionManager
    {
        public SelectionType CurrentSelectionType { get; private set; }
        public Person? SelectedPerson { get; private set; }
        public Building? SelectedBuilding { get; private set; }
        public Tile? SelectedTile { get; private set; }
        public WildlifeEntity? SelectedWildlife { get; private set; }  // NEW: Selected wildlife

        // For paging through multiple people on same tile
        public int SelectedPersonIndex { get; private set; }
        public List<Person>? PeopleAtSelectedTile { get; private set; }

        // NEW: For paging through multiple wildlife on same tile
        public int SelectedWildlifeIndex { get; private set; }
        public List<WildlifeEntity>? WildlifeAtSelectedTile { get; private set; }

        public void SelectPerson(Person person)
        {
            CurrentSelectionType = SelectionType.Person;
            SelectedPerson = person;
            SelectedBuilding = null;
            SelectedTile = null;
            SelectedWildlife = null;
            SelectedPersonIndex = 0;
            PeopleAtSelectedTile = null;
            SelectedWildlifeIndex = 0;
            WildlifeAtSelectedTile = null;
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
            SelectedTile = null;
            SelectedWildlife = null;
            SelectedWildlifeIndex = 0;
            WildlifeAtSelectedTile = null;
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
            SelectedTile = null;
            SelectedWildlife = null;
            SelectedPersonIndex = 0;
            PeopleAtSelectedTile = null;
            SelectedWildlifeIndex = 0;
            WildlifeAtSelectedTile = null;
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
            SelectedWildlife = null;
            SelectedPersonIndex = 0;
            PeopleAtSelectedTile = null;
            SelectedWildlifeIndex = 0;
            WildlifeAtSelectedTile = null;
        }

        public void ClearSelection()
        {
            CurrentSelectionType = SelectionType.None;
            SelectedPerson = null;
            SelectedBuilding = null;
            SelectedTile = null;
            SelectedWildlife = null;
            SelectedPersonIndex = 0;
            PeopleAtSelectedTile = null;
            SelectedWildlifeIndex = 0;
            WildlifeAtSelectedTile = null;
        }

        public bool HasSelection()
        {
            return CurrentSelectionType != SelectionType.None;
        }

                public bool HasMultiplePeople()
                {
                    return PeopleAtSelectedTile != null && PeopleAtSelectedTile.Count > 1;
                }

                // Wildlife selection methods

                public void SelectWildlife(WildlifeEntity wildlife)
                {
                    CurrentSelectionType = SelectionType.Wildlife;
                    SelectedWildlife = wildlife;
                    SelectedPerson = null;
                    SelectedBuilding = null;
                    SelectedTile = null;
                    SelectedPersonIndex = 0;
                    PeopleAtSelectedTile = null;
                    SelectedWildlifeIndex = 0;
                    WildlifeAtSelectedTile = null;
                }

                public void SelectWildlifeAtTile(List<WildlifeEntity> wildlifeAtTile, int initialIndex = 0)
                {
                    if (wildlifeAtTile == null || wildlifeAtTile.Count == 0) return;

                    var wildlifeCopy = new List<WildlifeEntity>(wildlifeAtTile);

                    CurrentSelectionType = SelectionType.Wildlife;
                    WildlifeAtSelectedTile = wildlifeCopy;
                    SelectedWildlifeIndex = Math.Clamp(initialIndex, 0, wildlifeCopy.Count - 1);
                    SelectedWildlife = wildlifeCopy[SelectedWildlifeIndex];
                    SelectedPerson = null;
                    SelectedBuilding = null;
                    SelectedTile = null;
                    SelectedPersonIndex = 0;
                    PeopleAtSelectedTile = null;
                }

                public void CycleNextWildlife()
                {
                    if (WildlifeAtSelectedTile != null && WildlifeAtSelectedTile.Count > 1)
                    {
                        SelectedWildlifeIndex = (SelectedWildlifeIndex + 1) % WildlifeAtSelectedTile.Count;
                        SelectedWildlife = WildlifeAtSelectedTile[SelectedWildlifeIndex];
                    }
                }

                public void CyclePreviousWildlife()
                {
                    if (WildlifeAtSelectedTile != null && WildlifeAtSelectedTile.Count > 1)
                    {
                        SelectedWildlifeIndex--;
                        if (SelectedWildlifeIndex < 0) SelectedWildlifeIndex = WildlifeAtSelectedTile.Count - 1;
                        SelectedWildlife = WildlifeAtSelectedTile[SelectedWildlifeIndex];
                    }
                }

                public void SelectWildlifeByIndex(int index)
                {
                    if (WildlifeAtSelectedTile != null && index >= 0 && index < WildlifeAtSelectedTile.Count)
                    {
                        SelectedWildlifeIndex = index;
                        SelectedWildlife = WildlifeAtSelectedTile[index];
                    }
                }

                public bool HasMultipleWildlife()
                {
                    return WildlifeAtSelectedTile != null && WildlifeAtSelectedTile.Count > 1;
                }
            }
        }
