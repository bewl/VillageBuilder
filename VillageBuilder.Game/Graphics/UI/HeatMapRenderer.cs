using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Raylib_cs;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.World;

namespace VillageBuilder.Game.Graphics.UI
{
    /// <summary>
    /// Types of heat maps that can be displayed
    /// </summary>
    public enum HeatMapType
    {
        None,
        WaterCoverage,
        // Future additions:
        // BuildingDensity,
        // ResourceProduction,
        // PopulationDensity,
        // Happiness,
        // Temperature
    }

    /// <summary>
    /// Renders heat map overlays on the game map
    /// </summary>
    public class HeatMapRenderer
    {
        private HeatMapType _currentHeatMap = HeatMapType.None;

        // Caching for performance optimization
        private Dictionary<(int x, int y), float>? _cachedWaterAccessMap = null;
        private int _lastWellCount = 0;
        private int _cacheMinX = 0;
        private int _cacheMaxX = 0;
        private int _cacheMinY = 0;
        private int _cacheMaxY = 0;

        public HeatMapType CurrentHeatMap
        {
            get => _currentHeatMap;
            set => _currentHeatMap = value;
        }

        /// <summary>
        /// Cycle to the next heat map type
        /// </summary>
        public void CycleHeatMap()
        {
            var values = Enum.GetValues<HeatMapType>();
            int currentIndex = Array.IndexOf(values, _currentHeatMap);
            _currentHeatMap = values[(currentIndex + 1) % values.Length];
        }

        /// <summary>
        /// Render the current heat map overlay
        /// </summary>
        public void Render(GameEngine engine, Camera2D camera)
        {
            if (_currentHeatMap == HeatMapType.None) return;

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

            // Render based on heat map type
            switch (_currentHeatMap)
            {
                case HeatMapType.WaterCoverage:
                    RenderWaterCoverage(engine, minX, maxX, minY, maxY, tileSize);
                    break;
            }

            // Draw legend
            DrawLegend();
        }

        /// <summary>
        /// Render water coverage heat map showing well access ranges
        /// </summary>
        private void RenderWaterCoverage(GameEngine engine, int minX, int maxX, int minY, int maxY, float tileSize)
        {
            // Get all constructed wells
            var wells = engine.Buildings.Where(b => b.Type == BuildingType.Well && b.IsConstructed).ToList();

            if (wells.Count == 0)
            {
                // Draw "No wells" message
                DrawNoDataMessage("No wells built yet");
                _cachedWaterAccessMap = null; // Clear cache
                return;
            }

            // Get water access map (cached or recalculated)
            var waterAccessMap = GetWaterAccessMap(engine, wells, minX, maxX, minY, maxY);

            // Render the heat map
            const int maxDistance = 10; // Same as HasWaterAccess check

            foreach (var kvp in waterAccessMap)
            {
                var (x, y) = kvp.Key;
                float distance = kvp.Value;

                // Calculate color based on distance
                Color heatColor;
                int alpha = 100; // Semi-transparent

                if (distance <= maxDistance)
                {
                    // Within range - green gradient (darker = closer)
                    float intensity = 1.0f - (distance / maxDistance);
                    int green = (int)(100 + intensity * 155);
                    heatColor = new Color(0, green, 0, alpha);
                }
                else
                {
                    // Out of range - red gradient (darker = farther)
                    float intensity = Math.Min(1.0f, (distance - maxDistance) / 10f);
                    int red = (int)(100 + intensity * 155);
                    heatColor = new Color(red, 0, 0, alpha);
                }

                // Draw heat map tile
                float worldX = x * tileSize;
                float worldY = y * tileSize;
                Raylib.DrawRectangle((int)worldX, (int)worldY, (int)tileSize, (int)tileSize, heatColor);
            }

            // Highlight houses without water access
            var houses = engine.Buildings.Where(b => b.Type == BuildingType.House && b.IsConstructed).ToList();
            foreach (var house in houses)
            {
                if (!house.HasWaterAccess(engine.Buildings, maxDistance))
                {
                    float houseX = house.X * tileSize;
                    float houseY = house.Y * tileSize;

                    // Draw red outline around house without water
                    Raylib.DrawRectangleLines((int)houseX - 2, (int)houseY - 2, 
                        (int)tileSize + 4, (int)tileSize + 4, new Color(255, 50, 50, 255));
                    Raylib.DrawRectangleLines((int)houseX - 1, (int)houseY - 1, 
                        (int)tileSize + 2, (int)tileSize + 2, new Color(255, 50, 50, 255));
                }
            }

            // Draw well locations prominently with diamond range indicator
            foreach (var well in wells)
            {
                float wellX = well.X * tileSize;
                float wellY = well.Y * tileSize;

                // Draw well highlight
                Raylib.DrawRectangle((int)wellX, (int)wellY, (int)tileSize, (int)tileSize, 
                    new Color(0, 200, 255, 180));

                // Draw range diamond (Manhattan distance visualization)
                float rangeCenterX = wellX + tileSize / 2;
                float rangeCenterY = wellY + tileSize / 2;
                float rangeSize = maxDistance * tileSize;

                Vector2[] diamondPoints = new[]
                {
                    new Vector2(rangeCenterX, rangeCenterY - rangeSize),            // Top
                    new Vector2(rangeCenterX + rangeSize, rangeCenterY),            // Right
                    new Vector2(rangeCenterX, rangeCenterY + rangeSize),            // Bottom
                    new Vector2(rangeCenterX - rangeSize, rangeCenterY)             // Left
                };

                // Draw diamond outline
                for (int i = 0; i < 4; i++)
                {
                    Raylib.DrawLineV(diamondPoints[i], diamondPoints[(i + 1) % 4], 
                        new Color(0, 200, 255, 150));
                }
            }
        }

