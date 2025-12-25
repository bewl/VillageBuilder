using System.Numerics;
using Raylib_cs;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Core;

namespace VillageBuilder.Game.Graphics.Rendering.Renderers
{
    /// <summary>
    /// Renders buildings including construction stages, lighting, and tile details.
    /// Buildings are rendered as multi-tile structures.
    /// </summary>
    public class BuildingRenderer : IRenderer<Building>
    {
        private static int FontSize => GraphicsConfig.SmallConsoleFontSize;
        
        public void Render(Building building, RenderContext context)
        {
            // Render construction or completed building
            if (!building.IsConstructed)
            {
                RenderConstruction(building, context);
            }
            else
            {
                RenderCompleted(building, context);
            }
            
            // Draw selection if selected
            if (context.SelectionManager?.SelectedBuilding == building)
            {
                DrawBuildingSelection(building, context);
            }
        }
        
        public bool ShouldRender(Building building, RenderContext context)
        {
            // Check if any tile of the building is visible
            var tiles = building.GetOccupiedTiles();
            return tiles.Any(t => context.IsVisible(t.X, t.Y));
        }
        
        private void RenderCompleted(Building building, RenderContext context)
        {
            bool showLights = building.ShouldShowLights(context.GameTime);
            var occupiedTiles = building.GetOccupiedTiles();

            // Check if we should use sprite mode
            bool useSpriteMode = context.UseSpriteMode && SpriteAtlasManager.Instance.SpriteModeEnabled;
            var spriteType = SpriteAtlasManager.GetBuildingSpriteType(building.Type);
            bool hasSprite = spriteType.HasValue && SpriteAtlasManager.Instance.HasSprite(spriteType.Value);

            // If sprite mode and we have a sprite, render building as single sprite on center tile
            if (useSpriteMode && hasSprite && occupiedTiles.Count > 0)
            {
                RenderBuildingAsSprite(building, occupiedTiles, spriteType.Value, context, showLights);
            }
            else
            {
                // ASCII mode - render each tile individually
                RenderBuildingAsASCII(building, occupiedTiles, context, showLights);
            }
        }

        private void RenderBuildingAsSprite(Building building, List<Engine.Buildings.Vector2Int> occupiedTiles, 
            Engine.World.DecorationType spriteType, RenderContext context, bool showLights)
        {
            // Get center tile
            var centerTile = occupiedTiles[occupiedTiles.Count / 2];
            var centerPos = context.GetWorldPosition(centerTile.X, centerTile.Y);

            // Draw background tiles first
            foreach (var tilePos in occupiedTiles)
            {
                var pos = context.GetWorldPosition(tilePos.X, tilePos.Y);
                Color bgColor = new Color(40, 35, 30, 255); // Dark brown for building background

                // Apply darkness
                if (context.DarknessFactor > 0)
                {
                    bgColor = RenderHelpers.DarkenColor(bgColor, context.DarknessFactor);
                }

                // Add warm glow if lights are on
                if (showLights)
                {
                    bgColor = AddWarmGlow(bgColor, 0.3f);
                }

                Raylib.DrawRectangle((int)pos.X, (int)pos.Y, context.TileSize, context.TileSize, bgColor);
            }

            // Draw sprite on center tile
            var sprite = SpriteAtlasManager.Instance.GetSprite(spriteType);
            if (sprite != null)
            {
                var texture = sprite.Value;
                int spriteSize = context.TileSize;

                // For multi-tile buildings, make sprite larger
                if (occupiedTiles.Count > 1)
                {
                    spriteSize = (int)(context.TileSize * 1.5f);
                    centerPos.X -= (spriteSize - context.TileSize) / 2;
                    centerPos.Y -= (spriteSize - context.TileSize) / 2;
                }

                var destRect = new Rectangle((int)centerPos.X, (int)centerPos.Y, spriteSize, spriteSize);
                var sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);

                // Tint based on time of day
                Color tint = Color.White;
                if (context.DarknessFactor > 0)
                {
                    int brightness = (int)(255 * (1 - context.DarknessFactor));
                    tint = new Color(brightness, brightness, brightness, 255);
                }
                if (showLights)
                {
                    tint = AddWarmGlow(tint, 0.4f);
                }

                Raylib.DrawTexturePro(texture, sourceRect, destRect, Vector2.Zero, 0, tint);
            }
        }

        private void RenderBuildingAsASCII(Building building, List<Engine.Buildings.Vector2Int> occupiedTiles, 
            RenderContext context, bool showLights)
        {
            foreach (var tilePos in occupiedTiles)
            {
                var buildingTile = building.GetTileAtWorldPosition(tilePos.X, tilePos.Y);
                if (buildingTile == null) continue;

                var pos = context.GetWorldPosition(tilePos.X, tilePos.Y);

                // Get tile color based on type
                Color tileColor = GetBuildingTileColor(building.Type, buildingTile.Value.Type);

                // Apply darkness
                if (context.DarknessFactor > 0)
                {
                    tileColor = RenderHelpers.DarkenColor(tileColor, context.DarknessFactor);
                }

                // Add warm glow if lights are on
                if (showLights && buildingTile.Value.Type == BuildingTileType.Floor)
                {
                    tileColor = AddWarmGlow(tileColor, 0.4f);
                }

                Raylib.DrawRectangle((int)pos.X, (int)pos.Y, context.TileSize, context.TileSize, tileColor);

                // Draw building glyph
                if (buildingTile.Value.Glyph != ' ')
                {
                    DrawBuildingGlyph(buildingTile.Value, pos, context, showLights);
                }
            }
        }
        
