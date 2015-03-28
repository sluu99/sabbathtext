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
    /// <typeparam name="T">The data type of the operation response</typeparam>
    public abstract class BaseOperation<T>
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
        protected async Task<OperationResponse<T>> CreateOrUpdateCheckpoint(CheckpointData<T> checkpointData)
        {
            if (checkpointData == null || string.IsNullOrWhiteSpace(checkpointData.AccountId))
            {
                throw new ArgumentException("The account ID in checkpoint data is required");
            }

            if (this.checkpoint == null)
            {
                this.checkpoint = new Checkpoint
                {
                    PartitionKey = checkpointData.AccountId,
                    RowKey = this.Context.TrackingId.Sha256(), // we need to hash this since the client can potentially pass in illegal chars
                    TrackingId = this.Context.TrackingId,
                    OperationType = this.operationType,
                    Status = CheckpointStatus.InProgress,
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
        protected async Task<OperationResponse<T>> Complete(
            CheckpointData<T> checkpointData,
            HttpStatusCode status,
            T responseData)
        {
            checkpointData.Response = new OperationResponse<T>
            {
                StatusCode = status,
                Data = responseData,
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
        protected async Task<OperationResponse<T>> DelayProcessing(
            CheckpointData<T> checkpointData,
            HttpStatusCode status,
            T responseData)
        {
            checkpointData.Response = new OperationResponse<T>
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
        protected abstract Task<OperationResponse<T>> Resume(string serializedCheckpointData);

        /// <summary>
        /// Gets or create an account using a phone number
        /// </summary>
        /// <param name="phoneNumber">The phone number</param>
        /// <returns>The account</returns>
        protected async Task<AccountEntity> GetOrCreateAccount(string phoneNumber)
        {
            string accountId = this.GetAccountId(phoneNumber);

            AccountEntity account = await this.Context.AccountStore.Get(accountId, accountId, this.Context.CancellationToken);

            if (account != null)
            {
                return account;
            }

            account = new AccountEntity
            {
                PartitionKey = accountId,
                RowKey = accountId,
                AccountId = accountId,
                CreationTime = Clock.UtcNow,
                PhoneNumber = phoneNumber,
                Status = AccountStatus.BrandNew,
            };

            account = await this.Context.AccountStore.InsertOrGet(account, this.Context.CancellationToken);

            return account;
        }

        /// <summary>
        /// Gets an account ID from a phone number
        /// </summary>
        /// <param name="phoneNumber">The phone umber</param>
        /// <returns>The account ID</returns>
        protected string GetAccountId(string phoneNumber)
        {
            return ("PhoneNumber:" + phoneNumber).Sha256();
        }
    }
}
