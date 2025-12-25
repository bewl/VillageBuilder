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
        public override string Title => "Person Info";
        
        protected override void RenderContent(int x, int y, int width, int height)
        {
            var coordinator = Context?.SelectionManager as SelectionCoordinator;
            var person = coordinator?.SelectedPerson;
            
            if (person == null) return;
            
            int currentY = y;
            
            // Name and basic info
            DrawText($"Name: {person.FirstName} {person.LastName}", x, currentY, Color.White);
            currentY += LineHeight;
            
            DrawText($"Age: {person.Age}", x, currentY, Color.LightGray);
            currentY += LineHeight;
            
            DrawText($"Gender: {person.Gender}", x, currentY, Color.LightGray);
            currentY += LineHeight;
            
            // Health
            var healthColor = person.Health > 70 ? Color.Green : 
                             person.Health > 30 ? Color.Yellow : Color.Red;
            DrawText($"Health: {person.Health}%", x, currentY, healthColor);
            currentY += LineHeight;
            
            // Hunger
            var hungerColor = person.Hunger < 30 ? Color.Green :
                             person.Hunger < 70 ? Color.Yellow : Color.Red;
            DrawText($"Hunger: {person.Hunger}%", x, currentY, hungerColor);
            currentY += LineHeight;
            
            // Task
            if (person.AssignedBuilding != null)
            {
                DrawText($"Working at: {person.AssignedBuilding.Type}", x, currentY, Color.Cyan);
                currentY += LineHeight;
            }
            else
            {
                DrawText("Status: Idle", x, currentY, Color.Gray);
                currentY += LineHeight;
            }
            
            // Family
            if (person.Family != null)
            {
                currentY += 5;
                DrawText($"Family: {person.Family.Members.Count(m => m.IsAlive)} members", x, currentY, Color.Yellow);
            }
        }
    }
}
