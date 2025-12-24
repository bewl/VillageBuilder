using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raylib_cs;

namespace VillageBuilder.Game.Graphics
{
    public static class GraphicsConfig
    {
        // Window settings - NOW DYNAMIC based on actual window size
        public static int ScreenWidth => Raylib.GetScreenWidth();
        public static int ScreenHeight => Raylib.GetScreenHeight();
        public const string WindowTitle = "Village Builder - Real-Time Strategy";
        public const int TargetFPS = 60;

        // Console emulation settings
        public const int TileSize = 16; // Size of each "character" cell
        public const int ConsoleCols = 120;
        public const int ConsoleRows = 67;

        // UI Layout - NOW DYNAMIC based on screen resolution
        // Status bar scales more conservatively (2.5x instead of 3x font height)
        public static int StatusBarHeight => (int)(ConsoleFontSize * 2.5f);
        public const int MapViewportWidth = 84; // 70% of console
        public const int SidebarWidth = 36; // 30% of console

        // Font settings - NOW DYNAMIC based on screen resolution
        public static Font ConsoleFont { get; private set; }
        public static Font EmojiFont { get; private set; }  // NEW: Separate font for emojis

        // Sprite settings - Modern emoji rendering via textures
        public static bool UseSpriteMode { get; private set; } = true;  // Default: prefer sprites over ASCII

        // Dynamic font sizing based on screen height
        // Base: 24px at 1080p (increased from 20px for better glyph accommodation)
        // Uses sublinear scaling to prevent oversized fonts at high resolutions
        private const int BaseFontSize = 24;
        private const int BaseScreenHeight = 1080;

        public static int ConsoleFontSize => CalculateFontSize(BaseFontSize);
        public static int SmallConsoleFontSize => CalculateFontSize(18);

        private static bool _fontLoaded = false;

        /// <summary>
        /// Calculates font size scaled to current screen resolution using sublinear scaling.
        /// This prevents fonts from becoming too large at high resolutions while still
        /// improving readability compared to fixed 20px.
        /// </summary>
        private static int CalculateFontSize(int baseSize)
        {
            int currentHeight = ScreenHeight;
            if (currentHeight == 0) currentHeight = BaseScreenHeight; // Fallback

            // Use square root scaling for more conservative growth
            // This gives better results at high resolutions where linear scaling overshoots
            float heightRatio = (float)currentHeight / BaseScreenHeight;
            float scale = (float)Math.Sqrt(heightRatio);

            // Apply scaling
            int scaledSize = (int)(baseSize * scale);

            // Clamp to reasonable bounds (12px min, 30px max)
            // Reduced max from 40 to 30 to prevent oversized text
            return Math.Clamp(scaledSize, 12, 30);
        }

        /// <summary>
        /// Initializes the Raylib window in borderless fullscreen windowed mode.
        /// This allows easy alt-tabbing and better debugging experience.
        /// </summary>
        /// <param name="allowFullscreenIfNeeded">Ignored - always uses borderless windowed mode.</param>
        public static void InitializeWindow(bool allowFullscreenIfNeeded = true)
        {
            // IMPORTANT: Must set config flags BEFORE InitWindow
            Raylib.SetConfigFlags(ConfigFlags.UndecoratedWindow | ConfigFlags.VSyncHint);

            // Initialize with small window first to enable monitor queries
            // We'll resize immediately after querying the monitor
            Raylib.InitWindow(800, 600, WindowTitle);

            // NOW we can query monitor size (after Raylib is initialized)
            int monitorWidth = Raylib.GetMonitorWidth(0);
            int monitorHeight = Raylib.GetMonitorHeight(0);

            Console.WriteLine($"GraphicsConfig: Detected monitor 0 resolution: {monitorWidth}x{monitorHeight}");

            // Fallback to common resolution if monitor query fails
            if (monitorWidth == 0 || monitorHeight == 0)
            {
                monitorWidth = 1920;
                monitorHeight = 1080;
                Console.WriteLine("WARNING: Monitor detection failed, using fallback 1920x1080");
            }

            // Resize window to full monitor size
            Raylib.SetWindowSize(monitorWidth, monitorHeight);

            // Set window position to top-left corner (0, 0) to cover entire screen including taskbar
            Raylib.SetWindowPosition(0, 0);

            // Disable ESC key as exit key (we handle ESC ourselves for UI navigation)
                Raylib.SetExitKey(0);

                Raylib.SetTargetFPS(TargetFPS);

                // Log font size calculation for debugging
                int fontSize = ConsoleFontSize;
                Console.WriteLine($"GraphicsConfig: Initialized borderless fullscreen at ({monitorWidth}x{monitorHeight}) position (0, 0)");
                Console.WriteLine($"GraphicsConfig: Calculated font size: {fontSize}px (base: {BaseFontSize}px at {BaseScreenHeight}p)");
            }

