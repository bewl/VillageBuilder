using System.Linq;
using Raylib_cs;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Game.Core.Selection;

namespace VillageBuilder.Game.Graphics.UI.Panels
{
    /// <summary>
    /// Panel for displaying detailed information about selected buildings.
    /// Phase 5: Modular UI panel system.
    /// </summary>
    public class BuildingInfoPanel : BasePanel
    {
        public override bool CanRender(PanelRenderContext context)
        {
            var coordinator = context.SelectionManager as SelectionCoordinator;
            return coordinator?.SelectedBuilding != null;
        }

        public override int Render(PanelRenderContext context)
        {
            var coordinator = context.SelectionManager as SelectionCoordinator;
            var building = coordinator?.SelectedBuilding;

            if (building == null) return 0;

            int y = context.StartY;
            int x = context.StartX;

            DrawSectionHeader("BUILDING INFO", x, y, context.FontSize);
            y += context.LineHeight + 5;

            // Building type
            GraphicsConfig.DrawConsoleText($"Type: {building.Type}", x, y, context.SmallFontSize, Color.White);
            y += context.LineHeight;

            // Construction status
            if (!building.IsConstructed)
            {
                var stage = building.GetConstructionStage();
                var progress = building.GetConstructionProgressPercent();

                GraphicsConfig.DrawConsoleText($"Construction: {stage}", x, y, context.SmallFontSize, Color.Yellow);
                y += context.LineHeight;

                GraphicsConfig.DrawConsoleText($"Progress: {progress}%", x, y, context.SmallFontSize, new Color(0, 255, 255, 255));
                y += context.LineHeight;

                var workerCount = building.ConstructionWorkers.Count;
                GraphicsConfig.DrawConsoleText($"Workers: {workerCount}", x, y, context.SmallFontSize, Color.LightGray);
                y += context.LineHeight;
            }
            else
            {
                GraphicsConfig.DrawConsoleText("Status: Operational", x, y, context.SmallFontSize, Color.Green);
                y += context.LineHeight;

                // Workers
                if (building.Workers.Count > 0)
                {
                    GraphicsConfig.DrawConsoleText($"Workers: {building.Workers.Count}", x, y, context.SmallFontSize, new Color(0, 255, 255, 255));
                    y += context.LineHeight;
                }

                // Production (if applicable)
                if (building.Type == BuildingType.Farm || 
                    building.Type == BuildingType.Mine ||
                    building.Type == BuildingType.Lumberyard)
                {
                    y += 5;
                    GraphicsConfig.DrawConsoleText("Production Active", x, y, context.SmallFontSize, Color.Green);
                    y += context.LineHeight;
                }
            }

            return y - context.StartY;
        }
    }
}
