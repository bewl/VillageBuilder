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
        // Window settings
        public const int ScreenWidth = 1920;
        public const int ScreenHeight = 1080;
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
            // Query primary monitor size with fallback
            int monitorWidth = Raylib.GetMonitorWidth(0);
            int monitorHeight = Raylib.GetMonitorHeight(0);
            
            // Fallback to common resolution if monitor query fails
            if (monitorWidth == 0 || monitorHeight == 0)
            {
                monitorWidth = 1920;
                monitorHeight = 1080;
                Console.WriteLine("WARNING: Monitor detection failed, using fallback 1920x1080");
            }

            // Disable MSAA and enable flags for pixel-perfect rendering
            Raylib.SetConfigFlags(ConfigFlags.UndecoratedWindow | ConfigFlags.VSyncHint);
            
            // Initialize window at monitor resolution
            Raylib.InitWindow(monitorWidth, monitorHeight, WindowTitle);
            
            // Set window position to top-left corner (0, 0) to cover taskbar
            Raylib.SetWindowPosition(0, 0);
            
            // Disable ESC key as exit key (we handle ESC ourselves for UI navigation)
            Raylib.SetExitKey(0);
            
            Raylib.SetTargetFPS(TargetFPS);
            
            Console.WriteLine($"GraphicsConfig: Initialized borderless windowed mode ({monitorWidth}x{monitorHeight}) at position (0, 0)");
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
            
            Array.Resize(ref codepoints, idx);

            // Try to load bundled fonts first, then fallback to system fonts
            // Prioritize fonts with good weight and clarity at medium sizes
            string[] fontPaths = new[]
            {
                "C:\\Windows\\Fonts\\consola.ttf",         // Consolas - good weight and readability
                "C:\\Windows\\Fonts\\lucon.ttf",           // Lucida Console - thick and clear
                "C:\\Windows\\Fonts\\cour.ttf",            // Courier New - too thin
                "assets/fonts/Px437_IBM_VGA_8x16.ttf",     // Authentic DOS VGA font
                "assets/fonts/PerfectDOSVGA437.ttf",       // Another DOS font option
                "assets/fonts/CascadiaMono.ttf",
                "assets/fonts/CascadiaCode.ttf",
                "assets/fonts/DejaVuSansMono.ttf",
                "/usr/share/fonts/truetype/dejavu/DejaVuSansMono.ttf",  // Linux
                "/System/Library/Fonts/Monaco.ttf"          // macOS
            };

            foreach (var fontPath in fontPaths)
            {
                if (System.IO.File.Exists(fontPath))
                {
                    try
                    {
                        ConsoleFont = Raylib.LoadFontEx(fontPath, ConsoleFontSize, codepoints, codepoints.Length);
                        
                        // CRITICAL: Set point filter (nearest neighbor) for crisp pixel-perfect rendering
                        // This must be done AFTER loading the font
                        Raylib.SetTextureFilter(ConsoleFont.Texture, TextureFilter.Point);
                        
                        // Verify the texture filter was set
                        System.Console.WriteLine($"✓ Font loaded: {System.IO.Path.GetFileName(fontPath)}");
                        System.Console.WriteLine($"  Font size: {ConsoleFontSize}px");
                        System.Console.WriteLine($"  Texture ID: {ConsoleFont.Texture.Id}");
                        System.Console.WriteLine($"  Texture filter: Point (nearest neighbor) - CRISP MODE");
                        
                        _fontLoaded = true;
                        return;
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"✗ Failed to load {fontPath}: {ex.Message}");
                    }
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
