using System;

namespace SabbathText.Core.Entities
{
    public class Message
    {
        public string MessageId { get; set; }
        public string ExternalId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Body { get; set; }
        public DateTime CreateOn { get; set; }
    }
}
