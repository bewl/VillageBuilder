using Raylib_cs;

namespace VillageBuilder.Game.Graphics.UI.Panels
{
    /// <summary>
    /// Base class for panels with common rendering utilities.
    /// Reduces code duplication across panel implementations.
    /// </summary>
    public abstract class BasePanel : IPanel
    {
        public abstract bool CanRender(PanelRenderContext context);
        public abstract int Render(PanelRenderContext context);
        public virtual int Priority => 0;
        
        /// <summary>
        /// Draw a section header with icon
        /// </summary>
        protected void DrawSectionHeader(string title, int x, int y, int fontSize)
        {
            var headerColor = new Color(120, 200, 255, 255);
            
            int currentX = x;
            currentX += GraphicsConfig.DrawUIIcon(
                UIIconType.ArrowRight,
                currentX, y,
                fontSize,
                headerColor,
                ">"
            );
            
            GraphicsConfig.DrawConsoleText(title, currentX + 5, y, fontSize, headerColor);
        }
        
        /// <summary>
        /// Draw a labeled value (e.g., "Health: 100")
        /// </summary>
        protected void DrawLabelValue(string label, string value, int x, int y, int fontSize, Color? valueColor = null)
        {
            var labelColor = new Color(180, 180, 180, 255);
            var defaultValueColor = new Color(255, 255, 200, 255);
            
            GraphicsConfig.DrawConsoleText(label, x, y, fontSize, labelColor);
            GraphicsConfig.DrawConsoleText(value, x + label.Length * (fontSize / 2) + 10, y, fontSize, valueColor ?? defaultValueColor);
        }
        
        /// <summary>
        /// Draw a stat bar (health, hunger, energy, etc.)
        /// </summary>
        protected void DrawStatBar(string label, float value, float maxValue, int x, int y, int width, int fontSize)
        {
            var labelColor = new Color(180, 180, 180, 255);
            var barBg = new Color(40, 40, 40, 255);
            
            // Draw label
            GraphicsConfig.DrawConsoleText(label, x, y, fontSize, labelColor);
            
            // Draw bar
            int barY = y + fontSize + 2;
            int barHeight = 6;
            
            // Background
            Raylib.DrawRectangle(x, barY, width, barHeight, barBg);
            
            // Foreground
            float fillPercent = Math.Clamp(value / maxValue, 0f, 1f);
            int fillWidth = (int)(width * fillPercent);
            
            Color barColor = GetStatBarColor(fillPercent);
            if (fillWidth > 0)
            {
                Raylib.DrawRectangle(x, barY, fillWidth, barHeight, barColor);
            }
            
            // Value text
            string valueText = $"{(int)value}/{(int)maxValue}";
            int textX = x + width + 5;
            GraphicsConfig.DrawConsoleText(valueText, textX, y, fontSize, new Color(200, 200, 200, 255));
        }
        
        /// <summary>
        /// Get color for stat bar based on percentage
        /// </summary>
        private Color GetStatBarColor(float percent)
        {
            if (percent > 0.6f)
                return new Color(0, 200, 0, 255); // Green
            else if (percent > 0.3f)
                return new Color(200, 200, 0, 255); // Yellow
            else
                return new Color(200, 0, 0, 255); // Red
        }
        
        /// <summary>
        /// Draw a divider line
        /// </summary>
        protected void DrawDivider(int x, int y, int width)
        {
            var dividerColor = new Color(60, 60, 70, 255);
            Raylib.DrawLine(x, y, x + width, y, dividerColor);
        }
        
        /// <summary>
        /// Draw cycling indicator (when multiple entities can be cycled)
        /// </summary>
        protected void DrawCyclingIndicator(int currentIndex, int totalCount, int x, int y, int fontSize)
        {
            if (totalCount <= 1) return;
            
            var cycleColor = new Color(255, 200, 100, 255);
            string cycleText = $"({currentIndex + 1}/{totalCount}) [TAB to cycle]";
            GraphicsConfig.DrawConsoleText(cycleText, x, y, fontSize, cycleColor);
        }
    }
}
