namespace VillageBuilder.Engine.Core
{
    public enum Season
    {
        Spring,
        Summer,
        Fall,
        Winter
    }

    public enum TimeOfDay
    {
        Night,      // 0-6
        Morning,    // 6-12
        Afternoon,  // 12-18
        Evening     // 18-24
    }

    public class GameTime
    {
        public int Year { get; private set; }
        public Season CurrentSeason { get; private set; }
        public int DayOfSeason { get; private set; }
        public int Hour { get; private set; }
        
        private const int DaysPerSeason = 90;
        private const int HoursPerDay = 24;
        
        // Time progression - how many ticks per game hour
        public const int TicksPerHour = 20; // 20 ticks = 1 hour, so 1 day = 480 ticks (24 seconds at 20 TPS)
        
        private int _ticksSinceLastHour = 0;
        
        // Work hours configuration
        public const int WorkStartHour = 6;
        public const int WorkEndHour = 18;
        public const int SleepStartHour = 22;
        public const int WakeUpHour = 6;

        public GameTime()
        {
            Year = 1;
            CurrentSeason = Season.Spring;
            DayOfSeason = 1;
            Hour = 6;
            _ticksSinceLastHour = 0;
        }

        /// <summary>
        /// Advance time by one tick. Returns true if an hour has passed.
        /// </summary>
        public bool AdvanceTick()
        {
            _ticksSinceLastHour++;
            
            if (_ticksSinceLastHour >= TicksPerHour)
            {
                _ticksSinceLastHour = 0;
                AdvanceHour();
                return true; // Hour changed
            }
            
            return false; // Hour not changed yet
        }

        private void AdvanceHour()
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

        /// <summary>
        /// Check if it's currently work hours (6 AM - 6 PM)
        /// </summary>
        public bool IsWorkHours()
        {
            return Hour >= WorkStartHour && Hour < WorkEndHour;
        }

        /// <summary>
        /// Check if it's currently sleep time (10 PM - 6 AM)
        /// </summary>
        public bool IsSleepTime()
        {
            return Hour >= SleepStartHour || Hour < WakeUpHour;
        }

        /// <summary>
        /// Check if it's nighttime (for visual purposes, 6 PM - 6 AM)
        /// </summary>
        public bool IsNight()
        {
            return Hour >= 18 || Hour < 6;
        }

        /// <summary>
        /// Check if it's nighttime (alias for IsNight)
        /// </summary>
        public bool IsNightTime() => IsNight();

        /// <summary>
        /// Check if it's evening time (6 PM - 10 PM)
        /// </summary>
        public bool IsEveningTime()
        {
            return Hour >= 18 && Hour < 22;
        }

        /// <summary>
        /// Get current time of day
        /// </summary>
        public TimeOfDay GetTimeOfDay()
        {
            if (Hour >= 0 && Hour < 6) return TimeOfDay.Night;
            if (Hour >= 6 && Hour < 12) return TimeOfDay.Morning;
            if (Hour >= 12 && Hour < 18) return TimeOfDay.Afternoon;
            return TimeOfDay.Evening;
        }

        /// <summary>
        /// Get darkness factor (0.0 = full day, 1.0 = full night)
        /// </summary>
        public float GetDarknessFactor()
        {
            if (Hour >= 6 && Hour < 18)
            {
                // Daytime
                return 0.0f;
            }
            else if (Hour >= 18 && Hour < 22)
            {
                // Evening - gradual darkening
                return (Hour - 18) / 4.0f;
            }
            else if (Hour >= 22 || Hour < 4)
            {
                // Night - full darkness
                return 1.0f;
            }
            else
            {
                // Dawn - gradual lightening (4-6 AM)
                return 1.0f - ((Hour - 4) / 2.0f);
            }
        }

        public override string ToString()
        {
            return $"Year {Year}, {CurrentSeason} Day {DayOfSeason}, Hour {Hour}:00";
        }
    }
}