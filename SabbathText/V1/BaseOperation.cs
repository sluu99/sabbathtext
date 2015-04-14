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
    public abstract class BaseOperation<TResponse>
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
        /// Resumes an operation base on the checkpoint
        /// </summary>
        /// <param name="checkpoint">The checkpoint</param>
        /// <returns>The operation response</returns>
        public Task<OperationResponse<TResponse>> Resume(Checkpoint checkpoint)
        {
            if (string.Equals(checkpoint.OperationType, this.operationType) == false)
            {
                throw new NotSupportedException(string.Format(
                    "Cannot resume an operation of type {0} using this implementation. The expected operation type is {1}.",
                    checkpoint.OperationType,
                    this.operationType));
            }

            this.checkpoint = checkpoint;
            return this.Resume(checkpoint.CheckpointData);
        }

        /// <summary>
        /// Resumes the operation
        /// </summary>
        /// <param name="serializedCheckpointData">The serialized checkpoint data</param>
        /// <returns>The operation response</returns>
        protected abstract Task<OperationResponse<TResponse>> Resume(string serializedCheckpointData);

        /// <summary>
        /// Updates the checkpoint to completed and returns a response
        /// </summary>
        /// <param name="checkpointData">The checkpoint data</param>
        /// <param name="status">The operation status</param>
        /// <param name="responseData">The operation data</param>
        /// <returns>The final operation response</returns>
        protected async Task<OperationResponse<TResponse>> CompleteCheckpoint(
            CheckpointData<TResponse> checkpointData,
            HttpStatusCode status,
            TResponse responseData)
        {
            checkpointData.Response = new OperationResponse<TResponse>
            {
                StatusCode = status,
                Data = responseData,
            };

            await this.CreateOrUpdateCheckpoint(checkpointData, CheckpointStatus.Completed);

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
            CheckpointData<TResponse> checkpointData,
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

            await this.CreateOrUpdateCheckpoint(checkpointData, CheckpointStatus.Completed);

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
            CheckpointData<TResponse> checkpointData,
            HttpStatusCode status,
            TResponse responseData)
        {
            checkpointData.Response = new OperationResponse<TResponse>
            {
                StatusCode = status,
                Data = responseData,
            };

            await this.CreateOrUpdateCheckpoint(checkpointData, CheckpointStatus.DelayedProcessing);
            await this.Context.Compensation.QueueCheckpoint(
                this.checkpoint,
                DelayProcessingCheckpointVisibilityDelay,
                this.Context.CancellationToken);

            return checkpointData.Response;
        }

        /// <summary>
        /// Creates or updates a checkpoint
        /// </summary>
        /// <param name="checkpointData">The checkpoint data</param>
        /// <returns>The response if it was already completed before, or null</returns>
        protected Task<OperationResponse<TResponse>> CreateOrUpdateCheckpoint(CheckpointData<TResponse> checkpointData)
        {
            return this.CreateOrUpdateCheckpoint(checkpointData, CheckpointStatus.InProgress);
        }

        /// <summary>
        /// Creates or updates a checkpoint
        /// </summary>
        /// <param name="checkpointData">The checkpoint data</param>
        /// <param name="checkpointStatus">The checkpoint status</param>
        /// <returns>The response if it was already completed before, or null</returns>
        private async Task<OperationResponse<TResponse>> CreateOrUpdateCheckpoint(CheckpointData<TResponse> checkpointData, CheckpointStatus checkpointStatus)
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
                    Status = checkpointStatus,
                    CheckpointData = checkpointData == null ? null : JsonConvert.SerializeObject(checkpointData),
                };

                this.checkpoint = await this.Context.Compensation.InsertOrGetCheckpoint(this.checkpoint, this.Context.CancellationToken);

                if (this.checkpoint.Status == CheckpointStatus.Completed || this.checkpoint.Status == CheckpointStatus.Cancelled)
                {
                    // the checkpoint is at terminal states
                    CheckpointData<TResponse> existingCheckpointData =
                        JsonConvert.DeserializeObject<CheckpointData<TResponse>>(this.checkpoint.CheckpointData);

                    return existingCheckpointData.Response;
                }
                else
                {
                    // the checkpoint is in progress
                    return new OperationResponse<TResponse>
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        ErrorCode = CommonErrorCodes.OperationInProgress,
                        ErrorDescription = "The operation is in progress",
                    };
                }
            }

            this.checkpoint.CheckpointData = JsonConvert.SerializeObject(checkpointData);
            this.checkpoint.Status = checkpointStatus;
            await this.Context.Compensation.UpdateCheckpoint(this.checkpoint, this.Context.CancellationToken);

            return null;
        }
    }
}
