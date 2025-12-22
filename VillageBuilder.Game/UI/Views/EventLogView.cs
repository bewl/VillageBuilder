using Terminal.Gui;
using System;

namespace VillageBuilder.Game.UI.Views
{
    public class EventLogView : FrameView
    {
        private readonly TextView _logTextView;

        public EventLogView() : base("Event Log")
        {
            _logTextView = new TextView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ReadOnly = true,
                WordWrap = true
            };

            Add(_logTextView);
        }

        public void LogMessage(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            _logTextView.Text += $"[{timestamp}] {message}\n";
            _logTextView.MoveEnd();
        }

        public void Clear()
        {
            _logTextView.Text = string.Empty;
        }
    }
}