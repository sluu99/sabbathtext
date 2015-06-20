namespace SabbathText.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Checkpoint data for <see cref="SabbathMessageOperation"/>
    /// </summary>
    public class SabbathMessageOperationCheckpointData : CheckpointData<bool>
    {
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        public SabbathMessageOperationCheckpointData(string accountId)
            : base(accountId)
        {
        }

        /// <summary>
        /// Gets or sets the operation state
        /// </summary>
        public ServiceMessageOperationState OperationState { get; set; }

        /// <summary>
        /// Gets or sets the outgoing message.
        /// </summary>
        public Message OutgoingMessage { get; set; }
    }
}
