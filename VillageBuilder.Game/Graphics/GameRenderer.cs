using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raylib_cs;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.World;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Commands.BuildingCommands;
using VillageBuilder.Game.Graphics.UI;
using System.Numerics;

namespace VillageBuilder.Game.Graphics
{
    public class GameRenderer
    {
        private readonly GameEngine _engine;
        private Camera2D _camera;
        private readonly StatusBarRenderer _statusBar;
        private readonly MapRenderer _mapRenderer;
        private readonly SidebarRenderer _sidebar;
        private readonly ParticleSystem _particleSystem;

        private float _cameraX = 50;
        private float _cameraY = 50;
        private float _zoom = 1.0f;

        // Building placement state
        private BuildingType? _selectedBuildingType = null;
        private BuildingRotation _currentRotation = BuildingRotation.North;
        private Vector2 _mouseWorldPos;
        private int _hoveredTileX = -1;
        private int _hoveredTileY = -1; 
        private int _hoveredTileZ = -1;
        private bool _roadSnapEnabled = true;

        public GameRenderer(GameEngine engine)
        {
            _engine = engine;
            
            _camera = new Camera2D
            {
                Target = new Vector2(_cameraX * GraphicsConfig.TileSize, _cameraY * GraphicsConfig.TileSize),
                Offset = new Vector2(GraphicsConfig.ScreenWidth / 2f, GraphicsConfig.ScreenHeight / 2f),
                Rotation = 0.0f,
                Zoom = _zoom
            };

            _statusBar = new StatusBarRenderer();
            _mapRenderer = new MapRenderer();
            _sidebar = new SidebarRenderer();
            _particleSystem = new ParticleSystem();
        }

        public void Initialize()
        {
            Raylib.InitWindow(GraphicsConfig.ScreenWidth, GraphicsConfig.ScreenHeight, GraphicsConfig.WindowTitle);
            Raylib.SetTargetFPS(GraphicsConfig.TargetFPS);
        }

        public bool ShouldClose()
        {
            return Raylib.WindowShouldClose();
        }

        public void HandleInput(out bool pause, out float timeScaleChange)
        {
            pause = false;
            timeScaleChange = 0;

            // Camera movement
            float baseSpeed = 300.0f;
            float deltaTime = Raylib.GetFrameTime();
            float moveAmount = baseSpeed * deltaTime / _zoom;

            if (Raylib.IsKeyDown(KeyboardKey.Up)) _cameraY -= moveAmount / GraphicsConfig.TileSize;
            if (Raylib.IsKeyDown(KeyboardKey.Down)) _cameraY += moveAmount / GraphicsConfig.TileSize;
            if (Raylib.IsKeyDown(KeyboardKey.Left)) _cameraX -= moveAmount / GraphicsConfig.TileSize;
            if (Raylib.IsKeyDown(KeyboardKey.Right)) _cameraX += moveAmount / GraphicsConfig.TileSize;

            _cameraX = Math.Clamp(_cameraX, 0, _engine.Grid.Width);
            _cameraY = Math.Clamp(_cameraY, 0, _engine.Grid.Height);

            // Zoom
            float wheel = Raylib.GetMouseWheelMove();
            if (wheel != 0)
            {
                _zoom += wheel * 0.1f;
                _zoom = Math.Clamp(_zoom, 0.5f, 3.0f);
            }

            // Building placement controls
            if (Raylib.IsKeyPressed(KeyboardKey.H)) _selectedBuildingType = BuildingType.House;
            if (Raylib.IsKeyPressed(KeyboardKey.F)) _selectedBuildingType = BuildingType.Farm;
            if (Raylib.IsKeyPressed(KeyboardKey.W)) _selectedBuildingType = BuildingType.Warehouse;
            if (Raylib.IsKeyPressed(KeyboardKey.L)) _selectedBuildingType = BuildingType.Lumberyard;
            if (Raylib.IsKeyPressed(KeyboardKey.M)) _selectedBuildingType = BuildingType.Mine;
            if (Raylib.IsKeyPressed(KeyboardKey.K)) _selectedBuildingType = BuildingType.Workshop;
            if (Raylib.IsKeyPressed(KeyboardKey.T)) _selectedBuildingType = BuildingType.TownHall;
            
            // Rotation (R key or middle mouse button)
            if (_selectedBuildingType.HasValue && (Raylib.IsKeyPressed(KeyboardKey.R) || Raylib.IsMouseButtonPressed(MouseButton.Middle)))
            {
                _currentRotation = _currentRotation switch
                {
                    BuildingRotation.North => BuildingRotation.East,
                    BuildingRotation.East => BuildingRotation.South,
                    BuildingRotation.South => BuildingRotation.West,
                    BuildingRotation.West => BuildingRotation.North,
                    _ => BuildingRotation.North
                };
            }

            // Toggle road snap (Tab key)
            if (Raylib.IsKeyPressed(KeyboardKey.Tab))
            {
                _roadSnapEnabled = !_roadSnapEnabled;
            }
            
            // Cancel building placement
            if (Raylib.IsKeyPressed(KeyboardKey.Escape) || Raylib.IsMouseButtonPressed(MouseButton.Right))
            {
                _selectedBuildingType = null;
                _currentRotation = BuildingRotation.North;
            }

            // Get mouse world position
            _mouseWorldPos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), _camera);
            _hoveredTileX = (int)(_mouseWorldPos.X / GraphicsConfig.TileSize);
            _hoveredTileY = (int)(_mouseWorldPos.Y / GraphicsConfig.TileSize);

