using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raylib_cs;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Entities.Wildlife;
using VillageBuilder.Engine.World;
using VillageBuilder.Engine.Buildings;
using System.Numerics;

namespace VillageBuilder.Game.Graphics.UI
{
    public class MapRenderer
    {
        // Font size now dynamic from GraphicsConfig
        private static int FontSize => GraphicsConfig.SmallConsoleFontSize;

        public void Render(GameEngine engine, Camera2D camera, VillageBuilder.Game.Core.Selection.SelectionCoordinator? selectionManager = null)
        {
            var grid = engine.Grid;
            var tileSize = GraphicsConfig.TileSize;

            // Calculate visible area for culling
            var screenWidth = GraphicsConfig.ScreenWidth;
            var screenHeight = GraphicsConfig.ScreenHeight;
            
            float worldLeft = camera.Target.X - (screenWidth / 2f) / camera.Zoom;
            float worldTop = camera.Target.Y - (screenHeight / 2f) / camera.Zoom;
            float worldRight = camera.Target.X + (screenWidth / 2f) / camera.Zoom;
            float worldBottom = camera.Target.Y + (screenHeight / 2f) / camera.Zoom;
            
            int minX = Math.Max(0, (int)(worldLeft / tileSize) - 1);
            int maxX = Math.Min(grid.Width, (int)(worldRight / tileSize) + 2);
            int minY = Math.Max(0, (int)(worldTop / tileSize) - 1);
            int maxY = Math.Min(grid.Height, (int)(worldBottom / tileSize) + 2);

            // Track which buildings we've already drawn
            var drawnBuildings = new HashSet<Building>();
            
            // Calculate darkness factor for day/night visual effect
            float darknessFactor = engine.Time.GetDarknessFactor();

            // Render only visible tiles (culling for performance)
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    var tile = grid.GetTile(x, y);
                    if (tile == null) continue;

                    var pos = new Vector2(x * tileSize, y * tileSize);

                    // If this tile has a building, only draw it once when we first encounter it
                    if (tile.Building != null)
                    {
                        if (!drawnBuildings.Contains(tile.Building))
                        {
                            DrawDetailedBuilding(tile.Building, tileSize, engine.Time);
                            drawnBuildings.Add(tile.Building);
                        }
                        // Skip drawing anything else for building tiles - the building handles it
                        continue;
                    }

                        // Draw tile background (only for non-building tiles)
                        var bgColor = GetTileBackgroundColor(tile);

                        // Apply darkness overlay for night
                        if (darknessFactor > 0)
                        {
                            bgColor = DarkenColor(bgColor, darknessFactor);
                        }

                                    Raylib.DrawRectangle((int)pos.X, (int)pos.Y, tileSize, tileSize, bgColor);

                                    // Draw base tile glyph (SKIP in sprite mode to reduce visual clutter)
                                    if (!GraphicsConfig.UseSpriteMode)
                                    {
                                        DrawTileGlyph(tile, pos, tileSize, darknessFactor);
                                    }

                                    // Draw terrain decorations on top
                                    DrawTerrainDecorations(tile, pos, tileSize, darknessFactor, engine.Time);
                                }
                        }
            
            // Render people on top of everything
            RenderPeople(engine, tileSize, minX, maxX, minY, maxY, selectionManager);

            // Render wildlife
            RenderWildlife(engine, tileSize, minX, maxX, minY, maxY, selectionManager);

                // Render selection indicators for buildings
                if (selectionManager?.SelectedBuilding != null)
                {
                    DrawBuildingSelection(selectionManager.SelectedBuilding, tileSize);
                }

