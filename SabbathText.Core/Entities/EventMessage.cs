using System;

namespace SabbathText.Core.Entities
{
    public class EventMessage : Message
    {
        public EventType EventType { get; set; }

        public static EventMessage Create(string accountId, EventType eventType, string parameters)
        {
            return new EventMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                Sender = accountId,
                Recipient = accountId,
                CreationTime = Clock.UtcNow,
                Body = string.IsNullOrEmpty(parameters)? eventType.ToString() : string.Format("{0} {1}", eventType.ToString(), parameters),
                EventType = eventType,                
            };
        }
    }
}
