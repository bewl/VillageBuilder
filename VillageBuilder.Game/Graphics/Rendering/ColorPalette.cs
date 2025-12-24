using Raylib_cs;
using VillageBuilder.Engine.Config;

namespace VillageBuilder.Game.Graphics.Rendering
{
    /// <summary>
    /// Centralized color palette for consistent visual style.
    /// All game colors defined in one place for easy modification.
    /// Can be extended to load from RenderConfig in the future.
    /// </summary>
    public static class ColorPalette
    {
        // === Tile Backgrounds (lightened for visual clarity) ===
        public static readonly Color GrassBackground = new(35, 50, 35, 255);
        public static readonly Color ForestBackground = new(25, 40, 25, 255);
        public static readonly Color WaterBackground = new(20, 35, 50, 255);
        public static readonly Color MountainBackground = new(40, 40, 45, 255);
        public static readonly Color FieldBackground = new(50, 45, 30, 255);
        public static readonly Color RoadBackground = new(50, 50, 50, 255);
        public static readonly Color DefaultBackground = new(30, 30, 30, 255);
        
        // === Tile Foregrounds ===
        public static readonly Color GrassForeground = new(80, 180, 80, 255);
        public static readonly Color ForestForeground = new(60, 150, 60, 255);
        public static readonly Color WaterForeground = new(80, 140, 220, 255);
        public static readonly Color MountainForeground = new(150, 150, 160, 255);
        public static readonly Color FieldForeground = new(200, 180, 100, 255);
        public static readonly Color RoadForeground = new(120, 120, 120, 255);
        
        // === Selection ===
        public static readonly Color SelectionYellow = new(255, 255, 0, 255);
        public static readonly Color SelectionBlue = new(100, 150, 255, 255);
        
        // === Health/Status ===
        public static readonly Color HealthGood = new(0, 200, 0, 255);
        public static readonly Color HealthMedium = new(200, 200, 0, 255);
        public static readonly Color HealthBad = new(200, 0, 0, 255);
        
        // === Entity Backgrounds ===
        public static readonly Color PersonMaleBackground = new(80, 120, 200, 255);
        public static readonly Color PersonFemaleBackground = new(200, 80, 120, 255);
        public static readonly Color WildlifeBackground = new(100, 80, 60, 200);
        
        // === Behavior Indicators ===
        public static readonly Color HuntingIndicator = new(255, 100, 100, 255);  // Red
        public static readonly Color FleeingIndicator = new(255, 255, 100, 255);  // Yellow
        public static readonly Color RestingIndicator = new(100, 100, 255, 255);  // Blue
        
        // === UI Elements ===
        public static readonly Color SidebarBackground = new(20, 20, 20, 230);
        public static readonly Color PanelHeader = new(40, 40, 40, 255);
        public static readonly Color TextDefault = new(220, 220, 220, 255);
        public static readonly Color TextHighlight = new(255, 255, 100, 255);
        public static readonly Color TextDim = new(150, 150, 150, 255);
        
        /// <summary>
        /// Get tile background color from RenderConfig or fallback to defaults
        /// </summary>
        public static Color GetTileBackgroundFromConfig(Engine.World.TileType tileType, RenderConfig? config = null)
        {
            if (config == null)
            {
                // Use static defaults
                return tileType switch
                {
                    Engine.World.TileType.Grass => GrassBackground,
                    Engine.World.TileType.Forest => ForestBackground,
                    Engine.World.TileType.Water => WaterBackground,
                    Engine.World.TileType.Mountain => MountainBackground,
                    Engine.World.TileType.Field => FieldBackground,
                    Engine.World.TileType.Road => RoadBackground,
                    _ => DefaultBackground
                };
            }
            
            // Use config values
            var rgb = tileType switch
            {
                Engine.World.TileType.Grass => config.Colors.GrassBackground,
                Engine.World.TileType.Forest => config.Colors.ForestBackground,
                Engine.World.TileType.Water => config.Colors.WaterBackground,
                Engine.World.TileType.Mountain => config.Colors.MountainBackground,
                Engine.World.TileType.Field => config.Colors.FieldBackground,
                Engine.World.TileType.Road => config.Colors.RoadBackground,
                _ => (30, 30, 30)
            };
            
            return new Color((byte)rgb.Item1, (byte)rgb.Item2, (byte)rgb.Item3, (byte)255);
        }
    }
}
