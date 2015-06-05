namespace SabbathText.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Checkpoint data for <see cref="UpdateZipCodeOperation"/>
    /// </summary>
    public class UpdateZipCodeOperationCheckpointData : CheckpointData<bool>
    {
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        public UpdateZipCodeOperationCheckpointData(string accountId)
            : base(accountId)
        {
        }

        /// <summary>
        /// Gets or sets the operation state.
        /// </summary>
        public GenericOperationState State { get; set; }

        /// <summary>
        /// Gets or sets the incoming message.
        /// </summary>
        public Message IncomingMessage { get; set; }

        /// <summary>
        /// Gets or sets the current ZIP code of the account.
        /// </summary>
        public string CurrentZipCode { get; set; }

        /// <summary>
        /// Gets or sets the outgoing message.
        /// </summary>
        public Message OutgoingMessage { get; set; }

        /// <summary>
        /// Gets or sets the incoming message entity ID.
        /// </summary>
        public string IncomingMessageId { get; set; }

        /// <summary>
        /// Gets or sets the outgoing message entity ID.
        /// </summary>
        public string OutgoingMessageId { get; set; }
    }
}
