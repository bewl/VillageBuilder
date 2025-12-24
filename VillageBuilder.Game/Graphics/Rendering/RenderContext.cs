using System.Numerics;
using Raylib_cs;
using VillageBuilder.Engine.Core;
using VillageBuilder.Game.Core;

namespace VillageBuilder.Game.Graphics.Rendering
{
    /// <summary>
    /// Encapsulates common rendering state and parameters passed to all renderers.
    /// Reduces parameter passing and provides consistent context across rendering systems.
    /// </summary>
    public class RenderContext
    {
        /// <summary>
        /// Camera controlling view transformation
        /// </summary>
        public Camera2D Camera { get; set; }
        
        /// <summary>
        /// Size of each tile in pixels
        /// </summary>
        public int TileSize { get; set; }
        
        /// <summary>
        /// Current game time (for animations, day/night)
        /// </summary>
        public GameTime GameTime { get; set; }
        
        /// <summary>
        /// Darkness factor for night rendering (0.0 = day, 0.35+ = night)
        /// </summary>
        public float DarknessFactor { get; set; }
        
        /// <summary>
        /// Visible tile bounds for culling (min/max X/Y)
        /// </summary>
        public Rectangle ViewBounds { get; set; }
        
        /// <summary>
        /// Selection manager for highlighting selected entities
        /// Phase 4: Updated to use SelectionCoordinator
        /// </summary>
        public VillageBuilder.Game.Core.Selection.SelectionCoordinator? SelectionManager { get; set; }
        
        /// <summary>
        /// Current zoom level (from camera)
        /// </summary>
        public float Zoom => Camera.Zoom;
        
        /// <summary>
        /// Screen width in pixels
        /// </summary>
        public int ScreenWidth { get; set; }
        
        /// <summary>
        /// Screen height in pixels
        /// </summary>
        public int ScreenHeight { get; set; }
        
        /// <summary>
        /// Whether sprite mode is enabled (vs ASCII)
        /// </summary>
        public bool UseSpriteMode { get; set; }
        
        /// <summary>
        /// Create a render context from game state
        /// Phase 4: Updated to use SelectionCoordinator
        /// </summary>
        public static RenderContext Create(
            GameEngine engine,
            Camera2D camera,
            int tileSize,
            VillageBuilder.Game.Core.Selection.SelectionCoordinator? selectionManager = null)
        {
            var screenWidth = GraphicsConfig.ScreenWidth;
            var screenHeight = GraphicsConfig.ScreenHeight;
            
            // Calculate visible bounds for culling
            float worldLeft = camera.Target.X - (screenWidth / 2f) / camera.Zoom;
            float worldTop = camera.Target.Y - (screenHeight / 2f) / camera.Zoom;
            float worldRight = camera.Target.X + (screenWidth / 2f) / camera.Zoom;
            float worldBottom = camera.Target.Y + (screenHeight / 2f) / camera.Zoom;
            
            int minX = Math.Max(0, (int)(worldLeft / tileSize) - 1);
            int maxX = Math.Min(engine.Grid.Width, (int)(worldRight / tileSize) + 2);
            int minY = Math.Max(0, (int)(worldTop / tileSize) - 1);
            int maxY = Math.Min(engine.Grid.Height, (int)(worldBottom / tileSize) + 2);
            
            return new RenderContext
            {
                Camera = camera,
                TileSize = tileSize,
                GameTime = engine.Time,
                DarknessFactor = engine.Time.GetDarknessFactor(),
                ViewBounds = new Rectangle(minX, minY, maxX - minX, maxY - minY),
                SelectionManager = selectionManager,
                ScreenWidth = screenWidth,
                ScreenHeight = screenHeight,
                UseSpriteMode = GraphicsConfig.UseSpriteMode
            };
        }
        
        /// <summary>
        /// Check if a tile position is within visible bounds
        /// </summary>
        public bool IsVisible(int x, int y)
        {
            return x >= ViewBounds.X && 
                   x < ViewBounds.X + ViewBounds.Width &&
                   y >= ViewBounds.Y && 
                   y < ViewBounds.Y + ViewBounds.Height;
        }
        
        /// <summary>
        /// Get world position for a tile coordinate
        /// </summary>
        public Vector2 GetWorldPosition(int tileX, int tileY)
        {
            return new Vector2(tileX * TileSize, tileY * TileSize);
        }
    }
}
