using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VillageBuilder.Engine.Entities
{
    // "You always start with Family."
    public class Family
    {
        public string FamilyName { get; set; }
        public List<Person> Members { get; set; }
        
        public Family(string familyName)
        {
            FamilyName = familyName;
            Members = new List<Person>();
        }
        
        public void AddMember(Person person)
        {
            Members.Add(person);
        }

        public List<Person> GetParents()
        {
            return Members.Where(p => p.Children.Any()).ToList();
        }

        public List<Person> GetChildren()
        {
            return Members.Where(p => p.Age < 18).ToList();
        }

        public Person? GetOldestMember()
        {
            return Members.OrderByDescending(p => p.Age).FirstOrDefault();
        }
    }
}
