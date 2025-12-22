using System;
using System.Collections.Generic;
using System.Linq;
using Raylib_cs;
using VillageBuilder.Engine.Core;
using VillageBuilder.Engine.Buildings;

namespace VillageBuilder.Game.Graphics.UI
{
    public class SidebarRenderer
    {
        private const int Padding = 10;
        private const int FontSize = 16;
        private const int SmallFontSize = 14;
        private const int LineHeight = 18;

        private int _sidebarX;
        private int _sidebarY;
        private int _sidebarWidth;
        private int _sidebarHeight;

        public void Render(GameEngine engine)
        {
            // Calculate sidebar position (right side of screen)
            _sidebarX = (int)(GraphicsConfig.ScreenWidth * 0.7f);
            _sidebarY = GraphicsConfig.StatusBarHeight;
            _sidebarWidth = GraphicsConfig.ScreenWidth - _sidebarX;
            _sidebarHeight = GraphicsConfig.ScreenHeight - _sidebarY;

            // Draw sidebar background (darker terminal-like)
            Raylib.DrawRectangle(_sidebarX, _sidebarY, _sidebarWidth, _sidebarHeight, new Color(15, 15, 20, 255));
            
            // Draw border using box-drawing characters
            DrawBorder(_sidebarX, _sidebarY, _sidebarWidth, _sidebarHeight);

            var currentY = _sidebarY + Padding;

            // Section 1: Quick Stats
            currentY = RenderQuickStats(engine, currentY);

            // Section 2: Commands/Buildings
            currentY = RenderCommands(engine, currentY);

            // Section 3: Event Log
            RenderEventLog(currentY);
        }

        private void DrawBorder(int x, int y, int width, int height)
        {
            var borderColor = new Color(100, 100, 120, 255);
            
            // Vertical line
            for (int i = 0; i < height; i += FontSize)
            {
                GraphicsConfig.DrawConsoleText("│", x, y + i, FontSize, borderColor);
            }
        }

        private void DrawSectionHeader(string title, int y)
        {
            var headerColor = new Color(120, 200, 255, 255);
            
            // Draw title with box-drawing decoration
            GraphicsConfig.DrawConsoleText($"┌─ {title} ", _sidebarX + Padding, y, FontSize, headerColor);
            
            // Draw horizontal line
            int lineX = _sidebarX + Padding + Raylib.MeasureText($"┌─ {title} ", FontSize);
            int remainingWidth = _sidebarWidth - Padding * 2 - (lineX - _sidebarX - Padding);
            string line = new string('─', Math.Max(0, remainingWidth / 8));
            GraphicsConfig.DrawConsoleText(line, lineX, y, FontSize, headerColor);
        }

        private int RenderQuickStats(GameEngine engine, int startY)
        {
            var y = startY;
            var textColor = new Color(200, 200, 200, 255);
            var dimColor = new Color(120, 120, 120, 255);

            DrawSectionHeader("QUICK STATS", y);
            y += LineHeight + 5;

            // Families
            if (engine.Families.Any())
            {
                var totalPopulation = engine.Families.Sum(f => f.Members.Count);
                GraphicsConfig.DrawConsoleText($"│ ☺ Families:    {engine.Families.Count}", _sidebarX + Padding, y, SmallFontSize, textColor);
                y += LineHeight;
                
                GraphicsConfig.DrawConsoleText($"│   Population:  {totalPopulation}", _sidebarX + Padding, y, SmallFontSize, textColor);
                y += LineHeight;
            }
            else
            {
                GraphicsConfig.DrawConsoleText("│ ☺ No families yet", _sidebarX + Padding, y, SmallFontSize, dimColor);
                y += LineHeight;
            }

            y += 5;

            // Buildings summary
            var constructedCount = engine.Buildings.Count(b => b.IsConstructed);
            GraphicsConfig.DrawConsoleText($"│ ▓ Buildings:   {constructedCount}/{engine.Buildings.Count}", 
                _sidebarX + Padding, y, SmallFontSize, textColor);
            y += LineHeight + 5;

            var buildingGroups = engine.Buildings.GroupBy(b => b.Type).Take(5);
            foreach (var group in buildingGroups)
            {
                var constructed = group.Count(b => b.IsConstructed);
                var icon = GetBuildingAsciiIcon(group.Key);
                GraphicsConfig.DrawConsoleText($"│   {icon} {group.Key,-12} {constructed}", 
                    _sidebarX + Padding, y, SmallFontSize, textColor);
                y += LineHeight;
            }

            // Draw section separator
            GraphicsConfig.DrawConsoleText("└" + new string('─', (_sidebarWidth - Padding * 2 - 8) / 8), 
                _sidebarX + Padding, y, FontSize, new Color(100, 100, 120, 255));
            y += LineHeight;

            return y + 5;
        }