                // Render selection indicator for tiles
                if (selectionManager?.SelectedTile != null)
                {
                    DrawTileSelection(selectionManager.SelectedTile, tileSize);
                }
            }

        private void DrawDetailedBuilding(Building building, int tileSize, GameTime time)
        {
            // If building is under construction, draw construction stages
            if (!building.IsConstructed)
            {
                DrawConstructionStages(building, tileSize);
                return;
            }

            var bgColor = GetBuildingBackgroundColor(building.Type);
            var wallColor = GetWallColor(building.Type);
            var floorColor = GetFloorColor(building.Type);
            var doorColor = new Color(120, 80, 40, 255);

            // Check if building should show lights at night
            bool showLights = building.ShouldShowLights(time);
            float darknessFactor = time.GetDarknessFactor();

            var occupiedTiles = building.GetOccupiedTiles();

            foreach (var tilePos in occupiedTiles)
            {
                var buildingTile = building.GetTileAtWorldPosition(tilePos.X, tilePos.Y);
                if (buildingTile == null) continue;

                var pos = new Vector2(tilePos.X * tileSize, tilePos.Y * tileSize);

                Color tileColor = buildingTile.Value.Type switch
                {
                    BuildingTileType.Wall => wallColor,
                    BuildingTileType.Floor => floorColor,
                    BuildingTileType.Door => doorColor,
                    _ => bgColor
                };

                // Apply darkness overlay
                if (darknessFactor > 0)
                {
                    tileColor = DarkenColor(tileColor, darknessFactor);
                }

                // Add warm glow if lights are on
                if (showLights && buildingTile.Value.Type == BuildingTileType.Floor)
                {
                    tileColor = AddWarmGlow(tileColor, 0.4f);
                }

                Raylib.DrawRectangle((int)pos.X, (int)pos.Y, tileSize, tileSize, tileColor);

                if (buildingTile.Value.Glyph != ' ')
                {
                    int textX = (int)pos.X + (tileSize - FontSize) / 2;
                    int textY = (int)pos.Y + (tileSize - FontSize) / 2;

                    Color glyphColor = buildingTile.Value.Type switch
                    {
                        BuildingTileType.Wall => new Color(255, 255, 255, 255),
                        BuildingTileType.Floor => new Color(200, 200, 200, 255),
                        BuildingTileType.Door => new Color(255, 255, 0, 255),
                        _ => Color.White
                    };

                    // Apply darkness to glyphs too
                    if (darknessFactor > 0 && !showLights)
                    {
                        glyphColor = DarkenColor(glyphColor, darknessFactor);
                    }
                    else if (showLights)
                    {
                        // Brighten glyphs when lights are on
                        glyphColor = new Color(255, 255, 200, 255);
                    }

                    // Use the actual glyph from the building definition
                    string displayChar = buildingTile.Value.Glyph.ToString();

                    GraphicsConfig.DrawConsoleText(displayChar, textX, textY, FontSize, glyphColor);
                }
            }
        }

        /// <summary>
        /// Draw construction stages for buildings under construction
        /// </summary>
        private void DrawConstructionStages(Building building, int tileSize)
        {
            var stage = building.GetConstructionStage();
            var occupiedTiles = building.GetOccupiedTiles();
            var progressPercent = building.GetConstructionProgressPercent();

            // Colors for each construction stage
            Color stageColor = stage switch
            {
                ConstructionStage.Foundation => new Color(101, 67, 33, 255),   // Dark brown
                ConstructionStage.Framing => new Color(139, 90, 43, 255),      // Medium brown
                ConstructionStage.Walls => new Color(160, 120, 80, 255),       // Light brown
                ConstructionStage.Finishing => new Color(180, 140, 100, 255),  // Almost complete
                _ => new Color(100, 100, 100, 255)
            };

            // Glyphs for each stage
            string stageGlyph = stage switch
            {
                ConstructionStage.Foundation => "¦",    // Light shade
                ConstructionStage.Framing => "¦",       // Medium shade
                ConstructionStage.Walls => "¦",         // Dark shade
                ConstructionStage.Finishing => "¦",     // Full block
                _ => "·"
            };

            foreach (var tilePos in occupiedTiles)
            {
                var pos = new Vector2(tilePos.X * tileSize, tilePos.Y * tileSize);

                // Draw background
                Raylib.DrawRectangle((int)pos.X, (int)pos.Y, tileSize, tileSize, stageColor);

                // Draw stage glyph
                int textX = (int)pos.X + (tileSize - FontSize) / 2;
                int textY = (int)pos.Y + (tileSize - FontSize) / 2;

                Color glyphColor = new Color(200, 200, 200, 255);
                GraphicsConfig.DrawConsoleText(stageGlyph, textX, textY, FontSize, glyphColor);
            }

            // Draw progress indicator on center tile
            if (occupiedTiles.Count > 0)
            {
                var centerTile = occupiedTiles[occupiedTiles.Count / 2];
                var centerPos = new Vector2(centerTile.X * tileSize, centerTile.Y * tileSize);

                // Draw progress percentage
                string progressText = $"{progressPercent}%";
                int progressX = (int)centerPos.X + (tileSize - progressText.Length * 8) / 2;
                int progressY = (int)centerPos.Y + tileSize - 18;

                GraphicsConfig.DrawConsoleText(progressText, progressX, progressY, 14, new Color(255, 255, 100, 255));

                // Draw worker count
                int workerCount = building.ConstructionWorkers.Count;
                if (workerCount > 0)
                {
                    string workerText = $"??{workerCount}";
                    int workerX = (int)centerPos.X + 4;
                    int workerY = (int)centerPos.Y + 4;
                    GraphicsConfig.DrawConsoleText(workerText, workerX, workerY, 12, new Color(255, 200, 100, 255));
                }
            }
        }

        private Color GetBuildingBackgroundColor(BuildingType type)
        {
            return type switch
            {
                BuildingType.House => new Color(80, 60, 40, 255),
                BuildingType.Warehouse => new Color(70, 70, 60, 255),
                BuildingType.Workshop => new Color(60, 45, 35, 255),
                BuildingType.Farm => new Color(60, 50, 30, 255),
                BuildingType.Mine => new Color(50, 45, 40, 255),
                BuildingType.Lumberyard => new Color(50, 40, 30, 255),
                BuildingType.Market => new Color(80, 70, 50, 255),
                BuildingType.TownHall => new Color(90, 80, 60, 255),
                _ => new Color(60, 60, 60, 255)
            };
        }

        private Color GetWallColor(BuildingType type)
        {
            return type switch
            {
                BuildingType.House => new Color(100, 80, 60, 255),
                BuildingType.Warehouse => new Color(90, 90, 80, 255),
                BuildingType.Workshop => new Color(80, 60, 50, 255),
                BuildingType.Farm => new Color(80, 65, 40, 255),
                BuildingType.Mine => new Color(70, 65, 60, 255),
                BuildingType.Lumberyard => new Color(70, 55, 40, 255),
                BuildingType.Market => new Color(110, 95, 70, 255),
                BuildingType.TownHall => new Color(120, 105, 80, 255),
                _ => new Color(80, 80, 80, 255)
            };
        }

        private Color GetFloorColor(BuildingType type)
        {
            return type switch
            {
                BuildingType.House => new Color(70, 55, 35, 255),
                BuildingType.Warehouse => new Color(60, 60, 50, 255),
                BuildingType.Workshop => new Color(50, 40, 30, 255),
                BuildingType.Farm => new Color(60, 45, 25, 255),
                BuildingType.Mine => new Color(40, 35, 30, 255),
                BuildingType.Lumberyard => new Color(55, 40, 25, 255),
                BuildingType.Market => new Color(80, 65, 45, 255),
                BuildingType.TownHall => new Color(85, 70, 50, 255),
                _ => new Color(50, 50, 50, 255)
            };
        }

        private Color GetTileBackgroundColor(Tile tile)
        {
            return tile.Type switch
            {
                // LIGHTENED for better visual clarity (sprites pop more)
                TileType.Grass => new Color(35, 50, 35, 255),      // Was: (20, 40, 20)
                TileType.Forest => new Color(25, 40, 25, 255),     // Was: (10, 30, 10)
                TileType.Water => new Color(20, 35, 50, 255),      // Was: (10, 20, 40)
                TileType.Mountain => new Color(40, 40, 45, 255),   // Was: (30, 30, 35)
                TileType.Field => new Color(50, 45, 30, 255),      // Was: (40, 35, 20)
                TileType.Road => new Color(50, 50, 50, 255),       // Was: (40, 40, 40)
                _ => new Color(30, 30, 30, 255)                    // Was: (20, 20, 20)
            };
        }

        private Color GetTileForegroundColor(Tile tile)
        {
            return tile.Type switch
            {
                TileType.Grass => new Color(80, 180, 80, 255),
                TileType.Forest => new Color(60, 150, 60, 255),
                TileType.Water => new Color(80, 140, 220, 255),
                TileType.Mountain => new Color(150, 150, 160, 255),
                TileType.Field => new Color(200, 180, 100, 255),
                TileType.Road => new Color(120, 120, 120, 255),
                _ => Color.Gray
            };
        }

        private void DrawTileGlyph(Tile tile, Vector2 pos, int size, float darknessFactor)
        {
            string glyph = GetTileGlyph(tile);
            var color = GetTileForegroundColor(tile);
            
            // Apply darkness
            if (darknessFactor > 0)
            {
                color = DarkenColor(color, darknessFactor);
            }
            
            int textX = (int)pos.X + (size - FontSize) / 2;
            int textY = (int)pos.Y + (size - FontSize) / 2;
            
            GraphicsConfig.DrawConsoleText(glyph, textX, textY, FontSize, color);
        }

        /// <summary>
        /// Darken a color by a factor (0.0 = no change, 1.0 = black)
        /// </summary>
        private Color DarkenColor(Color color, float factor)
        {
            factor = Math.Clamp(factor, 0f, 1f);
            float multiplier = 1.0f - (factor * 0.7f); // Don't go completely black
            
            return new Color(
                (int)(color.R * multiplier),
                (int)(color.G * multiplier),
                (int)(color.B * multiplier),
                color.A
            );
        }

        /// <summary>
        /// Add a warm glow to a color (for lit buildings at night)
        /// </summary>
        private Color AddWarmGlow(Color color, float intensity)
        {
            intensity = Math.Clamp(intensity, 0f, 1f);
            
            return new Color(
                Math.Min(255, (int)(color.R + 80 * intensity)),
                Math.Min(255, (int)(color.G + 40 * intensity)),
                (int)(color.B),
                color.A
            );
        }
        
        private void RenderPeople(GameEngine engine, int tileSize, int minX, int maxX, int minY, int maxY, VillageBuilder.Game.Core.Selection.SelectionCoordinator? selectionManager)
        {
            // Group people by FAMILY - families always render together as a stack
            foreach (var family in engine.Families)
            {
                var aliveMembers = family.Members.Where(p => p.IsAlive).ToList();
                if (aliveMembers.Count == 0)
                    continue;

                // Use first alive member's position as the family's visual position
                // (internally they may have different positions, but we render them stacked)
                var displayPerson = aliveMembers[0];
                int posX = displayPerson.Position.X;
                int posY = displayPerson.Position.Y;

                // Only render if in visible area
                if (posX < minX || posX >= maxX || posY < minY || posY >= maxY)
                    continue;

                var pos = new Vector2(posX * tileSize, posY * tileSize);

                // Draw path for family if moving
                if (displayPerson.CurrentPath != null && displayPerson.CurrentPath.Count > 0 && 
                    displayPerson.CurrentTask == VillageBuilder.Engine.Entities.PersonTask.MovingToLocation)
                {
                    for (int i = displayPerson.PathIndex; i < displayPerson.CurrentPath.Count - 1; i++)
                    {
                        var pathStart = displayPerson.CurrentPath[i];
                        var pathEnd = displayPerson.CurrentPath[i + 1];

                        var startPos = new Vector2(pathStart.X * tileSize + tileSize / 2, pathStart.Y * tileSize + tileSize / 2);
                        var endPos = new Vector2(pathEnd.X * tileSize + tileSize / 2, pathEnd.Y * tileSize + tileSize / 2);

                        Raylib.DrawLine((int)startPos.X, (int)startPos.Y, (int)endPos.X, (int)endPos.Y, new Color(255, 255, 0, 100));
                    }
                }

                // Draw family background (use gender color of head of household)
                var bgColor = displayPerson.Gender == VillageBuilder.Engine.Entities.Gender.Male 
                    ? new Color(80, 120, 200, 220)
                    : new Color(200, 80, 120, 220);

                Raylib.DrawRectangle((int)pos.X, (int)pos.Y, tileSize, tileSize, bgColor);

                // Draw selection indicator if any family member is selected
                bool familySelected = aliveMembers.Any(p => selectionManager?.SelectedPerson == p);
                if (familySelected)
                {
                    Raylib.DrawRectangleLines((int)pos.X, (int)pos.Y, tileSize, tileSize, new Color(255, 255, 0, 255));
                    Raylib.DrawRectangleLines((int)pos.X + 1, (int)pos.Y + 1, tileSize - 2, tileSize - 2, new Color(255, 255, 0, 255));
                }

                // Draw family symbol (multiple people icon)
                string glyph = aliveMembers.Count > 1 ? "?" : "?"; // Filled circle for families, regular for singles
                var glyphColor = new Color(255, 255, 255, 255);

                int textX = (int)pos.X + (tileSize - FontSize) / 2;
                int textY = (int)pos.Y + (tileSize - FontSize) / 2;

                GraphicsConfig.DrawConsoleText(glyph, textX, textY, FontSize, glyphColor);

                        // Always draw family count badge
                        if (aliveMembers.Count > 1)
                        {
                            var countText = aliveMembers.Count.ToString();
                            var countColor = new Color(255, 255, 0, 255);
                            Raylib.DrawCircle((int)pos.X + tileSize - 8, (int)pos.Y + 8, 8, new Color(100, 50, 150, 220));
                            GraphicsConfig.DrawConsoleText(countText, (int)pos.X + tileSize - 12, (int)pos.Y + 2, 14, countColor);
                        }

                        // Draw task indicator if any family member is working
                        bool anyWorking = aliveMembers.Any(p => p.AssignedBuilding != null);
                        if (anyWorking)
                        {
                            var taskColor = new Color(255, 255, 0, 255);
                            Raylib.DrawRectangle((int)pos.X, (int)pos.Y, 3, 3, taskColor);
                                        }
                                    }
                                }

                        private void RenderWildlife(GameEngine engine, int tileSize, int minX, int maxX, int minY, int maxY, VillageBuilder.Game.Core.Selection.SelectionCoordinator? selectionManager)
                        {
                            if (engine.WildlifeManager == null) return;

                            foreach (var wildlife in engine.WildlifeManager.Wildlife.Where(w => w.IsAlive))
                            {
                                int posX = wildlife.Position.X;
                                int posY = wildlife.Position.Y;

                                // Only render if in visible area
                                if (posX < minX || posX >= maxX || posY < minY || posY >= maxY)
                                    continue;

                                var pos = new Vector2(posX * tileSize, posY * tileSize);

                                // Get color based on animal type
                                var bgColor = GetWildlifeColor(wildlife.Type, wildlife.IsPredator);

                                // Dim color if fleeing
                                if (wildlife.CurrentBehavior == WildlifeBehavior.Fleeing)
                                {
                                    bgColor = new Color(
                                        (byte)(bgColor.R * 0.7f),
                                        (byte)(bgColor.G * 0.7f),
                                        (byte)(bgColor.B * 0.7f),
                                        bgColor.A
                                    );
                                }

                                // Check if we'll use sprite rendering
                                var spriteType = GetWildlifeSpriteType(wildlife.Type);
                                bool willUseSprite = spriteType.HasValue && 
                                                    GraphicsConfig.UseSpriteMode && 
                                                    SpriteAtlasManager.Instance.GetSprite(spriteType.Value).HasValue;

                                // Only draw background rectangle if NOT using sprites (for text fallback)
                                if (!willUseSprite)
                                {
                                    Raylib.DrawRectangle((int)pos.X, (int)pos.Y, tileSize, tileSize, bgColor);
                                }

                                // Draw selection indicator if selected
                                bool isSelected = selectionManager?.SelectedWildlife == wildlife;
                                if (isSelected)
                                {
                                    Raylib.DrawRectangleLines((int)pos.X, (int)pos.Y, tileSize, tileSize, new Color(255, 255, 0, 255));
                                    Raylib.DrawRectangleLines((int)pos.X + 1, (int)pos.Y + 1, tileSize - 2, tileSize - 2, new Color(255, 255, 0, 255));
                                }

                                // Draw wildlife emoji/glyph
                                string glyph = GetWildlifeGlyph(wildlife.Type);
                                var glyphColor = new Color(255, 255, 255, 255);

                                int textX = (int)pos.X + (tileSize - FontSize) / 2;
                                int textY = (int)pos.Y + (tileSize - FontSize) / 2;

                                // Use sprite if available, otherwise fall back to text
                                if (spriteType.HasValue && GraphicsConfig.UseSpriteMode)
                                {
                                    var sprite = SpriteAtlasManager.Instance.GetSprite(spriteType.Value);
                                    if (sprite.HasValue)
                                    {
                                        // Render wildlife sprite WITH TRANSPARENCY
                                        int spriteSize = (int)(tileSize * 0.8f);
                                        int spriteX = (int)pos.X + (tileSize - spriteSize) / 2;
                                        int spriteY = (int)pos.Y + (tileSize - spriteSize) / 2;

                                        var sourceRect = new Rectangle(0, 0, sprite.Value.Width, sprite.Value.Height);
                                        var destRect = new Rectangle(spriteX, spriteY, spriteSize, spriteSize);
                                        var origin = new System.Numerics.Vector2(0, 0);

                                        // Use White color to preserve sprite's original colors and transparency
                                        Raylib.DrawTexturePro(sprite.Value, sourceRect, destRect, origin, 0f, Color.White);
                                        }
                                        else
                                        {
                                            // Fallback to text rendering with emoji font support
                                            GraphicsConfig.DrawConsoleTextAuto(glyph, textX, textY, FontSize, glyphColor);
                                        }
                                    }
                                    else
                                    {
                                        // Text-only mode - use auto font selection for emoji support
                                        GraphicsConfig.DrawConsoleTextAuto(glyph, textX, textY, FontSize, glyphColor);
                                    }

                                // Draw health bar if injured
                                if (wildlife.Health < 100)
                                {
                                    int barWidth = tileSize - 4;
                                    int barHeight = 3;
                                    int barX = (int)pos.X + 2;
                                    int barY = (int)pos.Y + tileSize - 5;

                                    // Background
                                    Raylib.DrawRectangle(barX, barY, barWidth, barHeight, new Color(60, 0, 0, 200));

                                    // Health
                                    int healthWidth = (int)(barWidth * (wildlife.Health / 100f));
                                    Color healthColor = wildlife.Health > 50 ? new Color(0, 200, 0, 255) : new Color(200, 0, 0, 255);
                                    Raylib.DrawRectangle(barX, barY, healthWidth, barHeight, healthColor);
                                }

                                // Draw behavior indicator for hunting/fleeing
                                if (wildlife.CurrentBehavior == WildlifeBehavior.Hunting)
                                {
                                    // Red dot for hunting
                                    Raylib.DrawCircle((int)pos.X + tileSize - 5, (int)pos.Y + 5, 3, new Color(255, 0, 0, 255));
                                }
                                else if (wildlife.CurrentBehavior == WildlifeBehavior.Fleeing)
                                {
                                    // Yellow dot for fleeing
                                    Raylib.DrawCircle((int)pos.X + tileSize - 5, (int)pos.Y + 5, 3, new Color(255, 255, 0, 255));
                                }
                            }
                        }

                        private Color GetWildlifeColor(WildlifeType type, bool isPredator)
                        {
                            if (isPredator)
                            {
                                return type switch
                                {
                                    WildlifeType.Wolf => new Color(100, 100, 120, 220),     // Gray
                                    WildlifeType.Fox => new Color(180, 90, 40, 220),        // Orange-brown
                                    WildlifeType.Bear => new Color(80, 60, 40, 220),        // Dark brown
                                    _ => new Color(120, 80, 60, 220)
                                };
                            }
                            else
                            {
                                return type switch
                                {
                                    WildlifeType.Rabbit => new Color(200, 180, 160, 220),  // Light brown
                                    WildlifeType.Deer => new Color(160, 120, 80, 220),     // Brown
                                    WildlifeType.Boar => new Color(100, 80, 70, 220),      // Dark brown
                                    WildlifeType.Bird => new Color(140, 180, 200, 220),    // Light blue
                                    WildlifeType.Duck => new Color(120, 160, 140, 220),    // Green-blue
                                    WildlifeType.Turkey => new Color(140, 100, 80, 220),   // Brown-red
                                    _ => new Color(150, 150, 150, 220)
                                };
                            }
                        }

                        private string GetWildlifeGlyph(WildlifeType type)
                        {
                            return type switch
                            {
                                WildlifeType.Rabbit => "??",
                                WildlifeType.Deer => "??",
                                WildlifeType.Boar => "??",
                                WildlifeType.Wolf => "??",
                                WildlifeType.Fox => "??",
                                WildlifeType.Bear => "??",
                                WildlifeType.Bird => "??",
                                WildlifeType.Duck => "??",
                                WildlifeType.Turkey => "??",
                                _ => "?"
                            };
                        }

                        private VillageBuilder.Engine.World.DecorationType? GetWildlifeSpriteType(WildlifeType type)
                        {
                            return type switch
                            {
                                WildlifeType.Rabbit => VillageBuilder.Engine.World.DecorationType.RabbitSmall,
                                WildlifeType.Deer => VillageBuilder.Engine.World.DecorationType.DeerGrazing,
                                WildlifeType.Bird => VillageBuilder.Engine.World.DecorationType.BirdFlying,
                                WildlifeType.Duck => VillageBuilder.Engine.World.DecorationType.BirdPerched,
                                WildlifeType.Turkey => VillageBuilder.Engine.World.DecorationType.BirdPerched,
                                // NEW: Predators with dedicated sprites
                                WildlifeType.Fox => VillageBuilder.Engine.World.DecorationType.FoxHunting,
                                WildlifeType.Wolf => VillageBuilder.Engine.World.DecorationType.WolfPack,
                                WildlifeType.Bear => VillageBuilder.Engine.World.DecorationType.BearGrizzly,
                                WildlifeType.Boar => VillageBuilder.Engine.World.DecorationType.BoarWild,
                                // For any future animals without sprites, return null to use text fallback
                                _ => null
                            };
                        }

                        private void DrawBuildingSelection(Building building, int tileSize)
        {
            var tiles = building.GetOccupiedTiles();
            foreach (var tile in tiles)
            {
                var pos = new Vector2(tile.X * tileSize, tile.Y * tileSize);
                Raylib.DrawRectangleLines((int)pos.X, (int)pos.Y, tileSize, tileSize, new Color(0, 255, 0, 255));
            }
        }

        private void DrawTileSelection(Tile tile, int tileSize)
        {
            var pos = new Vector2(tile.X * tileSize, tile.Y * tileSize);

            // Draw subtle highlight - semi-transparent white overlay
            Raylib.DrawRectangle((int)pos.X, (int)pos.Y, tileSize, tileSize, new Color(255, 255, 255, 30));

            // Draw cyan border for selected tile
            Raylib.DrawRectangleLines((int)pos.X, (int)pos.Y, tileSize, tileSize, new Color(100, 200, 255, 200));
        }

        private string GetTileGlyph(Tile tile)
        {
            // Use terrain variant for visual variety (0-3)
            int variant = tile.TerrainVariant;

            return tile.Type switch
            {
                // Grass - multiple variants for natural look
                TileType.Grass => variant switch
                {
                    0 => "\"",
                    1 => "'",
                    2 => ",",
                    _ => "."
                },

                // Forest - tree variations
                TileType.Forest => variant switch
                {
                    0 => "?",
                    1 => "?",
                    2 => "?",
                    _ => "?"
                },

                // Water - wave variations
                TileType.Water => variant switch
                {
                    0 => "˜",
                    1 => "~",
                    2 => "~",
                    _ => "?"
                },

                // Mountain - rock variations
                TileType.Mountain => variant switch
                {
                    0 => "?",
                    1 => "?",
                    2 => "?",
                    _ => "^"
                },

                TileType.Field => "=",
                TileType.Road => "·",
                _ => "?"
            };
        }

        /// <summary>
        /// Render terrain decorations for visual richness and life
        /// </summary>
        private void DrawTerrainDecorations(Tile tile, Vector2 pos, int size, float darknessFactor, GameTime time)
        {
            if (tile.Decorations.Count == 0) return;

            // Get time of day as 0-1 float (0 = midnight, 0.5 = noon)
            float timeOfDay = time.Hour / 24f;
            int seasonIndex = (int)time.CurrentSeason;

                foreach (var decoration in tile.Decorations)
                {
                    // SKIP static animal decorations - these are now rendered as live wildlife entities
                    if (IsAnimalDecoration(decoration.Type))
                    {
                        continue;
                    }

                    // Get decoration color
                    var decorColor = decoration.GetColor(seasonIndex, timeOfDay);

                    // Convert DecorationColor to Raylib Color
                    var color = new Color(decorColor.R, decorColor.G, decorColor.B, decorColor.A);

                    // Apply darkness
                    if (darknessFactor > 0)
                    {
                        color = DarkenColor(color, darknessFactor);
                    }

                    // Calculate position with slight offset based on decoration type
                    // This prevents all decorations from rendering at exact tile center
                    int offsetX = 0;
                    int offsetY = 0;

                    // Offset wildlife and some plants for more natural placement
                    switch (decoration.Type)
                    {
                        case VillageBuilder.Engine.World.DecorationType.BirdFlying:
                        case VillageBuilder.Engine.World.DecorationType.Butterfly:
                            // Flying creatures - offset based on animation phase
                            offsetX = (int)(Math.Sin(decoration.AnimationPhase * Math.PI * 2) * 3);
                            offsetY = (int)(Math.Cos(decoration.AnimationPhase * Math.PI * 2) * 2);
                            break;

                        case VillageBuilder.Engine.World.DecorationType.GrassTuft:
                        case VillageBuilder.Engine.World.DecorationType.FlowerWild:
                            // Slight offset for natural scattering
                            offsetX = (decoration.VariantIndex % 2 == 0) ? 2 : -2;
                            offsetY = (decoration.VariantIndex > 1) ? 2 : -2;
                            break;
                    }

                    // NEW: Sprite-based rendering (modern, colorful emojis)
                    if (GraphicsConfig.UseSpriteMode)
                    {
                        var sprite = SpriteAtlasManager.Instance.GetSprite(decoration.Type);

                        if (sprite.HasValue)
                        {
                            // Render sprite texture centered in tile
                            int spriteSize = (int)(size * 0.9f); // Slightly smaller than tile for padding
                            int spriteX = (int)pos.X + (size - spriteSize) / 2 + offsetX;
                            int spriteY = (int)pos.Y + (size - spriteSize) / 2 + offsetY;

                                    var sourceRect = new Rectangle(0, 0, sprite.Value.Width, sprite.Value.Height);
                                    var destRect = new Rectangle(spriteX, spriteY, spriteSize, spriteSize);
                                    var origin = new System.Numerics.Vector2(0, 0);

                                    // Render sprite WITHOUT color tinting - Twemoji sprites are already colored!
                                    // Use Color.White to preserve the sprite's original colors
                                    Raylib.DrawTexturePro(sprite.Value, sourceRect, destRect, origin, 0f, Color.White);
                                    continue; // Skip text rendering
                                }
                            }

                            // FALLBACK: Text-based rendering (ASCII symbols)
                    // Used when sprite mode is disabled or sprite not available
                    string glyph = decoration.GetGlyph();
                    int textX = (int)pos.X + (size - FontSize) / 2 + offsetX;
                    int textY = (int)pos.Y + (size - FontSize) / 2 + offsetY;

                                        // Render decoration with automatic font selection (emoji font for emojis)
                                        GraphicsConfig.DrawConsoleTextAuto(glyph, textX, textY, FontSize, color);
                                    }
                                }

                                    /// <summary>
                                    /// Check if a decoration type represents an animal (which should be rendered as wildlife entities instead)
                                    /// </summary>
                                    private bool IsAnimalDecoration(VillageBuilder.Engine.World.DecorationType type)
                                    {
                                        return type == VillageBuilder.Engine.World.DecorationType.RabbitSmall ||
                                               type == VillageBuilder.Engine.World.DecorationType.DeerGrazing ||
                                               type == VillageBuilder.Engine.World.DecorationType.FishInWater ||
                                               type == VillageBuilder.Engine.World.DecorationType.BirdFlying ||
                                               type == VillageBuilder.Engine.World.DecorationType.BirdPerched ||
                                               type == VillageBuilder.Engine.World.DecorationType.FoxHunting ||
                                               type == VillageBuilder.Engine.World.DecorationType.WolfPack ||
                                               type == VillageBuilder.Engine.World.DecorationType.BearGrizzly ||
                                               type == VillageBuilder.Engine.World.DecorationType.BoarWild;
                                    }
                                }
                            }
