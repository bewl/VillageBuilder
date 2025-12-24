using System;
using System.Linq;
using Raylib_cs;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Entities;
using VillageBuilder.Engine.Commands.PersonCommands;
using VillageBuilder.Game.Core;

namespace VillageBuilder.Game.Graphics.UI
{
    public class SelectionPanelRenderer
    {
        private const int Padding = 10;
        private const int LineHeight = 22;
        // Font size now dynamic from GraphicsConfig
        private static int FontSize => GraphicsConfig.ConsoleFontSize;
        private const int ButtonHeight = 30;
        private const int PanelWidth = 300;

        public void Render(GameEngine engine, SelectionManager selectionManager, int mouseX, int mouseY, bool mouseClicked)
        {
            if (!selectionManager.HasSelection())
                return;

            if (selectionManager.SelectedPerson != null)
            {
                RenderPersonPanel(engine, selectionManager.SelectedPerson, mouseX, mouseY, mouseClicked);
            }
            else if (selectionManager.SelectedBuilding != null)
            {
                RenderBuildingPanel(engine, selectionManager.SelectedBuilding, mouseX, mouseY, mouseClicked);
            }
        }

        private void RenderPersonPanel(GameEngine engine, Person person, int mouseX, int mouseY, bool mouseClicked)
        {
            int panelX = GraphicsConfig.ScreenWidth - PanelWidth - 20;
            int panelY = GraphicsConfig.StatusBarHeight + 20;
            int currentY = panelY + Padding;

            // Calculate panel height
            int panelHeight = 180;

            // Draw background
            Raylib.DrawRectangle(panelX, panelY, PanelWidth, panelHeight, new Color(20, 20, 30, 240));
            Raylib.DrawRectangleLines(panelX, panelY, PanelWidth, panelHeight, new Color(100, 150, 255, 255));

            // Title
            var titleColor = new Color(255, 255, 255, 255);
            GraphicsConfig.DrawConsoleText($"{person.FullName}", panelX + Padding, currentY, FontSize, titleColor);
            currentY += LineHeight + 10;

            // Status
            var statusColor = new Color(200, 200, 200, 255);
            GraphicsConfig.DrawConsoleText($"Age: {person.Age} | {person.Gender}", panelX + Padding, currentY, FontSize, statusColor);
            currentY += LineHeight;
            
            if (person.Family != null)
            {
                GraphicsConfig.DrawConsoleText($"Family: {person.Family.FamilyName}", panelX + Padding, currentY, FontSize, statusColor);
                currentY += LineHeight;
            }
            
            GraphicsConfig.DrawConsoleText($"Task: {person.CurrentTask}", panelX + Padding, currentY, FontSize, statusColor);
            currentY += LineHeight;
            GraphicsConfig.DrawConsoleText($"Energy: {person.Energy}/100", panelX + Padding, currentY, FontSize, statusColor);
            currentY += LineHeight;
            GraphicsConfig.DrawConsoleText($"Hunger: {person.Hunger}/100", panelX + Padding, currentY, FontSize, statusColor);
            currentY += LineHeight + 10;

            // Current assignment
            if (person.AssignedBuilding != null)
            {
                GraphicsConfig.DrawConsoleText($"Working at: {person.AssignedBuilding.Type}", 
                    panelX + Padding, currentY, FontSize, new Color(150, 255, 150, 255));
                currentY += LineHeight;
            }
            else
            {
                GraphicsConfig.DrawConsoleText("Not assigned to work", 
                    panelX + Padding, currentY, FontSize, new Color(150, 150, 150, 255));
                currentY += LineHeight;
            }
            
            // Info message
            GraphicsConfig.DrawConsoleText("(Assign jobs by selecting buildings)", 
                panelX + Padding, currentY, 14, new Color(120, 120, 120, 255));
        }

