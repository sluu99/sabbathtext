namespace SabbathText.V1
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// An in-memory implementation of the message client
    /// </summary>
    public class InMemoryMessageClient : MessageClient
    {
        private List<string> messages;

        /// <summary>
        /// Gets the messages in the client
        /// </summary>
        public IReadOnlyCollection<Message> Messages
        {
            get { return this.messages.Select(x => JsonConvert.DeserializeObject<Message>(x)).ToList(); }
        }

        /// <summary>
        /// Initializes the instance to use in-memory storage
        /// </summary>
        public void InitMemory()
        {
            this.messages = new List<string>();
        }

        /// <summary>
        /// Adds the message to the message collection
        /// </summary>
        /// <param name="message">The messages</param>
        /// <param name="trackingId">The tracking ID for the message.</param>
        /// <returns>The operation task</returns>
        public override Task SendMessage(Message message, string trackingId)
        {
            message.ExternalId = Guid.NewGuid().ToString();
            string messageStr = JsonConvert.SerializeObject(message, Formatting.Indented);
            this.messages.Add(messageStr);

            Trace.TraceInformation("Sending message :" + Environment.NewLine + messageStr);

            return Task.FromResult<object>(null);
        }
    }
}
