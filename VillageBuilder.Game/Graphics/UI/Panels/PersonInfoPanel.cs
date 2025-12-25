using System.Linq;
using Raylib_cs;
using VillageBuilder.Engine.Entities;
using VillageBuilder.Game.Core.Selection;

namespace VillageBuilder.Game.Graphics.UI.Panels
{
    /// <summary>
    /// Panel for displaying detailed information about a selected person/family.
    /// Phase 5: Modular UI panel system.
    /// </summary>
    public class PersonInfoPanel : BasePanel
    {
        public override bool CanRender(PanelRenderContext context)
        {
            var coordinator = context.SelectionManager as SelectionCoordinator;
            return coordinator?.SelectedPerson != null;
        }

        public override int Render(PanelRenderContext context)
        {
            var coordinator = context.SelectionManager as SelectionCoordinator;
            var person = coordinator?.SelectedPerson;

            if (person == null) return 0;

            int y = context.StartY;
            int x = context.StartX;

            DrawSectionHeader("PERSON INFO", x, y, context.FontSize);
            y += context.LineHeight + 5;

            // Name and basic info
            GraphicsConfig.DrawConsoleText($"{person.FirstName} {person.LastName}", x, y, context.SmallFontSize, Color.White);
            y += context.LineHeight;

            GraphicsConfig.DrawConsoleText($"Age: {person.Age}", x, y, context.SmallFontSize, Color.LightGray);
            y += context.LineHeight;

            GraphicsConfig.DrawConsoleText($"Gender: {person.Gender}", x, y, context.SmallFontSize, Color.LightGray);
            y += context.LineHeight;

            // Health
            var healthColor = person.Health > 70 ? Color.Green : 
                             person.Health > 30 ? Color.Yellow : Color.Red;
            GraphicsConfig.DrawConsoleText($"Health: {person.Health}%", x, y, context.SmallFontSize, healthColor);
            y += context.LineHeight;

            // Hunger
            var hungerColor = person.Hunger < 30 ? Color.Green :
                             person.Hunger < 70 ? Color.Yellow : Color.Red;
            GraphicsConfig.DrawConsoleText($"Hunger: {person.Hunger}%", x, y, context.SmallFontSize, hungerColor);
            y += context.LineHeight;

            // Task
            if (person.AssignedBuilding != null)
            {
                GraphicsConfig.DrawConsoleText($"Working at: {person.AssignedBuilding.Type}", x, y, context.SmallFontSize, new Color(0, 255, 255, 255));
                y += context.LineHeight;
            }
            else
            {
                GraphicsConfig.DrawConsoleText("Status: Idle", x, y, context.SmallFontSize, Color.Gray);
                y += context.LineHeight;
            }

            // Family
            if (person.Family != null)
            {
                y += 5;
                var familySize = person.Family.Members.Count(m => m.IsAlive);
                GraphicsConfig.DrawConsoleText($"Family: {familySize} members", x, y, context.SmallFontSize, Color.Yellow);
                y += context.LineHeight;
            }

            return y - context.StartY;
        }
    }
}
