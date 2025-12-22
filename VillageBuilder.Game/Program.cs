using VillageBuilder.Engine.Core;
using VillageBuilder.Game.Graphics;
using VillageBuilder.Game.UI.ViewModels;
using VillageBuilder.Game.Core.Services;

var config = new GameConfiguration
{
    MapWidth = 100,
    MapHeight = 100,
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

var renderer = new GameRenderer(engine);

gameLoop.Start();

while (!renderer.ShouldClose())
{
    renderer.HandleInput(out bool togglePause, out float timeScaleChange);
    
    if (togglePause)
        viewModel.TogglePause();
    
    if (timeScaleChange != 0)
    {
        if (timeScaleChange == 0.0f) // Special case: reset to 1x
            viewModel.SetTimeScale(1.0f);
        else
            viewModel.MultiplyTimeScale(timeScaleChange);
    }
    
    float deltaTime = Raylib_cs.Raylib.GetFrameTime();
    renderer.Update(deltaTime);
    renderer.Render(viewModel.TimeScale, viewModel.IsPaused);
}

gameLoop.Stop();

// Cleanup
GraphicsConfig.UnloadFont();
renderer.Shutdown();