            // Apply road snapping if enabled
            if (_selectedBuildingType.HasValue && _roadSnapEnabled)
            {
                var snapped = TrySnapToRoad(_selectedBuildingType.Value, _hoveredTileX, _hoveredTileY, ref _currentRotation);
                if (snapped.HasValue)
                {
                    _hoveredTileX = snapped.Value.X;
                    _hoveredTileY = snapped.Value.Y;
                }
            }

            // Place building on left click
            if (_selectedBuildingType.HasValue && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                TryPlaceBuilding(_selectedBuildingType.Value, _hoveredTileX, _hoveredTileY, _currentRotation);
                // Keep building mode active but reset rotation
                _currentRotation = BuildingRotation.North;
            }

            // Game controls
            if (Raylib.IsKeyPressed(KeyboardKey.Space)) pause = true;
            if (Raylib.IsKeyPressed(KeyboardKey.Equal)) timeScaleChange = 2.0f;
            if (Raylib.IsKeyPressed(KeyboardKey.Minus)) timeScaleChange = 0.5f;

            // Update camera
            _camera.Target = new Vector2(_cameraX * GraphicsConfig.TileSize, _cameraY * GraphicsConfig.TileSize);
            _camera.Zoom = _zoom;
        }

        private Vector2Int? TrySnapToRoad(BuildingType buildingType, int baseX, int baseY, ref BuildingRotation rotation)
        {
            var definition = BuildingDefinition.Definitions.Get(buildingType);
            var roadTiles = new List<Vector2Int>();

            // Find nearby road tiles
            int searchRadius = Math.Max(definition.Width, definition.Height) + 2;
            for (int x = baseX - searchRadius; x <= baseX + searchRadius; x++)
            {
                for (int y = baseY - searchRadius; y <= baseY + searchRadius; y++)
                {
                    var tile = _engine.Grid.GetTile(x, y);
                    if (tile?.Type == TileType.Road)
                    {
                        roadTiles.Add(new Vector2Int(x, y));
                    }
                }
            }

            if (roadTiles.Count == 0) return null;

            // Try all rotations and find best snap
            Vector2Int? bestPosition = null;
            BuildingRotation bestRotation = rotation;
            float bestDistance = float.MaxValue;

            var rotations = new[] { BuildingRotation.North, BuildingRotation.East, BuildingRotation.South, BuildingRotation.West };
            
            foreach (var rot in rotations)
            {
                var doorPositions = definition.GetDoorPositions(baseX, baseY, rot);

                foreach (var doorPos in doorPositions)
                {
                    foreach (var roadTile in roadTiles)
                    {
                        int dx = Math.Abs(doorPos.X - roadTile.X);
                        int dy = Math.Abs(doorPos.Y - roadTile.Y);

                        if ((dx == 0 && dy == 1) || (dx == 1 && dy == 0))
                        {
                            float dist = Vector2.Distance(new Vector2(baseX, baseY), new Vector2(roadTile.X, roadTile.Y));
                            if (dist < bestDistance)
                            {
                                bestDistance = dist;
                                bestPosition = new Vector2Int(baseX, baseY);
                                bestRotation = rot;
                            }
                        }
                    }
                }
            }

            if (bestPosition.HasValue)
            {
                rotation = bestRotation;
            }

            return bestPosition;
        }

        private void TryPlaceBuilding(BuildingType buildingType, int x, int y, BuildingRotation rotation)
        {
            var definition = BuildingDefinition.Definitions.Get(buildingType);
            var tempBuilding = new Building(buildingType, x, y, rotation);
            var occupiedTiles = tempBuilding.GetOccupiedTiles();

            // Check if all tiles are valid
            foreach (var tilePos in occupiedTiles)
            {
                var tile = _engine.Grid.GetTile(tilePos.X, tilePos.Y);
                if (tile == null || !tile.IsWalkable)
                {
                    AddParticleEffect(new Vector2(x * GraphicsConfig.TileSize + GraphicsConfig.TileSize / 2, 
                                                 y * GraphicsConfig.TileSize + GraphicsConfig.TileSize / 2), 
                                     ParticleType.Error);
                    return;
                }
            }

            // Check resources
            var costs = tempBuilding.GetConstructionCost();
            foreach (var cost in costs)
            {
                if (!_engine.VillageResources.Has(cost.Key, cost.Value))
                {
                    AddParticleEffect(new Vector2(x * GraphicsConfig.TileSize + GraphicsConfig.TileSize / 2, 
                                                 y * GraphicsConfig.TileSize + GraphicsConfig.TileSize / 2), 
                                     ParticleType.Error);
                    return;
                }
            }

            // Submit command with rotation
            var command = new ConstructBuildingCommand(
                playerId: 0,
                targetTick: _engine.CurrentTick + 1,
                buildingType: buildingType,
                x: x,
                y: y,
                rotation: rotation
            );

            _engine.SubmitCommand(command);
            
            AddParticleEffect(new Vector2(x * GraphicsConfig.TileSize + GraphicsConfig.TileSize / 2, 
                                         y * GraphicsConfig.TileSize + GraphicsConfig.TileSize / 2), 
                             ParticleType.Build);
        }

        public void Render(float timeScale, bool isPaused)
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(GraphicsConfig.Colors.Background);

            // Render everything in world space first
            Raylib.BeginMode2D(_camera);
            
            // Render the map (tiles, buildings, lighting - all handled by MapRenderer)
            _mapRenderer.Render(_engine, _camera);
            
            // Draw particles (also in world space)
            _particleSystem.Render();
            
            // IMPORTANT: Draw building preview LAST in world space, AFTER lighting
            // This ensures it's always visible and not affected by blend modes
            if (_selectedBuildingType.HasValue)
            {
                // Reset to normal alpha blending before drawing preview
                Raylib.BeginBlendMode(BlendMode.Alpha);
                DrawMultiTileBuildingPreview(_selectedBuildingType.Value, _hoveredTileX, _hoveredTileY, _currentRotation);
                Raylib.EndBlendMode();
            }
            
            Raylib.EndMode2D();

            // Render UI (screen space - no camera)
            _statusBar.Render(_engine, timeScale, isPaused);
            _sidebar.Render(_engine);

            // Draw building placement UI
            if (_selectedBuildingType.HasValue)
            {
                DrawBuildingPlacementUI(_selectedBuildingType.Value);
            }

            // Debug info - positioned below status bar in top-right corner
            var debugY = GraphicsConfig.StatusBarHeight + 10;
            Raylib.DrawFPS(GraphicsConfig.ScreenWidth - 100, debugY);
            GraphicsConfig.DrawConsoleText(
                $"Camera: ({_cameraX:F1}, {_cameraY:F1}) Zoom: {_zoom:F2}x", 
                GraphicsConfig.ScreenWidth - 350, 
                debugY + 20, 
                14, 
                new Color((byte)200, (byte)200, (byte)200, (byte)255)
            );

            Raylib.EndDrawing();
        }

        private void DrawMultiTileBuildingPreview(BuildingType buildingType, int tileX, int tileY, BuildingRotation rotation)
        {
            var definition = BuildingDefinition.Definitions.Get(buildingType);
            var tempBuilding = new Building(buildingType, tileX, tileY, rotation);
            var occupiedTiles = tempBuilding.GetOccupiedTiles();
            var doorPositions = tempBuilding.GetDoorPositions();
            var doorPos = doorPositions.Count > 0 ? doorPositions[0] : new Vector2Int(tileX, tileY);

            // Debug: Check why tiles are invalid
            bool canPlace = true;
            string debugReason = "";
            int validTiles = 0;
            int invalidTiles = 0;
            
            foreach (var tilePos in occupiedTiles)
            {
                var tile = _engine.Grid.GetTile(tilePos.X, tilePos.Y);
                if (tile == null)
                {
                    canPlace = false;
                    debugReason = $"Tile null at ({tilePos.X}, {tilePos.Y}) - Out of bounds";
                    invalidTiles++;
                }
                else if (!tile.IsWalkable)
                {
                    canPlace = false;
                    debugReason = $"Not walkable at ({tilePos.X}, {tilePos.Y}), Type: {tile.Type}, HasBuilding: {tile.Building != null}";
                    invalidTiles++;
                }
                else
                {
                    validTiles++;
                }
            }

            // Check resources
            if (canPlace)
            {
                var costs = tempBuilding.GetConstructionCost();
                foreach (var cost in costs)
                {
                    if (!_engine.VillageResources.Has(cost.Key, cost.Value))
                    {
                        canPlace = false;
                        debugReason = $"Insufficient {cost.Key}";
                        break;
                    }
                }
            }

            var color = canPlace 
                ? new Color((byte)100, (byte)255, (byte)100, (byte)128)
                : new Color((byte)255, (byte)100, (byte)100, (byte)128);

            // Draw all occupied tiles with a solid background first
            foreach (var tilePos in occupiedTiles)
            {
                var pos = new Vector2(tilePos.X * GraphicsConfig.TileSize, tilePos.Y * GraphicsConfig.TileSize);
                
                // Draw filled rectangle for the ghost
                Raylib.DrawRectangle((int)pos.X, (int)pos.Y, GraphicsConfig.TileSize, GraphicsConfig.TileSize, color);
                
                // Draw outline for each tile
                Raylib.DrawRectangleLines((int)pos.X, (int)pos.Y, GraphicsConfig.TileSize, GraphicsConfig.TileSize,
                    new Color((byte)255, (byte)255, (byte)255, (byte)200));
            }

            // Draw door indicator (brighter and more visible)
            var doorWorldPos = new Vector2(doorPos.X * GraphicsConfig.TileSize, doorPos.Y * GraphicsConfig.TileSize);
            Raylib.DrawRectangle(
                (int)doorWorldPos.X + GraphicsConfig.TileSize / 4, 
                (int)doorWorldPos.Y + GraphicsConfig.TileSize / 4,
                GraphicsConfig.TileSize / 2,
                GraphicsConfig.TileSize / 2,
                new Color((byte)255, (byte)255, (byte)0, (byte)220)
            );
            
            // Draw door outline
            Raylib.DrawRectangleLines(
                (int)doorWorldPos.X + GraphicsConfig.TileSize / 4, 
                (int)doorWorldPos.Y + GraphicsConfig.TileSize / 4,
                GraphicsConfig.TileSize / 2,
                GraphicsConfig.TileSize / 2,
                new Color((byte)255, (byte)255, (byte)255, (byte)255)
            );
            
            // Draw building glyph at origin (more opaque)
            var glyph = GetBuildingGlyph(buildingType);
            int textX = tileX * GraphicsConfig.TileSize + (GraphicsConfig.TileSize - 16) / 2;
            int textY = tileY * GraphicsConfig.TileSize + (GraphicsConfig.TileSize - 16) / 2;
            GraphicsConfig.DrawConsoleText(glyph, textX, textY, 16, 
                new Color((byte)255, (byte)255, (byte)255, (byte)255));
            
            // ENHANCED Debug info - always show for now
            int debugTextY = GraphicsConfig.StatusBarHeight + 150;
            
            // Show placement status
            var statusColor = canPlace 
                ? new Color((byte)100, (byte)255, (byte)100, (byte)255)
                : new Color((byte)255, (byte)100, (byte)100, (byte)255);
            
            GraphicsConfig.DrawConsoleText($"Status: {(canPlace ? "CAN PLACE" : "CANNOT PLACE")}", 10, debugTextY, 12, statusColor);
            debugTextY += 15;
            
            // Show building info
            GraphicsConfig.DrawConsoleText($"Building: {buildingType} ({definition.Width}x{definition.Height})", 10, debugTextY, 12, 
                new Color((byte)200, (byte)200, (byte)200, (byte)255));
            debugTextY += 15;
            
            GraphicsConfig.DrawConsoleText($"Origin: ({tileX}, {tileY}), Rotation: {rotation}", 10, debugTextY, 12, 
                new Color((byte)200, (byte)200, (byte)200, (byte)255));
            debugTextY += 15;
            
            // Show tile validation
            GraphicsConfig.DrawConsoleText($"Tiles: {occupiedTiles.Count} total, {validTiles} valid, {invalidTiles} invalid", 10, debugTextY, 12, 
                new Color((byte)200, (byte)200, (byte)200, (byte)255));
            debugTextY += 15;
            
            if (!canPlace && !string.IsNullOrEmpty(debugReason))
            {
                GraphicsConfig.DrawConsoleText($"Reason: {debugReason}", 10, debugTextY, 12, 
                    new Color((byte)255, (byte)100, (byte)100, (byte)255));
                debugTextY += 15;
            }
            
            // Show first few tile positions
            debugTextY += 5;
            GraphicsConfig.DrawConsoleText("Occupied tile positions:", 10, debugTextY, 12, 
                new Color((byte)150, (byte)150, (byte)150, (byte)255));
            debugTextY += 15;
            
            for (int i = 0; i < Math.Min(5, occupiedTiles.Count); i++)
            {
                var tp = occupiedTiles[i];
                var tile = _engine.Grid.GetTile(tp.X, tp.Y);
                string tileInfo = tile == null ? "NULL" : $"{tile.Type}, Walk:{tile.IsWalkable}, Bld:{tile.Building != null}";
                
                GraphicsConfig.DrawConsoleText($"  [{i}] ({tp.X}, {tp.Y}) - {tileInfo}", 10, debugTextY, 11, 
                    new Color((byte)180, (byte)180, (byte)180, (byte)255));
                debugTextY += 14;
            }
        }

        private void DrawBuildingPlacementUI(BuildingType buildingType)
        {
            var tempBuilding = new Building(buildingType, 0, 0);
            var costs = tempBuilding.GetConstructionCost();
            var definition = BuildingDefinition.Definitions.Get(buildingType);

            int boxWidth = 350;
            int boxHeight = 120 + (costs.Count * 20);
            int boxX = (GraphicsConfig.ScreenWidth - boxWidth) / 2;
            int boxY = GraphicsConfig.StatusBarHeight + 10;

            Raylib.DrawRectangle(boxX, boxY, boxWidth, boxHeight, 
                new Color((byte)20, (byte)20, (byte)30, (byte)230));
            Raylib.DrawRectangleLines(boxX, boxY, boxWidth, boxHeight, 
                new Color((byte)100, (byte)100, (byte)150, (byte)255));

            GraphicsConfig.DrawConsoleText($"Place {buildingType} ({definition.Width}x{definition.Height})", 
                boxX + 10, boxY + 10, 18, 
                new Color((byte)255, (byte)220, (byte)100, (byte)255));

            int yOffset = boxY + 35;
            GraphicsConfig.DrawConsoleText($"Rotation: {_currentRotation} ({(int)_currentRotation}°)", 
                boxX + 10, yOffset, 14, 
                new Color((byte)150, (byte)200, (byte)255, (byte)255));
            yOffset += 20;

            GraphicsConfig.DrawConsoleText($"Road Snap: {(_roadSnapEnabled ? "ON" : "OFF")}", 
                boxX + 10, yOffset, 14, 
                new Color((byte)150, (byte)200, (byte)255, (byte)255));
            yOffset += 25;

            GraphicsConfig.DrawConsoleText("Costs:", boxX + 10, yOffset, 14, 
                new Color((byte)200, (byte)200, (byte)200, (byte)255));
            yOffset += 20;

            foreach (var cost in costs)
            {
                bool hasEnough = _engine.VillageResources.Has(cost.Key, cost.Value);
                var color = hasEnough 
                    ? new Color((byte)100, (byte)255, (byte)100, (byte)255)
                    : new Color((byte)255, (byte)100, (byte)100, (byte)255);

                int current = _engine.VillageResources.Get(cost.Key);
                GraphicsConfig.DrawConsoleText($"  {cost.Key}: {current}/{cost.Value}", 
                    boxX + 20, yOffset, 14, color);
                yOffset += 20;
            }

            GraphicsConfig.DrawConsoleText("R: Rotate | Tab: Toggle Snap | Left Click: Place | Right Click/ESC: Cancel", 
                boxX + 10, boxY + boxHeight - 25, 11, 
                new Color((byte)150, (byte)150, (byte)150, (byte)255));
        }

        private string GetBuildingGlyph(BuildingType buildingType)
        {
            return buildingType switch
            {
                BuildingType.House => "█",
                BuildingType.Farm => "♠",
                BuildingType.Warehouse => "■",
                BuildingType.Mine => "╬",
                BuildingType.Lumberyard => "╪",
                BuildingType.Workshop => "◘",
                BuildingType.Market => "☼",
                BuildingType.Well => "○",
                BuildingType.TownHall => "▓",
                _ => "?"
            };
        }

        public void Update(float deltaTime)
        {
            _particleSystem.Update(deltaTime);
        }

        public void Shutdown()
        {
            Raylib.CloseWindow();
        }

        public void AddParticleEffect(Vector2 position, ParticleType type)
        {
            _particleSystem.Emit(position, type);
        }
    }
}
