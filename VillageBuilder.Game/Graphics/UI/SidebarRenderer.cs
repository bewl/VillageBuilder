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

        public void Render(GameEngine engine, VillageBuilder.Game.Core.SelectionManager? selectionManager = null)
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

            // Show context-aware content based on selection
            if (selectionManager != null && selectionManager.HasSelection())
            {
                if (selectionManager.SelectedPerson != null)
                {
                    // Show person info
                    currentY = RenderPersonInfo(engine, selectionManager.SelectedPerson, currentY, selectionManager);
                }
                else if (selectionManager.SelectedBuilding != null)
                {
                    // Show building info with family assignment
                    currentY = RenderBuildingInfo(engine, selectionManager.SelectedBuilding, currentY);
                }
            }
            else
            {
                // Default view - show quick stats and map commands
                currentY = RenderQuickStats(engine, currentY);
                currentY = RenderCommands(engine, currentY);
            }

            // Event Log is always shown at bottom
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

            y += 5;

            // Save/Load status
            var saveFiles = VillageBuilder.Game.Core.SaveLoadService.GetSaveFiles();
            var quicksaves = saveFiles.Where(s => s.StartsWith("quicksave_")).OrderByDescending(s => s).ToArray();
            if (quicksaves.Any())
            {
                var latestSave = quicksaves.First();
                var saveInfo = VillageBuilder.Game.Core.SaveLoadService.GetSaveInfo(latestSave);
                if (saveInfo != null)
                {
                    var timeSince = DateTime.Now - saveInfo.SavedAt;
                    string timeText = timeSince.TotalMinutes < 1 
                        ? "< 1 min ago" 
                        : $"{(int)timeSince.TotalMinutes} min ago";

                    GraphicsConfig.DrawConsoleText($"│ ⚡ Last Save:  {timeText}", 
                        _sidebarX + Padding, y, SmallFontSize, new Color(255, 200, 50, 255));
                    y += LineHeight;
                }
            }
            else
            {
                GraphicsConfig.DrawConsoleText("│ ⚡ No saves yet (F5)", 
                    _sidebarX + Padding, y, SmallFontSize, dimColor);
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
                ("+", "Speed Up (max 16x)", new Color(255, 255, 100, 255)),
                ("-", "Slow Down", new Color(255, 255, 100, 255)),
                ("0", "Reset to 1x", new Color(255, 255, 100, 255)),
                ("", "", Color.White), // Spacer
                ("F5", "Quick Save", new Color(255, 200, 50, 255)),
                ("F9", "Quick Load", new Color(255, 200, 50, 255)),
                ("", "", Color.White), // Spacer
                ("H", "Build House", new Color(255, 180, 100, 255)),
                ("F", "Build Farm", new Color(255, 180, 100, 255)),
                ("W", "Build Warehouse", new Color(255, 180, 100, 255)),
                ("L", "Build Lumberyard", new Color(255, 180, 100, 255)),
                ("M", "Build Mine", new Color(255, 180, 100, 255)),
                ("K", "Build Workshop", new Color(255, 180, 100, 255)),
                ("E", "Build Well", new Color(255, 180, 100, 255)),
                ("T", "Build Town Hall", new Color(255, 180, 100, 255)),
                ("", "", Color.White), // Spacer
                ("R", "Rotate Building", new Color(150, 200, 255, 255)),
                ("TAB", "Toggle Road Snap", new Color(150, 200, 255, 255)),
                ("ESC", "Cancel Placement", new Color(255, 150, 150, 255)),
                ("", "", Color.White), // Spacer
                ("V", "Toggle Heat Map", new Color(100, 200, 255, 255)),
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
        
        private int RenderPersonInfo(GameEngine engine, VillageBuilder.Engine.Entities.Person person, int startY, VillageBuilder.Game.Core.SelectionManager? selectionManager = null)
        {
            var y = startY;
            var textColor = new Color(200, 200, 200, 255);

            DrawSectionHeader("PERSON INFO", y);
            y += LineHeight + 5;

            // Show visual list of people on tile if multiple people present
            if (selectionManager != null && selectionManager.HasMultiplePeople())
            {
                var headerText = $"│ People on this tile ({selectionManager.PeopleAtSelectedTile!.Count}):";
                GraphicsConfig.DrawConsoleText(headerText, _sidebarX + Padding, y, SmallFontSize, new Color(150, 150, 255, 255));
                y += LineHeight;

                // Render each person in the list with selection indicator
                for (int i = 0; i < selectionManager.PeopleAtSelectedTile.Count; i++)
                {
                    var p = selectionManager.PeopleAtSelectedTile[i];
                    bool isSelected = i == selectionManager.SelectedPersonIndex;

                    // Selection indicator (arrow or bullet)
                    string indicator = isSelected ? "│ ► " : "│   ";
                    var nameColor = isSelected ? new Color(255, 255, 100, 255) : new Color(180, 180, 180, 255);

                    // Show name with task indicator
                    string taskIcon = GetTaskIcon(p.CurrentTask);
                    string displayText = $"{indicator}{taskIcon} {p.FirstName} {p.LastName}";

                    GraphicsConfig.DrawConsoleText(displayText, _sidebarX + Padding, y, SmallFontSize - 2, nameColor);
                    y += LineHeight - 2;
                }

                y += 5;
                GraphicsConfig.DrawConsoleText("│ [Click name] or [Arrows/Tab] to switch", _sidebarX + Padding, y, SmallFontSize - 2, new Color(120, 120, 150, 255));
                y += LineHeight + 5;
            }

            // Person details
            GraphicsConfig.DrawConsoleText($"│ {person.FullName}", _sidebarX + Padding, y, SmallFontSize, new Color(255, 255, 100, 255));
            y += LineHeight;
            GraphicsConfig.DrawConsoleText($"│ Age: {person.Age} | {person.Gender}", _sidebarX + Padding, y, SmallFontSize, textColor);
            y += LineHeight;
            
            if (person.Family != null)
            {
                GraphicsConfig.DrawConsoleText($"│ Family: {person.Family.FamilyName}", _sidebarX + Padding, y, SmallFontSize, textColor);
                y += LineHeight;
            }
            
            // Show current task/status
            var taskText = GetPersonTaskText(person);
            var taskColor = GetPersonTaskColor(person);
            GraphicsConfig.DrawConsoleText($"│ Status: {taskText}", _sidebarX + Padding, y, SmallFontSize, taskColor);
            y += LineHeight;
            
            GraphicsConfig.DrawConsoleText($"│ Energy: {person.Energy}/100", _sidebarX + Padding, y, SmallFontSize, textColor);
            y += LineHeight;
            GraphicsConfig.DrawConsoleText($"│ Hunger: {person.Hunger}/100", _sidebarX + Padding, y, SmallFontSize, textColor);
            y += LineHeight;
            
            if (person.AssignedBuilding != null)
            {
                GraphicsConfig.DrawConsoleText($"│ Workplace: {person.AssignedBuilding.Type}", _sidebarX + Padding, y, SmallFontSize, new Color(150, 255, 150, 255));
                y += LineHeight;
            }
            
            if (person.HomeBuilding != null)
            {
                GraphicsConfig.DrawConsoleText($"│ Home: House at ({person.HomeBuilding.X}, {person.HomeBuilding.Y})", _sidebarX + Padding, y, SmallFontSize, new Color(255, 200, 150, 255));
                y += LineHeight;
            }
            
            y += 10;

            // Commands
            DrawSectionHeader("COMMANDS", y);
            y += LineHeight + 5;
            
            GraphicsConfig.DrawConsoleText("│ [ESC  ] Back to Map", _sidebarX + Padding, y, SmallFontSize, new Color(150, 200, 255, 255));
            y += LineHeight;
            
            if (selectionManager != null && selectionManager.HasMultiplePeople())
            {
                GraphicsConfig.DrawConsoleText("│ [Arrows] Cycle People", _sidebarX + Padding, y, SmallFontSize, new Color(150, 200, 255, 255));
                y += LineHeight;
            }
            
            GraphicsConfig.DrawConsoleText("│", _sidebarX + Padding, y, SmallFontSize, textColor);
            y += LineHeight;
            
            // Show different message based on person's current job status
            if (person.AssignedBuilding != null)
            {
                GraphicsConfig.DrawConsoleText("│ Click building to change job", _sidebarX + Padding, y, SmallFontSize, new Color(120, 120, 120, 255));
            }
            else
            {
                GraphicsConfig.DrawConsoleText("│ Click building to assign job", _sidebarX + Padding, y, SmallFontSize, new Color(120, 120, 120, 255));
            }
            y += LineHeight;

            // Draw section separator
            GraphicsConfig.DrawConsoleText("└" + new string('─', (_sidebarWidth - Padding * 2 - 8) / 8), 
                _sidebarX + Padding, y, FontSize, new Color(100, 100, 120, 255));
            y += LineHeight;

            return y + 5;
        }
        
        private string GetPersonTaskText(VillageBuilder.Engine.Entities.Person person)
        {
            return person.CurrentTask switch
            {
                VillageBuilder.Engine.Entities.PersonTask.Sleeping => "Sleeping",
                VillageBuilder.Engine.Entities.PersonTask.GoingHome => "Going Home",
                VillageBuilder.Engine.Entities.PersonTask.GoingToWork => "Going to Work",
                VillageBuilder.Engine.Entities.PersonTask.WorkingAtBuilding => "Working",
                VillageBuilder.Engine.Entities.PersonTask.Resting => "Resting",
                VillageBuilder.Engine.Entities.PersonTask.MovingToLocation => "Traveling",
                VillageBuilder.Engine.Entities.PersonTask.Idle => "Idle",
                _ => "Unknown"
            };
        }
        
        private Color GetPersonTaskColor(VillageBuilder.Engine.Entities.Person person)
        {
            return person.CurrentTask switch
            {
                VillageBuilder.Engine.Entities.PersonTask.Sleeping => new Color(150, 150, 255, 255),
                VillageBuilder.Engine.Entities.PersonTask.GoingHome => new Color(200, 200, 150, 255),
                VillageBuilder.Engine.Entities.PersonTask.GoingToWork => new Color(200, 200, 150, 255),
                VillageBuilder.Engine.Entities.PersonTask.WorkingAtBuilding => new Color(150, 255, 150, 255),
                VillageBuilder.Engine.Entities.PersonTask.Resting => new Color(200, 200, 200, 255),
                VillageBuilder.Engine.Entities.PersonTask.MovingToLocation => new Color(255, 200, 100, 255),
                VillageBuilder.Engine.Entities.PersonTask.Idle => new Color(200, 200, 200, 255),
                _ => Color.Gray
            };
        }

        private string GetTaskIcon(VillageBuilder.Engine.Entities.PersonTask task)
        {
            return task switch
            {
                VillageBuilder.Engine.Entities.PersonTask.Sleeping => "💤",
                VillageBuilder.Engine.Entities.PersonTask.GoingHome => "🏠",
                VillageBuilder.Engine.Entities.PersonTask.GoingToWork => "➜",
                VillageBuilder.Engine.Entities.PersonTask.WorkingAtBuilding => "⚒",
                VillageBuilder.Engine.Entities.PersonTask.Constructing => "🔨",
                VillageBuilder.Engine.Entities.PersonTask.Resting => "☺",
                VillageBuilder.Engine.Entities.PersonTask.MovingToLocation => "↔",
                VillageBuilder.Engine.Entities.PersonTask.Idle => "○",
                _ => "·"
            };
        }
        
        private int RenderBuildingInfo(GameEngine engine, Building building, int startY)
        {
            var y = startY;
            var textColor = new Color(200, 200, 200, 255);
            var mousePos = Raylib.GetMousePosition();
            bool mouseClicked = Raylib.IsMouseButtonPressed(MouseButton.Left);

            DrawSectionHeader("BUILDING INFO", y);
            y += LineHeight + 5;

            // Building details
            GraphicsConfig.DrawConsoleText($"│ {building.Name}", _sidebarX + Padding, y, SmallFontSize, new Color(255, 200, 100, 255));
            y += LineHeight;
            GraphicsConfig.DrawConsoleText($"│ Type: {building.Type}", _sidebarX + Padding, y, SmallFontSize, textColor);
            y += LineHeight;

            // Construction status
            if (!building.IsConstructed)
            {
                var stage = building.GetConstructionStage();
                var progressPercent = building.GetConstructionProgressPercent();
                var stageColor = new Color(255, 200, 100, 255);

                GraphicsConfig.DrawConsoleText($"│ Status: Under Construction", _sidebarX + Padding, y, SmallFontSize, new Color(255, 150, 50, 255));
                y += LineHeight;

                GraphicsConfig.DrawConsoleText($"│ Stage: {stage}", _sidebarX + Padding, y, SmallFontSize, stageColor);
                y += LineHeight;

                // Progress bar
                GraphicsConfig.DrawConsoleText($"│ Progress: {progressPercent}%", _sidebarX + Padding, y, SmallFontSize, textColor);
                y += LineHeight;

                // Draw visual progress bar
                int barWidth = 150;
                int barHeight = 12;
                int barX = _sidebarX + Padding + 15;
                int barY = y;

                // Background
                Raylib.DrawRectangle(barX, barY, barWidth, barHeight, new Color(50, 50, 50, 255));

                // Fill
                int fillWidth = (int)(barWidth * (progressPercent / 100f));
                Color fillColor = progressPercent switch
                {
                    < 25 => new Color(139, 90, 43, 255),
                    < 50 => new Color(160, 120, 80, 255),
                    < 75 => new Color(180, 140, 100, 255),
                    _ => new Color(100, 255, 100, 255)
                };
                Raylib.DrawRectangle(barX, barY, fillWidth, barHeight, fillColor);

                // Border
                Raylib.DrawRectangleLines(barX, barY, barWidth, barHeight, new Color(150, 150, 150, 255));

                y += barHeight + 5;

                // Construction workers
                GraphicsConfig.DrawConsoleText($"│ Builders: {building.ConstructionWorkers.Count}", _sidebarX + Padding, y, SmallFontSize, new Color(150, 255, 150, 255));
                y += LineHeight;

                if (building.ConstructionWorkers.Count > 0)
                {
                    var workersByFamily = building.ConstructionWorkers.GroupBy(w => w.Family).Where(g => g.Key != null);
                    foreach (var familyGroup in workersByFamily)
                    {
                        var family = familyGroup.Key!;
                        GraphicsConfig.DrawConsoleText($"│   • {family.FamilyName} ({familyGroup.Count()})", 
                            _sidebarX + Padding + 10, y, SmallFontSize - 2, textColor);
                        y += LineHeight;
                    }
                }
                else
                {
                    GraphicsConfig.DrawConsoleText($"│   (No builders assigned)", 
                        _sidebarX + Padding + 10, y, SmallFontSize - 2, new Color(255, 150, 50, 255));
                    y += LineHeight;
                }
            }
            else
            {
                GraphicsConfig.DrawConsoleText($"│ Status: Operational", 
                    _sidebarX + Padding, y, SmallFontSize, new Color(100, 255, 100, 255));
                y += LineHeight;
            }

            y += 5;
            
            // Show workers or residents depending on building type
            if (building.Type == BuildingType.House)
            {
                // Show residents for houses
                GraphicsConfig.DrawConsoleText($"│ Residents: {building.Residents.Count}", _sidebarX + Padding, y, SmallFontSize, new Color(150, 255, 150, 255));
                y += LineHeight;
                
                if (building.Residents.Count > 0)
                {
                    var residentsByFamily = building.Residents.Where(r => r.IsAlive).GroupBy(r => r.Family).Where(g => g.Key != null);
                    foreach (var familyGroup in residentsByFamily)
                    {
                        var family = familyGroup.Key!;
                        GraphicsConfig.DrawConsoleText($"│   • {family.FamilyName} ({familyGroup.Count()})", 
                            _sidebarX + Padding + 10, y, SmallFontSize - 2, textColor);
                        y += LineHeight;
                    }
                }
            }
            else
            {
                // Show workers for other buildings
                GraphicsConfig.DrawConsoleText($"│ Workers: {building.Workers.Count}", _sidebarX + Padding, y, SmallFontSize, new Color(150, 255, 150, 255));
                y += LineHeight;
                
                if (building.Workers.Count > 0)
                {
                    var workersByFamily = building.Workers.GroupBy(w => w.Family).Where(g => g.Key != null);
                    foreach (var familyGroup in workersByFamily)
                    {
                        var family = familyGroup.Key!;
                        GraphicsConfig.DrawConsoleText($"│   • {family.FamilyName} ({familyGroup.Count()})", 
                            _sidebarX + Padding + 10, y, SmallFontSize - 2, textColor);
                        y += LineHeight;
                    }
                }
            }
            
            y += 10;

            // Assign families section
            bool isHouse = building.Type == BuildingType.House;
            DrawSectionHeader(isHouse ? "ASSIGN RESIDENTS" : "ASSIGN WORKERS", y);
            y += LineHeight + 5;
            
            // Show legend for work buildings
            if (!isHouse)
            {
                GraphicsConfig.DrawConsoleText("│ Legend:", _sidebarX + Padding, y, SmallFontSize - 2, new Color(150, 150, 150, 255));
                y += LineHeight - 2;
                GraphicsConfig.DrawConsoleText("│ Green = Working here", _sidebarX + Padding + 10, y, SmallFontSize - 3, new Color(150, 255, 150, 255));
                y += LineHeight - 3;
                GraphicsConfig.DrawConsoleText("│ Yellow = Working elsewhere", _sidebarX + Padding + 10, y, SmallFontSize - 3, new Color(200, 200, 100, 255));
                y += LineHeight - 3;
                GraphicsConfig.DrawConsoleText("│ White = Available", _sidebarX + Padding + 10, y, SmallFontSize - 3, new Color(200, 200, 200, 255));
                y += LineHeight;
            }
            
            // Family assignment buttons
            foreach (var family in engine.Families.Where(f => f.GetAdults().Any(p => p.IsAlive)))
            {
                var buttonX = _sidebarX + Padding;
                var buttonY = y;
                var buttonWidth = _sidebarWidth - Padding * 2;
                var buttonHeight = LineHeight + 4;

                bool isHovered = mousePos.X >= buttonX && mousePos.X <= buttonX + buttonWidth &&
                                mousePos.Y >= buttonY && mousePos.Y <= buttonY + buttonHeight;

                var familyWorkersHere = building.Workers.Count(w => w.Family?.Id == family.Id);
                var isAssignedHere = familyWorkersHere > 0;
                
                // Check if family is working at another building
                var workingAdults = family.GetAdults().Where(p => p.IsAlive && p.AssignedBuilding != null).ToList();
                var workingElsewhere = workingAdults.Where(p => p.AssignedBuilding?.Id != building.Id).ToList();
                var otherWorkplace = workingElsewhere.FirstOrDefault()?.AssignedBuilding;

                // Draw button background
                Color buttonColor;
                if (isAssignedHere)
                {
                    buttonColor = new Color(40, 70, 40, 255); // Green - working here
                }
                else if (workingElsewhere.Any())
                {
                    buttonColor = new Color(60, 60, 30, 255); // Dark yellow - working elsewhere
                }
                else if (isHovered)
                {
                    buttonColor = new Color(60, 80, 120, 255); // Bright - hovering
                }
                else
                {
                    buttonColor = new Color(30, 40, 60, 255); // Dark - available
                }
                
                Raylib.DrawRectangle(buttonX, buttonY, buttonWidth, buttonHeight, buttonColor);
                
                if (isHovered)
                {
                    Raylib.DrawRectangleLines(buttonX, buttonY, buttonWidth, buttonHeight, new Color(150, 200, 255, 255));
                }

                // Button text
                var adultCount = family.GetAdults().Count(p => p.IsAlive);
                var availableCount = family.GetAdults().Count(p => p.IsAlive && p.AssignedBuilding == null);
                
                string buttonText;
                Color textColorToUse;
                
                if (isHouse)
                {
                    // House assignment logic
                    var familyHomesHere = family.Members.Any(m => m.HomeBuilding?.Id == building.Id);
                    buttonText = familyHomesHere ? 
                        $"│ {family.FamilyName} (Living here)" : 
                        $"│ {family.FamilyName} ({adultCount} adults)";
                    textColorToUse = familyHomesHere ? new Color(150, 255, 150, 255) : textColor;
                }
                else
                {
                    // Work building assignment logic
                    if (isAssignedHere)
                    {
                        buttonText = $"│ {family.FamilyName} ({familyWorkersHere} working here)";
                        textColorToUse = new Color(150, 255, 150, 255);
                    }
                    else if (workingElsewhere.Any() && otherWorkplace != null)
                    {
                        buttonText = $"│ {family.FamilyName} (at {otherWorkplace.Type})";
                        textColorToUse = new Color(200, 200, 100, 255); // Yellow text
                    }
                    else if (availableCount > 0)
                    {
                        buttonText = $"│ {family.FamilyName} ({availableCount} available)";
                        textColorToUse = new Color(200, 200, 200, 255);
                    }
                    else
                    {
                        buttonText = $"│ {family.FamilyName} (no workers available)";
                        textColorToUse = new Color(120, 120, 120, 255); // Grayed out
                    }
                }
                
                GraphicsConfig.DrawConsoleText(buttonText, buttonX + 5, buttonY + 3, SmallFontSize - 2, textColorToUse);

                // Handle click
                if (isHovered && mouseClicked)
                {
                    if (isHouse && !family.Members.Any(m => m.HomeBuilding?.Id == building.Id))
                    {
                        // Assign family to live in house - immediate execution
                        Console.WriteLine($"[HOME] Assigning {family.FamilyName} family to live in house");
                        var command = new VillageBuilder.Engine.Commands.FamilyCommands.AssignFamilyHomeCommand(
                            0, engine.CurrentTick, family.Id, building.Id);
                        engine.SubmitCommand(command);
                    }
                    else if (!isHouse && !isAssignedHere && availableCount > 0)
                    {
                        // Check if building is under construction
                        if (!building.IsConstructed)
                        {
                            // Assign family to construction work
                            Console.WriteLine($"[CONSTRUCTION] Assigning {family.FamilyName} family to build {building.Type}");
                            var command = new VillageBuilder.Engine.Commands.BuildingCommands.AssignConstructionWorkersCommand(
                                0, engine.CurrentTick, family.Id, building.Id);
                            engine.SubmitCommand(command);
                        }
                        else
                        {
                            // Assign family to work at building - immediate execution
                            Console.WriteLine($"[JOB] Assigning {family.FamilyName} family to work at {building.Type}");
                            var command = new VillageBuilder.Engine.Commands.PersonCommands.AssignFamilyJobCommand(
                                0, engine.CurrentTick, family.Id, building.Id);
                            engine.SubmitCommand(command);
                        }
                    }
                }

                y += buttonHeight + 3;
            }
            
            y += 10;
            
            // Commands
            DrawSectionHeader("COMMANDS", y);
            y += LineHeight + 5;
            
            GraphicsConfig.DrawConsoleText("│ [ESC  ] Back to Map", _sidebarX + Padding, y, SmallFontSize, new Color(150, 200, 255, 255));
            y += LineHeight;
            
            // Draw section separator
            GraphicsConfig.DrawConsoleText("└" + new string('─', (_sidebarWidth - Padding * 2 - 8) / 8), 
                _sidebarX + Padding, y, FontSize, new Color(100, 100, 120, 255));
            y += LineHeight;

            return y + 5;
        }
    }
}
