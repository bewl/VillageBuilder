using System;
using System.Linq;
using Raylib_cs;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.World;
using VillageBuilder.Engine.Resources;
using System.Numerics;

namespace VillageBuilder.Game.Graphics.UI
{
    public class StatusBarRenderer
    {
        private const int Padding = 8;
        // Font sizes are now dynamic properties, not constants
        private static int FontSize => GraphicsConfig.ConsoleFontSize;
        private static int SmallFontSize => GraphicsConfig.SmallConsoleFontSize;

        public void Render(GameEngine engine, float timeScale, bool isPaused, HeatMapRenderer? heatMapRenderer = null)
        {
            var barHeight = GraphicsConfig.StatusBarHeight;

            // Draw background panel (dark terminal style)
            Raylib.DrawRectangle(0, 0, GraphicsConfig.ScreenWidth, barHeight, new Color(15, 15, 20, 255));

            // Draw bottom border
            DrawHorizontalBorder(0, barHeight - 1, GraphicsConfig.ScreenWidth);

            // Row 1: Time, Weather, Speed (adjusted spacing for 20px font)
            RenderTimeInfo(engine, Padding, Padding);
            RenderWeatherInfo(engine, 450, Padding);
            RenderSpeedInfo(timeScale, isPaused, GraphicsConfig.ScreenWidth - 300, Padding);

            // Row 2: Key Resources (adjusted spacing)
            RenderResources(engine, Padding, Padding + 26);

            // Row 3: Heat Map Status (if active)
            if (heatMapRenderer != null && heatMapRenderer.CurrentHeatMap != HeatMapType.None)
            {
                RenderHeatMapStatus(heatMapRenderer, Padding, Padding + 52);
            }
        }

        private void DrawHorizontalBorder(int x, int y, int width)
        {
            var borderColor = new Color(100, 100, 120, 255);
            string line = new string('═', width / 8);
            GraphicsConfig.DrawConsoleText(line, x, y, 8, borderColor);
        }

        private void RenderTimeInfo(GameEngine engine, int x, int y)
        {
            var time = engine.Time;
            var season = GetSeasonAscii(time.CurrentSeason);
            var seasonColor = GetSeasonColor(time.CurrentSeason);
            
            // Get time of day for display
            var timeOfDay = time.GetTimeOfDay();
            var timeOfDayIcon = GetTimeOfDayIcon(timeOfDay);
            var timeOfDayColor = GetTimeOfDayColor(timeOfDay);

            // Draw compact time display
            var yearText = $"Y{time.Year}";
            GraphicsConfig.DrawConsoleText(
                yearText,
                x,
                y,
                FontSize,
                new Color(200, 200, 200, 255)
            );
            x += GraphicsConfig.MeasureText(yearText, FontSize) + 8;

            GraphicsConfig.DrawConsoleText("|", x, y, FontSize, new Color(80, 80, 80, 255));
            x += GraphicsConfig.MeasureText("|", FontSize) + 8;

            GraphicsConfig.DrawConsoleText(season, x, y, FontSize, seasonColor);
            x += GraphicsConfig.MeasureText(season, FontSize) + 8;

            var dayText = $"D{time.DayOfSeason + 1}";
            GraphicsConfig.DrawConsoleText(dayText, x, y, FontSize, new Color(200, 200, 200, 255));
            x += GraphicsConfig.MeasureText(dayText, FontSize) + 8;

            GraphicsConfig.DrawConsoleText("|", x, y, FontSize, new Color(80, 80, 80, 255));
            x += GraphicsConfig.MeasureText("|", FontSize) + 8;

            // Show time of day icon
            GraphicsConfig.DrawConsoleText(timeOfDayIcon, x, y, FontSize, timeOfDayColor);
            x += GraphicsConfig.MeasureText(timeOfDayIcon, FontSize) + 8;

            var hourText = $"{time.Hour:D2}:00";
            GraphicsConfig.DrawConsoleText(hourText, x, y, FontSize, new Color(150, 200, 255, 255));
        }

