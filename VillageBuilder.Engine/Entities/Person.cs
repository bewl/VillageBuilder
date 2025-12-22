namespace VillageBuilder.Engine.Entities
{
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public Person? Spouse { get; set; }
        public List<Person> Children { get; set; }
        
        public Person(string firstName, string lastName, int age, string gender = "Unknown")
        {
            FirstName = firstName;
            LastName = lastName;
            Age = age;
            Gender = gender;
            Children = new List<Person>();
        }

        public void MarryTo(Person spouse)
        {
            Spouse = spouse;
            spouse.Spouse = this;
        }

        public void AddChild(Person child)
        {
            Children.Add(child);
        }
    }
}