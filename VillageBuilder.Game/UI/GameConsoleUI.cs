using Terminal.Gui;
using VillageBuilder.Engine.Buildings;
using VillageBuilder.Engine.Commands.BuildingCommands;
using VillageBuilder.Engine.Core;
using VillageBuilder.Game.UI.ViewModels;
using VillageBuilder.Game.UI.Views;
using System;
using VillageBuilder.Game.Core.Services;

namespace VillageBuilder.Game.UI
{
    public class GameConsoleUI
    {
        private readonly GameViewModel _viewModel;
        private readonly GameLoopService _gameLoop;
        
        private Window _mainWindow = null!;
        private StatusBarView _statusBar = null!;
        private MapView _mapView = null!;
        private GameStateView _gameStateView = null!;
        private ControlPanelView _controlPanelView = null!;
        private EventLogView _eventLogView = null!;

        private readonly int _playerId;

        public GameConsoleUI(GameEngine engine, int playerId = 1)
        {
            _playerId = playerId;
            _viewModel = new GameViewModel(engine);
            _gameLoop = new GameLoopService(_viewModel);
            
            InitializeUI();
            WireUpEvents();
        }

        private void InitializeUI()
        {
            Application.Init();
            
            _mainWindow = new Window("Village Builder - Real-Time Strategy")
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            // Top Status Bar (3 rows)
            _statusBar = new StatusBarView(_viewModel)
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = 3
            };

            // Large Map View (center-left, dominant)
            _mapView = new MapView
            {
                X = 0,
                Y = Pos.Bottom(_statusBar),
                Width = Dim.Percent(70),
                Height = Dim.Fill()
            };

            // Right side - Top: Game State (compact)
            _gameStateView = new GameStateView
            {
                X = Pos.Right(_mapView),
                Y = Pos.Bottom(_statusBar),
                Width = Dim.Fill(),
                Height = Dim.Percent(25)
            };

            // Right side - Middle: Command Panel
            _controlPanelView = new ControlPanelView
            {
                X = Pos.Right(_mapView),
                Y = Pos.Bottom(_gameStateView),
                Width = Dim.Fill(),
                Height = Dim.Percent(40)
            };

            // Right side - Bottom: Event Log
            _eventLogView = new EventLogView
            {
                X = Pos.Right(_mapView),
                Y = Pos.Bottom(_controlPanelView),
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            _mainWindow.Add(_statusBar, _mapView, _gameStateView, _controlPanelView, _eventLogView);
            Application.Top.Add(_mainWindow);
            
            // Use RootKeyEvent to capture keys before they're processed by child views
            Application.RootKeyEvent = OnRootKeyEvent;
        }

        private void WireUpEvents()
        {
            _viewModel.StateChanged += OnViewModelStateChanged;
            _viewModel.LogMessage += OnViewModelLogMessage;
            
            _controlPanelView.PauseResumeClicked += (s, e) => _viewModel.TogglePause();
            _controlPanelView.TimeScaleChanged += (s, scale) =>
            {
                if (scale == 1.0f)
                    _viewModel.SetTimeScale(1.0f);
                else
                    _viewModel.MultiplyTimeScale(scale);
            };
            _controlPanelView.BuildingRequested += (s, buildingType) => ShowBuildingDialog(buildingType);
        }

        private void OnViewModelStateChanged(object? sender, EventArgs e)
        {
            Application.MainLoop.Invoke(() =>
            {
                _statusBar.UpdateStatus(_viewModel.GetEngine());
                _gameStateView.UpdateState(_viewModel.GetEngine());
                _mapView.UpdateMap(_viewModel.GetEngine());
                Application.Refresh();
            });
        }

        private void OnViewModelLogMessage(object? sender, string message)
        {
            Application.MainLoop.Invoke(() =>
            {
                _eventLogView.LogMessage(message);
            });
        }

        private bool OnRootKeyEvent(KeyEvent keyEvent)
        {
            var key = keyEvent.Key;
            
            // Arrow keys for map panning
            switch (key)
            {
                case Key.CursorUp:
                    _mapView.PanMap(0, -5, _viewModel.GetEngine());
                    _mapView.UpdateMap(_viewModel.GetEngine());
                    return true;
                case Key.CursorDown:
                    _mapView.PanMap(0, 5, _viewModel.GetEngine());
                    _mapView.UpdateMap(_viewModel.GetEngine());
                    return true;
                case Key.CursorLeft:
                    _mapView.PanMap(-5, 0, _viewModel.GetEngine());
                    _mapView.UpdateMap(_viewModel.GetEngine());
                    return true;
                case Key.CursorRight:
                    _mapView.PanMap(5, 0, _viewModel.GetEngine());
                    _mapView.UpdateMap(_viewModel.GetEngine());
                    return true;
            }
            
            // Handle special keys
            if (key == Key.Space)
            {
                _viewModel.TogglePause();
                return true;
            }

            if (key == (Key.Q | Key.CtrlMask))
            {
                Application.RequestStop();
                return true;
            }

            // Handle character keys
            var keyChar = (char)(key & Key.CharMask);
            
            switch (keyChar)
            {
                case '+':
                case '=':
                    _viewModel.MultiplyTimeScale(2.0f);
                    return true;
                
                case '-':
                case '_':
                    _viewModel.MultiplyTimeScale(0.5f);
                    return true;
                
                case 'f':
                case 'F':
                    ShowBuildingDialog(BuildingType.Farm);
                    return true;
                
                case 'h':
                case 'H':
                    ShowBuildingDialog(BuildingType.House);
                    return true;
                
                case 'w':
                case 'W':
                    ShowBuildingDialog(BuildingType.Warehouse);
                    return true;
                
                case 'q':
                case 'Q':
                    Application.RequestStop();
                    return true;
            }

            return false;
        }

        private void ShowBuildingDialog(BuildingType buildingType)
        {
            var dialog = new Dialog($"Build {buildingType}", 50, 10);

            var xLabel = new Label("X: ") { X = 1, Y = 1 };
            var xField = new TextField("50") { X = Pos.Right(xLabel), Y = 1, Width = 10 };

            var yLabel = new Label("Y: ") { X = 1, Y = 2 };
            var yField = new TextField("50") { X = Pos.Right(yLabel), Y = 2, Width = 10 };

            var okButton = new Button("Build", true)
            {
                X = Pos.Center() - 10,
                Y = 4
            };

            okButton.Clicked += () =>
            {
                if (int.TryParse(xField.Text.ToString(), out int x) &&
                    int.TryParse(yField.Text.ToString(), out int y))
                {
                    var command = new ConstructBuildingCommand(
                        _playerId,
                        _viewModel.CurrentTick + 1,
                        buildingType,
                        x,
                        y
                    );

                    _viewModel.SubmitCommand(command);
                    Application.RequestStop();
                }
            };

            var cancelButton = new Button("Cancel")
            {
                X = Pos.Center() + 5,
                Y = 4
            };

            cancelButton.Clicked += () => Application.RequestStop();

            dialog.Add(xLabel, xField, yLabel, yField, okButton, cancelButton);
            Application.Run(dialog);
        }

        public void Run()
        {
            _gameLoop.Start();
            _eventLogView.LogMessage("=== Welcome to Village Builder ===");
            _eventLogView.LogMessage("Controls:");
            _eventLogView.LogMessage("  Arrow Keys - Pan Map");
            _eventLogView.LogMessage("  F/H/W - Build Farm/House/Warehouse");
            _eventLogView.LogMessage("  Space - Pause/Resume | +/- Speed | Q - Quit");
            
            Application.Run();
            
            _gameLoop.Stop();
            Application.Shutdown();
        }
    }
}