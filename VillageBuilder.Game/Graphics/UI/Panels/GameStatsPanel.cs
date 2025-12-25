using System.Linq;
using Raylib_cs;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Buildings;

namespace VillageBuilder.Game.Graphics.UI.Panels
{
    /// <summary>
    /// Panel for displaying comprehensive village statistics.
    /// Phase 5: Modular UI panel system - Enhanced with detailed breakdowns.
    /// </summary>
    public class GameStatsPanel : BasePanel
    {
        public override bool CanRender(PanelRenderContext context)
        {
            // Always render - this is the default stats panel
            return true;
        }

        public override int Render(PanelRenderContext context)
        {
            if (context.Engine == null) return 0;

            var engine = context.Engine;
            int y = context.StartY;
            int x = context.StartX;

            DrawSectionHeader("VILLAGE STATS", x, y, context.FontSize);
            y += context.LineHeight + 5;

            // Population breakdown
            var totalPeople = engine.Families.Sum(f => f.Members.Count(p => p.IsAlive));
            var adults = engine.Families.Sum(f => f.Members.Count(p => p.IsAlive && p.Age >= 18));
            var children = totalPeople - adults;

            GraphicsConfig.DrawConsoleText($"Population: {totalPeople}", x, y, context.SmallFontSize, Color.White);
            y += context.LineHeight;
            GraphicsConfig.DrawConsoleText($"  Adults: {adults}", x + 10, y, context.SmallFontSize - 2, Color.LightGray);
            y += context.LineHeight;
            GraphicsConfig.DrawConsoleText($"  Children: {children}", x + 10, y, context.SmallFontSize - 2, Color.LightGray);
            y += context.LineHeight;

            GraphicsConfig.DrawConsoleText($"Families: {engine.Families.Count}", x, y, context.SmallFontSize, Color.White);
            y += context.LineHeight + 5;

            // Buildings breakdown
            var constructedCount = engine.Buildings.Count(b => b.IsConstructed);
            var underConstruction = engine.Buildings.Count - constructedCount;

            GraphicsConfig.DrawConsoleText($"Buildings: {constructedCount}/{engine.Buildings.Count}", 
                x, y, context.SmallFontSize, Color.White);
            y += context.LineHeight;

            if (underConstruction > 0)
            {
                GraphicsConfig.DrawConsoleText($"  Under Construction: {underConstruction}", 
                    x + 10, y, context.SmallFontSize - 2, Color.Yellow);
                y += context.LineHeight;
            }

            // Building types breakdown
            var buildingGroups = engine.Buildings.Where(b => b.IsConstructed).GroupBy(b => b.Type);
            foreach (var group in buildingGroups)
            {
                GraphicsConfig.DrawConsoleText($"  {group.Key}: {group.Count()}", 
                    x + 10, y, context.SmallFontSize - 2, Color.LightGray);
                y += context.LineHeight;
            }

            y += 5;

            // Resources (if accessible)
            if (engine.VillageResources != null)
            {
                GraphicsConfig.DrawConsoleText("Resources:", x, y, context.SmallFontSize, Color.Yellow);
                y += context.LineHeight;

                var resourceList = engine.VillageResources.GetAll().OrderByDescending(r => r.Value);
                foreach (var resource in resourceList.Take(10)) // Show top 10
                {
                    var colorValue = new Color(0, 255, 255, 255);
                    GraphicsConfig.DrawConsoleText($"  {resource.Key}: {resource.Value}", 
                        x + 10, y, context.SmallFontSize - 2, colorValue);
                    y += context.LineHeight;
                }
            }

            y += 5;

            // Wildlife count
            if (engine.WildlifeManager != null)
            {
                var wildlifeCount = engine.WildlifeManager.Wildlife.Count(w => w.IsAlive);
                var predators = engine.WildlifeManager.Wildlife.Count(w => w.IsAlive && w.IsPredator);
                var prey = wildlifeCount - predators;

                GraphicsConfig.DrawConsoleText($"Wildlife: {wildlifeCount}", x, y, context.SmallFontSize, Color.Green);
                y += context.LineHeight;
                GraphicsConfig.DrawConsoleText($"  Predators: {predators}", x + 10, y, context.SmallFontSize - 2, 
                    new Color(255, 150, 150, 255));
                y += context.LineHeight;
                GraphicsConfig.DrawConsoleText($"  Prey: {prey}", x + 10, y, context.SmallFontSize - 2, 
                    new Color(150, 255, 150, 255));
                y += context.LineHeight;
            }

            return y - context.StartY;
        }
    }
}
