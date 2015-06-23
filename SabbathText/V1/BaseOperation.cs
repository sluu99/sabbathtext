namespace SabbathText.V1
{
    using System;
    using System.Diagnostics;
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
        private Checkpoint checkpoint;
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
        /// Gets a value indicating whether the operation is being cancelled
        /// </summary>
        protected bool IsCancelling
        {
            get { return this.checkpoint != null && this.checkpoint.Status == CheckpointStatus.Cancelling; }
        }

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

            await this.SetCheckpoint(checkpointData, CheckpointStatus.Completed);

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

            await this.SetCheckpoint(checkpointData, CheckpointStatus.Completed);

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
        protected async Task<OperationResponse<TResponse>> HandOffCheckpoint(
            TimeSpan delay,
            CheckpointData<TResponse> checkpointData,
            HttpStatusCode status,
            TResponse responseData)
        {
            checkpointData.IsHandOffProcessing = true;
            checkpointData.ProcessAfter = (delay == TimeSpan.Zero) ? DateTime.MinValue : Clock.UtcNow + delay;
            checkpointData.Response = new OperationResponse<TResponse>
            {
                StatusCode = status,
                Data = responseData,
            };

            OperationResponse<TResponse> response = null;

            if (this.checkpoint == null)
            {
                response = await this.CreateCheckpoint(checkpointData, CheckpointStatus.InProgress);
            }
            else
            {
                response = await this.UpdateCheckpoint(checkpointData, CheckpointStatus.InProgress);
            }

            if (response != null)
            {
                return response;
            }
            
            if (checkpointData.ProcessAfter != null)
            {
                // delay process checkpoint
                TimeSpan visibilityTimeout = checkpointData.ProcessAfter.Value - Clock.UtcNow;
                if (visibilityTimeout < TimeSpan.Zero)
                {
                    visibilityTimeout = TimeSpan.Zero;
                }

                if (this.checkpoint.QueueMessage == null)
                {
                    await this.Bag.CompensationClient.QueueCheckpoint(
                        this.checkpoint,
                        visibilityTimeout,
                        this.Context.CancellationToken);
                }
                else
                {
                    await this.Bag.CompensationClient.ExtendMessageTimeout(
                        this.checkpoint.QueueMessage,
                        visibilityTimeout,
                        this.Context.CancellationToken);
                }
            }

            return checkpointData.Response;
        }

        /// <summary>
        /// Creates or updates a checkpoint
        /// </summary>
        /// <param name="checkpointData">The checkpoint data</param>
        /// <returns>The response if it was already completed before, or null</returns>
        protected Task<OperationResponse<TResponse>> SetCheckpoint(CheckpointData<TResponse> checkpointData)
        {
            return this.SetCheckpoint(checkpointData, CheckpointStatus.InProgress);
        }

        /// <summary>
        /// Add processed messages to an account
        /// </summary>
        /// <param name="incomingMessageId">The message ID for the incoming message entity.</param>
        /// <param name="incomingMessage">The incoming message.</param>
        /// <param name="outgoingMessageId">The message ID for the outgoing message entity.</param>
        /// <param name="outgoingMessage">The outgoing message.</param>
        /// <returns>Whether a message was added to the account.</returns>
        protected bool AddProcessedMessages(
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

            return incomingMessageAdded || outgoingMessageAdded;
        }

        /// <summary>
        /// Reserve a Bible for a specific tracking ID.
        /// </summary>
        /// <param name="trackingId">The tracking ID used for the Bible verse.</param>
        /// <returns>A Bible verse number</returns>
        protected async Task<string> ReserveBibleVerse(string trackingId)
        {
            if (this.Context.Account.ReservedBibleVerse.ContainsKey(trackingId))
            {
                // already reserved the Bible verse
                return this.Context.Account.ReservedBibleVerse[trackingId];
            }

            if (this.Context.Account.RecentVerses.Count >= DomainData.BibleVerses.Count)
            {
                // the account has seen all the verse
                // remove one of the older ones
                this.Context.Account.RecentVerses.RemoveAt(
                    StaticRand.Next(this.Context.Account.RecentVerses.Count / 2));
            }

            var availVerses = DomainData.BibleVerses.Keys;
            availVerses = availVerses.Except(this.Context.Account.RecentVerses);
            string bibleVerse = availVerses.RandomElement();

            this.Context.Account.ReservedBibleVerse.Add(trackingId, bibleVerse);
            this.Context.Account.RecentVerses.Add(bibleVerse);

            await this.Bag.AccountStore.Update(this.Context.Account, this.Context.CancellationToken);
            this.Bag.TelemetryTracker.BibleVerseReserved(bibleVerse);

            return bibleVerse;
        }

        /// <summary>
        /// Creates or updates a checkpoint
        /// </summary>
        /// <param name="checkpointData">The checkpoint data</param>
        /// <param name="checkpointStatus">The checkpoint status</param>
        /// <returns>The response if it was already completed before, or null</returns>
        private async Task<OperationResponse<TResponse>> SetCheckpoint(
            CheckpointData<TResponse> checkpointData,
            CheckpointStatus checkpointStatus)
        {
            if (this.Context == null || string.IsNullOrWhiteSpace(this.Context.Account.AccountId))
            {
                throw new ArgumentException("The account ID in checkpoint data is required");
            }

            if (this.checkpoint == null)
            {
                OperationResponse<TResponse> response = await this.CreateCheckpoint(checkpointData, checkpointStatus);

                if (response != null)
                {
                    return response;
                }

                TimeSpan visibilityTimeout = this.Bag.Settings.CheckpointVisibilityTimeout;
                if (checkpointData.ProcessAfter != null)
                {
                    visibilityTimeout = checkpointData.ProcessAfter.Value - Clock.UtcNow;
                }

                if (visibilityTimeout < TimeSpan.Zero)
                {
                    visibilityTimeout = TimeSpan.Zero;
                }

                await this.Bag.CompensationClient.QueueCheckpoint(
                    this.checkpoint,
                    visibilityTimeout,
                    this.Context.CancellationToken);
            }

            return await this.UpdateCheckpoint(checkpointData, checkpointStatus);
        }

        private async Task<OperationResponse<TResponse>> UpdateCheckpoint(CheckpointData<TResponse> checkpointData, CheckpointStatus checkpointStatus)
        {
            Debug.Assert(this.checkpoint != null, "Cannot update null checkpoint");

            this.checkpoint.CheckpointData = JsonConvert.SerializeObject(checkpointData);
            this.checkpoint.Status = checkpointStatus;
            await this.Bag.CompensationClient.UpdateCheckpoint(this.checkpoint, this.Context.CancellationToken);

            return null;
        }

        private async Task<OperationResponse<TResponse>> CreateCheckpoint(CheckpointData<TResponse> checkpointData, CheckpointStatus checkpointStatus)
        {
            Debug.Assert(this.checkpoint == null, "Checkpoint is already created");

            this.checkpoint = new Checkpoint
            {
                AccountId = this.Context.Account.AccountId,
                TrackingId = this.Context.TrackingId,
                OperationType = this.operationType,
                Status = checkpointStatus,
                CheckpointData = checkpointData == null ? null : JsonConvert.SerializeObject(checkpointData),
            };

            bool checkpointAlreadyExists = true;
            try
            {
                await this.Bag.CompensationClient.InsertCheckpoint(this.checkpoint, this.Context.CancellationToken);
                checkpointAlreadyExists = false;
            }
            catch (DuplicateKeyException)
            {
            }

            if (checkpointAlreadyExists)
            {
                this.checkpoint = await this.Bag.CompensationClient.GetCheckpoint(
                    this.checkpoint.PartitionKey,
                    this.checkpoint.RowKey,
                    this.Context.CancellationToken);
                Debug.Assert(this.checkpoint != null, "Cannot create nor find the checkpoint");

                if (this.checkpoint.CheckpointData != null)
                {
                    CheckpointData<TResponse> existingCheckpointData =
                        JsonConvert.DeserializeObject<CheckpointData<TResponse>>(this.checkpoint.CheckpointData);

                    if (existingCheckpointData.Response != null)
                    {
                        return existingCheckpointData.Response;
                    }
                }

                return new OperationResponse<TResponse>
                {
                    StatusCode = HttpStatusCode.Conflict,
                    ErrorCode = CommonErrorCodes.OperationInProgress,
                };
            }

            return null;
        }
    }
}
