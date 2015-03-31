namespace SabbathText.V1
{
    using System;

    /// <summary>
    /// Operation data
    /// </summary>
    /// <typeparam name="TResponse">The response data type</typeparam>
    /// <typeparam name="TState">The operation enumeration type</typeparam>
    public class CheckpointData<TResponse, TState>
        where TState : struct, IConvertible /* enum */
    {
        /// <summary>
        /// Gets or sets the operation response
        /// </summary>
        public OperationResponse<TResponse> Response { get; set; }

        /// <summary>
        /// Gets or sets the operation state
        /// </summary>
        public TState OperationState { get; set; }
    }
}
