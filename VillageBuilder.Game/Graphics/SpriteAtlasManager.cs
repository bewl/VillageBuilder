using System;
using System.Collections.Generic;
using System.IO;
using Raylib_cs;
using VillageBuilder.Engine.World;

namespace VillageBuilder.Game.Graphics
{
    /// <summary>
    /// Manages emoji sprite textures for modern terrain decoration rendering.
    /// Provides fast lookup and efficient rendering of emoji sprites as an alternative to ASCII symbols.
    /// </summary>
    public class SpriteAtlasManager
    {
        private static SpriteAtlasManager? _instance;
        public static SpriteAtlasManager Instance => _instance ??= new SpriteAtlasManager();

        private readonly Dictionary<DecorationType, Texture2D> _sprites = new();
        private readonly Dictionary<DecorationType, string> _spriteFilePaths = new();
        private bool _spritesLoaded = false;

        /// <summary>
        /// Check if sprite mode is enabled and sprites are loaded
        /// </summary>
        public bool SpriteModeEnabled => _spritesLoaded && _sprites.Count > 0;

        private SpriteAtlasManager()
        {
            // Define sprite file mappings for each decoration type
            // These map to Twemoji PNG files (72x72 resolution)
            _spriteFilePaths[DecorationType.TreeOak] = "1f333.png";           // ?? deciduous tree
            _spriteFilePaths[DecorationType.TreePine] = "1f332.png";          // ?? evergreen tree
            _spriteFilePaths[DecorationType.TreeDead] = "1fab5.png";          // ?? wood
            
            _spriteFilePaths[DecorationType.BushRegular] = "1f33f.png";       // ?? herb
            _spriteFilePaths[DecorationType.BushBerry] = "1fad0.png";         // ?? blueberries
            _spriteFilePaths[DecorationType.BushFlowering] = "1f33a.png";     // ?? hibiscus
            
            _spriteFilePaths[DecorationType.FlowerWild] = "1f33c.png";        // ?? blossom
            _spriteFilePaths[DecorationType.FlowerRare] = "1f338.png";        // ?? cherry blossom
            
            _spriteFilePaths[DecorationType.GrassTuft] = "1f33e.png";         // ?? sheaf of rice
            _spriteFilePaths[DecorationType.TallGrass] = "1f33e.png";         // ?? sheaf of rice
            _spriteFilePaths[DecorationType.Fern] = "1f33f.png";              // ?? herb
            _spriteFilePaths[DecorationType.Reeds] = "1f33f.png";             // ?? herb

            _spriteFilePaths[DecorationType.RockBoulder] = "1faa8.png";       // ?? rock
            _spriteFilePaths[DecorationType.RockPebble] = "1faa8.png";        // ?? rock
            _spriteFilePaths[DecorationType.StumpOld] = "1fab5.png";          // ?? wood
            _spriteFilePaths[DecorationType.LogFallen] = "1fab5.png";         // ?? wood
            _spriteFilePaths[DecorationType.Mushroom] = "1f344.png";          // ?? mushroom

            _spriteFilePaths[DecorationType.BirdFlying] = "1f426.png";        // ?? bird
            _spriteFilePaths[DecorationType.BirdPerched] = "1f99c.png";       // ?? parrot
            _spriteFilePaths[DecorationType.Butterfly] = "1f98b.png";         // ?? butterfly
            _spriteFilePaths[DecorationType.RabbitSmall] = "1f430.png";       // ?? rabbit face
            _spriteFilePaths[DecorationType.DeerGrazing] = "1f98c.png";       // ?? deer
            _spriteFilePaths[DecorationType.FishInWater] = "1f41f.png";       // ?? fish
        }

        /// <summary>
        /// Load all emoji sprite textures from disk
        /// </summary>
        public void LoadSprites()
        {
            if (_spritesLoaded) return;

            string spriteDir = "assets/sprites/emojis/";
            
            Console.WriteLine("=== Emoji Sprite Loading ===");
            Console.WriteLine($"Loading emoji sprites from: {spriteDir}");

            int loadedCount = 0;
            int failedCount = 0;

            foreach (var kvp in _spriteFilePaths)
            {
                var decorationType = kvp.Key;
                var filename = kvp.Value;
                var fullPath = Path.Combine(spriteDir, filename);

                if (File.Exists(fullPath))
                {
                    try
                    {
                        var texture = Raylib.LoadTexture(fullPath);
                        
                        // Set point filter for crisp pixel-perfect rendering
                        Raylib.SetTextureFilter(texture, TextureFilter.Point);
                        
                        _sprites[decorationType] = texture;
                        loadedCount++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  ? Failed to load {filename}: {ex.Message}");
                        failedCount++;
                    }
                }
                else
                {
                    failedCount++;
                }
            }

            _spritesLoaded = loadedCount > 0;

            if (_spritesLoaded)
            {
                Console.WriteLine($"? Loaded {loadedCount} emoji sprites successfully!");
                Console.WriteLine($"  Sprite mode: ENABLED");
                Console.WriteLine($"  Terrain will render with colorful emoji sprites");
                
                if (failedCount > 0)
                {
                    Console.WriteLine($"  ? {failedCount} sprites missing (will use ASCII fallback)");
                }
            }
            else
            {
                Console.WriteLine("? No emoji sprites loaded - using ASCII-only mode");
                Console.WriteLine("  To enable sprites, run: assets/sprites/download_twemoji.ps1");
            }
            
            Console.WriteLine("==========================");
        }

        /// <summary>
        /// Get sprite texture for a decoration type (returns null if not loaded)
        /// </summary>
        public Texture2D? GetSprite(DecorationType type)
        {
            if (_sprites.TryGetValue(type, out var texture))
            {
                return texture;
            }
            return null;
        }

        /// <summary>
        /// Check if a specific decoration type has a sprite available
        /// </summary>
        public bool HasSprite(DecorationType type)
        {
            return _sprites.ContainsKey(type);
        }

        /// <summary>
        /// Unload all sprite textures from memory
        /// </summary>
        public void UnloadSprites()
        {
            foreach (var texture in _sprites.Values)
            {
                Raylib.UnloadTexture(texture);
            }
            
            _sprites.Clear();
            _spritesLoaded = false;
            
            Console.WriteLine("? Emoji sprites unloaded");
        }

        /// <summary>
        /// Get the number of loaded sprites
        /// </summary>
        public int LoadedSpriteCount => _sprites.Count;
    }
}
