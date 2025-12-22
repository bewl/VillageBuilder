namespace VillageBuilder.Game.Graphics.UI
{
    public class EventLog
    {
        private static EventLog? _instance;
        private readonly List<LogEntry> _entries = new();
        private const int MaxEntries = 100;

        public static EventLog Instance => _instance ??= new EventLog();

        public IReadOnlyList<LogEntry> Entries => _entries;

        private EventLog() { }

        public void AddMessage(string message, LogLevel level = LogLevel.Info)
        {
            _entries.Add(new LogEntry
            {
                Message = message,
                Level = level,
                Timestamp = System.DateTime.Now
            });

            // Keep only recent entries
            if (_entries.Count > MaxEntries)
                _entries.RemoveAt(0);
        }

        public void Clear()
        {
            _entries.Clear();
        }

        public void Draw()
        {
            var y = 10;
            foreach (var entry in _entries)
            {
                var color = entry.Level switch
                {
                    LogLevel.Info => GraphicsConfig.Colors.White,
                    LogLevel.Warning => GraphicsConfig.Colors.Yellow,
                    LogLevel.Error => GraphicsConfig.Colors.Red,
                    LogLevel.Success => GraphicsConfig.Colors.Green,
                    _ => GraphicsConfig.Colors.White
                };

                GraphicsConfig.DrawConsoleText(entry.Message, 10, y, 20, color);

                y += 30;
            }
        }
    }

    public class LogEntry
    {
        public string Message { get; set; } = string.Empty;
        public LogLevel Level { get; set; }
        public System.DateTime Timestamp { get; set; }
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Success
    }
}
