using Raylib_cs;
using VillageBuilder.Engine.World;

namespace VillageBuilder.Game.Graphics.UI
{
    /// <summary>
    /// Renders detailed inspection information for a selected tile.
    /// Shows terrain type, decorations, people, buildings, and properties.
    /// </summary>
    public class TileInspector
    {
        private readonly int _fontSize;
        private readonly int _smallFontSize;

        public TileInspector(int fontSize, int smallFontSize)
        {
            _fontSize = fontSize;
            _smallFontSize = smallFontSize;
        }

        /// <summary>
        /// Renders the tile inspection panel in the sidebar
        /// </summary>
        /// <param name="tile">The tile to inspect</param>
        /// <param name="x">X position to render at</param>
        /// <param name="y">Starting Y position</param>
        /// <param name="width">Panel width</param>
        /// <returns>Height used by the panel</returns>
        public int Render(Tile tile, int x, int y, int width)
        {
            int currentY = y;
            var textColor = GraphicsConfig.Colors.Text;
            var highlightColor = GraphicsConfig.Colors.Highlight;

            // Header
            DrawSectionHeader("TILE INSPECTION", x, currentY, width);
            currentY += _fontSize + 8;

            // Position
            var posText = $"Position: ({tile.X}, {tile.Y})";
            GraphicsConfig.DrawConsoleText(posText, x + 4, currentY, _smallFontSize, textColor);
            currentY += _smallFontSize + 4;

            // Terrain type
            var terrainText = $"Terrain: {tile.GetTerrainName()}";
            GraphicsConfig.DrawConsoleText(terrainText, x + 4, currentY, _smallFontSize, textColor);
            currentY += _smallFontSize + 4;

            // Variant
            var variantText = $"Variant: {tile.TerrainVariant}";
            GraphicsConfig.DrawConsoleText(variantText, x + 4, currentY, _smallFontSize, textColor);
            currentY += _smallFontSize + 4;

            // Walkability (using sprite icons)
            int walkX = x + 4;
            var walkableColor = tile.IsWalkable ? GraphicsConfig.Colors.Green : GraphicsConfig.Colors.Red;
            var walkIconType = tile.IsWalkable ? UIIconType.Success : UIIconType.Error;
            walkX += GraphicsConfig.DrawUIIcon(
                walkIconType,
                walkX, currentY,
                _smallFontSize,
                walkableColor,
                tile.IsWalkable ? "+" : "X"
            );
            var walkableText = tile.IsWalkable ? " Walkable" : " Not Walkable";
            GraphicsConfig.DrawConsoleText(walkableText, walkX, currentY, _smallFontSize, walkableColor);
            currentY += _smallFontSize + 8;

            // Decorations section
            if (tile.Decorations.Count > 0)
            {
                DrawSubheader("Decorations", x, currentY, width);
                currentY += _smallFontSize + 4;

                int count = 0;
                foreach (var decoration in tile.Decorations.Take(8)) // Show max 8 decorations
                {
                    int decorX = x + 4;

                    // Get decoration color from Engine
                    var decorColor = decoration.GetColor();
                    var raylibColor = new Color(decorColor.R, decorColor.G, decorColor.B, decorColor.A);

                    // Draw decoration glyph with sprite support
                    var glyph = decoration.GetGlyph();
                    if (GraphicsConfig.UseSpriteMode && SpriteAtlasManager.Instance.SpriteModeEnabled)
                    {
                        // Try to draw as sprite first
                        var sprite = SpriteAtlasManager.Instance.GetSprite(decoration.Type);
                        if (sprite.HasValue)
                        {
                            var sourceRect = new Rectangle(0, 0, sprite.Value.Width, sprite.Value.Height);
                            var destRect = new Rectangle(decorX, currentY, _smallFontSize, _smallFontSize);
                            var origin = new System.Numerics.Vector2(0, 0);
                            Raylib.DrawTexturePro(sprite.Value, sourceRect, destRect, origin, 0f, raylibColor);
                            decorX += _smallFontSize + 2;
                        }
                        else
                        {
                            // Fallback to ASCII with proper color
                            GraphicsConfig.DrawConsoleText(glyph, decorX, currentY, _smallFontSize, raylibColor);
                            decorX += GraphicsConfig.MeasureText(glyph, _smallFontSize) + 2;
                        }
                    }
                    else
                    {
                        // ASCII mode with proper color
                        GraphicsConfig.DrawConsoleText(glyph, decorX, currentY, _smallFontSize, raylibColor);
                        decorX += GraphicsConfig.MeasureText(glyph, _smallFontSize) + 2;
                    }

                    // Draw decoration name
                    var decorName = GetDecorationName(decoration.Type);
                    GraphicsConfig.DrawConsoleText(decorName, decorX, currentY, _smallFontSize, textColor);

                    currentY += _smallFontSize + 2;
                    count++;
                }

                if (tile.Decorations.Count > 8)
                {
                    var moreText = $"  ... and {tile.Decorations.Count - 8} more";
                    GraphicsConfig.DrawConsoleText(moreText, x + 4, currentY, _smallFontSize, 
                        new Color(180, 180, 180, 255));
                    currentY += _smallFontSize + 2;
                }

                currentY += 4;
            }
            else
            {
                GraphicsConfig.DrawConsoleText("No decorations", x + 4, currentY, _smallFontSize, 
                    new Color(150, 150, 150, 255));
                currentY += _smallFontSize + 8;
            }

            // People section
            if (tile.PeopleOnTile.Count > 0)
            {
                DrawSubheader($"People ({tile.PeopleOnTile.Count})", x, currentY, width);
                currentY += _smallFontSize + 4;

                foreach (var person in tile.PeopleOnTile.Take(5)) // Show max 5 people
                {
                    var personText = $"  ☺ {person.FirstName} {person.LastName}";
                    GraphicsConfig.DrawConsoleText(personText, x + 4, currentY, _smallFontSize, highlightColor);
                    currentY += _smallFontSize + 2;
                }

                if (tile.PeopleOnTile.Count > 5)
                {
                    var moreText = $"  ... and {tile.PeopleOnTile.Count - 5} more";
                    GraphicsConfig.DrawConsoleText(moreText, x + 4, currentY, _smallFontSize, 
                        new Color(180, 180, 180, 255));
                    currentY += _smallFontSize + 2;
                }

                // Hint to click person
                var hintText = "[Click to select person]";
                GraphicsConfig.DrawConsoleText(hintText, x + 4, currentY, _smallFontSize, 
                    new Color(100, 150, 255, 200));
                currentY += _smallFontSize + 8;
            }

            // Building section
            if (tile.Building != null)
            {
                DrawSubheader("Building", x, currentY, width);
                currentY += _smallFontSize + 4;

                var buildingText = $"  {tile.Building.Name}";
                GraphicsConfig.DrawConsoleText(buildingText, x + 4, currentY, _smallFontSize, 
                    GraphicsConfig.Colors.Yellow);
                currentY += _smallFontSize + 2;

                var statusText = tile.Building.IsConstructed ? "  Status: Complete" : $"  Construction: {tile.Building.GetConstructionProgressPercent()}%";
                GraphicsConfig.DrawConsoleText(statusText, x + 4, currentY, _smallFontSize, textColor);
                currentY += _smallFontSize + 8;
            }

            return currentY - y; // Return height used
        }

