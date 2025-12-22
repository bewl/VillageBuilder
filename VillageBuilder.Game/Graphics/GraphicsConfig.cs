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
        public const int StatusBarHeight = 48; // 3 rows of tiles
        public const int MapViewportWidth = 84; // 70% of console
        public const int SidebarWidth = 36; // 30% of console

        // Font settings
        public static Font ConsoleFont { get; private set; }
        public const int ConsoleFontSize = 16;
        public const int SmallConsoleFontSize = 14;
        private static bool _fontLoaded = false;

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
            string[] fontPaths = new[]
            {
                "assets/fonts/DejaVuSansMono.ttf",
                "assets/fonts/CascadiaCode.ttf",
                "assets/fonts/CascadiaMono.ttf",
                "C:\\Windows\\Fonts\\consola.ttf",      // Windows Consolas
                "C:\\Windows\\Fonts\\DejaVuSansMono.ttf"
            };

            foreach (var fontPath in fontPaths)
            {
                if (System.IO.File.Exists(fontPath))
                {
                    try
                    {
                        ConsoleFont = Raylib.LoadFontEx(fontPath, ConsoleFontSize, codepoints, codepoints.Length);
                        _fontLoaded = true;
                        System.Console.WriteLine($"✓ Font loaded: {System.IO.Path.GetFileName(fontPath)}");
                        return;
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"✗ Failed to load {fontPath}: {ex.Message}");
                    }
                }
            }

            // Final fallback
            System.Console.WriteLine("⚠ Warning: No font with Unicode support found. Symbols may not display correctly.");
            ConsoleFont = Raylib.GetFontDefault();
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
            Raylib.DrawTextEx(
                ConsoleFont,
                text,
                new System.Numerics.Vector2(x, y),
                fontSize,
                2.0f, // Increased spacing for better readability
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
