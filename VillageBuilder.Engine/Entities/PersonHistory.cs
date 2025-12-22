using VillageBuilder.Engine.Core;

namespace VillageBuilder.Engine.Entities
{
    public enum EventType
    {
        Birth,
        Death,
        Marriage,
        JobAssignment,
        SkillLearned,
        Achievement,
        Injury,
        Recovery
    }

    public class HistoricalEvent
    {
        public EventType Type { get; }
        public string Description { get; }
        public GameTime Timestamp { get; }
        public string? RelatedPersonName { get; }

        public HistoricalEvent(EventType type, string description, GameTime timestamp, string? relatedPerson = null)
        {
            Type = type;
            Description = description;
            Timestamp = new GameTime { /* Copy timestamp */ };
            RelatedPersonName = relatedPerson;
        }

        public override string ToString()
        {
            return $"[{Timestamp}] {Description}";
        }
    }

    public class PersonHistory
    {
        private List<HistoricalEvent> _events;

        public PersonHistory()
        {
            _events = new List<HistoricalEvent>();
        }

        public void AddEvent(EventType type, string description, GameTime timestamp, string? relatedPerson = null)
        {
            _events.Add(new HistoricalEvent(type, description, timestamp, relatedPerson));
        }

        public List<HistoricalEvent> GetEvents() => new List<HistoricalEvent>(_events);
    }
}