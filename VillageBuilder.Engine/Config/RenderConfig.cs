namespace VillageBuilder.Engine.Config
{
    /// <summary>
    /// Configuration for rendering, visuals, and UI display.
    /// Color values use byte (0-255) RGB tuples.
    /// </summary>
    public class RenderConfig
    {
        // Display Settings
        public int ScreenWidth { get; set; } = 1920;
        public int ScreenHeight { get; set; } = 1080;
        public int TargetFPS { get; set; } = 60;
        public bool VSync { get; set; } = true;
        
        // Tile & Zoom
        public int TileSize { get; set; } = 32;
        public float DefaultZoom { get; set; } = 1.0f;
        public float MinZoom { get; set; } = 0.5f;
        public float MaxZoom { get; set; } = 3.0f;
        
        // Font Sizes
        public int SmallFontSize { get; set; } = 20;
        public int MediumFontSize { get; set; } = 24;
        public int LargeFontSize { get; set; } = 32;
        
        // Sprite Mode
        public bool UseSpriteMode { get; set; } = true;         // Use emoji sprites vs ASCII
        
        // Visual Clarity
        public bool ShowTileGlyphsInSpriteMode { get; set; } = false;   // Hide ASCII under sprites
        public bool ShowPathLines { get; set; } = true;
        public bool ShowHealthBars { get; set; } = true;
        public bool ShowBehaviorIndicators { get; set; } = true;
        
        // Selection
        public byte SelectionBorderThickness { get; set; } = 2;
        public bool UseMultiLayerSelection { get; set; } = true;
        
        // Performance
        public bool EnableViewCulling { get; set; } = true;     // Only render visible tiles
        public int CullingMargin { get; set; } = 2;             // Extra tiles around screen edge
        
        // Colors (stored as RGB bytes for easy conversion to Raylib.Color)
        public ColorConfig Colors { get; set; } = new();
    }
    
    /// <summary>
    /// Color palette for the game. Values are RGB (0-255).
    /// </summary>
    public class ColorConfig
    {
        // Tile Backgrounds (lightened for visual clarity)
        public (byte R, byte G, byte B) GrassBackground { get; set; } = (35, 50, 35);
        public (byte R, byte G, byte B) ForestBackground { get; set; } = (25, 40, 25);
        public (byte R, byte G, byte B) WaterBackground { get; set; } = (20, 35, 50);
        public (byte R, byte G, byte B) MountainBackground { get; set; } = (40, 40, 45);
        public (byte R, byte G, byte B) FieldBackground { get; set; } = (50, 45, 30);
        public (byte R, byte G, byte B) RoadBackground { get; set; } = (50, 50, 50);
        
        // Tile Foregrounds
        public (byte R, byte G, byte B) GrassForeground { get; set; } = (80, 180, 80);
        public (byte R, byte G, byte B) ForestForeground { get; set; } = (60, 150, 60);
        public (byte R, byte G, byte B) WaterForeground { get; set; } = (80, 140, 220);
        public (byte R, byte G, byte B) MountainForeground { get; set; } = (150, 150, 160);
        public (byte R, byte G, byte B) FieldForeground { get; set; } = (200, 180, 100);
        public (byte R, byte G, byte B) RoadForeground { get; set; } = (120, 120, 120);
        
        // Selection
        public (byte R, byte G, byte B) SelectionHighlight { get; set; } = (255, 255, 0);
        
        // Health Bars
        public (byte R, byte G, byte B) HealthGood { get; set; } = (0, 200, 0);
        public (byte R, byte G, byte B) HealthBad { get; set; } = (200, 0, 0);
        
        // Entity Backgrounds
                public (byte R, byte G, byte B) PersonMale { get; set; } = (80, 120, 200);
                public (byte R, byte G, byte B) PersonFemale { get; set; } = (200, 80, 120);
            }
        }
