﻿namespace SabbathText.V1
{
    using System;
    using System.Globalization;
    using System.Linq;
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
            this.Bag = GoodieBag.Create();
        }

        /// <summary>
        /// Gets the operation context.
        /// </summary>
        protected OperationContext Context { get; private set; }

        /// <summary>
        /// Gets the goodie bag.
        /// </summary>
        protected GoodieBag Bag { get; private set; }

        /// <summary>
        /// Try adding a message to the account.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="message">The message.</param>
        /// <returns>True if the message is added, false otherwise.</returns>
        protected static bool TryAddMessageEntity(AccountEntity account, MessageEntity message)
        {
            if (!account.RecentMessages.Any(m => m.MessageId == message.MessageId))
            {
                account.RecentMessages.Add(message);
                return true;
            }

            return false;
        }

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

            await this.CreateOrUpdateCheckpoint(checkpointData, CheckpointStatus.Completed, processAfter: null);

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

            await this.CreateOrUpdateCheckpoint(checkpointData, CheckpointStatus.Completed, processAfter: null);

            return checkpointData.Response;
        }

        /// <summary>
        /// Mark a checkpoint for delay processing
        /// </summary>
        /// <param name="delay">The timeout before a delay processing checkpoint could be picked up.</param>
        /// <param name="checkpointData">The checkpoint data</param>
        /// <param name="status">The operation response status</param>
        /// <param name="responseData">The operation response data</param>
        /// <returns>The operation response</returns>
        protected async Task<OperationResponse<TResponse>> DelayProcessingCheckpoint(
            TimeSpan delay,
            CheckpointData<TResponse> checkpointData,            
            HttpStatusCode status,
            TResponse responseData)
        {
            checkpointData.Response = new OperationResponse<TResponse>
            {
                StatusCode = status,
                Data = responseData,
            };

            await this.CreateOrUpdateCheckpoint(
                checkpointData,
                CheckpointStatus.DelayedProcessing,
                processAfter: Clock.UtcNow + delay);
            await this.Bag.CompensationClient.QueueCheckpoint(
                this.checkpoint,
                delay,
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
            return this.CreateOrUpdateCheckpoint(checkpointData, CheckpointStatus.InProgress, processAfter: null);
        }

        /// <summary>
        /// Add processed messages to an account
        /// </summary>
        /// <param name="incomingMessageId">The message ID for the incoming message entity.</param>
        /// <param name="incomingMessage">The incoming message.</param>
        /// <param name="outgoingMessageId">The message ID for the outgoing message entity.</param>
        /// <param name="outgoingMessage">The outgoing message.</param>
        /// <returns>A TPL task</returns>
        protected Task AddProcessedMessages(
            string incomingMessageId,
            Message incomingMessage,
            string outgoingMessageId,
            Message outgoingMessage)
        {
            MessageEntity incomingMessageEntity = incomingMessage.ToEntity(
                this.Context.Account.AccountId,
                incomingMessageId,
                MessageDirection.Incoming,
                outgoingMessage == null ? MessageStatus.Received : MessageStatus.Responded);
            bool incomingMessageAdded = TryAddMessageEntity(this.Context.Account, incomingMessageEntity);

            bool outgoingMessageAdded = false;
            if (outgoingMessage != null)
            {
                MessageEntity outgoingMessageEntity = outgoingMessage.ToEntity(
                    this.Context.Account.AccountId,
                    outgoingMessageId,
                    MessageDirection.Outgoing,
                    MessageStatus.Sent);
                outgoingMessageAdded = TryAddMessageEntity(this.Context.Account, outgoingMessageEntity);
            }

            if (incomingMessageAdded || outgoingMessageAdded)
            {
                return this.Bag.AccountStore.Update(this.Context.Account, this.Context.CancellationToken);
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Creates or updates a checkpoint
        /// </summary>
        /// <param name="checkpointData">The checkpoint data</param>
        /// <param name="checkpointStatus">The checkpoint status</param>
        /// <returns>The response if it was already completed before, or null</returns>
        private async Task<OperationResponse<TResponse>> CreateOrUpdateCheckpoint(
            CheckpointData<TResponse> checkpointData,
            CheckpointStatus checkpointStatus,
            DateTime? processAfter)
        {
            if (this.Context == null || string.IsNullOrWhiteSpace(this.Context.Account.AccountId))
            {
                throw new ArgumentException("The account ID in checkpoint data is required");
            }

            if (this.checkpoint == null)
            {
                this.checkpoint = new Checkpoint
                {
                    AccountId = this.Context.Account.AccountId,
                    TrackingId = this.Context.TrackingId,
                    OperationType = this.operationType,
                    Status = checkpointStatus,
                    ProcessAfter = processAfter,
                    CheckpointData = checkpointData == null ? null : JsonConvert.SerializeObject(checkpointData),
                };

                this.checkpoint = await this.Bag.CompensationClient.InsertOrGetCheckpoint(this.checkpoint, this.Context.CancellationToken);

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
            this.checkpoint.ProcessAfter = processAfter;
            await this.Bag.CompensationClient.UpdateCheckpoint(this.checkpoint, this.Context.CancellationToken);

            return null;
        }
    }
}
