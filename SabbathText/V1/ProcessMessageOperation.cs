namespace SabbathText.V1
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using SabbathText.Entities;

    /// <summary>
    /// This operation processes an incoming message
    /// </summary>
    public class ProcessMessageOperation : BaseOperation<bool>
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
            this.checkpointData.OperationState = ProcessMessageState.Processing;

            return this.DelayProcessingCheckpoint(
                this.checkpointData,
                HttpStatusCode.Accepted,
                true /* response data */);
        }

        private Task<OperationResponse<bool>> EnterProcessing()
        {
            ConversationProcessor processor = new ConversationProcessor();
            Message outgoingMessage = processor.Process(this.Context.Account, this.checkpointData.IncomingMessage.Body);

            return this.TransitionToUpdatingAccount(outgoingMessage);
        }

        private async Task<OperationResponse<bool>> TransitionToSendingReply(Message outgoingMessage)
        {
            this.checkpointData.OutgoingMessage = outgoingMessage;
            this.checkpointData.OperationState = ProcessMessageState.SendingReply;

            return
                await this.CreateOrUpdateCheckpoint(this.checkpointData) ??
                await this.EnterSendingReply();
        }

        private async Task<OperationResponse<bool>> EnterSendingReply()
        {
            await this.Context.MessageClient.SendMessage(this.checkpointData.OutgoingMessage);

            return await this.TranstitionToUpdatingMessageStatus();
        }

        private async Task<OperationResponse<bool>> TranstitionToUpdatingMessageStatus()
        {
            this.checkpointData.OperationState = ProcessMessageState.UpdatingMessageStatus;
            return
                await this.CreateOrUpdateCheckpoint(this.checkpointData) ??
                await this.EnterUpdatingMessageStatus();
        }

        private async Task<OperationResponse<bool>> EnterUpdatingMessageStatus()
        {
            MessageEntity incomingMessage = this.Context.Account.RecentMessages.FirstOrDefault(m =>
                m.MessageId == this.checkpointData.IncomingMessageId);

            if (incomingMessage != null)
            {
                incomingMessage.Status = MessageStatus.Responded;
            }

            MessageEntity outgoingMessage = this.Context.Account.RecentMessages.FirstOrDefault(m =>
                m.MessageId == this.checkpointData.OutgoingMessageId);

            if (outgoingMessage != null)
            {
                outgoingMessage.Status = MessageStatus.Sent;
            }

            await this.Context.AccountStore.Update(this.Context.Account);
            return await this.CompleteCheckpoint(this.checkpointData, HttpStatusCode.OK, true);
        }

        private async Task<OperationResponse<bool>> TransitionToUpdatingAccount(Message outgoingMessage)
        {
            this.checkpointData.IncomingMessageId = Guid.NewGuid().ToString();
            this.checkpointData.OutgoingMessageId = Guid.NewGuid().ToString();
            this.checkpointData.OutgoingMessage = outgoingMessage;

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
                        Status = MessageStatus.Received,
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
                        Status = MessageStatus.Queued,
                        Template = this.checkpointData.OutgoingMessage.Template,
                    });
            }
            
            await this.Context.AccountStore.Update(this.Context.Account);
            return await this.TransitionToSendingReply(this.checkpointData.OutgoingMessage);
        }

        /// <summary>
        /// Resumes the operation
        /// </summary>
        /// <param name="serializedCheckpointData">The serialized checkpoint data</param>
        /// <returns>The operation response</returns>
        protected override Task<OperationResponse<bool>> Resume(string serializedCheckpointData)
        {
            this.checkpointData = JsonConvert.DeserializeObject<ProcessMessageCheckpointData>(serializedCheckpointData);

            switch (this.checkpointData.OperationState)
            {
                case ProcessMessageState.Processing:
                    return this.EnterProcessing();
                case ProcessMessageState.SendingReply:
                    return this.EnterSendingReply();
                case ProcessMessageState.UpdatingAccount:
                    return this.EnterUpdatingAccount();
                default:
                    throw new NotSupportedException("{0} state is not supported for resume.".InvariantFormat(this.checkpointData.OperationState));
            }
        }
    }
}