        private void RenderConstruction(Building building, RenderContext context)
        {
            var stage = building.GetConstructionStage();
            var occupiedTiles = building.GetOccupiedTiles();
            var progressPercent = building.GetConstructionProgressPercent();
            
            Color stageColor = stage switch
            {
                ConstructionStage.Foundation => new Color(101, 67, 33, 255),
                ConstructionStage.Framing => new Color(139, 90, 43, 255),
                ConstructionStage.Walls => new Color(160, 120, 80, 255),
                ConstructionStage.Finishing => new Color(180, 140, 100, 255),
                _ => new Color(100, 100, 100, 255)
            };
            
            string stageGlyph = stage switch
            {
                ConstructionStage.Foundation => "?",
                ConstructionStage.Framing => "?",
                ConstructionStage.Walls => "?",
                ConstructionStage.Finishing => "?",
                _ => "·"
            };
            
            foreach (var tilePos in occupiedTiles)
            {
                var pos = context.GetWorldPosition(tilePos.X, tilePos.Y);
                
                Raylib.DrawRectangle((int)pos.X, (int)pos.Y, context.TileSize, context.TileSize, stageColor);
                
                int textX = (int)pos.X + (context.TileSize - FontSize) / 2;
                int textY = (int)pos.Y + (context.TileSize - FontSize) / 2;
                
                GraphicsConfig.DrawConsoleText(stageGlyph, textX, textY, FontSize, new Color(200, 200, 200, 255));
            }
            
            // Draw progress on center tile
            if (occupiedTiles.Count > 0)
            {
                var centerTile = occupiedTiles[occupiedTiles.Count / 2];
                var centerPos = context.GetWorldPosition(centerTile.X, centerTile.Y);
                
                string progressText = $"{progressPercent}%";
                int progressX = (int)centerPos.X + (context.TileSize - progressText.Length * 8) / 2;
                int progressY = (int)centerPos.Y + context.TileSize - 18;
                
                RenderHelpers.DrawTextWithShadow(progressText, progressX, progressY, 14, new Color(255, 255, 100, 255));
                
                // Draw worker count
                if (building.ConstructionWorkers.Count > 0)
                {
                    string workerText = $"??{building.ConstructionWorkers.Count}";
                    int workerX = (int)centerPos.X + 2;
                    int workerY = (int)centerPos.Y + 2;
                    RenderHelpers.DrawTextWithShadow(workerText, workerX, workerY, 14, Color.White);
                }
            }
        }
        
        private void DrawBuildingGlyph(BuildingTile tile, Vector2 pos, RenderContext context, bool showLights)
        {
            int textX = (int)pos.X + (context.TileSize - FontSize) / 2;
            int textY = (int)pos.Y + (context.TileSize - FontSize) / 2;
            
            Color glyphColor = tile.Type switch
            {
                BuildingTileType.Wall => new Color(255, 255, 255, 255),
                BuildingTileType.Floor => new Color(200, 200, 200, 255),
                BuildingTileType.Door => new Color(255, 255, 0, 255),
                _ => Color.White
            };
            
            // Apply darkness or brightness
            if (context.DarknessFactor > 0 && !showLights)
            {
                glyphColor = RenderHelpers.DarkenColor(glyphColor, context.DarknessFactor);
            }
            else if (showLights)
            {
                glyphColor = new Color(255, 255, 200, 255);
            }
            
            GraphicsConfig.DrawConsoleText(tile.Glyph.ToString(), textX, textY, FontSize, glyphColor);
        }
        
        private void DrawBuildingSelection(Building building, RenderContext context)
        {
            var tiles = building.GetOccupiedTiles();
            foreach (var tile in tiles)
            {
                var pos = context.GetWorldPosition(tile.X, tile.Y);
                var bounds = new Rectangle((int)pos.X, (int)pos.Y, context.TileSize, context.TileSize);
                
                // Use green for building selection (different from people/wildlife)
                RenderHelpers.DrawSelectionHighlight(bounds, new Color(0, 255, 0, 255));
            }
        }
        
        private Color GetBuildingTileColor(BuildingType type, BuildingTileType tileType)
        {
            return tileType switch
            {
                BuildingTileType.Wall => GetWallColor(type),
                BuildingTileType.Floor => GetFloorColor(type),
                BuildingTileType.Door => new Color(120, 80, 40, 255),
                _ => GetBackgroundColor(type)
            };
        }
        
        private Color GetBackgroundColor(BuildingType type)
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
        
        private Color AddWarmGlow(Color color, float intensity)
        {
            intensity = Math.Clamp(intensity, 0f, 1f);

            return new Color(
                (byte)Math.Min(255, (int)(color.R + 80 * intensity)),
                (byte)Math.Min(255, (int)(color.G + 40 * intensity)),
                color.B,
                color.A
            );
        }
    }
}
