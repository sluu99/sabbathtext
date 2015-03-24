namespace QueueStorage
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents a queue message
    /// </summary>
    public class QueueMessage
    {
        /// <summary>
        /// Gets the message ID
        /// </summary>
        [JsonProperty]
        public string MessageId { get; internal set; }

        /// <summary>
        /// Gets the message body
        /// </summary>
        [JsonProperty]
        public string Body { get; internal set; }

        /// <summary>
        /// Gets the insertion time
        /// </summary>
        [JsonProperty]
        public DateTime InsertionTime { get; internal set; }
        
        /// <summary>
        /// Gets the message expiration time
        /// </summary>
        [JsonProperty]
        public DateTime ExpirationTime { get; internal set; }

        /// <summary>
        /// Gets the next visible time of the message
        /// </summary>
        [JsonProperty]
        public DateTime NextVisibleTime { get; internal set; }

        /// <summary>
        /// Gets the number of times that the message has been de-queued
        /// </summary>
        [JsonProperty]
        public int DequeueCount { get; internal set; }
    }
}
