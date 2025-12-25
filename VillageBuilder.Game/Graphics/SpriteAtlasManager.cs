using System;
using System.Collections.Generic;
using System.IO;
using Raylib_cs;
using VillageBuilder.Engine.World;

namespace VillageBuilder.Game.Graphics
{
    /// <summary>
    /// Types of UI icons available as emoji sprites
    /// </summary>
    public enum UIIconType
    {
        // Resources
        Wood,
        Stone,
        Grain,
        Tools,
        Firewood,

        // People & Buildings
        People,
        Buildings,
        House,
        Workshop,
        Mine,

        // Status
        Save,
        Construction,
        Settings,
        Stats,

        // Activities
        Sleeping,
        Walking,
        Resting,
        Idle,

        // Log Levels
        Info,
        Warning,
        Error,
        Success,

        // UI Decorations
        ArrowRight,
        ArrowLeft,
        SeparatorLine
    }

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
        private readonly Dictionary<UIIconType, Texture2D> _uiIcons = new();
        private readonly Dictionary<UIIconType, string> _uiIconFilePaths = new();
        private bool _spritesLoaded = false;
        private bool _uiIconsLoaded = false;

        /// <summary>
        /// Check if sprite mode is enabled and sprites are loaded
        /// </summary>
        public bool SpriteModeEnabled => _spritesLoaded && _sprites.Count > 0;

        /// <summary>
        /// Check if UI icons are loaded and available
        /// </summary>
        public bool UIIconsEnabled => _uiIconsLoaded && _uiIcons.Count > 0;

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
            // NEW: Predators and other wildlife
            _spriteFilePaths[DecorationType.FoxHunting] = "1f98a.png";        // ?? fox
            _spriteFilePaths[DecorationType.WolfPack] = "1f43a.png";          // ?? wolf
            _spriteFilePaths[DecorationType.BearGrizzly] = "1f43b.png";       // ?? bear
            _spriteFilePaths[DecorationType.BoarWild] = "1f417.png";          // ?? boar

            // NEW: Building sprites
            _spriteFilePaths[DecorationType.BuildingHouse] = "1f3e0.png";     // ?? house
            _spriteFilePaths[DecorationType.BuildingFarm] = "1f33e.png";      // ?? sheaf of rice (farm)
            _spriteFilePaths[DecorationType.BuildingWarehouse] = "1f3e6.png"; // ?? bank (warehouse)
            _spriteFilePaths[DecorationType.BuildingWorkshop] = "1f3ed.png";  // ?? factory
            _spriteFilePaths[DecorationType.BuildingMine] = "26cf.png";       // ?? pickaxe
            _spriteFilePaths[DecorationType.BuildingLumberyard] = "1fab5.png";// ?? wood
            _spriteFilePaths[DecorationType.BuildingMarket] = "1f3ea.png";    // ?? convenience store
            _spriteFilePaths[DecorationType.BuildingWell] = "1f6b0.png";      // ?? potable water
            _spriteFilePaths[DecorationType.BuildingTownHall] = "1f3db.png";  // ??? classical building

            // NEW: Building component sprites (detailed mode)
            // Floors
            _spriteFilePaths[DecorationType.BuildingFloorWood] = "1fab5.png";   // ?? wood
            _spriteFilePaths[DecorationType.BuildingFloorStone] = "1faa8.png";  // ?? rock
            _spriteFilePaths[DecorationType.BuildingFloorCarpet] = "1f7eb.png"; // ?? brown square
            _spriteFilePaths[DecorationType.BuildingFloorDirt] = "1f7eb.png";   // ?? brown square

            // Walls
            _spriteFilePaths[DecorationType.BuildingWallBrick] = "1f9f1.png";   // ?? brick
            _spriteFilePaths[DecorationType.BuildingWallStone] = "1faa8.png";   // ?? rock
            _spriteFilePaths[DecorationType.BuildingWallWood] = "1fab5.png";    // ?? wood
            _spriteFilePaths[DecorationType.BuildingWallPlaster] = "2b1c.png";  // ? white square

            // Doors
            _spriteFilePaths[DecorationType.BuildingDoorClosed] = "1f6aa.png";  // ?? door
            _spriteFilePaths[DecorationType.BuildingDoorOpen] = "1f6b6.png";    // ?? person walking
            _spriteFilePaths[DecorationType.BuildingDoorLocked] = "1f512.png";  // ?? locked

