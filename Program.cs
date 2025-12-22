using VillageBuilder.Engine.Core;
using VillageBuilder.Game.Core;
using VillageBuilder.Game.Core.Services;
using VillageBuilder.Game.Graphics;

var config = new GameConfiguration
{
    MapWidth = 100,
    MapHeight = 100,
    TickRate = 60,
    MaxPlayers = 4,
    Seed = 42
};

var engine = new GameEngine(gameId: 1, config);
var controller = new GameController(engine);
var gameLoop = new GameLoopService(controller);

// Initialize Raylib before creating GameRenderer
Raylib_cs.Raylib.InitWindow(GraphicsConfig.ScreenWidth, GraphicsConfig.ScreenHeight, GraphicsConfig.WindowTitle);
Raylib_cs.Raylib.SetTargetFPS(GraphicsConfig.TargetFPS);

var renderer = new GameRenderer(engine);

gameLoop.Start();

while (!renderer.ShouldClose())
{
    renderer.HandleInput(out bool togglePause, out float timeScaleChange);
    
    if (togglePause)
        controller.TogglePause();
    
    if (timeScaleChange != 0)
        controller.MultiplyTimeScale(timeScaleChange);
    
    float deltaTime = Raylib_cs.Raylib.GetFrameTime();
    renderer.Update(deltaTime);
    renderer.Render(controller.TimeScale, controller.IsPaused);
}

gameLoop.Stop();
renderer.Shutdown();