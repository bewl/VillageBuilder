using System.Linq;
using Raylib_cs;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Game.Core.Selection;

namespace VillageBuilder.Game.Graphics.UI.Panels
{
    /// <summary>
    /// Panel for displaying detailed information about selected buildings.
    /// Phase 5: Modular UI panel system - Enhanced with construction progress and worker details.
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

            // Building identity
            GraphicsConfig.DrawConsoleText(building.Name, x, y, context.SmallFontSize, new Color(255, 200, 100, 255));
            y += context.LineHeight;

            GraphicsConfig.DrawConsoleText($"Type: {building.Type}", x, y, context.SmallFontSize, Color.White);
            y += context.LineHeight;

            GraphicsConfig.DrawConsoleText($"Location: ({building.X}, {building.Y})", x, y, context.SmallFontSize, Color.LightGray);
            y += context.LineHeight + 5;

            // Construction status
            if (!building.IsConstructed)
            {
                var stage = building.GetConstructionStage();
                var progress = building.GetConstructionProgressPercent();

                GraphicsConfig.DrawConsoleText("Status: Under Construction", x, y, context.SmallFontSize, 
                    new Color(255, 150, 50, 255));
                y += context.LineHeight;

                GraphicsConfig.DrawConsoleText($"Stage: {stage}", x, y, context.SmallFontSize, Color.Yellow);
                y += context.LineHeight;

                // Visual progress bar
                DrawStatBar("Progress", progress, 100, x, y, context.Width - 20, context.SmallFontSize);
                y += context.LineHeight + 10;

                // Construction workers
                var workerCount = building.ConstructionWorkers.Count;
                GraphicsConfig.DrawConsoleText($"Builders: {workerCount}", x, y, context.SmallFontSize, 
                    new Color(150, 255, 150, 255));
                y += context.LineHeight;

                if (workerCount > 0)
                {
                    var workersByFamily = building.ConstructionWorkers.GroupBy(w => w.Family).Where(g => g.Key != null);
                    foreach (var familyGroup in workersByFamily)
                    {
                        var family = familyGroup.Key!;
                        GraphicsConfig.DrawConsoleText($"  • {family.FamilyName} ({familyGroup.Count()})", 
                            x + 10, y, context.SmallFontSize - 2, Color.LightGray);
                        y += context.LineHeight;
                    }
                }
                else
                {
                    GraphicsConfig.DrawConsoleText("  (No builders assigned)", x + 10, y, context.SmallFontSize - 2, 
                        new Color(255, 150, 50, 255));
                    y += context.LineHeight;
                }
            }
            else
            {
                GraphicsConfig.DrawConsoleText("Status: Operational", x, y, context.SmallFontSize, Color.Green);
                y += context.LineHeight + 10;

                // Show workers or residents
                if (building.Type == BuildingType.House)
                {
                    var residentCount = building.Residents.Count;
                    GraphicsConfig.DrawConsoleText($"Residents: {residentCount}", x, y, context.SmallFontSize, 
                        new Color(150, 255, 150, 255));
                    y += context.LineHeight;

                    if (residentCount > 0)
                    {
                        var residentsByFamily = building.Residents.Where(r => r.IsAlive).GroupBy(r => r.Family).Where(g => g.Key != null);
                        foreach (var familyGroup in residentsByFamily)
                        {
                            var family = familyGroup.Key!;
                            GraphicsConfig.DrawConsoleText($"  • {family.FamilyName} ({familyGroup.Count()})", 
                                x + 10, y, context.SmallFontSize - 2, Color.LightGray);
                            y += context.LineHeight;
                        }
                    }
                }
                else
                {
                    var workerCount = building.Workers.Count;
                    GraphicsConfig.DrawConsoleText($"Workers: {workerCount}", x, y, context.SmallFontSize, 
                        new Color(150, 255, 150, 255));
                    y += context.LineHeight;

                    if (workerCount > 0)
                    {
                        var workersByFamily = building.Workers.GroupBy(w => w.Family).Where(g => g.Key != null);
                        foreach (var familyGroup in workersByFamily)
                        {
                            var family = familyGroup.Key!;
                            GraphicsConfig.DrawConsoleText($"  • {family.FamilyName} ({familyGroup.Count()})", 
                                x + 10, y, context.SmallFontSize - 2, Color.LightGray);
                            y += context.LineHeight;
                        }
                    }

                    // Production status for resource buildings
                    if (building.Type == BuildingType.Farm || 
                        building.Type == BuildingType.Mine ||
                        building.Type == BuildingType.Lumberyard)
                    {
                        y += 5;
                        var productionStatus = workerCount > 0 ? "Production Active" : "Production Idle";
                        var productionColor = workerCount > 0 ? Color.Green : Color.Gray;
                        GraphicsConfig.DrawConsoleText(productionStatus, x, y, context.SmallFontSize, productionColor);
                        y += context.LineHeight;
                    }
                }
            }

            return y - context.StartY;
        }
    }
}
