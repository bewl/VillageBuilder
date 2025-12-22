using System;
using System.Collections.Generic;
using System.Linq;
using VillageBuilder.Engine.Buildings;

namespace VillageBuilder.Engine.Entities
{
    // "You always start with Family."
    public class Family
    {
        public int Id { get; set; }
        public string FamilyName { get; set; }
        public List<Person> Members { get; set; }
        public Vector2Int? HomePosition { get; set; } // Where the family lives
        
        public Family(int id, string familyName)
        {
            Id = id;
            FamilyName = familyName;
            Members = new List<Person>();
        }
        
        public void AddMember(Person person)
        {
            if (!Members.Contains(person))
            {
                Members.Add(person);
                person.Family = this;
            }
        }

        public void RemoveMember(Person person)
        {
            Members.Remove(person);
            person.Family = null;
        }

        public List<Person> GetParents()
        {
            return Members.Where(p => p.Children.Any()).ToList();
        }

        public List<Person> GetChildren()
        {
            return Members.Where(p => p.Age < 18).ToList();
        }

        public List<Person> GetAdults()
        {
            return Members.Where(p => p.Age >= 18).ToList();
        }

        public Person? GetOldestMember()
        {
            return Members.OrderByDescending(p => p.Age).FirstOrDefault();
        }

        public int GetTotalHunger()
        {
            return Members.Sum(p => p.Hunger);
        }

        public int GetAverageEnergy()
        {
            if (Members.Count == 0) return 0;
            return (int)Members.Average(p => p.Energy);
        }

        public List<Person> GetIdleMembers()
        {
            return Members.Where(p => p.IsAlive && p.CurrentTask == PersonTask.Idle).ToList();
        }

        public List<Person> GetWorkingMembers()
        {
            return Members.Where(p => p.IsAlive && p.AssignedBuilding != null).ToList();
        }
    }
}
