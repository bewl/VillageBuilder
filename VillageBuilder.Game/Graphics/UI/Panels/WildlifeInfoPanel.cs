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
        public override bool CanRender(PanelRenderContext context)
        {
            var coordinator = context.SelectionManager as SelectionCoordinator;
            return coordinator?.SelectedWildlife != null;
        }

        public override int Render(PanelRenderContext context)
        {
            var coordinator = context.SelectionManager as SelectionCoordinator;
            var wildlife = coordinator?.SelectedWildlife;

            if (wildlife == null) return 0;

            int y = context.StartY;
            int x = context.StartX;

            DrawSectionHeader("WILDLIFE INFO", x, y, context.FontSize);
            y += context.LineHeight + 5;

            // Type and basic info
            GraphicsConfig.DrawConsoleText($"Type: {wildlife.Type}", x, y, context.SmallFontSize, Color.White);
            y += context.LineHeight;

            GraphicsConfig.DrawConsoleText($"Age: {wildlife.Age} days", x, y, context.SmallFontSize, Color.LightGray);
            y += context.LineHeight;

            // Health
            var healthColor = wildlife.Health > 70 ? Color.Green : 
                             wildlife.Health > 30 ? Color.Yellow : Color.Red;
            GraphicsConfig.DrawConsoleText($"Health: {wildlife.Health}%", x, y, context.SmallFontSize, healthColor);
            y += context.LineHeight;

            // Hunger
            var hungerColor = wildlife.Hunger < 30 ? Color.Green :
                             wildlife.Hunger < 70 ? Color.Yellow : Color.Red;
            GraphicsConfig.DrawConsoleText($"Hunger: {wildlife.Hunger}%", x, y, context.SmallFontSize, hungerColor);
            y += context.LineHeight;

            // Behavior
            var behaviorColor = wildlife.CurrentBehavior == WildlifeBehavior.Hunting ? Color.Red :
                               wildlife.CurrentBehavior == WildlifeBehavior.Fleeing ? Color.Yellow :
                               Color.Green;
            GraphicsConfig.DrawConsoleText($"Behavior: {wildlife.CurrentBehavior}", x, y, context.SmallFontSize, behaviorColor);
            y += context.LineHeight;

            // Predator status
            if (wildlife.IsPredator)
            {
                GraphicsConfig.DrawConsoleText("? Predator", x, y, context.SmallFontSize, Color.Orange);
                y += context.LineHeight;
            }

            return y - context.StartY;
        }
    }
}
