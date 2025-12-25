using Raylib_cs;
using VillageBuilder.Engine.Entities.Wildlife;
using VillageBuilder.Game.Core.Selection;

namespace VillageBuilder.Game.Graphics.UI.Panels
{
    /// <summary>
    /// Panel for displaying detailed information about selected wildlife.
    /// Phase 5: Modular UI panel system.
    /// </summary>
    public class WildlifeInfoPanel : BasePanel
    {
        public override string Title => "Wildlife Info";
        
        protected override void RenderContent(int x, int y, int width, int height)
        {
            var coordinator = Context?.SelectionManager as SelectionCoordinator;
            var wildlife = coordinator?.SelectedWildlife;
            
            if (wildlife == null) return;
            
            int currentY = y;
            
            // Type and basic info
            DrawText($"Type: {wildlife.Type}", x, currentY, Color.White);
            currentY += LineHeight;
            
            DrawText($"Age: {wildlife.Age} days", x, currentY, Color.LightGray);
            currentY += LineHeight;
            
            // Health
            var healthColor = wildlife.Health > 70 ? Color.Green : 
                             wildlife.Health > 30 ? Color.Yellow : Color.Red;
            DrawText($"Health: {wildlife.Health}%", x, currentY, healthColor);
            currentY += LineHeight;
            
            // Hunger
            var hungerColor = wildlife.Hunger < 30 ? Color.Green :
                             wildlife.Hunger < 70 ? Color.Yellow : Color.Red;
            DrawText($"Hunger: {wildlife.Hunger}%", x, currentY, hungerColor);
            currentY += LineHeight;
            
            // Behavior
            var behaviorColor = wildlife.CurrentBehavior == WildlifeBehavior.Hunting ? Color.Red :
                               wildlife.CurrentBehavior == WildlifeBehavior.Fleeing ? Color.Yellow :
                               Color.Green;
            DrawText($"Behavior: {wildlife.CurrentBehavior}", x, currentY, behaviorColor);
            currentY += LineHeight;
            
            // Predator status
            if (wildlife.IsPredator)
            {
                DrawText("? Predator", x, currentY, Color.Orange);
            }
        }
    }
}
