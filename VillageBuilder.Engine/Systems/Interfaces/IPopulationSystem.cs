using VillageBuilder.Engine.Entities;

namespace VillageBuilder.Engine.Systems.Interfaces
{
    /// <summary>
    /// Manages people and families in the village.
    /// Handles births, deaths, aging, and family relationships.
    /// </summary>
    public interface IPopulationSystem
    {
        /// <summary>
        /// All living people in the village
        /// </summary>
        List<Person> AllPeople { get; }
        
        /// <summary>
        /// All families in the village
        /// </summary>
        List<Family> AllFamilies { get; }
        
        /// <summary>
        /// Total population count
        /// </summary>
        int Population { get; }
        
        /// <summary>
        /// Update all people (needs, tasks, movement) for one tick
        /// </summary>
        void UpdatePeople(Core.GameTime time, World.VillageGrid grid);
        
        /// <summary>
        /// Create a new family with starting members
        /// </summary>
        Family CreateFamily(string familyName, int startingMembers);
        
        /// <summary>
        /// Remove a person from the world (death)
        /// </summary>
        void RemovePerson(Person person);
        
        /// <summary>
        /// Add a new person to a family (birth, immigration)
        /// </summary>
        Person AddPersonToFamily(Family family, string firstName, Gender gender, int age);
        
        /// <summary>
        /// Get all people at a specific tile position
        /// </summary>
        List<Person> GetPeopleAt(int x, int y);
    }
}
