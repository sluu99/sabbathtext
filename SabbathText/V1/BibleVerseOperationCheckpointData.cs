namespace SabbathText.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Checkpoint data for the Bible verse operation
    /// </summary>
    public class BibleVerseOperationCheckpointData : CheckpointData<bool>
    {
        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="accountId">The account ID</param>
        public BibleVerseOperationCheckpointData(string accountId)
            : base(accountId)
        {
        }

        /// <summary>
        /// Gets or sets the incoming message ID
        /// </summary>
        public string IncomingMessageId { get; set; }

        /// <summary>
        /// Gets or sets the incoming message
        /// </summary>
        public Message IncomingMessage { get; set; }

        /// <summary>
        /// Gets or sets the outgoing message ID
        /// </summary>
        public string OutgoingMessageId { get; set; }

        /// <summary>
        /// Gets or sets the outgoing message
        /// </summary>
        public Message OutgoingMessage { get; set; }

        /// <summary>
        /// Gets or sets the operation state
        /// </summary>
        public RespondingMessageOperationState OperationState { get; set; }
    }
}
