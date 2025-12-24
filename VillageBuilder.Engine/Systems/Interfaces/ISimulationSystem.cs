using VillageBuilder.Engine.Config;

namespace VillageBuilder.Engine.Systems.Interfaces
{
    /// <summary>
    /// Core simulation system managing time progression and game state updates.
    /// Orchestrates the tick-based simulation loop.
    /// </summary>
    public interface ISimulationSystem
    {
        /// <summary>
        /// Current game time
        /// </summary>
        Core.GameTime Time { get; }
        
        /// <summary>
        /// Current time scale multiplier (0.0 = paused, 1.0 = normal, 2.0 = double speed)
        /// </summary>
        float TimeScale { get; }
        
        /// <summary>
        /// Total number of ticks since game start
        /// </summary>
        long TotalTicks { get; }
        
        /// <summary>
        /// Configuration for simulation parameters
        /// </summary>
        SimulationConfig Config { get; }
        
        /// <summary>
        /// Advance simulation by one tick. Called each frame when not paused.
        /// </summary>
        void Tick();
        
        /// <summary>
        /// Set the simulation speed multiplier
        /// </summary>
        void SetTimeScale(float scale);
        
        /// <summary>
        /// Pause simulation (time scale = 0)
        /// </summary>
        void Pause();
        
        /// <summary>
        /// Resume simulation (restore previous time scale)
        /// </summary>
        void Resume();
        
        /// <summary>
        /// Check if simulation is paused
        /// </summary>
        bool IsPaused { get; }
    }
}
