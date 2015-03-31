namespace SabbathText.V1
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using SabbathText.Entities;

    /// <summary>
    /// Process message operation state
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProcessMessageOperationState
    {
        /// <summary>
        /// The message is marked for processing
        /// </summary>
        MarkedForProcessing,

        /// <summary>
        /// The incoming message is being recorded
        /// </summary>
        RecordingIncomingMessage,

        /// <summary>
        /// Recording the outgoing message
        /// </summary>
        RecordingOutgoingMessage,

        /// <summary>
        /// Sending the outgoing message
        /// </summary>
        SendingOutgoingMessage,

        /// <summary>
        /// Updating the account conversation context
        /// </summary>
        UpdatingAccountContext,

        /// <summary>
        /// Updating the outgoing message status
        /// </summary>
        UpdatingOutgoingMessageStatus,

        /// <summary>
        /// Updating the incoming message status
        /// </summary>
        UpdatingIncomingMessageStatus,
    }

    /// <summary>
    /// This operation processes an incoming message
    /// </summary>
    public class ProcessMessageOperation : BaseOperation<bool, ProcessMessageOperationState>
    {
        private ProcessMessageCheckpointData checkpointData;
        private MessageEntity incomingMessageEntity;

        /// <summary>
        /// Creates a new instance of this operation
        /// </summary>
        /// <param name="context">The operation context</param>
        public ProcessMessageOperation(OperationContext context)
            : base(context, "ProcessMessageOperation.V1")
        {
        }

        /// <summary>
        /// Processes a message
        /// </summary>
        /// <param name="message">The message to be processed</param>
        /// <returns>The operation response</returns>
        public Task<OperationResponse<bool>> Run(Message message)
        {
            string phoneNumber = message.Recipient.ExtractUSPhoneNumber();

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return Task.FromResult(new OperationResponse<bool>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorCode = CommonErrorCodes.InvalidInput,
                    ErrorDescription = "The recipient must be a valid US phone number",
                });
            }

            this.checkpointData = new ProcessMessageCheckpointData
            {
                Message = message,
            };

            return this.TransitionToMarkedForProcessing();
        }

        private Task<OperationResponse<bool>> TransitionToMarkedForProcessing()
        {
            return this.DelayProcessingCheckpoint(
                this.checkpointData,
                HttpStatusCode.Accepted,
                true /* response data */);
        }

        /// <summary>
        /// This method will be called when the operation is resumed from delayed processing
        /// </summary>
        /// <returns>The operation response</returns>
        private Task<OperationResponse<bool>> EnterProcessing()
        {
            return this.TransitionToRecordingIncomingMessage();
        }

        private async Task<OperationResponse<bool>> TransitionToRecordingIncomingMessage()
        {
            this.checkpointData.IncomingMessageId = Guid.NewGuid().ToString();
            this.checkpointData.State = ProcessMessageOperationState.RecordingIncomingMessage;

            return
                await this.CreateOrUpdateCheckpoint(this.checkpointData) ??
                await this.EnterRecordingIncomingMessage();
        }

        private async Task<OperationResponse<bool>> EnterRecordingIncomingMessage()
        {
            this.incomingMessageEntity = new MessageEntity
            {
                AccountId = this.Context.Account.AccountId,
                MessageId = this.checkpointData.IncomingMessageId,
                PartitionKey = this.Context.Account.AccountId,
                RowKey = this.checkpointData.IncomingMessageId,
                Sender = this.checkpointData.Message.Sender,
                Recipient = this.checkpointData.Message.Recipient,
                Body = this.checkpointData.Message.Body,
                Direction = MessageDirection.Incoming,
                Status = MessageStatus.Received,
                MessageTimestamp = this.checkpointData.Message.Timestamp,
            };

            await this.Context.MessageStore.InsertOrGet(this.incomingMessageEntity);
            
            return await this.TransitionToRecordingOutgoingMessage();
        }

        private async Task<OperationResponse<bool>> TransitionToRecordingOutgoingMessage()
        {
            this.checkpointData.OutgoingMessageId = Guid.NewGuid().ToString();
            this.checkpointData.State = ProcessMessageOperationState.RecordingOutgoingMessage;

            return
                await this.CreateOrUpdateCheckpoint(this.checkpointData) ??
                await this.EnterRecordingOutgoingMessage();
        }

        private Task<OperationResponse<bool>> EnterRecordingOutgoingMessage()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Resumes the operation
        /// </summary>
        /// <param name="serializedCheckpointData">The serialized checkpoint data</param>
        /// <returns>The operation response</returns>
        protected override Task<OperationResponse<bool>> Resume(string serializedCheckpointData)
        {
            throw new NotImplementedException();
        }

        private class ProcessMessageCheckpointData : CheckpointData<bool, ProcessMessageOperationState>
        {
            /// <summary>
            /// Gets or sets the message to be processed
            /// </summary>
            public Message Message { get; set; }

            /// <summary>
            /// Gets or sets the ID of the incoming message
            /// </summary>
            public string IncomingMessageId { get; set; }

            /// <summary>
            /// Gets or sets the ID of the outgoing message
            /// </summary>
            public string OutgoingMessageId { get; set; }

            /// <summary>
            /// Gets or sets the operation state
            /// </summary>
            public ProcessMessageOperationState State { get; set; }
        }
    }
}
