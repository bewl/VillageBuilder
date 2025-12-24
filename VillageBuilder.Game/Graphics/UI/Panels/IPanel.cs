using VillageBuilder.Engine.Core;

namespace VillageBuilder.Game.Graphics.UI.Panels
{
    /// <summary>
    /// Context passed to all panels for rendering
    /// </summary>
    public class PanelRenderContext
    {
        public GameEngine Engine { get; set; } = null!;
        public VillageBuilder.Game.Core.SelectionManager? SelectionManager { get; set; }
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int Width { get; set; }
        public int FontSize { get; set; }
        public int SmallFontSize { get; set; }
        public int LineHeight { get; set; }
        public int Padding { get; set; }
    }
    
    /// <summary>
    /// Common interface for all UI panels in the sidebar.
    /// Enables modular, self-contained panels with clear responsibilities.
    /// </summary>
    public interface IPanel
    {
        /// <summary>
        /// Check if this panel can render with the current selection/context
        /// </summary>
        bool CanRender(PanelRenderContext context);
        
        /// <summary>
        /// Render the panel and return the next Y position
        /// </summary>
        /// <returns>Y position after rendering (for next panel)</returns>
        int Render(PanelRenderContext context);
        
        /// <summary>
        /// Priority for panel rendering (lower = higher priority)
        /// Used when multiple panels can render
        /// </summary>
        int Priority { get; }
    }
}
