namespace SabbathText.V1
{
    /// <summary>
    /// Checkpoint data for <see cref="InspectAccountOperation"/>.
    /// </summary>
    public class InspectAccountOperationCheckpointData : CheckpointData<bool>
    {
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        public InspectAccountOperationCheckpointData(string accountId)
            : base(accountId)
        {
        }

        /// <summary>
        /// Gets or sets the operation state.
        /// </summary>
        public InspectAccountOperationState State { get; set; }

        /// <summary>
        /// Gets or sets the Sabbath message entity ID.
        /// </summary>
        public string SabbathMessageId { get; set; }

        /// <summary>
        /// Gets or sets the Sabbath message.
        /// </summary>
        public Message SabbathMesage { get; set; }
    }
}
