using System.Numerics;
using Raylib_cs;
using VillageBuilder.Engine.Entities;

namespace VillageBuilder.Game.Graphics.Rendering.Renderers
{
    /// <summary>
    /// Renders people grouped by families.
    /// Families are stacked at one position with count badge.
    /// </summary>
    public class PersonRenderer : IBatchRenderer<Family>
    {
        private static int FontSize => GraphicsConfig.SmallConsoleFontSize;
        
        public void RenderBatch(IEnumerable<Family> families, RenderContext context)
        {
            foreach (var family in families)
            {
                if (ShouldRender(family, context))
                {
                    Render(family, context);
                }
            }
        }
        
        public void Render(Family family, RenderContext context)
        {
            var aliveMembers = family.Members.Where(p => p.IsAlive).ToList();
            if (aliveMembers.Count == 0)
                return;
            
            // Use first alive member's position as the family's visual position
            var displayPerson = aliveMembers[0];
            var pos = context.GetWorldPosition(displayPerson.Position.X, displayPerson.Position.Y);
            
            // Draw path if family is moving
            if (displayPerson.CurrentPath != null && displayPerson.CurrentPath.Count > 0 &&
                displayPerson.CurrentTask == PersonTask.MovingToLocation)
            {
                DrawPath(displayPerson, context);
            }
            
            // Draw family background (gender-based color)
            var bgColor = displayPerson.Gender == Gender.Male
                ? ColorPalette.PersonMaleBackground
                : ColorPalette.PersonFemaleBackground;
            
            Raylib.DrawRectangle((int)pos.X, (int)pos.Y, context.TileSize, context.TileSize, bgColor);
            
            // Draw selection indicator if any family member is selected
            bool familySelected = aliveMembers.Any(p => context.SelectionManager?.SelectedPerson == p);
            if (familySelected)
            {
                var selectionBounds = new Rectangle((int)pos.X, (int)pos.Y, context.TileSize, context.TileSize);
                RenderHelpers.DrawSelectionHighlight(selectionBounds, ColorPalette.SelectionYellow);
            }
            
            // Draw family symbol
            string glyph = aliveMembers.Count > 1 ? "?" : "?"; // Filled for families, regular for singles
            var glyphColor = new Color(255, 255, 255, 255);
            
            int textX = (int)pos.X + (context.TileSize - FontSize) / 2;
            int textY = (int)pos.Y + (context.TileSize - FontSize) / 2;
            
            GraphicsConfig.DrawConsoleText(glyph, textX, textY, FontSize, glyphColor);
            
            // Draw family count badge
            if (aliveMembers.Count > 1)
            {
                DrawFamilyCountBadge(aliveMembers.Count, pos, context.TileSize);
            }
            
            // Draw task indicator if any family member is working
            bool anyWorking = aliveMembers.Any(p => p.AssignedBuilding != null);
            if (anyWorking)
            {
                RenderHelpers.DrawIndicatorDot(
                    (int)pos.X + 3,
                    (int)pos.Y + 3,
                    new Color(255, 255, 0, 255)
                );
            }
        }
        
        public bool ShouldRender(Family family, RenderContext context)
        {
            var aliveMembers = family.Members.Where(p => p.IsAlive).ToList();
            if (aliveMembers.Count == 0)
                return false;
            
            var displayPerson = aliveMembers[0];
            return context.IsVisible(displayPerson.Position.X, displayPerson.Position.Y);
        }
        
        private void DrawPath(Person person, RenderContext context)
        {
            if (person.CurrentPath == null || person.CurrentPath.Count == 0)
                return;
            
            for (int i = person.PathIndex; i < person.CurrentPath.Count - 1; i++)
            {
                var pathStart = person.CurrentPath[i];
                var pathEnd = person.CurrentPath[i + 1];
                
                var startPos = new Vector2(
                    pathStart.X * context.TileSize + context.TileSize / 2,
                    pathStart.Y * context.TileSize + context.TileSize / 2
                );
                var endPos = new Vector2(
                    pathEnd.X * context.TileSize + context.TileSize / 2,
                    pathEnd.Y * context.TileSize + context.TileSize / 2
                );
                
                Raylib.DrawLine(
                    (int)startPos.X, (int)startPos.Y,
                    (int)endPos.X, (int)endPos.Y,
                    new Color(255, 255, 0, 100)
                );
            }
        }
        
        private void DrawFamilyCountBadge(int count, Vector2 pos, int tileSize)
        {
            var countText = count.ToString();
            var badgeColor = new Color(100, 50, 150, 220);
            var textColor = new Color(255, 255, 0, 255);
            
            int badgeX = (int)pos.X + tileSize - 8;
            int badgeY = (int)pos.Y + 8;
            
            Raylib.DrawCircle(badgeX, badgeY, 8, badgeColor);
            GraphicsConfig.DrawConsoleText(countText, badgeX - 4, badgeY - 6, 14, textColor);
        }
    }
}
