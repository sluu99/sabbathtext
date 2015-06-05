namespace SabbathText.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using KeyValueStorage;

    /// <summary>
    /// This class is used to help make sure we won't do something twice.
    /// If the tracker entity exists, it means that we should skip the action.
    /// This is mostly used to prevent us from spamming the user, since <c>Twilio</c> does not support idempotent calls.
    /// For example, we would insert this tracker into the entity store, then send the message.
    /// If sending the message fails, we would rather skip it than potentially retry multiple times and spams the user.
    /// </summary>
    public class TrackerEntity : KeyValueEntity
    {
        /// <summary>
        /// Gets or sets the tracking ID
        /// </summary>
        public string TrackingId { get; set; }

        /// <summary>
        /// Gets or sets the payload.
        /// </summary>
        public string PayLoad { get; set; }

        /// <summary>
        /// Gets the partition key
        /// </summary>
        public override string PartitionKey
        {
            get { return this.TrackingId; }
        }

        /// <summary>
        /// Gets the row key
        /// </summary>
        public override string RowKey
        {
            get { return this.TrackingId; }
        }
    }
}
