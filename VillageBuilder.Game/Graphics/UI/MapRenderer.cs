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
        private RenderTexture2D _lightingTexture;
        private bool _initialized = false;
        private const int FontSize = 16;

        public void Render(GameEngine engine, Camera2D camera)
        {
            if (!_initialized)
            {
                _lightingTexture = Raylib.LoadRenderTexture(GraphicsConfig.ScreenWidth, GraphicsConfig.ScreenHeight);
                _initialized = true;
            }

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

            // Render only visible tiles (culling for performance)
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    var tile = grid.GetTile(x, y);
                    if (tile == null) continue;

                    var pos = new Vector2(x * tileSize, y * tileSize);

                    // Draw tile background
                    var bgColor = GetTileBackgroundColor(tile);
                    Raylib.DrawRectangle((int)pos.X, (int)pos.Y, tileSize, tileSize, bgColor);

                    // Draw building only once per unique building instance
                    if (tile.Building != null && !drawnBuildings.Contains(tile.Building))
                    {
                        DrawDetailedBuilding(tile.Building, tileSize);
                        drawnBuildings.Add(tile.Building);
                    }
                    else if (tile.Building == null)
                    {
                        // Draw tile glyph only if no building
                        DrawTileGlyph(tile, pos, tileSize);
                    }
                }
            }

            // Render modern lighting system using render texture
            RenderModernLighting(engine, camera, tileSize);
            
            // CRITICAL: Reset blend mode to default after all lighting operations
            Raylib.BeginBlendMode(BlendMode.Alpha);
            Raylib.EndBlendMode();
        }

        private void DrawDetailedBuilding(Building building, int tileSize)
        {
            var definition = building.Definition;
            var bgColor = GetBuildingBackgroundColor(building.Type);
            var wallColor = GetWallColor(building.Type);
            var floorColor = GetFloorColor(building.Type);
            var doorColor = new Color(120, 80, 40, 255);

            // Draw each tile directly from the layout
            for (int y = 0; y < definition.Height; y++)
            {
                for (int x = 0; x < definition.Width; x++)
                {
                    var buildingTile = definition.Layout[y, x];
                    
                    if (buildingTile.Type == BuildingTileType.Empty)
                        continue;

                    // Calculate world position with rotation
                    var offset = RotateOffset(new Vector2Int(x, y), building.Rotation);
                    int worldX = building.X + offset.X;
                    int worldY = building.Y + offset.Y;
                    
                    var pos = new Vector2(worldX * tileSize, worldY * tileSize);

                    // Draw background based on tile type
                    Color tileColor = buildingTile.Type switch
                    {
                        BuildingTileType.Wall => wallColor,
                        BuildingTileType.Floor => floorColor,
                        BuildingTileType.Door => doorColor,
                        _ => bgColor
                    };

                    Raylib.DrawRectangle((int)pos.X, (int)pos.Y, tileSize, tileSize, tileColor);

                    // Draw the character/glyph - SIMPLIFIED for testing
                    if (buildingTile.Glyph != ' ')
                    {
                        int textX = (int)pos.X + (tileSize - FontSize) / 2;
                        int textY = (int)pos.Y + (tileSize - FontSize) / 2;

                        Color glyphColor = buildingTile.Type switch
                        {
                            BuildingTileType.Wall => new Color(255, 255, 255, 255), // Bright white
                            BuildingTileType.Floor => new Color(200, 200, 200, 255),
                            BuildingTileType.Door => new Color(255, 255, 0, 255), // Yellow
                            _ => Color.White
                        };

                        // TEMPORARY: Use simple ASCII instead of box-drawing
                        string displayChar = buildingTile.Type switch
                        {
                            BuildingTileType.Wall => "#",  // Use # for walls
                            BuildingTileType.Floor => ".",
                            BuildingTileType.Door => "D",  // Use D for door
                            _ => buildingTile.Glyph.ToString()
                        };

                        GraphicsConfig.DrawConsoleText(
                            displayChar,
                            textX,
                            textY,
                            FontSize,
                            glyphColor
                        );
                    }
                }
            }

            // Add construction indicator if not complete
            if (!building.IsConstructed)
            {
                var occupiedTiles = building.GetOccupiedTiles();
                foreach (var tilePos in occupiedTiles)
                {
                    var pos = new Vector2(tilePos.X * tileSize, tilePos.Y * tileSize);
                    int textX = (int)pos.X + (tileSize - FontSize) / 2;
                    int textY = (int)pos.Y + (tileSize - FontSize) / 2;
                    GraphicsConfig.DrawConsoleText("?", textX, textY, FontSize, new Color(255, 255, 100, 255));
                }
            }
        }

        // This helper method should match the one in BuildingDefinition exactly
        private Vector2Int RotateOffset(Vector2Int offset, BuildingRotation rotation)
        {
            return rotation switch
            {
                BuildingRotation.North => offset,
                BuildingRotation.East => new Vector2Int(-offset.Y, offset.X),
                BuildingRotation.South => new Vector2Int(-offset.X, -offset.Y),
                BuildingRotation.West => new Vector2Int(offset.Y, -offset.X),
                _ => offset
            };
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

        private void RenderModernLighting(GameEngine engine, Camera2D camera, int tileSize)
        {
            var hour = engine.Time.Hour;
            var ambientLight = CalculateAmbientLight(hour);
            
            // Only apply lighting effects during evening/night
            if (hour < 6 || hour > 17)
            {
                // Render lights to texture for smooth blending
                Raylib.BeginTextureMode(_lightingTexture);
                Raylib.ClearBackground(new Color(0, 0, 0, 0)); // Transparent background
                
                // Switch to world space
                Raylib.BeginMode2D(camera);
                
                // Use additive blending for light accumulation
                Raylib.BeginBlendMode(BlendMode.Additive);
                
                // Draw each light source
                foreach (var building in engine.Buildings.Where(b => b.IsConstructed))
                {
                    DrawSmoothLight(building, tileSize, hour);
                }
                
                Raylib.EndBlendMode();
                Raylib.EndMode2D();
                Raylib.EndTextureMode();
                
                // IMPORTANT: We're now back in the calling context (still in world space from GameRenderer)
                // So we need to temporarily exit world space to draw screen-space lighting effects
                Raylib.EndMode2D(); // Exit world space temporarily
                
                // Draw the lighting texture over the scene with multiplicative blending
                Raylib.BeginBlendMode(BlendMode.Multiplied);
                Raylib.DrawTextureRec(
                    _lightingTexture.Texture,
                    new Rectangle(0, 0, _lightingTexture.Texture.Width, -_lightingTexture.Texture.Height),
                    new Vector2(0, 0),
                    Color.White
                );
                Raylib.EndBlendMode();
                
                // Draw darkness overlay
                var darknessAlpha = (byte)(255 - ambientLight);
                if (darknessAlpha > 0)
                {
                    Raylib.BeginBlendMode(BlendMode.Alpha);
                    Raylib.DrawRectangle(0, 0, GraphicsConfig.ScreenWidth, GraphicsConfig.ScreenHeight,
                        new Color((byte)5, (byte)10, (byte)25, darknessAlpha));
                    Raylib.EndBlendMode();
                }
                
                // Re-enter world space so caller's context is maintained
                Raylib.BeginMode2D(camera);
            }
        }

        private void DrawSmoothLight(Building building, int tileSize, int hour)
        {
            var centerPos = new Vector2(
                building.X * tileSize + tileSize / 2f,
                building.Y * tileSize + tileSize / 2f
            );

            // Get light properties
            var (lightColor, intensity, radius, flicker) = GetLightProperties(building.Type);
            
            // Skip if no light
            if (intensity <= 0) return;
            
            // Flickering effect
            float flickerAmount = flicker 
                ? (float)(Math.Sin(Raylib.GetTime() * 6 + building.X * building.Y) * 0.08 + 0.92) 
                : 1.0f;
            
            // Time-based intensity
            float timeMultiplier = CalculateLightTimeMultiplier(hour);
            intensity *= timeMultiplier * flickerAmount;
            
            if (intensity <= 0) return;

            float maxRadius = radius * tileSize;
            
            // Draw smooth gradient using fewer, larger circles
            int steps = 12;
            for (int i = steps; i > 0; i--)
            {
                float t = (float)i / steps;
                float currentRadius = maxRadius * t;
                
                // Smooth falloff
                float falloff = 1.0f - (t * t);
                byte alpha = (byte)(intensity * falloff * 180);
                
                if (alpha > 3)
                {
                    var layerColor = new Color(
                        lightColor.R,
                        lightColor.G,
                        lightColor.B,
                        alpha
                    );
                    
                    Raylib.DrawCircleGradient(
                        (int)centerPos.X,
                        (int)centerPos.Y,
                        currentRadius,
                        layerColor,
                        new Color((byte)lightColor.R, (byte)lightColor.G, (byte)lightColor.B, (byte)0)
                    );
                }
            }
        }

        private (Color color, float intensity, float radius, bool flicker) GetLightProperties(BuildingType buildingType)
        {
            return buildingType switch
            {
                BuildingType.House => (new Color(255, 200, 130, 255), 0.9f, 5.0f, true),
                BuildingType.Workshop => (new Color(255, 160, 80, 255), 1.2f, 6.0f, true),
                BuildingType.Mine => (new Color(255, 210, 150, 255), 0.7f, 4.0f, true),
                BuildingType.Lumberyard => (new Color(255, 230, 180, 255), 0.5f, 4.0f, false),
                BuildingType.Market => (new Color(255, 240, 200, 255), 1.0f, 6.0f, false),
                BuildingType.TownHall => (new Color(255, 245, 220, 255), 1.3f, 8.0f, false),
                BuildingType.Farm => (new Color(255, 220, 160, 255), 0.4f, 3.0f, false),
                BuildingType.Warehouse => (new Color(255, 230, 190, 255), 0.3f, 3.0f, false),
                BuildingType.Well => (new Color(0, 0, 0, 0), 0.0f, 0.0f, false),
                _ => (new Color(255, 230, 180, 255), 0.5f, 4.0f, false)
            };
        }

        private float CalculateLightTimeMultiplier(int hour)
        {
            return hour switch
            {
                >= 19 or <= 5 => 1.0f,      // Full night
                18 => 0.8f,                  // Dusk
                17 => 0.4f,                  // Early evening
                6 => 0.3f,                   // Dawn
                _ => 0.0f                    // Day
            };
        }

        private byte CalculateAmbientLight(int hour)
        {
            return hour switch
            {
                >= 8 and <= 16 => 255,       // Full daylight
                7 or 17 => 200,              // Morning/Evening
                6 or 18 => 120,              // Dawn/Dusk
                5 or 19 => 60,               // Early/Late twilight
                4 or 20 => 35,               // Deep twilight
                _ => 20                      // Night
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

        private void DrawTileGlyph(Tile tile, Vector2 pos, int size)
        {
            string glyph = GetTileGlyph(tile);
            var color = GetTileForegroundColor(tile);
            
            int textX = (int)pos.X + (size - FontSize) / 2;
            int textY = (int)pos.Y + (size - FontSize) / 2;
            
            GraphicsConfig.DrawConsoleText(glyph, textX, textY, FontSize, color);
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
