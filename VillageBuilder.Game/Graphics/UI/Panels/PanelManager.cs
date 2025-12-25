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

        public PanelManager()
        {
            _personPanel = new PersonInfoPanel();
            _wildlifePanel = new WildlifeInfoPanel();
            _buildingPanel = new BuildingInfoPanel();
            _gameStatsPanel = new GameStatsPanel();
            _quickStatsPanel = new QuickStatsPanel();
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
                StartY = y
            };

            int currentY = y;
            int panelSpacing = 10;

            // Always show quick stats at top
            var quickStatsHeight = _quickStatsPanel.Render(context);
            currentY += quickStatsHeight + panelSpacing;

            // Update context for next panel
            context.StartY = currentY;

            // Show context-specific panel based on selection
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
                else
                {
                    // Show general stats if only tile selected
                    _gameStatsPanel.Render(context);
                }
            }
            else
            {
                // No selection - show general game stats
                _gameStatsPanel.Render(context);
            }
        }
    }
}
