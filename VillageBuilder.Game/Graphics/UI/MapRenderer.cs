using System;
using Raylib_cs;
using VillageBuilder.Engine.Core;
using VillageBuilder.Game.Graphics.Rendering;
using VillageBuilder.Game.Core.Selection;

namespace VillageBuilder.Game.Graphics.UI
{
    /// <summary>
    /// Thin wrapper around CompositeMapRenderer.
    /// Phase 3: All rendering logic delegated to specialized renderers.
    /// </summary>
    public class MapRenderer
    {
        private readonly CompositeMapRenderer _compositeRenderer = new();

        public void Render(GameEngine engine, Camera2D camera, SelectionCoordinator? selectionManager = null)
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

            // Delegate all rendering to CompositeMapRenderer
            _compositeRenderer.RenderMap(engine, camera, selectionManager, tileSize, minX, maxX, minY, maxY);
        }
    }
}
