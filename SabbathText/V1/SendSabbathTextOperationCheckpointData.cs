namespace SabbathText.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Checkpoint data for <see cref="SendSabbathTextOperation"/>
    /// </summary>
    public class SendSabbathTextOperationCheckpointData : CheckpointData<bool>
    {
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        public SendSabbathTextOperationCheckpointData(string accountId)
            : base(accountId)
        {
        }

        /// <summary>
        /// Gets or sets the operation state.
        /// </summary>
        public ServiceMessageOperationState OperationState { get; set; }

        /// <summary>
        /// Gets or sets the outgoing message ID
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets the outgoing message
        /// </summary>
        public Message Message { get; set; }
    }
}
