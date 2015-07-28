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
        private const int MaxMessageLength = 160;

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
        /// <returns>Whether the message was sent</returns>
        public override async Task<bool> SendMessage(Message message, string trackingId, CancellationToken cancellationToken)
        {
            if (message.Body.Length > MaxMessageLength)
            {
                Trace.WriteLine("Body: " + message.Body);
                throw new NotSupportedException(string.Format("Message length ({0}) exceeds the maximum length allowed {1}", message.Body.Length, MaxMessageLength));
            }

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
                    bag.TelemetryTracker.MessageSkipped(message.Template, trackingId);
                    return false;
                }
            }

            message.ExternalId = Guid.NewGuid().ToString();
            string messageStr = JsonConvert.SerializeObject(message, Formatting.Indented);
            this.messages.Add(messageStr);

            bag.TelemetryTracker.MessageSent(message.Template, message.Body, trackingId);

            return true;
        }
    }
}
