namespace SabbathText.Tests.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SabbathText.V1;

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
        /// <returns>The operation task</returns>
        public override Task SendMessage(Message message)
        {
            message.ExternalId = Guid.NewGuid().ToString();
            this.messages.Add(JsonConvert.SerializeObject(message));

            return Task.FromResult<object>(null);
        }
    }
}
