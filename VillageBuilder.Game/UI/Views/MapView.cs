using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.World;
using System.Text;
using Attribute = Terminal.Gui.Attribute;

namespace VillageBuilder.Game.UI.Views
{
    public class MapView : FrameView
    {
        private readonly TextView _mapView;
        private int _centerX = 50;
        private int _centerY = 50;

        public MapView() : base("Map (Use Arrow Keys to Pan)")
        {
            _mapView = new TextView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ReadOnly = true,
                WordWrap = false
            };

            Add(_mapView);
        }

        public void UpdateMap(GameEngine engine)
        {
            var sb = new StringBuilder();
            var grid = engine.Grid;

            // Calculate view dimensions based on available space
            var viewWidth = (int)(Bounds.Width * 0.9);
            var viewHeight = (int)(Bounds.Height * 0.9);

            sb.AppendLine($"╔{new string('═', viewWidth - 2)}╗");

            for (int y = _centerY - viewHeight / 2; y <= _centerY + viewHeight / 2; y++)
            {
                sb.Append("║");
                for (int x = _centerX - viewWidth / 2; x <= _centerX + viewWidth / 2; x++)
                {
                    var tile = grid.GetTile(x, y);
                    if (tile == null)
                    {
                        sb.Append(' ');
                        continue;
                    }

                    string symbol = GetColoredTileSymbol(tile);
                    sb.Append(symbol);
                }
                sb.AppendLine("║");
            }

            sb.AppendLine($"╚{new string('═', viewWidth - 2)}╝");
            sb.AppendLine($"📍 Center: ({_centerX}, {_centerY}) | Map: {grid.Width}x{grid.Height}");

            _mapView.Text = sb.ToString();
        }

        private string GetColoredTileSymbol(Tile tile)
        {
            if (tile.Building != null)
            {
                return tile.Building.Type switch
                {
                    VillageBuilder.Engine.Buildings.BuildingType.House => "🏠",
                    VillageBuilder.Engine.Buildings.BuildingType.Farm => "🌾",
                    VillageBuilder.Engine.Buildings.BuildingType.Warehouse => "📦",
                    VillageBuilder.Engine.Buildings.BuildingType.Mine => "⛏️",
                    VillageBuilder.Engine.Buildings.BuildingType.Lumberyard => "🪓",
                    VillageBuilder.Engine.Buildings.BuildingType.Workshop => "🔨",
                    VillageBuilder.Engine.Buildings.BuildingType.Market => "🏪",
                    VillageBuilder.Engine.Buildings.BuildingType.Well => "🚰",
                    VillageBuilder.Engine.Buildings.BuildingType.TownHall => "🏛️",
                    _ => "🏗️"
                };
            }

            return tile.Type switch
            {
                TileType.Grass => "·",           // Light grass
                TileType.Forest => "🌲",         // Trees
                TileType.Water => "≈",           // Water waves
                TileType.Mountain => "⛰️",        // Mountains
                TileType.Field => "🌿",          // Cultivated field
                TileType.Road => "═",            // Road
                TileType.BuildingFoundation => "▢", // Foundation
                _ => "?"
            };
        }

        public void PanMap(int dx, int dy, GameEngine engine)
        {
            var maxX = engine.Grid.Width;
            var maxY = engine.Grid.Height;
            
            _centerX = Math.Clamp(_centerX + dx, 30, maxX - 30);
            _centerY = Math.Clamp(_centerY + dy, 15, maxY - 15);
        }
    }
}