        /// <summary>
        /// Get water access map with caching for performance
        /// </summary>
        private Dictionary<(int x, int y), float> GetWaterAccessMap(
            GameEngine engine, List<Building> wells, int minX, int maxX, int minY, int maxY)
        {
            int currentWellCount = wells.Count;

            // Check if cache is valid
            bool cacheValid = _cachedWaterAccessMap != null 
                && _lastWellCount == currentWellCount
                && _cacheMinX <= minX 
                && _cacheMaxX >= maxX
                && _cacheMinY <= minY
                && _cacheMaxY >= maxY;

            if (cacheValid)
            {
                // Return subset of cached map
                return _cachedWaterAccessMap!
                    .Where(kvp => kvp.Key.x >= minX && kvp.Key.x < maxX 
                               && kvp.Key.y >= minY && kvp.Key.y < maxY)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            // Recalculate - expand visible area slightly for smoother panning
            int expandedMinX = Math.Max(0, minX - 20);
            int expandedMaxX = Math.Min(engine.Grid.Width, maxX + 20);
            int expandedMinY = Math.Max(0, minY - 20);
            int expandedMaxY = Math.Min(engine.Grid.Height, maxY + 20);

            var waterAccessMap = new Dictionary<(int x, int y), float>();

            for (int x = expandedMinX; x < expandedMaxX; x++)
            {
                for (int y = expandedMinY; y < expandedMaxY; y++)
                {
                    // Calculate minimum distance to any well
                    float minDistance = float.MaxValue;
                    foreach (var well in wells)
                    {
                        int distance = Math.Abs(x - well.X) + Math.Abs(y - well.Y);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                        }
                    }

                    waterAccessMap[(x, y)] = minDistance;
                }
            }

            // Update cache
            _cachedWaterAccessMap = waterAccessMap;
            _lastWellCount = currentWellCount;
            _cacheMinX = expandedMinX;
            _cacheMaxX = expandedMaxX;
            _cacheMinY = expandedMinY;
            _cacheMaxY = expandedMaxY;

            // Return visible subset
            return waterAccessMap
                .Where(kvp => kvp.Key.x >= minX && kvp.Key.x < maxX 
                           && kvp.Key.y >= minY && kvp.Key.y < maxY)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Draw legend explaining the heat map colors
        /// </summary>
        private void DrawLegend()
        {
            // Calculate legend position dynamically to avoid sidebar overlap
            // Sidebar starts at 70% of screen width, so position legend at 65% with some padding
            int sidebarStartX = (int)(GraphicsConfig.ScreenWidth * 0.7f);
            int legendWidth = 200;
            int legendX = sidebarStartX - legendWidth - 20; // 20px padding from sidebar
            int legendY = GraphicsConfig.StatusBarHeight + 20; // Below status bar with padding
            int legendHeight = 0;

            switch (_currentHeatMap)
            {
                case HeatMapType.WaterCoverage:
                    legendHeight = 160;
                    Raylib.DrawRectangle(legendX, legendY, legendWidth, legendHeight, 
                        new Color(0, 0, 0, 200));
                    Raylib.DrawRectangleLines(legendX, legendY, legendWidth, legendHeight, 
                        new Color(255, 255, 255, 255));

                    GraphicsConfig.DrawConsoleText("Water Coverage", legendX + 10, legendY + 10, 16, 
                        new Color(255, 255, 255, 255));

                    // Green box - good coverage
                    Raylib.DrawRectangle(legendX + 10, legendY + 40, 20, 20, 
                        new Color(0, 255, 0, 150));
                    GraphicsConfig.DrawConsoleText("Good Coverage", legendX + 40, legendY + 42, 14, 
                        new Color(200, 200, 200, 255));

                    // Red box - no coverage
                    Raylib.DrawRectangle(legendX + 10, legendY + 70, 20, 20, 
                        new Color(255, 0, 0, 150));
                    GraphicsConfig.DrawConsoleText("No Coverage", legendX + 40, legendY + 72, 14, 
                        new Color(200, 200, 200, 255));

                    // Blue box - well location
                    Raylib.DrawRectangle(legendX + 10, legendY + 100, 20, 20, 
                        new Color(0, 200, 255, 180));
                    GraphicsConfig.DrawConsoleText("Well Location", legendX + 40, legendY + 102, 14, 
                        new Color(200, 200, 200, 255));

                    // Red outline - house without water
                    Raylib.DrawRectangleLines(legendX + 10, legendY + 130, 20, 20, 
                        new Color(255, 50, 50, 255));
                    Raylib.DrawRectangleLines(legendX + 11, legendY + 131, 18, 18, 
                        new Color(255, 50, 50, 255));
                    GraphicsConfig.DrawConsoleText("No Water", legendX + 40, legendY + 132, 14, 
                        new Color(200, 200, 200, 255));
                    break;
            }
        }

        /// <summary>
        /// Draw a message when no data is available for the heat map
        /// </summary>
        private void DrawNoDataMessage(string message)
        {
            int centerX = GraphicsConfig.ScreenWidth / 2 - 150;
            int centerY = GraphicsConfig.ScreenHeight / 2 - 50;
            
            Raylib.DrawRectangle(centerX, centerY, 300, 100, new Color(0, 0, 0, 200));
            Raylib.DrawRectangleLines(centerX, centerY, 300, 100, new Color(255, 255, 100, 255));
            
            GraphicsConfig.DrawConsoleText(message, centerX + 20, centerY + 35, 20, 
                new Color(255, 255, 100, 255));
        }

        /// <summary>
        /// Get the name of the current heat map
        /// </summary>
        public string GetCurrentHeatMapName()
        {
            return _currentHeatMap switch
            {
                HeatMapType.None => "None",
                HeatMapType.WaterCoverage => "Water Coverage",
                _ => "Unknown"
            };
        }
    }
}
