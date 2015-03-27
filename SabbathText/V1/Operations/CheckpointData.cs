namespace SabbathText.V1.Operations
{
    /// <summary>
    /// Operation data
    /// </summary>
    /// <typeparam name="T">The response data type</typeparam>
    public class CheckpointData<T>
    {
        /// <summary>
        /// Gets or sets the operation response
        /// </summary>
        public OperationResponse<T> Response { get; set; }
    }
}
