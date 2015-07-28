namespace SabbathText.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// This operation processes incoming "Hello" or "Hi" messages
    /// </summary>
    public class HelloMessageOperation : BaseOperation<bool>
    {
        private HelloMessageOperationCheckpointData checkpointData;

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="context">The operation context used for this operation.</param>
        public HelloMessageOperation(OperationContext context)
            : base(context, "HelloMessageOperation.V1")
        {
            this.checkpointData = new HelloMessageOperationCheckpointData(this.Context.Account.AccountId);
        }

        /// <summary>
        /// Runs the operation
        /// </summary>
        /// <param name="incomingMessage">The incoming message.</param>
        /// <returns>The operation response.</returns>
        public Task<OperationResponse<bool>> Run(Message incomingMessage)
        {
            return this.TransitionToSendMessage(incomingMessage);
        }

        private async Task<OperationResponse<bool>> TransitionToSendMessage(Message incomingMessage)
        {
            this.checkpointData.OperationState = RespondingMessageOperationState.SendingResponse;
            this.checkpointData.IncomingMessage = incomingMessage;
            this.checkpointData.OutgoingMessageId = Guid.NewGuid().ToString();
            this.checkpointData.AccountStatus = this.Context.Account.Status;

            return
                await this.SetCheckpoint(this.checkpointData) ??
                await this.EnterSendMessage();
        }

        private async Task<OperationResponse<bool>> EnterSendMessage()
        {
            Message outgoingMessage = null;
            if (this.checkpointData.AccountStatus == Entities.AccountStatus.BrandNew)
            {
                outgoingMessage = Message.CreateGreetingMessage(this.Context.Account.PhoneNumber);
            }
            else if (this.checkpointData.AccountStatus == Entities.AccountStatus.Subscribed)
            {
                outgoingMessage = Message.CreateCommandListMessage(this.Context.Account.PhoneNumber);
            }
            else
            {
                throw new InvalidOperationException(string.Format("Hello messages are not supported for accounts with status {0}", this.checkpointData.AccountStatus));
            }

            await this.Bag.MessageClient.SendMessage(outgoingMessage, this.checkpointData.OutgoingMessageId /* trackingId */, this.Context.CancellationToken);

            return await this.TransitionToUpdateAccount(
                this.checkpointData.IncomingMessage,
                this.checkpointData.OutgoingMessageId,
                outgoingMessage);
        }

        private async Task<OperationResponse<bool>> TransitionToUpdateAccount(Message incomingMessage, string outgoingMessageId, Message outgoingMessage)
        {
            this.checkpointData.OperationState = RespondingMessageOperationState.UpdatingAccount;
            this.checkpointData.IncomingMessage = incomingMessage;
            this.checkpointData.OutgoingMessage = outgoingMessage;
            this.checkpointData.OutgoingMessageId = outgoingMessageId;
            this.checkpointData.IncomingMessageId = Guid.NewGuid().ToString();

            return
                await this.SetCheckpoint(this.checkpointData) ??
                await this.EnterUpdateAccount();
        }

        private async Task<OperationResponse<bool>> EnterUpdateAccount()
        {
            bool messageAdded = this.AddProcessedMessages(
                this.checkpointData.IncomingMessageId,
                this.checkpointData.IncomingMessage,
                this.checkpointData.OutgoingMessageId,
                this.checkpointData.OutgoingMessage);

            if (messageAdded)
            {
                await this.Bag.AccountStore.Update(this.Context.Account, this.Context.CancellationToken);
            }

            return await this.CompleteCheckpoint(this.checkpointData, System.Net.HttpStatusCode.OK, true);
        }

        /// <summary>
        /// Resumes the operation.
        /// </summary>
        /// <param name="serializedCheckpointData">The serialized checkpoint data.</param>
        /// <returns>The operation response.</returns>
        protected override Task<OperationResponse<bool>> Resume(string serializedCheckpointData)
        {
            this.checkpointData = JsonConvert.DeserializeObject<HelloMessageOperationCheckpointData>(serializedCheckpointData);
            
            switch (this.checkpointData.OperationState)
            {
                case RespondingMessageOperationState.SendingResponse:
                    return this.EnterSendMessage();
                case RespondingMessageOperationState.UpdatingAccount:
                    return this.EnterSendMessage();
                default:
                    throw new InvalidOperationException(string.Format("HelloMessageOperation cannot be resumed with the state {0}", this.checkpointData.OperationState));
            }
        }
    }
}