        private void RenderBuildingPanel(GameEngine engine, Building building, int mouseX, int mouseY, bool mouseClicked)
        {
            int panelX = GraphicsConfig.ScreenWidth - PanelWidth - 20;
            int panelY = GraphicsConfig.StatusBarHeight + 20;
            int currentY = panelY + Padding;
            
            // Calculate available families
            int availableFamilies = engine.Families.Count(f => f.GetAdults().Any(p => p.IsAlive));
            
            // Calculate panel height
            int panelHeight = 250 + (availableFamilies * (ButtonHeight + 5));

            // Draw background
            Raylib.DrawRectangle(panelX, panelY, PanelWidth, panelHeight, new Color(20, 20, 30, 240));
            Raylib.DrawRectangleLines(panelX, panelY, PanelWidth, panelHeight, new Color(200, 150, 100, 255));

            // Title
            var titleColor = new Color(255, 255, 255, 255);
            GraphicsConfig.DrawConsoleText($"{building.Name}", panelX + Padding, currentY, FontSize, titleColor);
            currentY += LineHeight + 10;

            // Info
            var infoColor = new Color(200, 200, 200, 255);
            GraphicsConfig.DrawConsoleText($"Type: {building.Type}", panelX + Padding, currentY, FontSize, infoColor);
            currentY += LineHeight;
            GraphicsConfig.DrawConsoleText($"Status: {(building.IsConstructed ? "Operational" : "Under Construction")}", 
                panelX + Padding, currentY, FontSize, infoColor);
            currentY += LineHeight + 10;

            // Workers by family
            GraphicsConfig.DrawConsoleText($"Workers ({building.Workers.Count}):", panelX + Padding, currentY, FontSize, new Color(255, 200, 100, 255));
            currentY += LineHeight + 5;

            if (building.Workers.Count == 0)
            {
                GraphicsConfig.DrawConsoleText("  No workers assigned", panelX + Padding + 10, currentY, 16, new Color(150, 150, 150, 255));
                currentY += LineHeight + 10;
            }
            else
            {
                // Group workers by family
                var workersByFamily = building.Workers.GroupBy(w => w.Family).Where(g => g.Key != null);
                foreach (var familyGroup in workersByFamily)
                {
                    var family = familyGroup.Key!;
                    var workerCount = familyGroup.Count();
                    GraphicsConfig.DrawConsoleText($"  • {family.FamilyName} family ({workerCount})", 
                        panelX + Padding + 10, currentY, 16, infoColor);
                    currentY += LineHeight;
                }
                currentY += 5;
            }
            
            // Assign Families header
            GraphicsConfig.DrawConsoleText("Assign Family:", panelX + Padding, currentY, FontSize, new Color(150, 255, 150, 255));
            currentY += LineHeight + 5;

            // Family assignment buttons
            foreach (var family in engine.Families.Where(f => f.GetAdults().Any(p => p.IsAlive)))
            {
                var buttonX = panelX + Padding;
                var buttonY = currentY;
                var buttonWidth = PanelWidth - Padding * 2;

                bool isHovered = mouseX >= buttonX && mouseX <= buttonX + buttonWidth &&
                                mouseY >= buttonY && mouseY <= buttonY + ButtonHeight;

                // Check if family is already working here
                var familyWorkersHere = building.Workers.Count(w => w.Family?.Id == family.Id);
                var isAssigned = familyWorkersHere > 0;

                // Draw button
                var buttonColor = isAssigned ? new Color(40, 70, 40, 255) :
                                     isHovered ? new Color(60, 80, 120, 255) : 
                                     new Color(40, 50, 70, 255);
                Raylib.DrawRectangle(buttonX, buttonY, buttonWidth, ButtonHeight, buttonColor);
                Raylib.DrawRectangleLines(buttonX, buttonY, buttonWidth, ButtonHeight, new Color(100, 120, 160, 255));

                // Button text
                var adultCount = family.GetAdults().Count(p => p.IsAlive);
                var buttonText = isAssigned ? 
                    $"{family.FamilyName} (Working here)" :
                    $"{family.FamilyName} ({adultCount} adults)";
                GraphicsConfig.DrawConsoleText(buttonText, buttonX + 10, buttonY + 6, 16, new Color(255, 255, 255, 255));

                // Handle click
                if (isHovered && mouseClicked && !isAssigned)
                {
                    Console.WriteLine($"[JOB] Assigning {family.FamilyName} family to {building.Type}");
                    var command = new VillageBuilder.Engine.Commands.PersonCommands.AssignFamilyJobCommand(
                        0, engine.CurrentTick + 1, family.Id, building.Id);
                    engine.SubmitCommand(command);
                }

                currentY += ButtonHeight + 5;
            }
        }
    }
}
