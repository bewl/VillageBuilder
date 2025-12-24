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

        // UI Layout
        public const int StatusBarHeight = 60; // Increased for 20px font with proper spacing
        public const int MapViewportWidth = 84; // 70% of console
        public const int SidebarWidth = 36; // 30% of console

        // Font settings
        public static Font ConsoleFont { get; private set; }
        public const int ConsoleFontSize = 20;
        public const int SmallConsoleFontSize = 18;
        private static bool _fontLoaded = false;

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

            Console.WriteLine($"GraphicsConfig: Initialized borderless fullscreen at ({monitorWidth}x{monitorHeight}) position (0, 0)");
        }

        public static void LoadFont()
        {
            if (_fontLoaded) return;

            // Load font with extended character support
            int[] codepoints = new int[512];
            
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

            Array.Resize(ref codepoints, idx);

            // Font priority order - UPDATED for full ASCII/Unicode support
            // Priority: Fonts that exist on user's system > Unicode coverage > Readability
            string[] fontPaths = new[]
            {
                // BEST: Cascadia fonts (check both Mono and Code variants)
                "C:\\Windows\\Fonts\\CascadiaCode.ttf",        // Cascadia Code - excellent Unicode
                "C:\\Windows\\Fonts\\CascadiaMono.ttf",        // Cascadia Mono - BEST for Unicode + readability
                "C:\\Windows\\Fonts\\Cascadia.ttf",            // Generic Cascadia

                // EXCELLENT: Open-source with full Unicode coverage
                "C:\\Windows\\Fonts\\DejaVuSansMono.ttf",      // DejaVu Sans Mono - excellent Unicode
                "/usr/share/fonts/truetype/dejavu/DejaVuSansMono.ttf",  // Linux

                // GOOD: Alternative modern fonts
                "C:\\Windows\\Fonts\\JetBrainsMono-Regular.ttf", // JetBrains Mono
                "C:\\Windows\\Fonts\\IBMPlexMono-Regular.ttf", // IBM Plex Mono

                // Bundled fonts (if provided with game)
                "assets/fonts/CascadiaCode.ttf",
                "assets/fonts/CascadiaMono.ttf",
                "assets/fonts/DejaVuSansMono.ttf",
                "assets/fonts/JetBrainsMono.ttf",
                "assets/fonts/IBMPlexMono.ttf",

                // FALLBACK: Standard Windows fonts (limited Unicode)
                "C:\\Windows\\Fonts\\consola.ttf",             // Consolas - limited Unicode (was loading before!)
                "C:\\Windows\\Fonts\\lucon.ttf",               // Lucida Console

                // RETRO: Authentic DOS fonts (good for aesthetic, limited support)
                "assets/fonts/Px437_IBM_VGA_8x16.ttf",         // Authentic DOS VGA font
                "assets/fonts/PerfectDOSVGA437.ttf",           // Another DOS font option

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
            _fontLoaded = true;
        }

        public static void UnloadFont()
        {
            // Fix: Font does not have a 'texture' property, so compare using BaseSize as a proxy for default font
            // Only unload if font is loaded and is not the default font
            if (_fontLoaded && ConsoleFont.BaseSize != Raylib.GetFontDefault().BaseSize)
            {
                Raylib.UnloadFont(ConsoleFont);
            }
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

        // Add helper method for measuring text correctly
        public static int MeasureText(string text, int fontSize)
        {
            var measured = Raylib.MeasureTextEx(ConsoleFont, text, fontSize, 2.0f);
            return (int)measured.X;
        }
    }
}
