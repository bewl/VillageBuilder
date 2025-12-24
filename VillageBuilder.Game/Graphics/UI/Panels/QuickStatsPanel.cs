using Raylib_cs;
using VillageBuilder.Engine.Core;

namespace VillageBuilder.Game.Graphics.UI.Panels
{
    /// <summary>
    /// Panel showing quick statistics (families, population, buildings, saves).
    /// Displayed when nothing is selected.
    /// </summary>
    public class QuickStatsPanel : BasePanel
    {
        public override int Priority => 10; // Low priority - only when nothing selected
        
        public override bool CanRender(PanelRenderContext context)
        {
            // Render when nothing is selected
            return context.SelectionManager == null || !context.SelectionManager.HasSelection();
        }
        
        public override int Render(PanelRenderContext context)
        {
            var y = context.StartY;
            var engine = context.Engine;
            var x = context.StartX;
            var textColor = new Color(200, 200, 200, 255);
            var dimColor = new Color(120, 120, 120, 255);
            
            DrawSectionHeader("QUICK STATS", x, y, context.FontSize);
            y += context.LineHeight + 5;
            
            // Families and Population
            if (engine.Families.Any())
            {
                var totalPopulation = engine.Families.Sum(f => f.Members.Count);
                
                var currentX = x + 2;
                currentX += GraphicsConfig.DrawUIIcon(UIIconType.People, currentX, y, context.SmallFontSize, textColor, "?");
                GraphicsConfig.DrawConsoleText($" Families:    {engine.Families.Count}", currentX, y, context.SmallFontSize, textColor);
                y += context.LineHeight;
                
                GraphicsConfig.DrawConsoleText($"     Population:  {totalPopulation}", x, y, context.SmallFontSize, textColor);
                y += context.LineHeight + 5;
            }
            else
            {
                var currentX = x + 2;
                currentX += GraphicsConfig.DrawUIIcon(UIIconType.People, currentX, y, context.SmallFontSize, dimColor, "?");
                GraphicsConfig.DrawConsoleText(" No families yet", currentX, y, context.SmallFontSize, dimColor);
                y += context.LineHeight + 5;
            }
            
            // Buildings summary
            var constructedCount = engine.Buildings.Count(b => b.IsConstructed);
            var buildingX = x + 2;
            buildingX += GraphicsConfig.DrawUIIcon(UIIconType.Construction, buildingX, y, context.SmallFontSize, textColor, "+");
            GraphicsConfig.DrawConsoleText($" Buildings:   {constructedCount}/{engine.Buildings.Count}", 
                buildingX, y, context.SmallFontSize, textColor);
            y += context.LineHeight + 5;
            
            // Building breakdown (top 5 types)
            var buildingGroups = engine.Buildings.GroupBy(b => b.Type).Take(5);
            foreach (var group in buildingGroups)
            {
                var constructed = group.Count(b => b.IsConstructed);
                var icon = GetBuildingIcon(group.Key);
                GraphicsConfig.DrawConsoleTextAuto($"     {icon} {group.Key,-12} {constructed}", 
                    x, y, context.SmallFontSize, textColor);
                y += context.LineHeight;
            }
            
            y += 5;
            DrawDivider(x, y, context.Width - context.Padding * 2);
            
            return y + context.LineHeight;
        }
        
        private string GetBuildingIcon(Engine.Buildings.BuildingType type)
        {
            return type switch
            {
                Engine.Buildings.BuildingType.House => "??",
                Engine.Buildings.BuildingType.Warehouse => "??",
                Engine.Buildings.BuildingType.Workshop => "??",
                Engine.Buildings.BuildingType.Farm => "??",
                Engine.Buildings.BuildingType.Mine => "?",
                Engine.Buildings.BuildingType.Lumberyard => "??",
                Engine.Buildings.BuildingType.Market => "??",
                Engine.Buildings.BuildingType.TownHall => "??",
                _ => "??"
            };
        }
    }
}
