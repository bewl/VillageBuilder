using System;
using System.Collections.Generic;

namespace VillageBuilder.Engine.Entities
{
    public static class NameGenerator
    {
        private static readonly string[] MaleFirstNames = new[]
        {
            "William", "John", "Thomas", "Henry", "Robert", "Edward", "Richard", "George",
            "James", "Charles", "Edmund", "Walter", "Ralph", "Hugh", "Roger", "Peter",
            "Simon", "Geoffrey", "Nicholas", "Stephen", "Martin", "Alan", "Gilbert"
        };

        private static readonly string[] FemaleFirstNames = new[]
        {
            "Alice", "Emma", "Joan", "Margaret", "Agnes", "Elizabeth", "Matilda", "Isabella",
            "Mary", "Catherine", "Anne", "Eleanor", "Beatrice", "Cecilia", "Edith", "Sarah",
            "Maud", "Lucy", "Rose", "Helen", "Juliana", "Constance", "Philippa"
        };

        private static readonly string[] LastNames = new[]
        {
            "Smith", "Miller", "Cooper", "Baker", "Fletcher", "Mason", "Thatcher", "Wright",
            "Carpenter", "Turner", "Taylor", "Carter", "Fisher", "Shepherd", "Weaver", "Potter",
            "Brewer", "Butcher", "Tanner", "Cook", "Hunter", "Parker", "Ward", "Woods"
        };

        public static string GetRandomMaleName(Random random)
        {
            return MaleFirstNames[random.Next(MaleFirstNames.Length)];
        }

        public static string GetRandomFemaleName(Random random)
        {
            return FemaleFirstNames[random.Next(FemaleFirstNames.Length)];
        }

        public static string GetRandomLastName(Random random)
        {
            return LastNames[random.Next(LastNames.Length)];
        }
    }
}