        public static void LoadFont()
        {
            if (_fontLoaded) return;

            // Load font with extended character support for terrain decorations
            int[] codepoints = new int[768]; // Increased from 512 to accommodate terrain decorations

            // Basic ASCII (0-127)
            for (int i = 0; i < 128; i++)
                codepoints[i] = i;

            // Extended ASCII / Code Page 437 (128-255)
            for (int i = 128; i < 256; i++)
                codepoints[i] = i;
            
            // Box drawing and symbols
            int idx = 256;
            codepoints[idx++] = 0x2500; // ─
            codepoints[idx++] = 0x2502; // │
            codepoints[idx++] = 0x250C; // ┌
            codepoints[idx++] = 0x2510; // ┐
            codepoints[idx++] = 0x2514; // └
            codepoints[idx++] = 0x2518; // ┘
            codepoints[idx++] = 0x251C; // ├
            codepoints[idx++] = 0x2524; // ┤
            codepoints[idx++] = 0x252C; // ┬
            codepoints[idx++] = 0x2534; // ┴
            codepoints[idx++] = 0x253C; // ┼
            codepoints[idx++] = 0x2550; // ═
            codepoints[idx++] = 0x2551; // ║
            codepoints[idx++] = 0x2554; // ╔
            codepoints[idx++] = 0x2557; // ╗
            codepoints[idx++] = 0x255A; // ╚
            codepoints[idx++] = 0x255D; // ╝
            codepoints[idx++] = 0x2560; // ╠
            codepoints[idx++] = 0x2563; // ╣
            codepoints[idx++] = 0x2566; // ╦
            codepoints[idx++] = 0x2569; // ╩
            codepoints[idx++] = 0x256C; // ╬
            codepoints[idx++] = 0x2592; // ▒
            codepoints[idx++] = 0x2591; // ░
            codepoints[idx++] = 0x2593; // ▓
            codepoints[idx++] = 0x2588; // █
            codepoints[idx++] = 0x2584; // ▄
            codepoints[idx++] = 0x258C; // ▌
            codepoints[idx++] = 0x2590; // ▐
            codepoints[idx++] = 0x2580; // ▀
            codepoints[idx++] = 0x263A; // ☺
            codepoints[idx++] = 0x263B; // ☻
            codepoints[idx++] = 0x2665; // ♥
            codepoints[idx++] = 0x2666; // ♦
            codepoints[idx++] = 0x2663; // ♣
            codepoints[idx++] = 0x2660; // ♠
            codepoints[idx++] = 0x25CF; // ●
            codepoints[idx++] = 0x25CB; // ○
            codepoints[idx++] = 0x25A0; // ■
            codepoints[idx++] = 0x25A1; // □
            codepoints[idx++] = 0x25B2; // ▲
            codepoints[idx++] = 0x25BC; // ▼
            codepoints[idx++] = 0x25C6; // ◆
            codepoints[idx++] = 0x25C7; // ◇
            codepoints[idx++] = 0x2248; // ≈
            codepoints[idx++] = 0x2261; // ≡
            codepoints[idx++] = 0x00B1; // ±
            codepoints[idx++] = 0x00D7; // ×
            codepoints[idx++] = 0x2713; // ✓
            codepoints[idx++] = 0x2022; // •
            codepoints[idx++] = 0x00B7; // ·
            codepoints[idx++] = 0x2014; // —
            codepoints[idx++] = 0x256A; // ╪
            codepoints[idx++] = 0x256B; // ╫
            codepoints[idx++] = 0x25CA; // ◊

            // Smoke characters (critical for particle effects)
            codepoints[idx++] = 0x2219; // ∙ (bullet operator)
            codepoints[idx++] = 0x02D9; // ˙ (dot above)
            codepoints[idx++] = 0x2218; // ∘ (ring operator)
            codepoints[idx++] = 0x25E6; // ◦ (white bullet)

            // Additional useful symbols
            codepoints[idx++] = 0x2192; // → (right arrow)
            codepoints[idx++] = 0x2190; // ← (left arrow)
            codepoints[idx++] = 0x2191; // ↑ (up arrow)
            codepoints[idx++] = 0x2193; // ↓ (down arrow)
            codepoints[idx++] = 0x2194; // ↔ (left-right arrow)
            codepoints[idx++] = 0x21D2; // ⇒ (rightwards double arrow)
            codepoints[idx++] = 0x279C; // ➜ (heavy round-tipped rightwards arrow)

            // Emojis (if supported by font)
            codepoints[idx++] = 0x1F3E0; // 🏠 (house)
            codepoints[idx++] = 0x1F528; // 🔨 (hammer)
            codepoints[idx++] = 0x2692; // ⚒ (hammer and pick)
            codepoints[idx++] = 0x1F4A4; // 💤 (zzz)

            // NEW: Terrain decoration emojis
            codepoints[idx++] = 0x1F333; // 🌳 (deciduous tree)
            codepoints[idx++] = 0x1F332; // 🌲 (evergreen tree)
            codepoints[idx++] = 0x1F384; // 🎄 (Christmas tree)
            codepoints[idx++] = 0x1FAB5; // 🪵 (wood/log)
            codepoints[idx++] = 0x1F33A; // 🌺 (hibiscus)
            codepoints[idx++] = 0x1FAD0; // 🫐 (blueberries)
            codepoints[idx++] = 0x1F338; // 🌸 (cherry blossom)
            codepoints[idx++] = 0x1F33C; // 🌼 (blossom)
            codepoints[idx++] = 0x1F33E; // 🌾 (rice/grain)
            codepoints[idx++] = 0x1F33F; // 🌿 (herb)
            codepoints[idx++] = 0x1F344; // 🍄 (mushroom)
            codepoints[idx++] = 0x1F985; // 🦅 (eagle)
            codepoints[idx++] = 0x1F426; // 🐦 (bird)
            codepoints[idx++] = 0x1F99C; // 🦜 (parrot)
            codepoints[idx++] = 0x1F98B; // 🦋 (butterfly)
            codepoints[idx++] = 0x1F430; // 🐰 (rabbit face)
            codepoints[idx++] = 0x1F98C; // 🦌 (deer)
            codepoints[idx++] = 0x1F41F; // 🐟 (fish)

            // Additional symbols for terrain
            codepoints[idx++] = 0x2318; // ⌘ (place of interest)
            codepoints[idx++] = 0x25C9; // ◉ (fisheye)
            codepoints[idx++] = 0x25CE; // ◎ (bullseye)
            codepoints[idx++] = 0x25C8; // ◈ (white diamond with black centre)
            codepoints[idx++] = 0x25CA; // ◊ (lozenge)
            codepoints[idx++] = 0x2726; // ✦ (black four-pointed star)
            codepoints[idx++] = 0x273F; // ✿ (black florette)
            codepoints[idx++] = 0x273E; // ✾ (six petals surrounded)
            codepoints[idx++] = 0x2740; // ❀ (white florette)
            codepoints[idx++] = 0x2741; // ❁ (eight petals)
            codepoints[idx++] = 0x274B; // ❋ (heavy eight teardrop-spoked asterisk)
            codepoints[idx++] = 0x2756; // ❖ (black diamond minus white X)
            codepoints[idx++] = 0x273A; // ✺ (sixteen pointed asterisk)
            codepoints[idx++] = 0x2605; // ★ (black star)
            codepoints[idx++] = 0x2606; // ☆ (white star)
            codepoints[idx++] = 0x25B4; // ▴ (black up-pointing small triangle)
            codepoints[idx++] = 0x25B3; // △ (white up-pointing triangle)
            codepoints[idx++] = 0x219F; // ↟ (upwards two-headed arrow)
            codepoints[idx++] = 0x2303; // ⌃ (up arrowhead)
            codepoints[idx++] = 0x223C; // ∼ (tilde operator)
            codepoints[idx++] = 0x224B; // ≋ (triple tilde)
            codepoints[idx++] = 0x2038; // ‸ (caret)
            codepoints[idx++] = 0x2219; // ∙ (bullet operator - for smoke)
            codepoints[idx++] = 0x02D9; // ˙ (dot above)
            codepoints[idx++] = 0x2218; // ∘ (ring operator)
            codepoints[idx++] = 0x25E6; // ◦ (white bullet)
            codepoints[idx++] = 0x2698; // ⚘ (flower)
            codepoints[idx++] = 0x2299; // ⊙ (circled dot)

            Array.Resize(ref codepoints, idx);

            // Font priority order - UPDATED for full ASCII/Unicode support
            // Priority: Fonts that exist on user's system > Unicode coverage > Readability
            string[] fontPaths = new[]
            {
                // PRIORITY 1: Bundled fonts (shipped with game) - JetBrains Mono is best!
                "assets/fonts/JetBrainsMono-Regular.ttf",     // NEW: Best clarity + Unicode
                "assets/fonts/CascadiaCode.ttf",
                "assets/fonts/CascadiaMono.ttf",
                "assets/fonts/DejaVuSansMono.ttf",

                // PRIORITY 2: System-installed modern fonts
                "C:\\Windows\\Fonts\\JetBrainsMono-Regular.ttf", // JetBrains Mono (if installed)
                "C:\\Windows\\Fonts\\CascadiaCode.ttf",        // Cascadia Code - excellent Unicode
                "C:\\Windows\\Fonts\\CascadiaMono.ttf",        // Cascadia Mono - BEST for Unicode + readability
                "C:\\Windows\\Fonts\\Cascadia.ttf",            // Generic Cascadia

                // GOOD: Alternative modern fonts
                "C:\\Windows\\Fonts\\DejaVuSansMono.ttf",      // DejaVu Sans Mono - excellent Unicode
                "/usr/share/fonts/truetype/dejavu/DejaVuSansMono.ttf",  // Linux
                "C:\\Windows\\Fonts\\IBMPlexMono-Regular.ttf", // IBM Plex Mono

                // FALLBACK: Standard Windows fonts (limited Unicode)
                "C:\\Windows\\Fonts\\consola.ttf",             // Consolas - limited Unicode
                "C:\\Windows\\Fonts\\lucon.ttf",               // Lucida Console

                // SYSTEM: macOS fallbacks
                "/System/Library/Fonts/Monaco.ttf",            // macOS
                "/System/Library/Fonts/Menlo.ttf"              // macOS alternative
            };

            System.Console.WriteLine("=== Font Loading Debug ===");
            System.Console.WriteLine($"Trying {fontPaths.Length} font paths in priority order...");

            foreach (var fontPath in fontPaths)
            {
                System.Console.WriteLine($"  Checking: {fontPath}");

                if (System.IO.File.Exists(fontPath))
                {
                    System.Console.WriteLine($"    → File exists! Attempting to load...");
                    try
                    {
                        ConsoleFont = Raylib.LoadFontEx(fontPath, ConsoleFontSize, codepoints, codepoints.Length);

                        // CRITICAL: Set point filter (nearest neighbor) for crisp pixel-perfect rendering
                        // This must be done AFTER loading the font
                        Raylib.SetTextureFilter(ConsoleFont.Texture, TextureFilter.Point);

                        // Verify the texture filter was set
                        System.Console.WriteLine($"✓ Font loaded successfully: {System.IO.Path.GetFileName(fontPath)}");
                        System.Console.WriteLine($"  Full path: {fontPath}");
                        System.Console.WriteLine($"  Font size: {ConsoleFontSize}px");
                        System.Console.WriteLine($"  Texture ID: {ConsoleFont.Texture.Id}");
                        System.Console.WriteLine($"  Texture filter: Point (nearest neighbor) - CRISP MODE");
                        System.Console.WriteLine("==========================");

                        _fontLoaded = true;

                        // NEW: Load emoji font for terrain decorations
                        LoadEmojiFont();

                        // NEW: Load emoji sprites for modern rendering
                        LoadEmojiSprites();

                        return;
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"    ✗ Failed to load: {ex.Message}");
                    }
                }
                else
                {
                    System.Console.WriteLine($"    ✗ File not found");
                }
            }

