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
        public override string Title => "Village Stats";
        
        protected override void RenderContent(int x, int y, int width, int height)
        {
            if (Context?.Engine == null) return;
            
            var engine = Context.Engine;
            int currentY = y;
            
            // Population
            var totalPeople = engine.Families.Sum(f => f.Members.Count(p => p.IsAlive));
            DrawText($"Population: {totalPeople}", x, currentY, Color.White);
            currentY += LineHeight;
            
            // Buildings
            DrawText($"Buildings: {engine.Buildings.Count}", x, currentY, Color.LightGray);
            currentY += LineHeight;
            
            // Resources (if accessible)
            if (engine.VillageResources != null)
            {
                currentY += 5;
                DrawText("Resources:", x, currentY, Color.Yellow);
                currentY += LineHeight;
                
                foreach (var resource in engine.VillageResources.GetAll())
                {
                    DrawText($"  {resource.Key}: {resource.Value}", x + 10, currentY, Color.Cyan);
                    currentY += LineHeight;
                }
            }
            
            // Wildlife count
            if (engine.WildlifeManager != null)
            {
                currentY += 5;
                var wildlifeCount = engine.WildlifeManager.Wildlife.Count(w => w.IsAlive);
                DrawText($"Wildlife: {wildlifeCount}", x, currentY, Color.Green);
            }
        }
    }
}
