using Raylib_cs;

namespace VillageBuilder.Game.Graphics.Rendering
{
    /// <summary>
    /// Shared rendering utility functions to promote DRY principle.
    /// Provides common rendering operations used across multiple renderers.
    /// </summary>
    public static class RenderHelpers
    {
        /// <summary>
        /// Draw a horizontal stat bar (health, hunger, energy, etc.)
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="width">Bar width</param>
        /// <param name="value">Current value (0-100)</param>
        /// <param name="maxValue">Maximum value (typically 100)</param>
        /// <param name="foregroundColor">Bar fill color</param>
        /// <param name="backgroundColor">Bar background color</param>
        public static void DrawStatBar(
            int x, int y, int width, 
            float value, float maxValue,
            Color foregroundColor, 
            Color? backgroundColor = null)
        {
            const int barHeight = 4;
            
            // Draw background
            var bgColor = backgroundColor ?? new Color(50, 50, 50, 200);
            Raylib.DrawRectangle(x, y, width, barHeight, bgColor);
            
            // Draw foreground (current value)
            float fillPercent = Math.Clamp(value / maxValue, 0f, 1f);
            int fillWidth = (int)(width * fillPercent);
            if (fillWidth > 0)
            {
                Raylib.DrawRectangle(x, y, fillWidth, barHeight, foregroundColor);
            }
        }
        
        /// <summary>
        /// Draw a health bar with gradient colors (green to red based on health)
        /// </summary>
        public static void DrawHealthBar(int x, int y, int width, float health, float maxHealth = 100f)
        {
            float healthPercent = health / maxHealth;
            
            // Gradient from red (low) to green (high)
            Color barColor;
            if (healthPercent > 0.6f)
                barColor = new Color(0, 200, 0, 255); // Green
            else if (healthPercent > 0.3f)
                barColor = new Color(200, 200, 0, 255); // Yellow
            else
                barColor = new Color(200, 0, 0, 255); // Red
            
            DrawStatBar(x, y, width, health, maxHealth, barColor);
        }
        
        /// <summary>
        /// Draw multi-layer selection highlighting (bold, visible)
        /// </summary>
        /// <param name="bounds">Rectangle to highlight</param>
        /// <param name="color">Highlight color (typically yellow)</param>
        public static void DrawSelectionHighlight(Rectangle bounds, Color? color = null)
        {
            var highlightColor = color ?? new Color(255, 255, 0, 255);
            
            int x = (int)bounds.X;
            int y = (int)bounds.Y;
            int width = (int)bounds.Width;
            int height = (int)bounds.Height;
            
            // Outer glow (semi-transparent)
            Raylib.DrawRectangleLines(x - 2, y - 2, width + 4, height + 4, 
                new Color((byte)highlightColor.R, (byte)highlightColor.G, (byte)highlightColor.B, (byte)100));

            // Middle border
            Raylib.DrawRectangleLines(x - 1, y - 1, width + 2, height + 2, 
                new Color((byte)highlightColor.R, (byte)highlightColor.G, (byte)highlightColor.B, (byte)200));
            
            // Inner border (bright)
            Raylib.DrawRectangleLines(x, y, width, height, highlightColor);
        }
        
        /// <summary>
        /// Draw a small indicator dot (for behavior states, etc.)
        /// </summary>
        public static void DrawIndicatorDot(int x, int y, Color color, int radius = 3)
        {
            Raylib.DrawCircle(x, y, radius, color);
            Raylib.DrawCircleLines(x, y, radius, new Color(0, 0, 0, 150));
        }
        
        /// <summary>
        /// Apply darkness overlay to a color (for day/night cycle)
        /// </summary>
        public static Color DarkenColor(Color color, float darknessFactor)
        {
            darknessFactor = Math.Clamp(darknessFactor, 0f, 1f);
            
            return new Color(
                (byte)(color.R * (1f - darknessFactor)),
                (byte)(color.G * (1f - darknessFactor)),
                (byte)(color.B * (1f - darknessFactor)),
                color.A
            );
        }
        
        /// <summary>
        /// Draw text with shadow for better visibility
        /// </summary>
        public static void DrawTextWithShadow(string text, int x, int y, int fontSize, Color color)
        {
            // Shadow
            Raylib.DrawText(text, x + 1, y + 1, fontSize, new Color(0, 0, 0, 150));
            // Text
            Raylib.DrawText(text, x, y, fontSize, color);
        }
    }
}
