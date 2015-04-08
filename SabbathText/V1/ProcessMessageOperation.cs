namespace SabbathText.V1
{
    using System;
    using System.Linq;
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
        /// The message is being processed
        /// </summary>
        Processing,

        /// <summary>
        /// Sending the outgoing message
        /// </summary>
        SendingReply,

        /// <summary>
        /// Updating the account conversation context
        /// </summary>
        UpdatingAccount,
    }

    /// <summary>
    /// This operation processes an incoming message
    /// </summary>
    public class ProcessMessageOperation : BaseOperation<bool, ProcessMessageOperationState>
    {
        private ProcessMessageCheckpointData checkpointData;

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
            this.checkpointData = new ProcessMessageCheckpointData(this.Context.Account.AccountId);
            return this.TransitionToProcessing(message);
        }

        private Task<OperationResponse<bool>> TransitionToProcessing(Message message)
        {
            this.checkpointData.IncomingMessage = message;
            this.checkpointData.State = ProcessMessageOperationState.Processing;

            return this.DelayProcessingCheckpoint(
                this.checkpointData,
                HttpStatusCode.Accepted,
                true /* response data */);
        }

        private Task<OperationResponse<bool>> EnterProcessing()
        {
            Message outgoingMessage = null;
            ConversationContext conversationContext = ConversationContext.Unknown;
            string body = this.checkpointData.IncomingMessage.Body.ExtractAlphaNumericSpace().Trim();
            string recipient = this.Context.Account.PhoneNumber;

            if (string.IsNullOrEmpty(body))
            {
                outgoingMessage = Message.CreateNotUnderstandable(recipient);
            }

            if (this.Context.Account.ConversationContext == ConversationContext.Unknown)
            {
                if ("subscribe".OicEquals(body))
                {
                    conversationContext = ConversationContext.SubscriptionConfirmed;
                    outgoingMessage = Message.CreateSubscriptionConfirmed(recipient);
                }
            }
            else if (this.Context.Account.ConversationContext == ConversationContext.Greetings)
            {
                if ("subscribe".OicEquals(body) ||
                    "yes".OicEquals(body) ||
                    "sure".OicEquals(body))
                {
                    conversationContext = ConversationContext.SubscriptionConfirmed;
                    outgoingMessage = Message.CreateSubscriptionConfirmed(recipient);
                }
            }

            if (outgoingMessage == null)
            {
                outgoingMessage = Message.CreateNotUnderstandable(recipient);
            }
            
            return this.TransitionToSendingReply(outgoingMessage, conversationContext);
        }

        private async Task<OperationResponse<bool>> TransitionToSendingReply(Message outgoingMessage, ConversationContext conversationContext)
        {
            this.checkpointData.OutgoingMessage = outgoingMessage;
            this.checkpointData.ConversationContext = conversationContext;

            return
                await this.CreateOrUpdateCheckpoint(this.checkpointData) ??
                await this.EnterSendingReply();
        }

        private async Task<OperationResponse<bool>> EnterSendingReply()
        {
            await this.Context.MessageClient.SendMessage(this.checkpointData.OutgoingMessage);

            return await this.TransitionToUpdatingAccount();
        }

        private async Task<OperationResponse<bool>> TransitionToUpdatingAccount()
        {
            this.checkpointData.IncomingMessageId = Guid.NewGuid().ToString();
            this.checkpointData.OutgoingMessageId = Guid.NewGuid().ToString();

            return
                await this.CreateOrUpdateCheckpoint(this.checkpointData) ??
                await this.EnterUpdatingAccount();
        }

        private async Task<OperationResponse<bool>> EnterUpdatingAccount()
        {
            if (this.Context.Account.RecentMessages.Any(m => m.MessageId == this.checkpointData.IncomingMessageId) == false)
            {
                this.Context.Account.RecentMessages.Add(
                    new MessageEntity
                    {
                        AccountId = this.Context.Account.AccountId,
                        Body = this.checkpointData.IncomingMessage.Body,
                        Direction = MessageDirection.Incoming,
                        MessageId = this.checkpointData.IncomingMessageId,
                        MessageTimestamp = this.checkpointData.IncomingMessage.Timestamp,
                        PartitionKey = this.Context.Account.AccountId,
                        Recipient = this.checkpointData.IncomingMessage.Recipient,
                        RowKey = this.checkpointData.IncomingMessageId,
                        Sender = this.checkpointData.IncomingMessage.Sender,
                        Status = MessageStatus.Responded,
                        Template = this.checkpointData.IncomingMessage.Template,
                    });
            }

            if (this.Context.Account.RecentMessages.Any(m => m.MessageId == this.checkpointData.OutgoingMessageId) == false)
            {
                this.Context.Account.RecentMessages.Add(
                    new MessageEntity
                    {
                        AccountId = this.Context.Account.AccountId,
                        Body = this.checkpointData.OutgoingMessage.Body,
                        Direction = MessageDirection.Outgoing,
                        MessageId = this.checkpointData.OutgoingMessageId,
                        MessageTimestamp = this.checkpointData.OutgoingMessage.Timestamp,
                        PartitionKey = this.Context.Account.AccountId,
                        Recipient = this.checkpointData.OutgoingMessage.Recipient,
                        RowKey = this.checkpointData.OutgoingMessageId,
                        Sender = this.checkpointData.OutgoingMessage.Sender,
                        Status = MessageStatus.Sent,
                        Template = this.checkpointData.OutgoingMessage.Template,
                    });
            }

            if (this.checkpointData.ConversationContext != ConversationContext.Unknown)
            {
                // we never change the conversation context to Unknown.
                // we only use that when we don't understand an incoming message
                this.Context.Account.ConversationContext = this.checkpointData.ConversationContext;
            }

            await this.Context.AccountStore.Update(this.Context.Account);
            return await this.CompleteCheckpoint(this.checkpointData, HttpStatusCode.OK, true);
        }

        /// <summary>
        /// Resumes the operation
        /// </summary>
        /// <param name="serializedCheckpointData">The serialized checkpoint data</param>
        /// <returns>The operation response</returns>
        protected override Task<OperationResponse<bool>> Resume(string serializedCheckpointData)
        {
            this.checkpointData = JsonConvert.DeserializeObject<ProcessMessageCheckpointData>(serializedCheckpointData);

            switch (this.checkpointData.State)
            {
                case ProcessMessageOperationState.Processing:
                    return this.EnterProcessing();
                default:
                    throw new NotSupportedException("{0} state is not supported for resume.".InvariantFormat(this.checkpointData.State));
            }
        }

        private class ProcessMessageCheckpointData : CheckpointData<bool, ProcessMessageOperationState>
        {
            public ProcessMessageCheckpointData(string accountId) : base(accountId)
            {
            }

            /// <summary>
            /// Gets or sets the message to be processed
            /// </summary>
            public Message IncomingMessage { get; set; }

            /// <summary>
            /// Gets or sets the ID of the incoming message
            /// </summary>
            public string IncomingMessageId { get; set; }

            /// <summary>
            /// Gets or sets the ID of the outgoing message
            /// </summary>
            public string OutgoingMessageId { get; set; }

            /// <summary>
            /// Gets or sets the outgoing message.
            /// </summary>
            public Message OutgoingMessage { get; set; }

            /// <summary>
            /// Gets or sets the operation state
            /// </summary>
            public ProcessMessageOperationState State { get; set; }

            /// <summary>
            /// Gets or sets the conversation context.
            /// </summary>
            public ConversationContext ConversationContext { get; set; }
        }
    }
}
