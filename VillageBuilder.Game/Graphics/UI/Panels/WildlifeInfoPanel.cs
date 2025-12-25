using System.Linq;
using Raylib_cs;
using VillageBuilder.Engine.Entities.Wildlife;
using VillageBuilder.Game.Core.Selection;

namespace VillageBuilder.Game.Graphics.UI.Panels
{
    /// <summary>
    /// Panel for displaying detailed information about selected wildlife.
    /// Phase 5: Modular UI panel system - Enhanced with comprehensive ecology data.
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

            // Multiple wildlife indicator
            if (coordinator?.HasMultipleWildlife() == true && coordinator.WildlifeAtSelectedTile != null)
            {
                var count = coordinator.WildlifeAtSelectedTile.Count;
                var index = coordinator.SelectedWildlifeIndex + 1;
                GraphicsConfig.DrawConsoleText($"Wildlife {index} of {count} on this tile", 
                    x, y, context.SmallFontSize - 2, new Color(255, 200, 100, 255));
                y += context.LineHeight;
                GraphicsConfig.DrawConsoleText("[Arrows/Tab] to switch", x, y, context.SmallFontSize - 2, 
                    new Color(120, 120, 150, 255));
                y += context.LineHeight + 5;
            }

            // Wildlife identity
            var typeColor = wildlife.IsPredator ? new Color(255, 100, 100, 255) : new Color(150, 255, 150, 255);
            GraphicsConfig.DrawConsoleText(wildlife.Name, x, y, context.SmallFontSize, new Color(255, 255, 100, 255));
            y += context.LineHeight;

            var categoryText = wildlife.IsPredator ? "Predator" : "Prey";
            GraphicsConfig.DrawConsoleText($"Type: {wildlife.Type} ({categoryText})", x, y, context.SmallFontSize, typeColor);
            y += context.LineHeight;

            GraphicsConfig.DrawConsoleText($"Age: {wildlife.Age} days | {wildlife.Gender}", x, y, context.SmallFontSize, Color.LightGray);
            y += context.LineHeight;

            // Current behavior with color coding
            var behaviorColor = GetWildlifeBehaviorColor(wildlife.CurrentBehavior);
            GraphicsConfig.DrawConsoleText($"Behavior: {wildlife.CurrentBehavior}", x, y, context.SmallFontSize, behaviorColor);
            y += context.LineHeight + 10;

            // Stats with bars
            DrawStatBar("Health", wildlife.Health, 100, x, y, context.Width - 20, context.SmallFontSize);
            y += context.LineHeight + 5;

            DrawStatBar("Hunger", wildlife.Hunger, 100, x, y, context.Width - 20, context.SmallFontSize);
            y += context.LineHeight + 5;

            DrawStatBar("Energy", wildlife.Energy, 100, x, y, context.Width - 20, context.SmallFontSize);
            y += context.LineHeight + 5;

            if (wildlife.Fear > 0)
            {
                DrawStatBar("Fear", wildlife.Fear, 100, x, y, context.Width - 20, context.SmallFontSize);
                y += context.LineHeight + 5;
            }

            y += 5;

            // Ecology section
            DrawSectionHeader("ECOLOGY", x, y, context.FontSize);
            y += context.LineHeight + 5;

            if (wildlife.IsPredator && wildlife.PreyTypes.Count > 0)
            {
                GraphicsConfig.DrawConsoleText($"Hunts: {string.Join(", ", wildlife.PreyTypes)}", 
                    x, y, context.SmallFontSize, new Color(255, 150, 150, 255));
                y += context.LineHeight;
            }

            if (wildlife.IsPrey && wildlife.PredatorTypes.Count > 0)
            {
                GraphicsConfig.DrawConsoleText($"Fears: {string.Join(", ", wildlife.PredatorTypes)}", 
                    x, y, context.SmallFontSize, new Color(255, 200, 100, 255));
                y += context.LineHeight;
            }

            // Resource drops
            if (wildlife.ResourceDrops.Count > 0)
            {
                var resources = string.Join(", ", wildlife.ResourceDrops.Select(r => $"{r.Value} {r.Key}"));
                GraphicsConfig.DrawConsoleText($"Drops: {resources}", x, y, context.SmallFontSize, 
                    new Color(150, 255, 150, 255));
                y += context.LineHeight;
            }

            return y - context.StartY;
        }

        private Color GetWildlifeBehaviorColor(WildlifeBehavior behavior)
        {
            return behavior switch
            {
                WildlifeBehavior.Idle => new Color(200, 200, 200, 255),
                WildlifeBehavior.Grazing => new Color(150, 255, 150, 255),
                WildlifeBehavior.Wandering => new Color(200, 200, 255, 255),
                WildlifeBehavior.Fleeing => new Color(255, 255, 0, 255),
                WildlifeBehavior.Hunting => new Color(255, 100, 100, 255),
                WildlifeBehavior.Eating => new Color(255, 200, 100, 255),
                WildlifeBehavior.Resting => new Color(150, 150, 255, 255),
                WildlifeBehavior.Breeding => new Color(255, 150, 255, 255),
                WildlifeBehavior.Dead => new Color(100, 100, 100, 255),
                _ => new Color(150, 150, 150, 255)
            };
        }
    }
}
