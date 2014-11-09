using System;

namespace SabbathText.Core.Entities
{
    public class EventMessage : Message
    {
        public EventType EventType { get; set; }

        public static EventMessage Create(string sender, EventType eventType, string parameters)
        {
            return new EventMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                Sender = sender,
                Recipient = sender,
                CreationTime = Clock.UtcNow,
                Body = string.IsNullOrEmpty(parameters)? eventType.ToString() : string.Format("{0} {1}", eventType.ToString(), parameters),
                EventType = eventType,                
            };
        }
    }
}
