using System;
using System.Collections.Generic;

namespace VillageBuilder.Engine.Core
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
                Timestamp = DateTime.Now
            });

            // Keep only recent entries
            if (_entries.Count > MaxEntries)
                _entries.RemoveAt(0);
        }

        public void Clear()
        {
            _entries.Clear();
        }
    }

    public class LogEntry
    {
        public string Message { get; set; } = string.Empty;
        public LogLevel Level { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Success
    }
}
