using VillageBuilder.Engine.Core;

namespace VillageBuilder.Engine.World
{
    public enum WeatherCondition
    {
        Clear,
        Cloudy,
        Rain,
        Snow,
        Storm,
        Blizzard
    }

    public class Weather
    {
        public WeatherCondition Condition { get; private set; }
        public int Temperature { get; private set; } // Celsius
        public double Precipitation { get; private set; } // mm
        
        private Random _random;

        public Weather(int seed)
        {
            _random = new Random(seed); // Deterministic for multiplayer
            Condition = WeatherCondition.Clear;
            Temperature = 20;
        }

        public void UpdateWeather(Season season, int dayOfSeason)
        {
            // Temperature based on season
            Temperature = season switch
            {
                Season.Spring => _random.Next(10, 20),
                Season.Summer => _random.Next(20, 35),
                Season.Fall => _random.Next(5, 15),
                Season.Winter => _random.Next(-15, 5),
                _ => 15
            };

            // Weather conditions based on season and temperature
            int weatherRoll = _random.Next(100);
            
            if (season == Season.Winter && Temperature < 0)
            {
                Condition = weatherRoll switch
                {
                    < 20 => WeatherCondition.Blizzard,
                    < 50 => WeatherCondition.Snow,
                    < 70 => WeatherCondition.Cloudy,
                    _ => WeatherCondition.Clear
                };
            }
            else if (season == Season.Spring || season == Season.Fall)
            {
                Condition = weatherRoll switch
                {
                    < 30 => WeatherCondition.Rain,
                    < 60 => WeatherCondition.Cloudy,
                    _ => WeatherCondition.Clear
                };
            }
            else
            {
                Condition = weatherRoll switch
                {
                    < 10 => WeatherCondition.Storm,
                    < 20 => WeatherCondition.Rain,
                    < 40 => WeatherCondition.Cloudy,
                    _ => WeatherCondition.Clear
                };
            }

            Precipitation = Condition switch
            {
                WeatherCondition.Rain => _random.NextDouble() * 10,
                WeatherCondition.Storm => _random.NextDouble() * 20,
                WeatherCondition.Snow => _random.NextDouble() * 5,
                WeatherCondition.Blizzard => _random.NextDouble() * 15,
                _ => 0
            };
        }

        public bool IsWorkable()
        {
            return Condition != WeatherCondition.Storm && Condition != WeatherCondition.Blizzard;
        }
    }
}