namespace SabbathText.Operations
{
    using System;
    using System.Globalization;
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
        /// Creates a new instance of the operation
        /// </summary>
        /// <param name="context">The operation context</param>
        public BaseOperation(OperationContext context)
        {
            if (context == null)
            {
                throw new ArgumentException("context");
            }

            if (string.IsNullOrWhiteSpace(context.TrackingId))
            {
                context.TrackingId = Guid.NewGuid().ToString();
            }

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
        /// <returns>The async task</returns>
        protected async Task CreateOrUpdateCheckpoint(string partitionKey, CheckpointStatus status, object checkpointData)
        {
            if (this.checkpoint == null)
            {
                this.checkpoint = new Checkpoint
                {
                    PartitionKey = partitionKey,
                    RowKey = this.Context.TrackingId,
                    TrackingId = this.Context.TrackingId,
                    Status = status,
                    CheckpointData = checkpointData == null ? null : JsonConvert.SerializeObject(checkpointData),
                };

                try
                {
                    await this.Context.CheckpointStore.Insert(this.checkpoint, this.Context.CancellationToken);
                    return;
                }
                catch (DuplicateKeyException)
                {
                    // this happens when the operation is resumed
                    this.checkpoint = null;
                }
            }

            if (this.checkpoint == null)
            {
                // the check point should already exist in the DB
                this.checkpoint = await this.Context.CheckpointStore.Get(partitionKey, this.Context.TrackingId, this.Context.CancellationToken);
                if (this.checkpoint == null)
                {
                    throw new ApplicationException(
                        string.Format(CultureInfo.InvariantCulture, "Cannot create nor find the checkpoint {0}/{1}", partitionKey, this.Context.TrackingId));
                }
            }

            this.checkpoint.Status = status;
            this.checkpoint.CheckpointData = JsonConvert.SerializeObject(checkpointData);
            await this.Context.CheckpointStore.Update(this.checkpoint, this.Context.CancellationToken);
        }

        /// <summary>
        /// Updates the checkpoint to completed and returns a response
        /// </summary>
        /// <param name="partitionKey">Partition key for the checkpoint</param>
        /// <param name="status">The operation status</param>
        /// <param name="responseData">The operation data</param>
        /// <returns>The final operation response</returns>
        protected async Task<OperationResponse<T>> Complete(string partitionKey, OperationStatusCode status, T responseData)
        {
            await this.CreateOrUpdateCheckpoint(partitionKey, CheckpointStatus.Completed, checkpointData: null);
            
            return new OperationResponse<T>
            {
                StatusCode = status,
                Data = responseData,
            };
        }

        /// <summary>
        /// Updates the checkpoint to completed and returns a response
        /// </summary>
        /// <param name="partitionKey">Partition key for the checkpoint</param>
        /// <param name="status">The operation status</param>
        /// <param name="errorMessage">The error message</param>
        /// <returns>The final operation response</returns>
        protected async Task<OperationResponse<T>> CompleteWithError(string partitionKey, OperationStatusCode status, string errorMessage)
        {
            await this.CreateOrUpdateCheckpoint(partitionKey, CheckpointStatus.Completed, checkpointData: null);

            return new OperationResponse<T>
            {
                StatusCode = status,
                ErrorMessage = errorMessage,
            };
        }
    }
}
