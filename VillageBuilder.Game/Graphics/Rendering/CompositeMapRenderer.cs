using System.Collections.Generic;
using System.Linq;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Core;
using VillageBuilder.Game.Core.Selection;
using VillageBuilder.Game.Graphics.Rendering.Renderers;
using Raylib_cs;

namespace VillageBuilder.Game.Graphics.Rendering
{
    /// <summary>
    /// Orchestrates multiple specialized renderers to render the complete game map.
    /// Follows the Composite pattern to manage rendering order and context.
    /// Phase 3: Main renderer integration point.
    /// </summary>
    public class CompositeMapRenderer
    {
        private readonly TerrainRenderer _terrainRenderer;
        private readonly BuildingRenderer _buildingRenderer;
        private readonly PersonRenderer _personRenderer;
        private readonly WildlifeRenderer _wildlifeRenderer;

        public CompositeMapRenderer()
        {
            _terrainRenderer = new TerrainRenderer();
            _buildingRenderer = new BuildingRenderer();
            _personRenderer = new PersonRenderer();
            _wildlifeRenderer = new WildlifeRenderer();
        }

        /// <summary>
        /// Render the visible portion of the game world using specialized renderers.
        /// </summary>
        public void RenderMap(
            GameEngine engine,
            Camera2D camera,
            SelectionCoordinator? selectionManager,
            int tileSize,
            int minX, int maxX,
            int minY, int maxY)
        {
            var grid = engine.Grid;
            var drawnBuildings = new HashSet<Building>();

            // Create render context once for all renderers
            var context = new RenderContext
            {
                TileSize = tileSize,
                DarknessFactor = engine.Time.GetDarknessFactor(),
                UseSpriteMode = GraphicsConfig.UseSpriteMode,
                GameTime = engine.Time,
                Camera = camera,
                SelectionManager = selectionManager,
                ViewBounds = new Rectangle(minX, minY, maxX - minX, maxY - minY),  // FIX: Set visible bounds for culling
                ScreenWidth = GraphicsConfig.ScreenWidth,
                ScreenHeight = GraphicsConfig.ScreenHeight
            };

            // Phase 1: Render terrain tiles and buildings
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    var tile = grid.GetTile(x, y);
                    if (tile == null) continue;

                    // Buildings take precedence over terrain
                    if (tile.Building != null)
                    {
                        if (!drawnBuildings.Contains(tile.Building))
                        {
                            _buildingRenderer.Render(tile.Building, context);
                            drawnBuildings.Add(tile.Building);
                        }
                    }
                    else
                    {
                        // Render terrain (background + decorations)
                        _terrainRenderer.Render(tile, context);
                    }
                }
            }

            // Phase 2: Render entities on top (people, wildlife)
            // PersonRenderer uses batch rendering for families
            var visibleFamilies = engine.Families.Where(f =>
            {
                var displayPerson = f.Members.FirstOrDefault(p => p.IsAlive);
                if (displayPerson == null) return false;
                return displayPerson.Position.X >= minX && displayPerson.Position.X < maxX &&
                       displayPerson.Position.Y >= minY && displayPerson.Position.Y < maxY;
            });
            _personRenderer.RenderBatch(visibleFamilies, context);

            // WildlifeRenderer uses individual rendering
            if (engine.WildlifeManager?.Wildlife != null)
            {
                foreach (var wildlife in engine.WildlifeManager.Wildlife)
                {
                    if (_wildlifeRenderer.ShouldRender(wildlife, context))
                    {
                        _wildlifeRenderer.Render(wildlife, context);
                    }
                }
            }

            // Phase 3: Render selection indicators
            RenderSelectionIndicators(selectionManager, tileSize);
        }

        /// <summary>
        /// Render selection highlights for selected entities.
        /// </summary>
        private void RenderSelectionIndicators(SelectionCoordinator? selectionManager, int tileSize)
        {
            if (selectionManager == null) return;

            // Building selection
            if (selectionManager.SelectedBuilding != null)
            {
                var tiles = selectionManager.SelectedBuilding.GetOccupiedTiles();
                foreach (var tile in tiles)
                {
                    var pos = new System.Numerics.Vector2(tile.X * tileSize, tile.Y * tileSize);
                    Raylib.DrawRectangleLines((int)pos.X, (int)pos.Y, tileSize, tileSize, new Color(0, 255, 0, 255));
                }
            }

            // Tile selection
            if (selectionManager.SelectedTile != null)
            {
                var tile = selectionManager.SelectedTile;
                var pos = new System.Numerics.Vector2(tile.X * tileSize, tile.Y * tileSize);

                // Draw subtle highlight
                Raylib.DrawRectangle((int)pos.X, (int)pos.Y, tileSize, tileSize, new Color(255, 255, 255, 30));

                // Draw border
                Raylib.DrawRectangleLines((int)pos.X, (int)pos.Y, tileSize, tileSize, new Color(100, 200, 255, 200));
            }
        }
    }
}
