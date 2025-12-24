using System.Numerics;
using Raylib_cs;
using VillageBuilder.Engine.Entities.Wildlife;
using VillageBuilder.Engine.World;

namespace VillageBuilder.Game.Graphics.Rendering.Renderers
{
    /// <summary>
    /// Renders wildlife entities with sprites or ASCII fallback.
    /// Includes health bars and behavior indicators.
    /// </summary>
    public class WildlifeRenderer : IRenderer<WildlifeEntity>
    {
        private static int FontSize => GraphicsConfig.SmallConsoleFontSize;
        private readonly SpriteAtlasManager _spriteAtlas;
        
        public WildlifeRenderer()
        {
            _spriteAtlas = SpriteAtlasManager.Instance;
        }
        
        public void Render(WildlifeEntity wildlife, RenderContext context)
        {
            var pos = context.GetWorldPosition(wildlife.Position.X, wildlife.Position.Y);
            
            // Get wildlife visuals
            var spriteType = GetWildlifeSpriteType(wildlife.Type);
            bool willUseSprite = spriteType.HasValue &&
                                context.UseSpriteMode &&
                                _spriteAtlas.GetSprite(spriteType.Value).HasValue;
            
            // Draw background only for ASCII mode
            if (!willUseSprite)
            {
                var bgColor = GetWildlifeBackgroundColor(wildlife);
                Raylib.DrawRectangle((int)pos.X, (int)pos.Y, context.TileSize, context.TileSize, bgColor);
            }
            
            // Draw selection indicator if selected
            bool isSelected = context.SelectionManager?.SelectedWildlife == wildlife;
            if (isSelected)
            {
                var selectionBounds = new Rectangle((int)pos.X, (int)pos.Y, context.TileSize, context.TileSize);
                RenderHelpers.DrawSelectionHighlight(selectionBounds, ColorPalette.SelectionYellow);
            }
            
            // Draw wildlife visual (sprite or glyph)
            if (willUseSprite && spriteType.HasValue)
            {
                DrawWildlifeSprite(wildlife, spriteType.Value, pos, context);
            }
            else
            {
                DrawWildlifeGlyph(wildlife, pos, context);
            }
            
            // Draw health bar if injured
            if (wildlife.Health < 100)
            {
                DrawHealthBar(wildlife, pos, context.TileSize);
            }
            
            // Draw behavior indicator
            DrawBehaviorIndicator(wildlife, pos, context.TileSize);
        }
        
        public bool ShouldRender(WildlifeEntity wildlife, RenderContext context)
        {
            if (!wildlife.IsAlive)
                return false;
            
            return context.IsVisible(wildlife.Position.X, wildlife.Position.Y);
        }
        
        private void DrawWildlifeSprite(WildlifeEntity wildlife, DecorationType spriteType, Vector2 pos, RenderContext context)
        {
            var sprite = _spriteAtlas.GetSprite(spriteType);
            if (!sprite.HasValue)
                return;
            
            int spriteSize = (int)(context.TileSize * 0.8f);
            int spriteX = (int)pos.X + (context.TileSize - spriteSize) / 2;
            int spriteY = (int)pos.Y + (context.TileSize - spriteSize) / 2;
            
            var sourceRect = new Rectangle(0, 0, sprite.Value.Width, sprite.Value.Height);
            var destRect = new Rectangle(spriteX, spriteY, spriteSize, spriteSize);
            
            // Use White to preserve original sprite colors and transparency
            Raylib.DrawTexturePro(
                sprite.Value,
                sourceRect,
                destRect,
                new Vector2(0, 0),
                0f,
                Color.White
            );
        }
        
        private void DrawWildlifeGlyph(WildlifeEntity wildlife, Vector2 pos, RenderContext context)
        {
            string glyph = GetWildlifeGlyph(wildlife.Type);
            var glyphColor = new Color(255, 255, 255, 255);
            
            int textX = (int)pos.X + (context.TileSize - FontSize) / 2;
            int textY = (int)pos.Y + (context.TileSize - FontSize) / 2;
            
            GraphicsConfig.DrawConsoleTextAuto(glyph, textX, textY, FontSize, glyphColor);
        }
        
        private void DrawHealthBar(WildlifeEntity wildlife, Vector2 pos, int tileSize)
        {
            int barWidth = tileSize - 4;
            int barX = (int)pos.X + 2;
            int barY = (int)pos.Y + tileSize - 5;
            
            RenderHelpers.DrawHealthBar(barX, barY, barWidth, wildlife.Health);
        }
        
        private void DrawBehaviorIndicator(WildlifeEntity wildlife, Vector2 pos, int tileSize)
        {
            Color? indicatorColor = wildlife.CurrentBehavior switch
            {
                WildlifeBehavior.Hunting => ColorPalette.HuntingIndicator,
                WildlifeBehavior.Fleeing => ColorPalette.FleeingIndicator,
                WildlifeBehavior.Resting => ColorPalette.RestingIndicator,
                _ => null
            };
            
            if (indicatorColor.HasValue)
            {
                RenderHelpers.DrawIndicatorDot(
                    (int)pos.X + tileSize - 5,
                    (int)pos.Y + 5,
                    indicatorColor.Value
                );
            }
        }
        
        private Color GetWildlifeBackgroundColor(WildlifeEntity wildlife)
        {
            // Base color
            Color baseColor = wildlife.IsPredator
                ? wildlife.Type switch
                {
                    WildlifeType.Wolf => new Color(100, 100, 120, 220),
                    WildlifeType.Fox => new Color(180, 90, 40, 220),
                    WildlifeType.Bear => new Color(80, 60, 40, 220),
                    _ => new Color(120, 80, 60, 220)
                }
                : wildlife.Type switch
                {
                    WildlifeType.Rabbit => new Color(200, 180, 160, 220),
                    WildlifeType.Deer => new Color(160, 120, 80, 220),
                    WildlifeType.Boar => new Color(100, 80, 70, 220),
                    WildlifeType.Bird => new Color(140, 180, 200, 220),
                    WildlifeType.Duck => new Color(120, 160, 140, 220),
                    WildlifeType.Turkey => new Color(140, 100, 80, 220),
                    _ => new Color(150, 150, 150, 220)
                };
            
            // Dim if fleeing
            if (wildlife.CurrentBehavior == WildlifeBehavior.Fleeing)
            {
                baseColor = new Color(
                    (byte)(baseColor.R * 0.7f),
                    (byte)(baseColor.G * 0.7f),
                    (byte)(baseColor.B * 0.7f),
                    baseColor.A
                );
            }
            
            return baseColor;
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
        
        private DecorationType? GetWildlifeSpriteType(WildlifeType type)
        {
            return type switch
            {
                WildlifeType.Rabbit => DecorationType.RabbitSmall,
                WildlifeType.Deer => DecorationType.DeerGrazing,
                WildlifeType.Bird => DecorationType.BirdFlying,
                WildlifeType.Duck => DecorationType.BirdPerched,
                WildlifeType.Turkey => DecorationType.BirdPerched,
                WildlifeType.Fox => DecorationType.FoxHunting,
                WildlifeType.Wolf => DecorationType.WolfPack,
                WildlifeType.Bear => DecorationType.BearGrizzly,
                WildlifeType.Boar => DecorationType.BoarWild,
                _ => null
            };
        }
    }
}
