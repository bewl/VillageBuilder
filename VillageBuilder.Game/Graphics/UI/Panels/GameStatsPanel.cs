using System.Linq;
using Raylib_cs;
using VillageBuilder.Engine.Core;

namespace VillageBuilder.Game.Graphics.UI.Panels
{
    /// <summary>
    /// Panel for displaying overall village statistics.
    /// Phase 5: Modular UI panel system.
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

            // Population
            var totalPeople = engine.Families.Sum(f => f.Members.Count(p => p.IsAlive));
            GraphicsConfig.DrawConsoleText($"Population: {totalPeople}", x, y, context.SmallFontSize, Color.White);
            y += context.LineHeight;

            // Buildings
            GraphicsConfig.DrawConsoleText($"Buildings: {engine.Buildings.Count}", x, y, context.SmallFontSize, Color.LightGray);
            y += context.LineHeight;

            // Resources (if accessible)
            if (engine.VillageResources != null)
            {
                y += 5;
                GraphicsConfig.DrawConsoleText("Resources:", x, y, context.SmallFontSize, Color.Yellow);
                y += context.LineHeight;

                foreach (var resource in engine.VillageResources.GetAll())
                {
                    var colorValue = new Color(0, 255, 255, 255); // Cyan
                    GraphicsConfig.DrawConsoleText($"  {resource.Key}: {resource.Value}", x + 10, y, context.SmallFontSize, colorValue);
                    y += context.LineHeight;
                }
            }

            // Wildlife count
            if (engine.WildlifeManager != null)
            {
                y += 5;
                var wildlifeCount = engine.WildlifeManager.Wildlife.Count(w => w.IsAlive);
                GraphicsConfig.DrawConsoleText($"Wildlife: {wildlifeCount}", x, y, context.SmallFontSize, Color.Green);
                y += context.LineHeight;
            }

            return y - context.StartY;
        }
    }
}
