namespace SabbathText.V1
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using KeyValueStorage;
    using Newtonsoft.Json;
    using SabbathText.Compensation.V1;
    using SabbathText.Entities;

    /// <summary>
    /// This is the base class for all the operations
    /// </summary>
    /// <typeparam name="TResponse">The data type of the operation response</typeparam>
    /// <typeparam name="TState">The operation state enumeration type</typeparam>
    public abstract class BaseOperation<TResponse, TState>
        where TState : struct, IConvertible
    {
        /// <summary>
        /// The timeout before a delay processing checkpoint could be picked up
        /// </summary>
        private static readonly TimeSpan DelayProcessingCheckpointVisibilityDelay = TimeSpan.Zero;

        /// <summary>
        /// The checkpoint for this operation
        /// </summary>
        private Checkpoint checkpoint;

        /// <summary>
        /// The operation type
        /// </summary>
        private string operationType;

        /// <summary>
        /// Creates a new instance of the operation
        /// </summary>
        /// <param name="context">The operation context</param>
        /// <param name="operationType">Name of the operation (including version)</param>
        public BaseOperation(OperationContext context, string operationType)
        {
            if (context == null)
            {
                throw new ArgumentException("context");
            }

            if (string.IsNullOrWhiteSpace(operationType))
            {
                throw new ArgumentException("The operation type is required", "operationType");
            }

            if (string.IsNullOrWhiteSpace(context.TrackingId))
            {
                context.TrackingId = Guid.NewGuid().ToString();
            }

            this.operationType = operationType;
            this.Context = context;
        }

        /// <summary>
        /// Gets the operation context
        /// </summary>
        protected OperationContext Context { get; private set; }

        /// <summary>
        /// Creates or updates a checkpoint
        /// </summary>
        /// <param name="checkpointData">The checkpoint data</param>
        /// <returns>The response if it was already completed before, or null</returns>
        protected async Task<OperationResponse<TResponse>> CreateOrUpdateCheckpoint(CheckpointData<TResponse, TState> checkpointData)
        {
            if (this.Context == null || string.IsNullOrWhiteSpace(this.Context.Account.AccountId))
            {
                throw new ArgumentException("The account ID in checkpoint data is required");
            }

            if (this.checkpoint == null)
            {
                this.checkpoint = new Checkpoint
                {
                    PartitionKey = this.Context.Account.AccountId,
                    RowKey = this.Context.TrackingId.Sha256(), // we need to hash this since the client can potentially pass in illegal chars
                    TrackingId = this.Context.TrackingId,
                    OperationType = this.operationType,
                    Status = CheckpointStatus.InProgress,
                    CheckpointData = checkpointData == null ? null : JsonConvert.SerializeObject(checkpointData),
                };

                this.checkpoint = await this.Context.Compensation.InsertOrGetCheckpoint(this.checkpoint, this.Context.CancellationToken);

                if (this.checkpoint.CheckpointData != null)
                {
                    CheckpointData<TResponse, TState> existingCheckpointData =
                        JsonConvert.DeserializeObject<CheckpointData<TResponse, TState>>(this.checkpoint.CheckpointData);

                    if (existingCheckpointData.Response != null)
                    {
                        return existingCheckpointData.Response;
                    }
                }
            }

            this.checkpoint.CheckpointData = JsonConvert.SerializeObject(checkpointData);
            await this.Context.Compensation.UpdateCheckpoint(this.checkpoint, this.Context.CancellationToken);

            return null;
        }

        /// <summary>
        /// Updates the checkpoint to completed and returns a response
        /// </summary>
        /// <param name="checkpointData">The checkpoint data</param>
        /// <param name="status">The operation status</param>
        /// <param name="responseData">The operation data</param>
        /// <returns>The final operation response</returns>
        protected async Task<OperationResponse<TResponse>> CompleteCheckpoint(
            CheckpointData<TResponse, TState> checkpointData,
            HttpStatusCode status,
            TResponse responseData)
        {
            checkpointData.Response = new OperationResponse<TResponse>
            {
                StatusCode = status,
                Data = responseData,
            };

            this.checkpoint.Status = CheckpointStatus.Completed;
            await this.CreateOrUpdateCheckpoint(checkpointData);

            return checkpointData.Response;
        }

        /// <summary>
        /// Updates the checkpoint to completed and returns a response with error
        /// </summary>
        /// <param name="checkpointData">The checkpoint data</param>
        /// <param name="status">The operation status</param>
        /// <param name="errorCode">The error code</param>
        /// <param name="errorDescription">The error description</param>
        /// <returns>The operation response</returns>
        protected async Task<OperationResponse<TResponse>> CompleteCheckpoint(
            CheckpointData<TResponse, TState> checkpointData,
            HttpStatusCode status,
            string errorCode,
            string errorDescription)
        {
            checkpointData.Response = new OperationResponse<TResponse>
            {
                StatusCode = status,
                ErrorCode = errorCode,
                ErrorDescription = errorDescription,
            };

            this.checkpoint.Status = CheckpointStatus.Completed;
            await this.CreateOrUpdateCheckpoint(checkpointData);

            return checkpointData.Response;
        }

        /// <summary>
        /// Mark a checkpoint for delay processing
        /// </summary>
        /// <param name="checkpointData">The checkpoint data</param>
        /// <param name="status">The operation response status</param>
        /// <param name="responseData">The operation response data</param>
        /// <returns>The operation response</returns>
        protected async Task<OperationResponse<TResponse>> DelayProcessingCheckpoint(
            CheckpointData<TResponse, TState> checkpointData,
            HttpStatusCode status,
            TResponse responseData)
        {
            checkpointData.Response = new OperationResponse<TResponse>
            {
                StatusCode = status,
                Data = responseData,
            };

            this.checkpoint.Status = CheckpointStatus.DelayedProcessing;
            await this.CreateOrUpdateCheckpoint(checkpointData);
            await this.Context.Compensation.QueueCheckpoint(
                this.checkpoint,
                DelayProcessingCheckpointVisibilityDelay,
                this.Context.CancellationToken);

            return checkpointData.Response;
        }

        /// <summary>
        /// Resumes the operation
        /// </summary>
        /// <param name="serializedCheckpointData">The serialized checkpoint data</param>
        /// <returns>The operation response</returns>
        protected abstract Task<OperationResponse<TResponse>> Resume(string serializedCheckpointData);
    }
}