            // Windows
            _spriteFilePaths[DecorationType.BuildingWindowDay] = "1fa9f.png";   // ?? window
            _spriteFilePaths[DecorationType.BuildingWindowNight] = "1f319.png"; // ?? crescent moon
            _spriteFilePaths[DecorationType.BuildingWindowBroken] = "1f525.png";// ?? fire (broken)

            // Roofs
            _spriteFilePaths[DecorationType.BuildingRoofTiles] = "1f53a.png";   // ?? red triangle
            _spriteFilePaths[DecorationType.BuildingRoofThatch] = "1f33e.png";  // ?? sheaf of rice
            _spriteFilePaths[DecorationType.BuildingRoofShingles] = "1f7e9.png";// ?? green square
            _spriteFilePaths[DecorationType.BuildingRoofWood] = "1fab5.png";    // ?? wood

            // Foundations
            _spriteFilePaths[DecorationType.BuildingFoundationStone] = "1faa8.png"; // ?? rock
            _spriteFilePaths[DecorationType.BuildingFoundationWood] = "1fab5.png";  // ?? wood

            // Decorations
            _spriteFilePaths[DecorationType.BuildingChimney] = "1f525.png";     // ?? fire
            _spriteFilePaths[DecorationType.BuildingFence] = "1f6a7.png";       // ?? construction
            _spriteFilePaths[DecorationType.BuildingSign] = "1f4cb.png";        // ?? clipboard

            // UI Icon mappings (stored in assets/sprites/ui_icons/)
            _uiIconFilePaths[UIIconType.Wood] = "1fab5.png";          // ?? wood
            _uiIconFilePaths[UIIconType.Stone] = "1faa8.png";         // ?? rock
            _uiIconFilePaths[UIIconType.Grain] = "1f33e.png";         // ?? grain
            _uiIconFilePaths[UIIconType.Tools] = "1f528.png";         // ?? hammer
            _uiIconFilePaths[UIIconType.Firewood] = "1f525.png";      // ?? fire

            _uiIconFilePaths[UIIconType.People] = "1f465.png";        // ?? people
            _uiIconFilePaths[UIIconType.Buildings] = "1f3d8.png";     // ??? buildings
            _uiIconFilePaths[UIIconType.House] = "1f3e0.png";         // ?? house
            _uiIconFilePaths[UIIconType.Workshop] = "1f3ed.png";      // ?? factory
            _uiIconFilePaths[UIIconType.Mine] = "26cf.png";           // ?? pickaxe

            _uiIconFilePaths[UIIconType.Save] = "1f4be.png";          // ?? floppy disk
            _uiIconFilePaths[UIIconType.Construction] = "1f3d7.png";  // ??? construction
            _uiIconFilePaths[UIIconType.Settings] = "2699.png";       // ?? gear
            _uiIconFilePaths[UIIconType.Stats] = "1f4ca.png";         // ?? chart

            _uiIconFilePaths[UIIconType.Sleeping] = "1f4a4.png";      // ?? zzz
            _uiIconFilePaths[UIIconType.Walking] = "1f6b6.png";       // ?? person walking
            _uiIconFilePaths[UIIconType.Resting] = "1f60c.png";       // ?? relieved face
            _uiIconFilePaths[UIIconType.Idle] = "1f9cd.png";          // ?? person standing

            _uiIconFilePaths[UIIconType.Info] = "2139.png";           // ?? information
            _uiIconFilePaths[UIIconType.Warning] = "26a0.png";        // ?? warning
                    _uiIconFilePaths[UIIconType.Error] = "274c.png";          // ? cross mark
                    _uiIconFilePaths[UIIconType.Success] = "2705.png";        // ? check mark

