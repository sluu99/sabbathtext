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
    /// An operation which sends the user a greeting message
    /// </summary>
    public class GreetUserOperation : BaseOperation<bool>
    {
        private GreetUserCheckpointData checkpointData;

        /// <summary>
        /// Creates a new instance of the operation
        /// </summary>
        /// <param name="context">The operation context</param>
        public GreetUserOperation(OperationContext context)
            : base(context, "GreetUserOperation.V1")
        {
        }

        /// <summary>
        /// Starts the operation
        /// </summary>
        /// <returns>The operation response</returns>
        public Task<OperationResponse<bool>> Run()
        {
            this.checkpointData = new GreetUserCheckpointData(this.Context.Account.AccountId);

            return this.TransitionToSendingMessage();
        }

        /// <summary>
        /// Resumes the operation
        /// </summary>
        /// <param name="serializedCheckpointData">The serialized operation checkpoint data</param>
        /// <returns>The operation response</returns>
        protected override Task<OperationResponse<bool>> Resume(string serializedCheckpointData)
        {
            this.checkpointData = JsonConvert.DeserializeObject<GreetUserCheckpointData>(serializedCheckpointData);

            switch (this.checkpointData.OperationState)
            {
                case GreetUserState.SendingMessage:
                    return this.EnterSendingMessage();
                case GreetUserState.UpdatingAccountContext:
                    return this.EnterUpdatingAccount();
            }

            throw new NotImplementedException();
        }
        
        private Task<OperationResponse<bool>> TransitionToSendingMessage()
        {
            this.checkpointData.OperationState = GreetUserState.SendingMessage;
            return this.DelayProcessingCheckpoint(
                this.checkpointData,
                HttpStatusCode.Accepted,
                responseData: true);
        }

        private async Task<OperationResponse<bool>> EnterSendingMessage()
        {
            Message message = Message.CreateGreetingMessage(this.Context.Account.PhoneNumber);
            await this.Context.MessageClient.SendMessage(message);

            this.checkpointData.Message = message;
            return await this.TransitionToUpdatingAccount();
        }

        private async Task<OperationResponse<bool>> TransitionToUpdatingAccount()
        {
            this.checkpointData.OperationState = GreetUserState.UpdatingAccountContext;
            this.checkpointData.MessageEntityId = Guid.NewGuid().ToString();

            return
                await this.CreateOrUpdateCheckpoint(this.checkpointData) ??
                await this.EnterUpdatingAccount();
        }

        private async Task<OperationResponse<bool>> EnterUpdatingAccount()
        {
            if (!this.Context.Account.RecentMessages.Any(
                    m => string.Equals(m.MessageId, this.checkpointData.MessageEntityId, StringComparison.InvariantCulture)))
            {
                // the account does not contain this entity
                MessageEntity messageEntity = new MessageEntity
                {
                    AccountId = this.Context.Account.AccountId,
                    Body = this.checkpointData.Message.Body,
                    Direction = MessageDirection.Outgoing,
                    MessageId = this.checkpointData.MessageEntityId,
                    MessageTimestamp = this.checkpointData.Message.Timestamp,
                    Recipient = this.Context.Account.PhoneNumber,
                    Status = MessageStatus.Sent,
                    Template = MessageTemplate.Greetings,
                };

                this.Context.Account.RecentMessages.Add(messageEntity);
                this.Context.Account.ConversationContext = ConversationContext.Greetings;

                await this.Context.AccountStore.Update(this.Context.Account);
            }

            return await this.CompleteCheckpoint(this.checkpointData, HttpStatusCode.OK, responseData: true);
        }
    }
}