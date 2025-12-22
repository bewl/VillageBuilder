using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raylib_cs;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.World;
using VillageBuilder.Engine.Buildings;
using System.Numerics;

namespace VillageBuilder.Game.Graphics.UI
{
    public class MapRenderer
    {
        private const int FontSize = 16;

        public void Render(GameEngine engine, Camera2D camera, VillageBuilder.Game.Core.SelectionManager? selectionManager = null)
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

                    // Draw tile glyph
                    DrawTileGlyph(tile, pos, tileSize, darknessFactor);
                }
            }
            
            // Render people on top of everything
            RenderPeople(engine, tileSize, minX, maxX, minY, maxY, selectionManager);
            
            // Render selection indicators for buildings
            if (selectionManager?.SelectedBuilding != null)
            {
                DrawBuildingSelection(selectionManager.SelectedBuilding, tileSize);
            }
        }

        private void DrawDetailedBuilding(Building building, int tileSize, GameTime time)
        {
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

            // Construction indicator (if any)
            if (!building.IsConstructed)
            {
                foreach (var tilePos in occupiedTiles)
                {
                    var pos = new Vector2(tilePos.X * tileSize, tilePos.Y * tileSize);
                    int textX = (int)pos.X + (tileSize - FontSize) / 2;
                    int textY = (int)pos.Y + (tileSize - FontSize) / 2;
                    GraphicsConfig.DrawConsoleText("?", textX, textY, FontSize, new Color(255, 255, 100, 255));
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
                TileType.Grass => new Color(20, 40, 20, 255),
                TileType.Forest => new Color(10, 30, 10, 255),
                TileType.Water => new Color(10, 20, 40, 255),
                TileType.Mountain => new Color(30, 30, 35, 255),
                TileType.Field => new Color(40, 35, 20, 255),
                TileType.Road => new Color(40, 40, 40, 255),
                _ => new Color(20, 20, 20, 255)
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
        
        private void RenderPeople(GameEngine engine, int tileSize, int minX, int maxX, int minY, int maxY, VillageBuilder.Game.Core.SelectionManager? selectionManager)
        {
            // Group people by position to handle multiple people per tile
            var peopleByPosition = engine.Families
                .SelectMany(f => f.Members)
                .Where(p => p.IsAlive)
                .GroupBy(p => new { p.Position.X, p.Position.Y });
            
            foreach (var group in peopleByPosition)
            {
                int posX = group.Key.X;
                int posY = group.Key.Y;
                
                // Only render if in visible area
                if (posX < minX || posX >= maxX || posY < minY || posY >= maxY)
                    continue;
                
                var pos = new Vector2(posX * tileSize, posY * tileSize);
                var peopleAtTile = group.ToList();
                var displayPerson = peopleAtTile[0]; // Show first person's details
                
                // Draw path for first person if moving
                if (displayPerson.CurrentPath != null && displayPerson.CurrentPath.Count > 0 && displayPerson.CurrentTask == VillageBuilder.Engine.Entities.PersonTask.MovingToLocation)
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
                
                // Draw person background
                var bgColor = displayPerson.Gender == VillageBuilder.Engine.Entities.Gender.Male 
                    ? new Color(80, 120, 200, 220)
                    : new Color(200, 80, 120, 220);
                
                Raylib.DrawRectangle((int)pos.X, (int)pos.Y, tileSize, tileSize, bgColor);
                
                // Draw selection indicator
                if (selectionManager?.SelectedPerson == displayPerson)
                {
                    Raylib.DrawRectangleLines((int)pos.X, (int)pos.Y, tileSize, tileSize, new Color(255, 255, 0, 255));
                    Raylib.DrawRectangleLines((int)pos.X + 1, (int)pos.Y + 1, tileSize - 2, tileSize - 2, new Color(255, 255, 0, 255));
                }
                
                // Draw universal person symbol
                string glyph = "☺";
                var glyphColor = new Color(255, 255, 255, 255);
                
                int textX = (int)pos.X + (tileSize - FontSize) / 2;
                int textY = (int)pos.Y + (tileSize - FontSize) / 2;
                
                GraphicsConfig.DrawConsoleText(glyph, textX, textY, FontSize, glyphColor);
                
                // Draw count if multiple people on this tile
                if (peopleAtTile.Count > 1)
                {
                    var countText = peopleAtTile.Count.ToString();
                    var countColor = new Color(255, 255, 0, 255);
                    Raylib.DrawCircle((int)pos.X + tileSize - 8, (int)pos.Y + 8, 8, new Color(200, 0, 0, 220));
                    GraphicsConfig.DrawConsoleText(countText, (int)pos.X + tileSize - 12, (int)pos.Y + 2, 14, countColor);
                }
                
                // Draw task indicator if working
                if (displayPerson.AssignedBuilding != null)
                {
                    var taskColor = new Color(255, 255, 0, 255);
                    Raylib.DrawRectangle((int)pos.X, (int)pos.Y, 3, 3, taskColor);
                }
            }
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

        private string GetTileGlyph(Tile tile)
        {
            return tile.Type switch
            {
                TileType.Grass => "\"",
                TileType.Forest => "♣",
                TileType.Water => "≈",
                TileType.Mountain => "▲",
                TileType.Field => "≡",
                TileType.Road => "·",
                _ => "?"
            };
        }
    }
}
