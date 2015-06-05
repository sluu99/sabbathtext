namespace SabbathText.V1
{
    /// <summary>
    /// Checkpoint data for the GreetUserOperation
    /// </summary>
    public class GreetUserOperationCheckpointData : CheckpointData<bool>
    {
        /// <summary>
        /// Creates a new instance of the operation checkpoint data
        /// </summary>
        /// <param name="accountId">The account ID</param>
        public GreetUserOperationCheckpointData(string accountId)
            : base(accountId)
        {
        }

        /// <summary>
        /// Gets or sets the message that was sent out
        /// </summary>
        public Message Message { get; set; }

        /// <summary>
        /// Gets or sets the message entity ID that will be ended into the account entity
        /// </summary>
        public string MessageEntityId { get; set; }

        /// <summary>
        /// Gets or sets the operation state
        /// </summary>
        public GreetUserOperationState OperationState { get; set; }
    }
}
