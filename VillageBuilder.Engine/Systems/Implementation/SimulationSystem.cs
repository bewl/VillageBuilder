using VillageBuilder.Engine.Config;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Systems.Interfaces;

namespace VillageBuilder.Engine.Systems.Implementation
{
    /// <summary>
    /// Default implementation of ISimulationSystem.
    /// Manages game time progression and simulation loop.
    /// </summary>
    public class SimulationSystem : ISimulationSystem
    {
        private float _timeScale = 1.0f;
        private float _previousTimeScale = 1.0f;
        
        public GameTime Time { get; private set; }
        public float TimeScale => _timeScale;
        public long TotalTicks { get; private set; }
        public SimulationConfig Config { get; private set; }
        public bool IsPaused => _timeScale == 0.0f;
        
        public SimulationSystem(SimulationConfig? config = null)
        {
            Config = config ?? GameConfig.Instance.Simulation;
            Time = new GameTime();
            TotalTicks = 0;
            _timeScale = Config.DefaultTimeScale;
        }
        
        public void Tick()
        {
            if (IsPaused) return;

            // Advance time
            Time.AdvanceTick();
            TotalTicks++;
        }
        
        public void SetTimeScale(float scale)
        {
            // Clamp to configured limits
            _timeScale = Math.Clamp(scale, Config.MinTimeScale, Config.MaxTimeScale);
            
            if (_timeScale != 0.0f)
            {
                _previousTimeScale = _timeScale;
            }
        }
        
        public void Pause()
        {
            if (!IsPaused)
            {
                _previousTimeScale = _timeScale;
                _timeScale = 0.0f;
            }
        }
        
        public void Resume()
        {
            if (IsPaused)
            {
                _timeScale = _previousTimeScale;
            }
        }
    }
}
