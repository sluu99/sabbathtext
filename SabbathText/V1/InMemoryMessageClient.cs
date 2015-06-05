namespace SabbathText.V1
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using KeyValueStorage;
    using Newtonsoft.Json;
    using SabbathText.Entities;

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
        /// Sends a message and sets its external ID
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="trackingId">The tracking ID used for this message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The operation task</returns>
        public override async Task SendMessage(Message message, string trackingId, CancellationToken cancellationToken)
        {
            GoodieBag bag = GoodieBag.Create();

            if (trackingId != null)
            {
                try
                {
                    await bag.TrackerStore.Insert(
                        new TrackerEntity
                        {
                            TrackingId = trackingId,
                        },
                        cancellationToken);
                }
                catch (DuplicateKeyException)
                {
                    Trace.TraceInformation("Message with tracking ID {0} skipped", trackingId);
                    return;
                }
            }

            message.ExternalId = Guid.NewGuid().ToString();
            string messageStr = JsonConvert.SerializeObject(message, Formatting.Indented);
            this.messages.Add(messageStr);

            Trace.TraceInformation("Sending message with tracking ID {0}:{1}{2}", trackingId, Environment.NewLine, messageStr);
        }
    }
}
