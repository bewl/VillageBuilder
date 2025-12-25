using VillageBuilder.Engine.Core;
using VillageBuilder.Game.Core.Selection;

namespace VillageBuilder.Game.Graphics.UI.Panels
{
    /// <summary>
    /// Manages and orchestrates UI panels based on selection state.
    /// Phase 5: Panel coordination and rendering.
    /// </summary>
    public class PanelManager
    {
        private readonly PersonInfoPanel _personPanel;
        private readonly WildlifeInfoPanel _wildlifePanel;
        private readonly BuildingInfoPanel _buildingPanel;
        private readonly GameStatsPanel _gameStatsPanel;
        private readonly QuickStatsPanel _quickStatsPanel;
        private readonly TileInfoPanel _tileInfoPanel;

        public PanelManager()
        {
            _personPanel = new PersonInfoPanel();
            _wildlifePanel = new WildlifeInfoPanel();
            _buildingPanel = new BuildingInfoPanel();
            _gameStatsPanel = new GameStatsPanel();
            _quickStatsPanel = new QuickStatsPanel();
            _tileInfoPanel = new TileInfoPanel();
        }

        /// <summary>
        /// Render appropriate panel based on current selection.
        /// </summary>
        public void RenderPanels(GameEngine engine, SelectionCoordinator? selectionManager, 
                                int x, int y, int width, int height)
        {
            // Create render context with proper properties
            var context = new PanelRenderContext
            {
                Engine = engine,
                SelectionManager = selectionManager,
                StartX = x,
                StartY = y,
                Width = width  // Fix: Set width for TileInspector and other panels
            };

                    // Show context-specific panel based on selection
                    // Only ONE panel renders to avoid overlap
                    if (selectionManager != null && selectionManager.HasSelection())
                    {
                        if (selectionManager.SelectedPerson != null)
                        {
                            _personPanel.Render(context);
                        }
                        else if (selectionManager.SelectedWildlife != null)
                        {
                            _wildlifePanel.Render(context);
                        }
                        else if (selectionManager.SelectedBuilding != null)
                        {
                            _buildingPanel.Render(context);
                        }
                        else if (selectionManager.SelectedTile != null)
                        {
                            // Show tile inspector for tile-only selection
                            _tileInfoPanel.Render(context);
                        }
                        else
                        {
                            // Show quick stats if selection exists but no specific entity
                            _quickStatsPanel.Render(context);
                        }
                    }
                    else
                    {
                        // No selection - show quick stats
                        _quickStatsPanel.Render(context);
                    }
                }
    }
}
