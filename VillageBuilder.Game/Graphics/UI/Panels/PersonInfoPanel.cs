using System.Linq;
using Raylib_cs;
using VillageBuilder.Engine.Entities;
using VillageBuilder.Game.Core.Selection;

namespace VillageBuilder.Game.Graphics.UI.Panels
{
    /// <summary>
    /// Panel for displaying detailed information about a selected person/family.
    /// Phase 5: Modular UI panel system - Enhanced with comprehensive data display.
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

            // Show multiple people on tile if applicable
            if (coordinator?.HasMultiplePeople() == true && coordinator.PeopleAtSelectedTile != null)
            {
                var headerText = $"People on tile ({coordinator.PeopleAtSelectedTile.Count}):";
                GraphicsConfig.DrawConsoleText(headerText, x, y, context.SmallFontSize, new Color(150, 150, 255, 255));
                y += context.LineHeight;

                // List all people with selection indicator
                for (int i = 0; i < coordinator.PeopleAtSelectedTile.Count; i++)
                {
                    var p = coordinator.PeopleAtSelectedTile[i];
                    bool isSelected = i == coordinator.SelectedPersonIndex;
                    var nameColor = isSelected ? new Color(255, 255, 100, 255) : new Color(180, 180, 180, 255);

                    string indicator = isSelected ? "> " : "  ";
                    string taskIcon = GetTaskIcon(p.CurrentTask);
                    GraphicsConfig.DrawConsoleText($"{indicator}{taskIcon} {p.FirstName} {p.LastName}", 
                        x, y, context.SmallFontSize - 2, nameColor);
                    y += context.LineHeight - 2;
                }

                y += 5;
                GraphicsConfig.DrawConsoleText("[Arrows/Tab] to cycle", x, y, context.SmallFontSize - 2, 
                    new Color(120, 120, 150, 255));
                y += context.LineHeight + 5;
            }

            // Person basic info
            GraphicsConfig.DrawConsoleText($"Name: {person.FullName}", x, y, context.SmallFontSize, Color.White);
            y += context.LineHeight;

            GraphicsConfig.DrawConsoleText($"Age: {person.Age} | {person.Gender}", x, y, context.SmallFontSize, Color.LightGray);
            y += context.LineHeight;

            // Family info
            if (person.Family != null)
            {
                var aliveMembers = person.Family.Members.Count(m => m.IsAlive);
                GraphicsConfig.DrawConsoleText($"Family: {person.Family.FamilyName} ({aliveMembers} members)", 
                    x, y, context.SmallFontSize, Color.Yellow);
                y += context.LineHeight;
            }

            y += 5;

            // Status bars with visual indicators
            DrawStatBar("Health", person.Health, 100, x, y, context.Width - 20, context.SmallFontSize);
            y += context.LineHeight + 5;

            DrawStatBar("Energy", person.Energy, 100, x, y, context.Width - 20, context.SmallFontSize);
            y += context.LineHeight + 5;

            DrawStatBar("Hunger", person.Hunger, 100, x, y, context.Width - 20, context.SmallFontSize);
            y += context.LineHeight + 10;

            // Current task/status
            var taskText = GetPersonTaskText(person);
            var taskColor = GetPersonTaskColor(person);
            GraphicsConfig.DrawConsoleText($"Status: {taskText}", x, y, context.SmallFontSize, taskColor);
            y += context.LineHeight;

            // Workplace
            if (person.AssignedBuilding != null)
            {
                GraphicsConfig.DrawConsoleText($"Workplace: {person.AssignedBuilding.Type}", 
                    x, y, context.SmallFontSize, new Color(150, 255, 150, 255));
                y += context.LineHeight;
            }
            else
            {
                GraphicsConfig.DrawConsoleText("Workplace: None (Idle)", x, y, context.SmallFontSize, Color.Gray);
                y += context.LineHeight;
            }

            // Home
            if (person.HomeBuilding != null)
            {
                GraphicsConfig.DrawConsoleText($"Home: House at ({person.HomeBuilding.X}, {person.HomeBuilding.Y})", 
                    x, y, context.SmallFontSize, new Color(255, 200, 150, 255));
                y += context.LineHeight;
            }
            else
            {
                GraphicsConfig.DrawConsoleText("Home: Homeless", x, y, context.SmallFontSize, new Color(255, 150, 150, 255));
                y += context.LineHeight;
            }

            return y - context.StartY;
        }

        private string GetTaskIcon(PersonTask task)
        {
            return task switch
            {
                PersonTask.Sleeping => "zzz",
                PersonTask.GoingHome => ">>",
                PersonTask.GoingToWork => "->",
                PersonTask.WorkingAtBuilding => "**",
                PersonTask.Constructing => "++",
                PersonTask.Resting => "~~",
                PersonTask.MovingToLocation => ">>",
                PersonTask.Idle => "..",
                _ => "?"
            };
        }

        private string GetPersonTaskText(Person person)
        {
            return person.CurrentTask switch
            {
                PersonTask.Sleeping => "Sleeping",
                PersonTask.GoingHome => "Going Home",
                PersonTask.GoingToWork => "Going to Work",
                PersonTask.WorkingAtBuilding => "Working",
                PersonTask.Constructing => "Building",
                PersonTask.Resting => "Resting",
                PersonTask.MovingToLocation => "Traveling",
                PersonTask.Idle => "Idle",
                _ => "Unknown"
            };
        }

        private Color GetPersonTaskColor(Person person)
        {
            return person.CurrentTask switch
            {
                PersonTask.Sleeping => new Color(150, 150, 255, 255),
                PersonTask.GoingHome or PersonTask.GoingToWork or PersonTask.MovingToLocation => new Color(200, 200, 150, 255),
                PersonTask.WorkingAtBuilding or PersonTask.Constructing => new Color(150, 255, 150, 255),
                PersonTask.Resting => new Color(200, 200, 200, 255),
                PersonTask.Idle => Color.Gray,
                _ => Color.Gray
            };
        }
    }
}
