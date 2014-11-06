using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace SabbathText.Core.Entities
{
    public class Message : TableEntity
    {
        public Message()
        {
            this.CreationTime = Clock.MinValue;
        }

        public string MessageId { get; set; }
        public string ExternalId { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string Body { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
