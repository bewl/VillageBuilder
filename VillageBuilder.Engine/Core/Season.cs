namespace VillageBuilder.Engine.Core
{
    public enum Season
    {
        Spring,
        Summer,
        Fall,
        Winter
    }

    public class GameTime
    {
        public int Year { get; private set; }
        public Season CurrentSeason { get; private set; }
        public int DayOfSeason { get; private set; }
        public int Hour { get; private set; }
        
        private const int DaysPerSeason = 90;
        private const int HoursPerDay = 24;

        public GameTime()
        {
            Year = 1;
            CurrentSeason = Season.Spring;
            DayOfSeason = 1;
            Hour = 6;
        }

        public void AdvanceHour()
        {
            Hour++;
            if (Hour >= HoursPerDay)
            {
                Hour = 0;
                AdvanceDay();
            }
        }

        private void AdvanceDay()
        {
            DayOfSeason++;
            if (DayOfSeason > DaysPerSeason)
            {
                DayOfSeason = 1;
                AdvanceSeason();
            }
        }

        private void AdvanceSeason()
        {
            if (CurrentSeason == Season.Winter)
            {
                CurrentSeason = Season.Spring;
                Year++;
            }
            else
            {
                CurrentSeason++;
            }
        }

        public override string ToString()
        {
            return $"Year {Year}, {CurrentSeason} Day {DayOfSeason}, Hour {Hour}:00";
        }
    }
}