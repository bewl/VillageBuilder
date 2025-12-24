using VillageBuilder.Engine.Core;
using VillageBuilder.Game.Graphics;
using VillageBuilder.Game.UI.ViewModels;
using VillageBuilder.Game.Core.Services;
using VillageBuilder.Game.Core;
using VillageBuilder.Game.Graphics.UI;

var config = new GameConfiguration
{
    MapWidth = 256,
    MapHeight = 256,
    TickRate = 60,
    MaxPlayers = 4,
    Seed = 42
};

var engine = new GameEngine(gameId: 1, config);
var viewModel = new GameViewModel(engine);
var gameLoop = new GameLoopService(viewModel);

// Initialize window (auto-fullscreen if configured size > monitor)
// GraphicsConfig.InitializeWindow(false) to disable auto fullscreen
GraphicsConfig.InitializeWindow();

// Load console font
GraphicsConfig.LoadFont();

// Initialize save directory
SaveLoadService.InitializeSaveDirectory();

var renderer = new GameRenderer(engine);

gameLoop.Start();

while (!renderer.ShouldClose())
{
    renderer.HandleInput(out bool togglePause, out float timeScaleChange, out bool saveRequested, out bool loadRequested);

    if (togglePause)
        viewModel.TogglePause();

    if (timeScaleChange != 0)
    {
        if (timeScaleChange == 0.0f) // Special case: reset to 1x
            viewModel.SetTimeScale(1.0f);
        else
            viewModel.MultiplyTimeScale(timeScaleChange);
    }

    // Handle save/load
    if (saveRequested)
    {
        var result = SaveLoadService.QuickSave(engine);
        EventLog.Instance.AddMessage(result.Message, result.Success ? LogLevel.Success : LogLevel.Error);
    }

    if (loadRequested)
    {
        var result = SaveLoadService.QuickLoad();
        if (result.Success && result.Engine != null)
        {
            // Stop old game loop
            gameLoop.Stop();

            // Replace engine
            engine = result.Engine;
            viewModel = new GameViewModel(engine);
            gameLoop = new GameLoopService(viewModel);
            renderer = new GameRenderer(engine);

            // Restart game loop
            gameLoop.Start();

            EventLog.Instance.AddMessage(result.Message, LogLevel.Success);
        }
        else
        {
            EventLog.Instance.AddMessage(result.Message, LogLevel.Error);
        }
    }

    float deltaTime = Raylib_cs.Raylib.GetFrameTime();
    renderer.Update(deltaTime);
    renderer.Render(viewModel.TimeScale, viewModel.IsPaused);
}

gameLoop.Stop();

// Cleanup
GraphicsConfig.UnloadFont();
renderer.Shutdown();
