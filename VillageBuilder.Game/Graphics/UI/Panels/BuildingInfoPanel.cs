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
        public override string Title => "Building Info";
        
        protected override void RenderContent(int x, int y, int width, int height)
        {
            var coordinator = Context?.SelectionManager as SelectionCoordinator;
            var building = coordinator?.SelectedBuilding;
            
            if (building == null) return;
            
            int currentY = y;
            
            // Building type
            DrawText($"Type: {building.Type}", x, currentY, Color.White);
            currentY += LineHeight;
            
            // Construction status
            if (!building.IsConstructed)
            {
                var stage = building.GetConstructionStage();
                var progress = building.GetConstructionProgressPercent();
                
                DrawText($"Construction: {stage}", x, currentY, Color.Yellow);
                currentY += LineHeight;
                
                DrawText($"Progress: {progress}%", x, currentY, Color.Cyan);
                currentY += LineHeight;
                
                var workerCount = building.ConstructionWorkers.Count;
                DrawText($"Workers: {workerCount}", x, currentY, Color.LightGray);
                currentY += LineHeight;
            }
            else
            {
                DrawText("Status: Operational", x, currentY, Color.Green);
                currentY += LineHeight;
                
                // Workers
                if (building.AssignedWorkers.Count > 0)
                {
                    DrawText($"Workers: {building.AssignedWorkers.Count}", x, currentY, Color.Cyan);
                    currentY += LineHeight;
                }
                
                // Production (if applicable)
                if (building.Type == BuildingType.Farm || 
                    building.Type == BuildingType.Mine ||
                    building.Type == BuildingType.Lumberyard)
                {
                    currentY += 5;
                    DrawText("Production Active", x, currentY, Color.Green);
                }
            }
        }
    }
}