        /// <summary>
        /// Renders a compact tooltip for hovering over tiles
        /// </summary>
        public void RenderTooltip(Tile tile, int mouseX, int mouseY)
        {
            var lines = new List<string>();
            
            // Terrain with emoji if using sprites
            var terrainLine = $"{tile.GetTerrainName()} ({tile.X}, {tile.Y})";
            lines.Add(terrainLine);

            // Top 3 decorations
            if (tile.Decorations.Count > 0)
            {
                lines.Add(""); // Separator
                foreach (var dec in tile.Decorations.Take(3))
                {
                    lines.Add($"{dec.GetGlyph()} {GetDecorationName(dec.Type)}");
                }
                if (tile.Decorations.Count > 3)
                {
                    lines.Add($"... +{tile.Decorations.Count - 3} more");
                }
            }

            // People count
            if (tile.PeopleOnTile.Count > 0)
            {
                lines.Add("");
                lines.Add($"{tile.PeopleOnTile.Count} {(tile.PeopleOnTile.Count == 1 ? "person" : "people")} here");
            }

            // Building
            if (tile.Building != null)
            {
                lines.Add("");
                lines.Add($"Building: {tile.Building.Name}");
            }

            // Hint
            if (tile.PeopleOnTile.Count > 0)
            {
                lines.Add("");
                lines.Add("[Ctrl+Click for tile details]");
            }

            // Render tooltip background and text
            RenderTooltipBox(lines, mouseX, mouseY);
        }

