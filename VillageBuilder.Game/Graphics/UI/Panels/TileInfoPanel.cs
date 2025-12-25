using Raylib_cs;
using VillageBuilder.Game.Core.Selection;

namespace VillageBuilder.Game.Graphics.UI.Panels
{
    /// <summary>
    /// Panel for displaying detailed information about a selected tile.
    /// Phase 5: Modular UI panel system - wraps TileInspector.
    /// </summary>
    public class TileInfoPanel : BasePanel
    {
        private readonly TileInspector _tileInspector;

        public TileInfoPanel()
        {
            _tileInspector = new TileInspector(
                GraphicsConfig.SmallConsoleFontSize,
                (int)(GraphicsConfig.SmallConsoleFontSize * 0.9f)
            );
        }

        public override bool CanRender(PanelRenderContext context)
        {
            var coordinator = context.SelectionManager as SelectionCoordinator;
            // Show tile info if a tile is selected (but no person/wildlife/building)
            return coordinator?.SelectedTile != null &&
                   coordinator.SelectedPerson == null &&
                   coordinator.SelectedWildlife == null &&
                   coordinator.SelectedBuilding == null;
        }

        public override int Render(PanelRenderContext context)
        {
            var coordinator = context.SelectionManager as SelectionCoordinator;
            var tile = coordinator?.SelectedTile;

            if (tile == null) return 0;

            // Use TileInspector to render tile details
            var height = _tileInspector.Render(
                tile,
                context.StartX,
                context.StartY,
                context.Width
            );

            return height;
        }
    }
}
