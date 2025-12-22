using Terminal.Gui;
using VillageBuilder.Engine.Core;
using System.Text;
using System.Linq;

namespace VillageBuilder.Game.UI.Views
{
    public class GameStateView : FrameView
    {
        private readonly TextView _contentView;

        public GameStateView() : base("Quick Stats")
        {
            _contentView = new TextView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ReadOnly = true,
                WordWrap = true
            };

            Add(_contentView);
        }

        public void UpdateState(GameEngine engine)
        {
            var sb = new StringBuilder();
            
            // Families (compact)
            if (engine.Families.Any())
            {
                var totalPopulation = engine.Families.Sum(f => f.Members.Count);
                sb.AppendLine($"👥 Families: {engine.Families.Count}");
                sb.AppendLine($"   Population: {totalPopulation}");
            }
            else
            {
                sb.AppendLine("👥 No families");
            }
            sb.AppendLine();
            
            // Buildings (compact summary)
            sb.AppendLine($"🏗️ Buildings: {engine.Buildings.Count(b => b.IsConstructed)}/{engine.Buildings.Count}");
            var buildingGroups = engine.Buildings.GroupBy(b => b.Type);
            foreach (var group in buildingGroups.OrderBy(g => g.Key.ToString()).Take(5))
            {
                var constructed = group.Count(b => b.IsConstructed);
                var icon = GetBuildingIcon(group.Key);
                sb.AppendLine($"  {icon} {group.Key}: {constructed}");
            }
            
            _contentView.Text = sb.ToString();
        }

        private string GetBuildingIcon(VillageBuilder.Engine.Buildings.BuildingType type)
        {
            return type switch
            {
                VillageBuilder.Engine.Buildings.BuildingType.House => "🏠",
                VillageBuilder.Engine.Buildings.BuildingType.Farm => "🌾",
                VillageBuilder.Engine.Buildings.BuildingType.Warehouse => "📦",
                VillageBuilder.Engine.Buildings.BuildingType.Mine => "⛏️",
                VillageBuilder.Engine.Buildings.BuildingType.Lumberyard => "🪓",
                VillageBuilder.Engine.Buildings.BuildingType.Workshop => "🔨",
                VillageBuilder.Engine.Buildings.BuildingType.Market => "🏪",
                VillageBuilder.Engine.Buildings.BuildingType.Well => "🚰",
                VillageBuilder.Engine.Buildings.BuildingType.TownHall => "🏛️",
                _ => "🏗️"
            };
        }
    }
}