        private void RenderWeatherInfo(GameEngine engine, int x, int y)
        {
            var weather = engine.Weather;
            var icon = GetWeatherAscii(weather.Condition);
            var tempColor = GetTemperatureColor(weather.Temperature);
            
            // Draw weather icon
            GraphicsConfig.DrawConsoleText(icon, x, y, FontSize, new Color(200, 200, 200, 255));
            x += GraphicsConfig.MeasureText(icon, FontSize) + 8;
            
            // Draw condition (abbreviated)
            var conditionText = weather.Condition.ToString().Substring(0, Math.Min(5, weather.Condition.ToString().Length));
            GraphicsConfig.DrawConsoleText(conditionText, x, y, FontSize, new Color(180, 180, 180, 255));
            x += GraphicsConfig.MeasureText(conditionText, FontSize) + 10;
            
            // Draw temperature with color coding
            var tempText = $"{weather.Temperature}°";
            GraphicsConfig.DrawConsoleText(tempText, x, y, FontSize, tempColor);
        }

        private void RenderSpeedInfo(float timeScale, bool isPaused, int x, int y)
        {
            var statusIcon = isPaused ? "║" : "►";
            var statusText = isPaused ? "PAUSE" : "RUN";
            var statusColor = isPaused ? new Color(255, 150, 50, 255) : new Color(100, 255, 100, 255);

            GraphicsConfig.DrawConsoleText($"[{statusIcon}]", x, y, FontSize, statusColor);
            GraphicsConfig.DrawConsoleText(statusText, x + 35, y, FontSize, statusColor);
            
            // Speed indicator
            var speedText = $"×{timeScale:F1}";
            GraphicsConfig.DrawConsoleText(speedText, x + 100, y, FontSize, new Color(255, 255, 100, 255));
        }

        private void RenderHeatMapStatus(HeatMapRenderer heatMapRenderer, int x, int y)
        {
            var heatMapName = heatMapRenderer.GetCurrentHeatMapName();
            var statusColor = new Color(100, 200, 255, 255);
            var iconColor = new Color(150, 220, 255, 255);

            // Draw heat map icon and label
            GraphicsConfig.DrawConsoleText("[◆]", x, y, FontSize, iconColor);
            GraphicsConfig.DrawConsoleText($"Heat Map: {heatMapName}", x + 40, y, FontSize, statusColor);

            // Draw hint
            var hintColor = new Color(150, 150, 150, 255);
            GraphicsConfig.DrawConsoleText("(Press V to toggle)", x + 250, y, SmallFontSize, hintColor);
        }

        private void RenderResources(GameEngine engine, int x, int y)
        {
            var resources = engine.VillageResources.GetAll();
            var keyResources = new[]
            {
                (ResourceType.Wood, UIIconType.Wood, "♣", new Color(150, 100, 60, 255)),
                (ResourceType.Stone, UIIconType.Stone, "▲", new Color(150, 150, 160, 255)),
                (ResourceType.Grain, UIIconType.Grain, "≡", new Color(200, 180, 100, 255)),
                (ResourceType.Tools, UIIconType.Tools, "╬", new Color(180, 160, 140, 255)),
                (ResourceType.Firewood, UIIconType.Firewood, "♠", new Color(255, 150, 50, 255))
            };

            var currentX = x;
            foreach (var (type, iconType, asciiFallback, color) in keyResources)
            {
                var amount = resources.ContainsKey(type) ? resources[type] : 0;
                var textColor = amount > 0 ? new Color(200, 200, 200, 255) : new Color(80, 80, 80, 255);

                // Draw icon using sprite (with ASCII fallback)
                currentX += GraphicsConfig.DrawUIIcon(
                    iconType, 
                    currentX, y, 
                    SmallFontSize, 
                    amount > 0 ? color : new Color(60, 60, 60, 255),
                    asciiFallback
                );
                currentX += 5;

                // Draw amount
                var amountText = amount.ToString();
                GraphicsConfig.DrawConsoleText(amountText, currentX, y, SmallFontSize, textColor);
                currentX += GraphicsConfig.MeasureText(amountText, SmallFontSize) + 20;
            }
        }

