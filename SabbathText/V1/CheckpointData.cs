namespace SabbathText.V1
{
    /// <summary>
    /// Operation data
    /// </summary>
    /// <typeparam name="T">The response data type</typeparam>
    public class CheckpointData<T>
    {
        /// <summary>
        /// Gets or sets the account ID associated with the checkpoint
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Gets or sets the operation response
        /// </summary>
        public OperationResponse<T> Response { get; set; }
    }
}