        private void RenderTooltipBox(List<string> lines, int mouseX, int mouseY)
        {
            if (lines.Count == 0) return;

            // Calculate tooltip size
            int maxWidth = 0;
            foreach (var line in lines)
            {
                var width = GraphicsConfig.MeasureText(line, _smallFontSize);
                if (width > maxWidth) maxWidth = width;
            }

            int padding = 8;
            int lineHeight = _smallFontSize + 2;
            int tooltipWidth = maxWidth + padding * 2;
            int tooltipHeight = lines.Count * lineHeight + padding * 2;

            // Position tooltip (offset from mouse, keep on screen)
            int tooltipX = mouseX + 15;
            int tooltipY = mouseY + 15;

            // Keep on screen
            if (tooltipX + tooltipWidth > GraphicsConfig.ScreenWidth)
                tooltipX = mouseX - tooltipWidth - 5;
            if (tooltipY + tooltipHeight > GraphicsConfig.ScreenHeight)
                tooltipY = mouseY - tooltipHeight - 5;

            // Draw background
            Raylib.DrawRectangle(tooltipX, tooltipY, tooltipWidth, tooltipHeight, 
                new Color(30, 30, 40, 240));
            
            // Draw border
            Raylib.DrawRectangleLines(tooltipX, tooltipY, tooltipWidth, tooltipHeight, 
                new Color(100, 150, 255, 255));

            // Draw text
            int currentY = tooltipY + padding;
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    currentY += lineHeight / 2; // Half height for separator
                    continue;
                }

                GraphicsConfig.DrawConsoleTextAuto(line, tooltipX + padding, currentY, 
                    _smallFontSize, GraphicsConfig.Colors.Text);
                currentY += lineHeight;
            }
        }

        private void DrawSectionHeader(string text, int x, int y, int width)
        {
            // Draw background bar
            Raylib.DrawRectangle(x, y, width, _fontSize + 4, new Color(60, 60, 80, 255));
            
            // Draw text
            GraphicsConfig.DrawConsoleText(text, x + 4, y + 2, _fontSize, 
                GraphicsConfig.Colors.Highlight);
        }

        private void DrawSubheader(string text, int x, int y, int width)
        {
            // Underline
            Raylib.DrawLine(x + 4, y + _smallFontSize + 2, x + width - 4, y + _smallFontSize + 2, 
                new Color(80, 80, 100, 255));
            
            // Text
            GraphicsConfig.DrawConsoleText(text, x + 4, y, _smallFontSize, 
                new Color(200, 220, 255, 255));
        }

        private string GetDecorationName(DecorationType type)
        {
            return type switch
            {
                DecorationType.TreeOak => "Oak Tree",
                DecorationType.TreePine => "Pine Tree",
                DecorationType.TreeDead => "Dead Tree",
                DecorationType.BushRegular => "Bush",
                DecorationType.BushBerry => "Berry Bush",
                DecorationType.BushFlowering => "Flowering Bush",
                DecorationType.FlowerWild => "Wildflower",
                DecorationType.FlowerRare => "Rare Flower",
                DecorationType.GrassTuft => "Grass Tuft",
                DecorationType.TallGrass => "Tall Grass",
                DecorationType.Fern => "Fern",
                DecorationType.Reeds => "Reeds",
                DecorationType.RockBoulder => "Boulder",
                DecorationType.RockPebble => "Pebbles",
                DecorationType.StumpOld => "Old Stump",
                DecorationType.LogFallen => "Fallen Log",
                DecorationType.Mushroom => "Mushroom",
                DecorationType.BirdFlying => "Bird (Flying)",
                DecorationType.BirdPerched => "Bird (Perched)",
                DecorationType.Butterfly => "Butterfly",
                DecorationType.RabbitSmall => "Rabbit",
                DecorationType.DeerGrazing => "Deer",
                DecorationType.FishInWater => "Fish",
                _ => "Unknown"
            };
        }
    }
}


