using System;

namespace VillageBuilder.Engine.Core
{
    /// <summary>
    /// Configuration settings for initializing a game engine instance
    /// </summary>
    public class GameConfiguration
    {
        /// <summary>
        /// Width of the game map in tiles
        /// </summary>
        public int MapWidth { get; set; } = 100;

        /// <summary>
        /// Height of the game map in tiles
        /// </summary>
        public int MapHeight { get; set; } = 100;

        /// <summary>
        /// Target simulation ticks per second
        /// </summary>
        public int TickRate { get; set; } = 20;

        /// <summary>
        /// Maximum number of players allowed in the game
        /// </summary>
        public int MaxPlayers { get; set; } = 4;

        /// <summary>
        /// Random seed for deterministic world generation
        /// </summary>
        public int? Seed { get; set; }

        /// <summary>
        /// Starting resources for the village
        /// </summary>
        public StartingResources? InitialResources { get; set; }

        /// <summary>
        /// Validate configuration settings
        /// </summary>
        public void Validate()
        {
            if (MapWidth < 10 || MapWidth > 1000)
                throw new ArgumentException("MapWidth must be between 10 and 1000", nameof(MapWidth));
            
            if (MapHeight < 10 || MapHeight > 1000)
                throw new ArgumentException("MapHeight must be between 10 and 1000", nameof(MapHeight));
            
            if (TickRate < 1 || TickRate > 120)
                throw new ArgumentException("TickRate must be between 1 and 120", nameof(TickRate));
            
            if (MaxPlayers < 1 || MaxPlayers > 16)
                throw new ArgumentException("MaxPlayers must be between 1 and 16", nameof(MaxPlayers));
        }
    }

    /// <summary>
    /// Starting resource amounts for a new game
    /// </summary>
    public class StartingResources
    {
        public int Wood { get; set; } = 10000;
        public int Stone { get; set; } = 5000;
        public int Tools { get; set; } = 1000;
        public int Grain { get; set; } = 20000;
        public int Firewood { get; set; } = 5000;
    }
}
