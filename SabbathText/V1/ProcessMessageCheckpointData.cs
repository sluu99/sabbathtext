namespace SabbathText.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Checkpoint data for ProcessMessageOperation
    /// </summary>
    public class ProcessMessageCheckpointData : CheckpointData<bool>
    {
        /// <summary>
        /// Create a new instance of this class
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        public ProcessMessageCheckpointData(string accountId)
            : base(accountId)
        {
        }

        /// <summary>
        /// Gets or sets the message to be processed
        /// </summary>
        public Message IncomingMessage { get; set; }

        /// <summary>
        /// Gets or sets the ID of the incoming message
        /// </summary>
        public string IncomingMessageId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the outgoing message
        /// </summary>
        public string OutgoingMessageId { get; set; }

        /// <summary>
        /// Gets or sets the outgoing message.
        /// </summary>
        public Message OutgoingMessage { get; set; }
                
        /// <summary>
        /// Gets or sets the operation state
        /// </summary>
        public ProcessMessageState OperationState { get; set; }
    }
}