        private int RenderCommands(GameEngine engine, int startY)
        {
            var y = startY;
            var textColor = new Color(200, 200, 200, 255);

            DrawSectionHeader("COMMANDS", y);
            y += LineHeight + 5;

            // Game controls with ASCII styling
            var controls = new[]
            {
                ("SPACE", "Pause/Resume", new Color(100, 255, 100, 255)),
                ("+", "Speed Up", new Color(255, 255, 100, 255)),
                ("-", "Slow Down", new Color(255, 255, 100, 255)),
                ("", "", Color.White), // Spacer
                ("H", "Build House", new Color(255, 180, 100, 255)),
                ("F", "Build Farm", new Color(255, 180, 100, 255)),
                ("W", "Build Warehouse", new Color(255, 180, 100, 255)),
                ("L", "Build Lumberyard", new Color(255, 180, 100, 255)),
                ("M", "Build Mine", new Color(255, 180, 100, 255)),
                ("K", "Build Workshop", new Color(255, 180, 100, 255)),
                ("T", "Build Town Hall", new Color(255, 180, 100, 255)),
                ("", "", Color.White), // Spacer
                ("R", "Rotate Building", new Color(150, 200, 255, 255)),
                ("TAB", "Toggle Road Snap", new Color(150, 200, 255, 255)),
                ("ESC", "Cancel Placement", new Color(255, 150, 150, 255)),
                ("", "", Color.White), // Spacer
                ("Q", "Quit Game", new Color(255, 100, 100, 255))
            };

            foreach (var (key, action, color) in controls)
            {
                if (string.IsNullOrEmpty(key))
                {
                    y += 5; // Spacer
                    continue;
                }

                // Draw command in roguelike style
                GraphicsConfig.DrawConsoleText($"│ [{key,-5}]", _sidebarX + Padding, y, SmallFontSize, color);
                GraphicsConfig.DrawConsoleText(action, _sidebarX + Padding + 80, y, SmallFontSize, textColor);

                y += LineHeight;
            }

            // Draw section separator
            GraphicsConfig.DrawConsoleText("└" + new string('─', (_sidebarWidth - Padding * 2 - 8) / 8), 
                _sidebarX + Padding, y, FontSize, new Color(100, 100, 120, 255));
            y += LineHeight;

            return y + 5;
        }

        private void RenderEventLog(int startY)
        {
            var y = startY;

            DrawSectionHeader("EVENT LOG", y);
            y += LineHeight + 5;

            var logHeight = _sidebarHeight - (y - _sidebarY) - Padding;

            // Draw log background with border
            Raylib.DrawRectangle(_sidebarX + Padding + 8, y, _sidebarWidth - Padding * 2 - 8, logHeight, 
                new Color(10, 10, 15, 255));

            // Render actual log entries (most recent at bottom)
            var entries = EventLog.Instance.Entries;
            var visibleCount = Math.Min(entries.Count, (logHeight - 10) / 15);
            var startIndex = Math.Max(0, entries.Count - visibleCount);

            var messageY = y + 5;
            for (int i = startIndex; i < entries.Count; i++)
            {
                var entry = entries[i];
                var time = entry.Timestamp.ToString("HH:mm");
                var color = GetLogColor(entry.Level);
                var prefix = GetLogPrefix(entry.Level);
                
                // Draw timestamp
                GraphicsConfig.DrawConsoleText($"│{time}", _sidebarX + Padding, messageY, 11, new Color(100, 100, 100, 255));
                
                // Draw message with level indicator
                var messageText = $"{prefix} {entry.Message}";
                if (Raylib.MeasureText(messageText, 11) > _sidebarWidth - Padding * 2 - 50)
                {
                    messageText = messageText.Substring(0, Math.Max(0, (_sidebarWidth - Padding * 2 - 80) / 6)) + "...";
                }
                GraphicsConfig.DrawConsoleText(messageText, _sidebarX + Padding + 50, messageY, 11, color);
                messageY += 15;
            }

            // Draw bottom border
            var bottomY = y + logHeight;
            GraphicsConfig.DrawConsoleText("└" + new string('─', (_sidebarWidth - Padding * 2 - 8) / 8), 
                _sidebarX + Padding, bottomY, FontSize, new Color(100, 100, 120, 255));
        }

        private string GetBuildingAsciiIcon(BuildingType type)
        {
            return type switch
            {
                BuildingType.House => "█",
                BuildingType.Farm => "♠",
                BuildingType.Warehouse => "■",
                BuildingType.Mine => "╬",
                BuildingType.Lumberyard => "╪",
                BuildingType.Workshop => "╫",
                BuildingType.Market => "╪",
                BuildingType.Well => "○",
                BuildingType.TownHall => "♦",
                _ => "?"
            };
        }

        private string GetLogPrefix(LogLevel level)
        {
            return level switch
            {
                LogLevel.Info => "·",
                LogLevel.Warning => "!",
                LogLevel.Error => "×",
                LogLevel.Success => "✓",
                _ => "·"
            };
        }

        private Color GetLogColor(LogLevel level)
        {
            return level switch
            {
                LogLevel.Info => new Color(180, 180, 180, 255),
                LogLevel.Warning => new Color(255, 200, 100, 255),
                LogLevel.Error => new Color(255, 100, 100, 255),
                LogLevel.Success => new Color(100, 255, 100, 255),
                _ => Color.White
            };
        }
    }
}
