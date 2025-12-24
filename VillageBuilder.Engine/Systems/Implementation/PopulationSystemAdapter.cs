using VillageBuilder.Engine.Entities;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Systems.Interfaces;
using VillageBuilder.Engine.World;

namespace VillageBuilder.Engine.Systems.Implementation
{
    /// <summary>
    /// Population system adapter wrapping existing people and family management.
    /// </summary>
    public class PopulationSystemAdapter : IPopulationSystem
    {
        public List<Person> AllPeople { get; private set; }
        public List<Family> AllFamilies { get; private set; }
        public int Population => AllPeople.Count(p => p.IsAlive);

        private int _nextPersonId = 1;
        private int _nextFamilyId = 1;

        public PopulationSystemAdapter(List<Family> families)
        {
            AllFamilies = families;
            AllPeople = new List<Person>();

            // Collect all people from all families
            foreach (var family in families)
            {
                AllPeople.AddRange(family.Members);

                // Track highest IDs
                foreach (var person in family.Members)
                {
                    if (person.Id >= _nextPersonId)
                        _nextPersonId = person.Id + 1;
                }

                if (family.Id >= _nextFamilyId)
                    _nextFamilyId = family.Id + 1;
            }
        }

        public void UpdatePeople(GameTime time, VillageGrid grid)
        {
            // People update logic would go here
            // Currently people are updated by GameEngine directly
            // This is a placeholder for future refactoring
        }

        public Family CreateFamily(string familyName, int startingMembers)
        {
            var family = new Family(_nextFamilyId++, familyName);

            // Add starting family members
            for (int i = 0; i < startingMembers; i++)
            {
                var gender = i % 2 == 0 ? Gender.Male : Gender.Female;
                var age = 20 + i * 5; // Stagger ages
                var firstName = NameGenerator.GetRandomMaleName(new Random());
                if (gender == Gender.Female)
                {
                    firstName = NameGenerator.GetRandomFemaleName(new Random());
                }

                var person = new Person(_nextPersonId++, firstName, familyName, age, gender);
                person.Family = family;

                family.Members.Add(person);
                AllPeople.Add(person);
            }

            AllFamilies.Add(family);
            return family;
        }

        public void RemovePerson(Person person)
        {
            person.IsAlive = false;
            // Person remains in lists but marked as dead
        }

        public Person AddPersonToFamily(Family family, string firstName, Gender gender, int age)
        {
            var person = new Person(_nextPersonId++, firstName, family.FamilyName, age, gender);
            person.Family = family;
            family.Members.Add(person);
            AllPeople.Add(person);
            return person;
        }

        public List<Person> GetPeopleAt(int x, int y)
        {
            return AllPeople.Where(p => p.IsAlive && p.Position.X == x && p.Position.Y == y).ToList();
        }
    }
}
