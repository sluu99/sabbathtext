namespace SabbathText.Operations
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Threading.Tasks;
    using KeyValueStorage;
    using Newtonsoft.Json;
    using SabbathText.Compensation;

    /// <summary>
    /// This is the base class for all the operations
    /// </summary>
    /// <typeparam name="T">The data type of the operation response</typeparam>
    public abstract class BaseOperation<T>
    {
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
        /// <param name="partitionKey">The partition key for the checkpoint</param>
        /// <param name="status">The checkpoint status</param>
        /// <param name="checkpointData">The checkpoint data</param>
        /// <returns>The response if it was already completed before, or null</returns>
        protected async Task<OperationResponse<T>> CreateOrUpdateCheckpoint(
            string partitionKey,
            CheckpointStatus status,
            CheckpointData<T> checkpointData)
        {
            if (this.checkpoint == null)
            {
                this.checkpoint = new Checkpoint
                {
                    PartitionKey = partitionKey,
                    RowKey = this.Context.TrackingId,
                    TrackingId = this.Context.TrackingId,
                    OperationType = this.operationType,
                    Status = status,
                    CheckpointData = checkpointData == null ? null : JsonConvert.SerializeObject(checkpointData),
                };

                this.checkpoint = await this.Context.Compensation.InsertOrGetCheckpoint(this.checkpoint, this.Context.CancellationToken);

                if (this.checkpoint.CheckpointData != null)
                {
                    CheckpointData<T> existingCheckpointData = JsonConvert.DeserializeObject<CheckpointData<T>>(this.checkpoint.CheckpointData);
                    
                    if (existingCheckpointData.Response != null)
                    {
                        return existingCheckpointData.Response;
                    }
                }
            }

            this.checkpoint.Status = status;
            this.checkpoint.CheckpointData = JsonConvert.SerializeObject(checkpointData);
            await this.Context.Compensation.UpdateCheckpoint(this.checkpoint, this.Context.CancellationToken);

            return null;
        }

        /// <summary>
        /// Updates the checkpoint to completed and returns a response
        /// </summary>
        /// <param name="partitionKey">Partition key for the checkpoint</param>
        /// <param name="status">The operation status</param>
        /// <param name="responseData">The operation data</param>
        /// <param name="checkpointData">The checkpoint data</param>
        /// <returns>The final operation response</returns>
        protected async Task<OperationResponse<T>> EnterCompletedState(string partitionKey, HttpStatusCode status, T responseData, CheckpointData<T> checkpointData)
        {
            checkpointData.Response = new OperationResponse<T>
            {
                StatusCode = status,
                Data = responseData,
            };
            
            await this.CreateOrUpdateCheckpoint(partitionKey, CheckpointStatus.Completed, checkpointData);

            return checkpointData.Response;
        }

        /// <summary>
        /// Updates the checkpoint to completed and returns a response
        /// </summary>
        /// <param name="partitionKey">Partition key for the checkpoint</param>
        /// <param name="status">The operation status</param>
        /// <param name="errorMessage">The error message</param>
        /// <param name="checkpointData">The checkpoint data</param>
        /// <returns>The final operation response</returns>
        protected async Task<OperationResponse<T>> EnterCompletedWithErrorState(string partitionKey, HttpStatusCode status, string errorMessage, CheckpointData<T> checkpointData)
        {
            checkpointData.Response = new OperationResponse<T>
            {
                StatusCode = status,
                ErrorMessage = errorMessage,
            };

            await this.CreateOrUpdateCheckpoint(partitionKey, CheckpointStatus.Completed, checkpointData);

            return checkpointData.Response;
        }
    }
}
