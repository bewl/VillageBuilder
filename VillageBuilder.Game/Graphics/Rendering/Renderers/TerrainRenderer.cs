using System.Numerics;
using Raylib_cs;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.World;

namespace VillageBuilder.Game.Graphics.Rendering.Renderers
{
    /// <summary>
    /// Renders terrain tiles including backgrounds, glyphs, and decorations.
    /// Extracted from MapRenderer for single responsibility.
    /// </summary>
    public class TerrainRenderer : IBatchRenderer<Tile>
    {
        private static int FontSize => GraphicsConfig.SmallConsoleFontSize;
        private readonly SpriteAtlasManager _spriteAtlas;
        
        public TerrainRenderer()
        {
            _spriteAtlas = SpriteAtlasManager.Instance;
        }
        
        public void RenderBatch(IEnumerable<Tile> tiles, RenderContext context)
        {
            foreach (var tile in tiles)
            {
                if (ShouldRender(tile, context))
                {
                    Render(tile, context);
                }
            }
        }
        
        public void Render(Tile tile, RenderContext context)
        {
            var pos = context.GetWorldPosition(tile.X, tile.Y);
            
            // Draw tile background
            var bgColor = ColorPalette.GetTileBackgroundFromConfig(tile.Type);
            
            // Apply darkness overlay for night
            if (context.DarknessFactor > 0)
            {
                bgColor = RenderHelpers.DarkenColor(bgColor, context.DarknessFactor);
            }
            
            Raylib.DrawRectangle((int)pos.X, (int)pos.Y, context.TileSize, context.TileSize, bgColor);
            
            // Draw base tile glyph (skip in sprite mode to reduce visual clutter)
            if (!context.UseSpriteMode)
            {
                DrawTileGlyph(tile, pos, context);
            }
            
            // Draw terrain decorations on top
            DrawTerrainDecorations(tile, pos, context);
        }
        
        public bool ShouldRender(Tile tile, RenderContext context)
        {
            // Skip tiles that have buildings - buildings render themselves
            if (tile.Building != null)
                return false;
            
            // Check if in visible bounds
            return context.IsVisible(tile.X, tile.Y);
        }
        
        private void DrawTileGlyph(Tile tile, Vector2 pos, RenderContext context)
        {
            string glyph = GetTileGlyph(tile);
            var color = GetTileForegroundColor(tile.Type);
            
            // Apply darkness
            if (context.DarknessFactor > 0)
            {
                color = RenderHelpers.DarkenColor(color, context.DarknessFactor);
            }
            
            int textX = (int)pos.X + (context.TileSize - FontSize) / 2;
            int textY = (int)pos.Y + (context.TileSize - FontSize) / 2;
            
            GraphicsConfig.DrawConsoleText(glyph, textX, textY, FontSize, color);
        }
        
        private void DrawTerrainDecorations(Tile tile, Vector2 pos, RenderContext context)
        {
            if (tile.Decorations.Count == 0) return;
            
            float timeOfDay = context.GameTime.Hour / 24f;
            int seasonIndex = (int)context.GameTime.CurrentSeason;
            
            foreach (var decoration in tile.Decorations)
            {
                // Skip static animal decorations - rendered as live wildlife entities
                if (IsAnimalDecoration(decoration.Type))
                    continue;
                
                // Try to render with sprite first
                if (context.UseSpriteMode && _spriteAtlas.SpriteModeEnabled)
                {
                    var sprite = _spriteAtlas.GetSprite(decoration.Type);
                    if (sprite.HasValue)
                    {
                        DrawDecorationSprite(decoration, sprite.Value, pos, context);
                        continue;
                    }
                }
                
                // Fallback to ASCII rendering
                DrawDecorationAscii(decoration, pos, context, seasonIndex, timeOfDay);
            }
        }
        
        private void DrawDecorationSprite(TerrainDecoration decoration, Texture2D sprite, Vector2 pos, RenderContext context)
        {
            // Calculate sprite position and size
            int spriteSize = (int)(context.TileSize * 0.9f); // 90% of tile
            int offsetX = (context.TileSize - spriteSize) / 2;
            int offsetY = (context.TileSize - spriteSize) / 2;
            
            int spriteX = (int)pos.X + offsetX;
            int spriteY = (int)pos.Y + offsetY;
            
            // Source rectangle (full sprite)
            Rectangle sourceRect = new Rectangle(0, 0, sprite.Width, sprite.Height);
            
            // Destination rectangle
            Rectangle destRect = new Rectangle(spriteX, spriteY, spriteSize, spriteSize);
            
            // Draw sprite with alpha channel preserved
            Raylib.DrawTexturePro(
                sprite,
                sourceRect,
                destRect,
                new Vector2(0, 0),
                0f,
                Color.White // Use White to preserve original colors
            );
            
            // Apply darkness overlay if needed
            if (context.DarknessFactor > 0.2f)
            {
                var darknessColor = new Color((byte)0, (byte)0, (byte)0, (byte)(context.DarknessFactor * 150));
                Raylib.DrawRectangle(spriteX, spriteY, spriteSize, spriteSize, darknessColor);
            }
        }
        
        private void DrawDecorationAscii(TerrainDecoration decoration, Vector2 pos, RenderContext context, int seasonIndex, float timeOfDay)
        {
            var decorColor = decoration.GetColor(seasonIndex, timeOfDay);
            var color = new Color(decorColor.R, decorColor.G, decorColor.B, decorColor.A);
            
            // Apply darkness
            if (context.DarknessFactor > 0)
            {
                color = RenderHelpers.DarkenColor(color, context.DarknessFactor);
            }
            
            string glyph = decoration.GetGlyph();
            int textX = (int)pos.X + (context.TileSize - FontSize) / 2;
            int textY = (int)pos.Y + (context.TileSize - FontSize) / 2;
            
            GraphicsConfig.DrawConsoleText(glyph, textX, textY, FontSize, color);
        }
        
        private bool IsAnimalDecoration(DecorationType type)
        {
            return type == DecorationType.RabbitSmall ||
                   type == DecorationType.DeerGrazing ||
                   type == DecorationType.BirdFlying ||
                   type == DecorationType.BirdPerched ||
                   type == DecorationType.FoxHunting ||
                   type == DecorationType.WolfPack ||
                   type == DecorationType.BearGrizzly ||
                   type == DecorationType.BoarWild;
        }
        
        private string GetTileGlyph(Tile tile)
        {
            int variant = tile.TerrainVariant % 4;
            
            return tile.Type switch
            {
                TileType.Grass => variant switch
                {
                    0 => "\"",
                    1 => "'",
                    2 => ",",
                    _ => "."
                },
                TileType.Forest => variant switch
                {
                    0 => "?",
                    1 => "?",
                    2 => "?",
                    _ => "?"
                },
                TileType.Water => variant switch
                {
                    0 => "?",
                    1 => "~",
                    2 => "?",
                    _ => "?"
                },
                TileType.Mountain => variant switch
                {
                    0 => "?",
                    1 => "?",
                    2 => "?",
                    _ => "?"
                },
                TileType.Field => "?",
                TileType.Road => "·",
                _ => "?"
            };
        }
        
        private Color GetTileForegroundColor(TileType type)
        {
            return type switch
            {
                TileType.Grass => ColorPalette.GrassForeground,
                TileType.Forest => ColorPalette.ForestForeground,
                TileType.Water => ColorPalette.WaterForeground,
                TileType.Mountain => ColorPalette.MountainForeground,
                TileType.Field => ColorPalette.FieldForeground,
                TileType.Road => ColorPalette.RoadForeground,
                _ => Color.Gray
            };
        }
    }
}
