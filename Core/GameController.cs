using System;
using VillageBuilder.Engine.Core;

namespace VillageBuilder.Game.Core
{
    /// <summary>
    /// Controls game state including pause, time scale, and simulation ticking.
    /// Acts as a facade over the GameEngine for application-level concerns.
    /// </summary>
    public class GameController
    {
        private readonly GameEngine _engine;
        
        public int CurrentTick => _engine.CurrentTick;
        public bool IsPaused { get; private set; }
        public float TimeScale { get; private set; } = 1.0f;
        
        public event EventHandler? StateChanged;
        public event EventHandler<string>? LogMessage;

        public GameController(GameEngine engine)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        }

        public void TogglePause()
        {
            IsPaused = !IsPaused;
            OnStateChanged();
            OnLogMessage($"Game {(IsPaused ? "paused" : "resumed")}");
        }

        public void SetTimeScale(float scale)
        {
            TimeScale = Math.Clamp(scale, 0.125f, 8.0f);
            OnStateChanged();
            OnLogMessage($"Time scale set to {TimeScale:F2}x");
        }

        public void MultiplyTimeScale(float multiplier)
        {
            SetTimeScale(TimeScale * multiplier);
        }

        public TimeSpan GetTickInterval()
        {
            return TimeSpan.FromMilliseconds(1000.0 / TimeScale);
        }

        public GameEngine GetEngine()
        {
            return _engine;
        }

        public void TickEngine()
        {
            if (!IsPaused)
            {
                var results = _engine.SimulateTick();
                
                // Log command results
                foreach (var result in results)
                {
                    OnLogMessage($"Command {result.Command.GetType().Name}: {result.Result.Message}");
                }
                
                OnStateChanged();
            }
        }

        public void SubmitCommand(ICommand command)
        {
            _engine.SubmitCommand(command);
            OnLogMessage($"Command queued: {command.GetType().Name} for tick {command.TargetTick}");
        }

        protected virtual void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnLogMessage(string message)
        {
            LogMessage?.Invoke(this, message);
        }
    }
}