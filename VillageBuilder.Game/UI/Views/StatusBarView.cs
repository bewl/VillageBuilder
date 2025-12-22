using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Resources;
using VillageBuilder.Game.UI.ViewModels;

namespace VillageBuilder.Game.UI.Views
{
    public class StatusBarView : FrameView
    {
        private readonly GameViewModel _viewModel;
        private Label _timeLabel = null!;
        private Label _weatherLabel = null!;
        private Label _resourcesLabel = null!;
        private Label _populationLabel = null!;
        private Label _speedLabel = null!;

        public StatusBarView(GameViewModel viewModel) : base("Village Status")
        {
            _viewModel = viewModel;
            InitializeLabels();
        }

        private void InitializeLabels()
        {
            // Row 1: Time and Weather
            _timeLabel = new Label("Time: Loading...")
            {
                X = 1,
                Y = 0,
                Width = 40
            };

            _weatherLabel = new Label("Weather: Loading...")
            {
                X = Pos.Right(_timeLabel) + 2,
                Y = 0,
                Width = 30
            };

            _speedLabel = new Label($"Speed: {_viewModel.TimeScale:F2}x")
            {
                X = Pos.Right(_weatherLabel) + 2,
                Y = 0,
                Width = 20
            };

            // Row 2: Key Resources
            _resourcesLabel = new Label("Resources: Loading...")
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill() - 2
            };

            // Row 3: Population and Buildings
            _populationLabel = new Label("Population: Loading...")
            {
                X = 1,
                Y = 2,
                Width = Dim.Fill() - 2
            };

            Add(_timeLabel, _weatherLabel, _speedLabel, _resourcesLabel, _populationLabel);
        }

        public void UpdateStatus(GameEngine engine)
        {
            // Time
            var time = engine.Time;
            _timeLabel.Text = $"⏰ Year {time.Year} | {time.CurrentSeason} Day {time.DayOfSeason + 1} | {time.Hour:D2}:00";

            // Weather
            var weather = engine.Weather;
            var weatherIcon = weather.Condition switch
            {
                VillageBuilder.Engine.World.WeatherCondition.Clear => "☀️",
                VillageBuilder.Engine.World.WeatherCondition.Cloudy => "☁️",
                VillageBuilder.Engine.World.WeatherCondition.Rain => "🌧️",
                VillageBuilder.Engine.World.WeatherCondition.Snow => "❄️",
                VillageBuilder.Engine.World.WeatherCondition.Storm => "⛈️",
                VillageBuilder.Engine.World.WeatherCondition.Blizzard => "🌨️",
                _ => "?"
            };
            _weatherLabel.Text = $"{weatherIcon} {weather.Condition} | {weather.Temperature}°C";

            // Speed
            var pausedText = _viewModel.IsPaused ? "[PAUSED]" : "[RUNNING]";
            _speedLabel.Text = $"⚡ {_viewModel.TimeScale:F2}x {pausedText}";

            // Resources (show top 5 most important)
            var resources = engine.VillageResources.GetAll();
            var keyResources = new[] { ResourceType.Wood, ResourceType.Stone, ResourceType.Grain, ResourceType.Tools, ResourceType.Firewood };
            var resourceText = string.Join(" | ", keyResources.Select(r => 
            {
                var icon = GetResourceIcon(r);
                var amount = resources.ContainsKey(r) ? resources[r] : 0;
                return $"{icon}{r}: {amount}";
            }));
            _resourcesLabel.Text = $"📦 {resourceText}";

            // Population and Buildings
            var totalPopulation = engine.Families.Sum(f => f.Members.Count);
            var constructedBuildings = engine.Buildings.Count(b => b.IsConstructed);
            var totalBuildings = engine.Buildings.Count;
            var pendingCommands = engine.CommandQueue.GetPendingCommandCount();
            
            _populationLabel.Text = $"👥 Pop: {totalPopulation} | 🏠 Buildings: {constructedBuildings}/{totalBuildings} | 📋 Pending: {pendingCommands}";
        }

        private string GetResourceIcon(ResourceType type)
        {
            return type switch
            {
                ResourceType.Wood => "🪵",
                ResourceType.Stone => "🪨",
                ResourceType.Grain => "🌾",
                ResourceType.Tools => "🔨",
                ResourceType.Firewood => "🔥",
                ResourceType.Iron => "⚒️",
                ResourceType.Coal => "⚫",
                _ => "📦"
            };
        }
    }
}