                // Final fallback
                System.Console.WriteLine("⚠ Warning: No font with Unicode support found. Using default font.");
                ConsoleFont = Raylib.GetFontDefault();
                Raylib.SetTextureFilter(ConsoleFont.Texture, TextureFilter.Point);
                System.Console.WriteLine($"  Default font texture filter: Point (nearest neighbor)");
                System.Console.WriteLine($"  ⚠ Default font lacks emoji support - enabling ASCII-only mode for terrain");
                        VillageBuilder.Engine.World.TerrainDecoration.UseAsciiOnly = true;
                        _fontLoaded = true;
                    }

                /// <summary>
                /// Load emoji font for terrain decorations and UI icons
                /// </summary>
                private static void LoadEmojiFont()
                {
                    System.Console.WriteLine("");
                    System.Console.WriteLine("=== Emoji Font Loading ===");

                    // Define emoji font paths in priority order
                    string[] emojiFontPaths = new[]
                    {
                        // PRIORITY 1: System fonts (monochrome vector - Raylib compatible)
                        "assets/fonts/NotoColorEmoji-Regular.ttf",
                        "C:\\Windows\\Fonts\\seguiemj.ttf",  // Segoe UI Emoji (Windows) - BEST for Raylib
                        "/System/Library/Fonts/Apple Color Emoji.ttc",  // Apple Color Emoji (macOS)

                        // PRIORITY 2: Bundled fonts (may not work - color emoji format)
                        // NOTE: Noto Color Emoji uses PNG/bitmap format that Raylib cannot load
                        // Keeping for future compatibility if Raylib adds color emoji support
                        "/usr/share/fonts/truetype/noto/NotoColorEmoji.ttf"  // Noto (Linux)
                    };

                    // Emoji codepoints (terrain decorations + UI icons)
                    int[] emojiCodepoints = new int[]
                    {
                        // Terrain decorations
                        0x1F333, // 🌳 (deciduous tree)
                        0x1F332, // 🌲 (evergreen tree)
                        0x1F384, // 🎄 (Christmas tree)
                        0x1FAB5, // 🪵 (wood/log)
                        0x1F33A, // 🌺 (hibiscus)
                        0x1FAD0, // 🫐 (blueberries)
                        0x1F338, // 🌸 (cherry blossom)
                        0x1F33C, // 🌼 (blossom)
                        0x1F33E, // 🌾 (rice/grain)
                        0x1F33F, // 🌿 (herb)
                        0x1F344, // 🍄 (mushroom)
                        0x1F985, // 🦅 (eagle)
                        0x1F426, // 🐦 (bird)
                        0x1F99C, // 🦜 (parrot)
                        0x1F98B, // 🦋 (butterfly)
                        0x1F430, // 🐰 (rabbit face)
                        0x1F98C, // 🦌 (deer)
                        0x1F41F, // 🐟 (fish)

                        // UI icons
                        0x1F3E0, // 🏠 (house)
                        0x1F528, // 🔨 (hammer)
                        0x1F4A4, // 💤 (zzz - sleeping)
                    };

                    foreach (var emojiPath in emojiFontPaths)
                    {
                        System.Console.WriteLine($"  Checking: {emojiPath}");

                        if (System.IO.File.Exists(emojiPath))
                        {
                            System.Console.WriteLine($"    → File exists! Attempting to load...");
                            try
                            {
                                EmojiFont = Raylib.LoadFontEx(emojiPath, ConsoleFontSize, emojiCodepoints, emojiCodepoints.Length);

                                // Set texture filter for crisp rendering
                                Raylib.SetTextureFilter(EmojiFont.Texture, TextureFilter.Point);

                                // Verify the font actually loaded (check if it has valid glyphs)
                                // Also check if it has the specific emojis we need (not just any glyphs)
                                if (EmojiFont.GlyphCount > 0 && EmojiFont.Texture.Id > 2)
                                {
                                    System.Console.WriteLine($"✓ Emoji font loaded successfully: {System.IO.Path.GetFileName(emojiPath)}");
                                    System.Console.WriteLine($"  Texture ID: {EmojiFont.Texture.Id}");
                                    System.Console.WriteLine($"  Glyph count: {EmojiFont.GlyphCount}");

                                    // IMPORTANT: Segoe UI Emoji has limited glyph coverage
                                    // It only has basic emojis, not newer ones like 🌳🌲🌸🦋🐰
                                    // These newer emojis will render as ? even though the font loaded
                                    System.Console.WriteLine($"  ⚠ Note: Segoe UI Emoji has limited coverage - using ASCII mode");
                                    System.Console.WriteLine($"  ⚠ Newer emojis (🌳🌸🦋) not supported - they will show as ASCII symbols");
                                    System.Console.WriteLine("==========================");

                                    // Use ASCII-only mode because Segoe UI Emoji doesn't have terrain emojis
                                    VillageBuilder.Engine.World.TerrainDecoration.UseAsciiOnly = true;

                                    return;
                                }
                                else
                                {
                                    System.Console.WriteLine($"    ✗ Font loaded but has no valid glyphs (color emoji format not supported)");
                                    System.Console.WriteLine($"    Texture ID: {EmojiFont.Texture.Id}, Glyphs: {EmojiFont.GlyphCount}");
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Console.WriteLine($"    ✗ Failed to load: {ex.Message}");
                            }
                        }
                        else
                        {
                            System.Console.WriteLine($"    ✗ File not found");
                        }
                    }

                    // Fallback: No emoji font found, use ASCII mode
                    System.Console.WriteLine("⚠ No emoji font found - falling back to ASCII-only mode");
                    System.Console.WriteLine("  Terrain decorations will use ASCII symbols instead");
                    System.Console.WriteLine("==========================");

                    VillageBuilder.Engine.World.TerrainDecoration.UseAsciiOnly = true;

                        // Use primary font as fallback
                        EmojiFont = ConsoleFont;
                    }

                    /// <summary>
                    /// Load emoji sprites for modern terrain decoration rendering
                    /// </summary>
                    private static void LoadEmojiSprites()
                    {
                        try
                        {
                            SpriteAtlasManager.Instance.LoadSprites();

                            if (SpriteAtlasManager.Instance.SpriteModeEnabled)
                            {
                                // Sprites loaded successfully - enable sprite mode
                                UseSpriteMode = true;

                                // Disable ASCII-only mode since we have beautiful sprites!
                                VillageBuilder.Engine.World.TerrainDecoration.UseAsciiOnly = false;

                                System.Console.WriteLine("✓ Sprite mode enabled - terrain will use colorful emoji sprites!");
                            }
                            else
                            {
                                // No sprites available - stick with ASCII mode
                                UseSpriteMode = false;
                                VillageBuilder.Engine.World.TerrainDecoration.UseAsciiOnly = true;

                                System.Console.WriteLine("⚠ Sprite mode disabled - using ASCII fallback");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine($"✗ Failed to load emoji sprites: {ex.Message}");
                            UseSpriteMode = false;
                            VillageBuilder.Engine.World.TerrainDecoration.UseAsciiOnly = true;
                        }
                    }

                    public static void UnloadFont()
                    {
                        // Only unload if font is loaded and is not the default font
                        if (_fontLoaded && ConsoleFont.BaseSize != Raylib.GetFontDefault().BaseSize)
                        {
                            Raylib.UnloadFont(ConsoleFont);
                        }

                        // Unload emoji font if it's different from the primary font
                        if (EmojiFont.BaseSize != ConsoleFont.BaseSize && 
                            EmojiFont.BaseSize != Raylib.GetFontDefault().BaseSize)
                        {
                            Raylib.UnloadFont(EmojiFont);
                        }

                        // Unload emoji sprites
                        SpriteAtlasManager.Instance.UnloadSprites();
                    }

        // Colors (with alpha for effects)
        public static class Colors
        {
            // Terrain  
            public static Color Grass = new(34, 139, 34, 255);
            public static Color Forest = new(0, 100, 0, 255);
            public static Color Water = new(30, 144, 255, 255);
            public static Color Mountain = new(139, 137, 137, 255);
            public static Color Field = new(154, 205, 50, 255);
            public static Color Road = new(139, 69, 19, 255);

            // Buildings
            public static Color House = new(210, 105, 30, 255);
            public static Color Farm = new(255, 215, 0, 255);
            public static Color Warehouse = new(128, 128, 128, 255);

            // UI
            public static Color Background = new(20, 20, 30, 255);
            public static Color Panel = new(40, 40, 50, 255);
            public static Color Text = new(220, 220, 220, 255);
            public static Color Highlight = new(100, 150, 255, 255);
            
            // Log levels / Status colors
            public static Color White = new(255, 255, 255, 255);
            public static Color Yellow = new(255, 255, 0, 255);
            public static Color Red = new(255, 0, 0, 255);
            public static Color Green = new(0, 255, 0, 255);
        }

        public static void DrawConsoleText(string text, int x, int y, int fontSize, Color color)
        {
            // Round coordinates to whole pixels for crisp rendering
            var pixelX = (float)Math.Round((double)x);
            var pixelY = (float)Math.Round((double)y);

            Raylib.DrawTextEx(
                ConsoleFont,
                text,
                new System.Numerics.Vector2(pixelX, pixelY),
                fontSize,
                1.0f, // Minimal spacing to prevent overlap while maintaining sharpness
                color
            );
        }

        /// <summary>
        /// Draw text with automatic font selection (emoji font for emojis, console font for everything else)
        /// </summary>
        public static void DrawConsoleTextAuto(string text, int x, int y, int fontSize, Color color)
        {
            // Try sprite rendering first for single emoji characters
            if (text.Length <= 2 && IsEmoji(text))
            {
                // Attempt to draw as sprite
                if (DrawEmojiSprite(text, x, y, fontSize, color))
                {
                    return; // Success - sprite rendered
                }
                // Fall through to font rendering if sprite failed
            }

            // Detect if text contains emoji codepoints
            Font fontToUse = IsEmoji(text) ? EmojiFont : ConsoleFont;

            // Round coordinates to whole pixels for crisp rendering
            var pixelX = (float)Math.Round((double)x);
            var pixelY = (float)Math.Round((double)y);

            Raylib.DrawTextEx(
                fontToUse,
                text,
                new System.Numerics.Vector2(pixelX, pixelY),
                fontSize,
                1.0f,
                        color
                    );
                }

                /// <summary>
                /// Draw an emoji as a sprite texture (for UI icons)
                /// This provides better emoji rendering than font-based text
                /// </summary>
                /// <param name="emoji">The emoji string (e.g., "🏠", "🌾")</param>
                /// <param name="x">X position</param>
                /// <param name="y">Y position</param>
                /// <param name="size">Size in pixels</param>
                /// <param name="tint">Color tint (use White for no tint)</param>
                /// <returns>True if sprite was drawn, false if fallback to text needed</returns>
                public static bool DrawEmojiSprite(string emoji, int x, int y, int size, Color tint)
                {
                    // Only works in sprite mode
                    if (!UseSpriteMode)
                        return false;

                    // Map emoji to decoration type
                    var decorType = GetDecorationTypeForEmoji(emoji);
                    if (!decorType.HasValue)
                        return false;

                    // Get sprite texture
                    var sprite = SpriteAtlasManager.Instance.GetSprite(decorType.Value);
                    if (!sprite.HasValue)
                        return false;

                    // Render sprite centered in the given size
                    var sourceRect = new Rectangle(0, 0, sprite.Value.Width, sprite.Value.Height);
                    var destRect = new Rectangle(x, y, size, size);
                    var origin = new System.Numerics.Vector2(0, 0);

                    Raylib.DrawTexturePro(sprite.Value, sourceRect, destRect, origin, 0f, tint);
                    return true;
                }

                /// <summary>
                /// Map emoji characters to DecorationType for sprite rendering
                /// </summary>
                private static VillageBuilder.Engine.World.DecorationType? GetDecorationTypeForEmoji(string emoji)
                {
                    return emoji switch
                    {
                        // Buildings
                        "🏠" => VillageBuilder.Engine.World.DecorationType.TreeOak, // House (use oak as placeholder)
                        "🌾" => VillageBuilder.Engine.World.DecorationType.GrassTuft, // Farm
                        "📦" => VillageBuilder.Engine.World.DecorationType.RockBoulder, // Warehouse
                        "⛏️" => VillageBuilder.Engine.World.DecorationType.RockPebble, // Mine
                        "🪓" => VillageBuilder.Engine.World.DecorationType.StumpOld, // Lumberyard  
                        "🔨" => VillageBuilder.Engine.World.DecorationType.LogFallen, // Workshop
                        "🏪" => VillageBuilder.Engine.World.DecorationType.BushFlowering, // Market
                        "💧" => VillageBuilder.Engine.World.DecorationType.FishInWater, // Well
                        "🏛️" => VillageBuilder.Engine.World.DecorationType.TreeOak, // Town Hall

                        // People/Status
                        "👥" => VillageBuilder.Engine.World.DecorationType.TreeOak, // Families
                        "💾" => VillageBuilder.Engine.World.DecorationType.RockPebble, // Save

                        // Tasks
                        "💤" => VillageBuilder.Engine.World.DecorationType.FlowerWild, // Sleeping
                        "🚶" => VillageBuilder.Engine.World.DecorationType.GrassTuft, // Walking
                        "😌" => VillageBuilder.Engine.World.DecorationType.FlowerRare, // Resting
                        "🧍" => VillageBuilder.Engine.World.DecorationType.GrassTuft, // Idle
                        "🏗️" => VillageBuilder.Engine.World.DecorationType.LogFallen, // Constructing

                        // Log levels
                        "ℹ️" => VillageBuilder.Engine.World.DecorationType.BirdPerched, // Info
                        "⚠️" => VillageBuilder.Engine.World.DecorationType.FlowerWild, // Warning
                        "❌" => VillageBuilder.Engine.World.DecorationType.RockBoulder, // Error
                        "✅" => VillageBuilder.Engine.World.DecorationType.FlowerRare, // Success

                        _ => null
                    };
                }

                /// <summary>
                /// Check if a string contains emoji characters
                /// </summary>
                private static bool IsEmoji(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;

            // Check first character - emojis are in high Unicode ranges
            int codepoint = char.ConvertToUtf32(text, 0);

            // Emoji ranges:
            // 0x1F300-0x1F9FF: Misc Symbols and Pictographs, Emoticons, Transport, etc.
            // 0x2600-0x26FF: Misc symbols (some emojis)
            return codepoint >= 0x1F300 || (codepoint >= 0x2600 && codepoint <= 0x26FF);
        }

        // Add helper method for measuring text correctly
        public static int MeasureText(string text, int fontSize)
        {
            var measured = Raylib.MeasureTextEx(ConsoleFont, text, fontSize, 2.0f);
            return (int)measured.X;
        }
    }
}