                    // UI Decorations (arrows, separators)
                    _uiIconFilePaths[UIIconType.ArrowRight] = "25b6.png";     // ? black right-pointing triangle
                    _uiIconFilePaths[UIIconType.ArrowLeft] = "25c0.png";      // ? black left-pointing triangle
                    _uiIconFilePaths[UIIconType.SeparatorLine] = "2796.png";  // ? heavy minus sign
                }

        /// <summary>
        /// Load all emoji sprite textures from disk
        /// </summary>
        public void LoadSprites()
        {
            if (_spritesLoaded) return;

            string spriteDir = "Assets/sprites/emojis/";
            string absolutePath = Path.GetFullPath(spriteDir);

            Console.WriteLine("=== Emoji Sprite Loading ===");
            Console.WriteLine($"Working Directory: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"Loading emoji sprites from: {spriteDir}");
            Console.WriteLine($"Absolute path: {absolutePath}");
            Console.WriteLine($"Directory exists: {Directory.Exists(absolutePath)}");

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

                        // Log successful wildlife sprite loads
                        if (decorationType == DecorationType.FoxHunting || 
                            decorationType == DecorationType.WolfPack || 
                            decorationType == DecorationType.BearGrizzly || 
                            decorationType == DecorationType.BoarWild)
                        {
                            Console.WriteLine($"  ? Loaded {decorationType}: {filename} (ID: {texture.Id}, Size: {texture.Width}x{texture.Height})");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  ? Failed to load {filename}: {ex.Message}");
                        failedCount++;
                    }
                }
                else
                {
                    // Log missing wildlife sprites specifically
                    if (decorationType == DecorationType.FoxHunting || 
                        decorationType == DecorationType.WolfPack || 
                        decorationType == DecorationType.BearGrizzly || 
                        decorationType == DecorationType.BoarWild)
                    {
                        Console.WriteLine($"  ? MISSING {decorationType}: {fullPath}");
                    }
                    failedCount++;
                }
            }

            _spritesLoaded = loadedCount > 0;

            if (_spritesLoaded)
            {
                Console.WriteLine($"? Loaded {loadedCount} emoji sprites successfully!");
                Console.WriteLine($"  Sprite mode: ENABLED");
                Console.WriteLine($"  Terrain will render with colorful emoji sprites");

                // Verify predator sprites specifically
                Console.WriteLine("");
                Console.WriteLine("Predator Sprite Check:");
                Console.WriteLine($"  Fox (FoxHunting): {(HasSprite(DecorationType.FoxHunting) ? "? LOADED" : "? MISSING")}");
                Console.WriteLine($"  Wolf (WolfPack): {(HasSprite(DecorationType.WolfPack) ? "? LOADED" : "? MISSING")}");
                Console.WriteLine($"  Bear (BearGrizzly): {(HasSprite(DecorationType.BearGrizzly) ? "? LOADED" : "? MISSING")}");
                Console.WriteLine($"  Boar (BoarWild): {(HasSprite(DecorationType.BoarWild) ? "? LOADED" : "? MISSING")}");

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

                /// <summary>
                /// Load all UI icon sprites from disk
                /// </summary>
                public void LoadUIIcons()
                {
                    if (_uiIconsLoaded) return;

                    string iconDir = "Assets/sprites/ui_icons/emojis/";

                    Console.WriteLine("=== UI Icon Sprite Loading ===");
                    Console.WriteLine($"Loading UI icon sprites from: {iconDir}");

                    int loadedCount = 0;
                    int failedCount = 0;

                    foreach (var kvp in _uiIconFilePaths)
                    {
                        var iconType = kvp.Key;
                        var filename = kvp.Value;
                        var fullPath = Path.Combine(iconDir, filename);

                        if (File.Exists(fullPath))
                        {
                            try
                            {
                                var texture = Raylib.LoadTexture(fullPath);

                                // Set point filter for crisp pixel-perfect rendering
                                Raylib.SetTextureFilter(texture, TextureFilter.Point);

                                _uiIcons[iconType] = texture;
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

                    _uiIconsLoaded = loadedCount > 0;

                    if (_uiIconsLoaded)
                    {
                        Console.WriteLine($"? Loaded {loadedCount} UI icon sprites successfully!");
                        Console.WriteLine($"  UI icon mode: ENABLED");
                        Console.WriteLine($"  UI will render with colorful emoji icon sprites");

                        if (failedCount > 0)
                        {
                            Console.WriteLine($"  ? {failedCount} icons missing (will use ASCII fallback)");
                        }
                    }
                    else
                    {
                        Console.WriteLine("? No UI icon sprites loaded - using ASCII-only mode");
                        Console.WriteLine("  To enable UI icons, run: assets/sprites/ui_icons/download_ui_icons.ps1");
                    }

                    Console.WriteLine("==========================");
                }

                /// <summary>
                /// Get UI icon texture (returns null if not loaded)
                /// </summary>
                public Texture2D? GetUIIcon(UIIconType type)
                {
                    if (_uiIcons.TryGetValue(type, out var texture))
                    {
                        return texture;
                    }
                    return null;
                }

                /// <summary>
                /// Check if a specific UI icon type has a sprite available
                /// </summary>
                public bool HasUIIcon(UIIconType type)
                {
                    return _uiIcons.ContainsKey(type);
                }

                        /// <summary>
                        /// Get the number of loaded UI icons
                        /// </summary>
                        public int LoadedUIIconCount => _uiIcons.Count;

                                /// <summary>
                                /// Get the decoration type for a building type (for sprite rendering)
                                /// </summary>
                                public static DecorationType? GetBuildingSpriteType(VillageBuilder.Engine.Buildings.BuildingType buildingType)
                                {
                                    return buildingType switch
                                    {
                                        VillageBuilder.Engine.Buildings.BuildingType.House => DecorationType.BuildingHouse,
                                        VillageBuilder.Engine.Buildings.BuildingType.Farm => DecorationType.BuildingFarm,
                                        VillageBuilder.Engine.Buildings.BuildingType.Warehouse => DecorationType.BuildingWarehouse,
                                        VillageBuilder.Engine.Buildings.BuildingType.Workshop => DecorationType.BuildingWorkshop,
                                        VillageBuilder.Engine.Buildings.BuildingType.Mine => DecorationType.BuildingMine,
                                        VillageBuilder.Engine.Buildings.BuildingType.Lumberyard => DecorationType.BuildingLumberyard,
                                        VillageBuilder.Engine.Buildings.BuildingType.Market => DecorationType.BuildingMarket,
                                        VillageBuilder.Engine.Buildings.BuildingType.Well => DecorationType.BuildingWell,
                                        VillageBuilder.Engine.Buildings.BuildingType.TownHall => DecorationType.BuildingTownHall,
                                        _ => null
                                    };
                                }

                                        /// <summary>
                                        /// Get the sprite type for a specific building tile (for detailed per-tile rendering)
                                        /// </summary>
                                        public static DecorationType? GetBuildingTileSprite(
                                            VillageBuilder.Engine.Buildings.BuildingType buildingType,
                                            VillageBuilder.Engine.Buildings.BuildingTileType tileType,
                                            bool lightsOn = false)
                                        {
                                            // Building-specific floor types
                                            var floorType = buildingType switch
                                            {
                                                VillageBuilder.Engine.Buildings.BuildingType.House => DecorationType.BuildingFloorWood,
                                                VillageBuilder.Engine.Buildings.BuildingType.Farm => DecorationType.BuildingFloorDirt,
                                                VillageBuilder.Engine.Buildings.BuildingType.Warehouse => DecorationType.BuildingFloorStone,
                                                VillageBuilder.Engine.Buildings.BuildingType.Workshop => DecorationType.BuildingFloorStone,
                                                VillageBuilder.Engine.Buildings.BuildingType.Mine => DecorationType.BuildingFloorStone,
                                                VillageBuilder.Engine.Buildings.BuildingType.TownHall => DecorationType.BuildingFloorCarpet,
                                                _ => DecorationType.BuildingFloorWood
                                            };

                                            // Building-specific wall types
                                            var wallType = buildingType switch
                                            {
                                                VillageBuilder.Engine.Buildings.BuildingType.House => DecorationType.BuildingWallBrick,
                                                VillageBuilder.Engine.Buildings.BuildingType.Workshop => DecorationType.BuildingWallStone,
                                                VillageBuilder.Engine.Buildings.BuildingType.TownHall => DecorationType.BuildingWallStone,
                                                _ => DecorationType.BuildingWallWood
                                            };

                                            return tileType switch
                                            {
                                                VillageBuilder.Engine.Buildings.BuildingTileType.Floor => floorType,
                                                VillageBuilder.Engine.Buildings.BuildingTileType.Wall => wallType,
                                                VillageBuilder.Engine.Buildings.BuildingTileType.Door => DecorationType.BuildingDoorClosed,
                                                _ => null
                                            };
                                        }
                                    }
                                }
