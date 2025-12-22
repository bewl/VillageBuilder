using VillageBuilder.Engine.Core;

namespace VillageBuilder.Game.Graphics.UI
{
    // This is just a namespace alias/wrapper for the Engine EventLog
    // so existing UI code doesn't break
    public class EventLogUI
    {
        public static EventLog Instance => EventLog.Instance;
    }
}
