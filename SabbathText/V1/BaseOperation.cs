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
        /// <param name="checkpointData">The checkpoint data</param>
        /// <returns>The response if it was already completed before, or null</returns>
        protected async Task<OperationResponse<T>> CreateOrUpdateCheckpoint(
            string partitionKey,
            CheckpointData<T> checkpointData)
        {
            if (this.checkpoint == null)
            {
                this.checkpoint = new Checkpoint
                {
                    PartitionKey = partitionKey,
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
        /// <param name="partitionKey">Partition key for the checkpoint</param>
        /// <param name="checkpointData">The checkpoint data</param>
        /// <param name="status">The operation status</param>
        /// <param name="responseData">The operation data</param>
        /// <param name="errorCode">The error code</param>
        /// <param name="errorDescription">The error description</param>
        /// <returns>The final operation response</returns>
        protected async Task<OperationResponse<T>> Complete(
            string partitionKey,
            CheckpointData<T> checkpointData,
            HttpStatusCode status,
            T responseData,
            string errorCode,
            string errorDescription)
        {
            checkpointData.Response = new OperationResponse<T>
            {
                StatusCode = status,
                Data = responseData,
                ErrorCode = errorCode,
                ErrorDescription = errorDescription
            };

            this.checkpoint.Status = CheckpointStatus.Completed;
            await this.CreateOrUpdateCheckpoint(partitionKey, checkpointData);

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
            string accountId = ("PhoneNumber:" + phoneNumber).Sha256();

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
    }
}