        private void RenderPopulationAndBuildings(GameEngine engine, int x, int y)
        {
            var totalPopulation = engine.Families.Sum(f => f.Members.Count);
            var constructedBuildings = engine.Buildings.Count(b => b.IsConstructed);
            var totalBuildings = engine.Buildings.Count;
            var pendingCommands = engine.CommandQueue.GetPendingCommandCount();

            // Population icon
            x += GraphicsConfig.DrawUIIcon(
                UIIconType.People,
                x, y,
                SmallFontSize,
                new Color(255, 200, 150, 255),
                "☺"
            );
            x += 5;
            GraphicsConfig.DrawConsoleText($"{totalPopulation}", x, y, SmallFontSize, new Color(200, 200, 200, 255));

            // Buildings icon
            x += GraphicsConfig.MeasureText(totalPopulation.ToString(), SmallFontSize) + 25;
            x += GraphicsConfig.DrawUIIcon(
                UIIconType.Buildings,
                x, y,
                SmallFontSize,
                new Color(150, 150, 150, 255),
                "▓"
            );
            x += 5;
            GraphicsConfig.DrawConsoleText($"{constructedBuildings}/{totalBuildings}", x, y, SmallFontSize, new Color(200, 200, 200, 255));

            // Pending commands icon
            x += GraphicsConfig.MeasureText($"{constructedBuildings}/{totalBuildings}", SmallFontSize) + 25;
            if (pendingCommands > 0)
            {
                x += GraphicsConfig.DrawUIIcon(
                    UIIconType.Settings,
                    x, y,
                    SmallFontSize,
                    new Color(255, 200, 100, 255),
                    "◊"
                );
                x += 5;
                GraphicsConfig.DrawConsoleText($"{pendingCommands}", x, y, SmallFontSize, new Color(255, 200, 100, 255));
            }
            else
            {
                x += GraphicsConfig.DrawUIIcon(
                    UIIconType.Settings,
                    x, y,
                    SmallFontSize,
                    new Color(60, 60, 60, 255),
                    "◊"
                );
                x += 5;
                GraphicsConfig.DrawConsoleText("0", x, y, SmallFontSize, new Color(100, 100, 100, 255));
            }
        }

        private string GetSeasonAscii(Season season)
        {
            return season switch
            {
                Season.Spring => "Spr",
                Season.Summer => "Sum",
                Season.Fall => "Fal",
                Season.Winter => "Win",
                _ => "???"
            };
        }

        private Color GetSeasonColor(Season season)
        {
            return season switch
            {
                Season.Spring => new Color(150, 255, 150, 255),
                Season.Summer => new Color(255, 255, 100, 255),
                Season.Fall => new Color(255, 150, 50, 255),
                Season.Winter => new Color(150, 200, 255, 255),
                _ => Color.White
            };
        }

        private string GetWeatherAscii(WeatherCondition condition)
        {
            return condition switch
            {
                WeatherCondition.Clear => "○",      // Clear sun
                WeatherCondition.Cloudy => "◐",    // Partial cloud
                WeatherCondition.Rain => "≈",      // Rain waves
                WeatherCondition.Snow => "❄",      // Snowflake (if supported) or "*"
                WeatherCondition.Storm => "≋",     // Storm waves
                WeatherCondition.Blizzard => "※",  // Blizzard star
                _ => "?"
            };
        }

        private Color GetTemperatureColor(int temperature)
        {
            if (temperature < 0) return new Color(100, 150, 255, 255); // Cold blue
            if (temperature < 15) return new Color(150, 200, 255, 255); // Cool blue
            if (temperature < 25) return new Color(255, 255, 200, 255); // Warm yellow
            return new Color(255, 100, 100, 255); // Hot red
        }

        private string GetTimeOfDayIcon(TimeOfDay timeOfDay)
        {
            return timeOfDay switch
            {
                TimeOfDay.Morning => "☀",   // Sun
                TimeOfDay.Afternoon => "☀", // Sun with rays
                TimeOfDay.Evening => "☽",   // Crescent moon
                TimeOfDay.Night => "☾",     // Full moon
                _ => "☀"
            };
        }

        private Color GetTimeOfDayColor(TimeOfDay timeOfDay)
        {
            return timeOfDay switch
            {
                TimeOfDay.Morning => new Color(255, 255, 150, 255),
                TimeOfDay.Afternoon => new Color(255, 220, 100, 255),
                TimeOfDay.Evening => new Color(150, 150, 255, 255),
                TimeOfDay.Night => new Color(100, 100, 150, 255),
                _ => Color.White
            };
        }
    }
}