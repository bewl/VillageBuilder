using System;
using Raylib_cs;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Entities;
using System.Numerics;

namespace VillageBuilder.Game.Graphics.UI
{
    public class TooltipRenderer
    {
        private const int Padding = 8;
        private const int LineHeight = 22;
        private const int FontSize = 18;
        private const int MaxWidth = 350;

        public void RenderTooltip(GameEngine engine, Person? hoveredPerson, Building? hoveredBuilding, int mouseX, int mouseY)
        {
            if (hoveredPerson != null)
            {
                RenderPersonTooltip(hoveredPerson, mouseX, mouseY);
            }
            else if (hoveredBuilding != null)
            {
                RenderBuildingTooltip(hoveredBuilding, mouseX, mouseY);
            }
        }

        private void RenderPersonTooltip(Person person, int mouseX, int mouseY)
        {
            var familyInfo = person.Family != null ? $"Family: {person.Family.FamilyName}" : "No family";
            
            var lines = new[]
            {
                $"{person.FullName}",
                $"Age: {person.Age} | {person.Gender}",
                familyInfo,
                $"Task: {person.CurrentTask}",
                $"Energy: {person.Energy}/100",
                $"Hunger: {person.Hunger}/100",
                person.AssignedBuilding != null ? $"Working at: {person.AssignedBuilding.Name}" : "Not assigned",
                "", // Spacer
                "(Click to select | ESC to close)"
            };

            DrawTooltipBox(lines, mouseX, mouseY, new Color(100, 150, 255, 240));
        }

        private void RenderBuildingTooltip(Building building, int mouseX, int mouseY)
        {
            var lines = new[]
            {
                $"{building.Name}",
                $"Type: {building.Type}",
                $"Status: {(building.IsConstructed ? "Constructed" : $"Building... {building.ConstructionProgress}%")}",
                $"Workers: {building.Workers.Count}",
                $"Position: ({building.X}, {building.Y})",
                $"Rotation: {building.Rotation}"
            };

            DrawTooltipBox(lines, mouseX, mouseY, new Color(200, 150, 100, 240));
        }

        private void DrawTooltipBox(string[] lines, int mouseX, int mouseY, Color bgColor)
        {
            // Calculate dimensions
            int maxLineWidth = 0;
            foreach (var line in lines)
            {
                int width = GraphicsConfig.MeasureText(line, FontSize);
                if (width > maxLineWidth) maxLineWidth = width;
            }

            int boxWidth = Math.Min(maxLineWidth + Padding * 2, MaxWidth);
            int boxHeight = lines.Length * LineHeight + Padding * 2;

            // Position tooltip near mouse, but keep on screen
            int tooltipX = mouseX + 15;
            int tooltipY = mouseY + 15;

            // Keep tooltip on screen
            if (tooltipX + boxWidth > GraphicsConfig.ScreenWidth)
                tooltipX = mouseX - boxWidth - 15;
            if (tooltipY + boxHeight > GraphicsConfig.ScreenHeight)
                tooltipY = mouseY - boxHeight - 15;

            // Draw background
            Raylib.DrawRectangle(tooltipX, tooltipY, boxWidth, boxHeight, bgColor);

            // Draw border
            Raylib.DrawRectangleLines(tooltipX, tooltipY, boxWidth, boxHeight, new Color(255, 255, 255, 255));

            // Draw text
            var textColor = new Color(255, 255, 255, 255);
            int textY = tooltipY + Padding;

            foreach (var line in lines)
            {
                GraphicsConfig.DrawConsoleText(line, tooltipX + Padding, textY, FontSize, textColor);
                textY += LineHeight;
            }
        }
    }
}
