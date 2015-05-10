namespace SabbathText.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SabbathText.Entities;

    /// <summary>
    /// This operation processes incoming "subscribe" messages
    /// </summary>
    public class SubscribeMessageOperation : BaseOperation<bool>
    {
        private SubscribeMessageOperationCheckpointData checkpointData;

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="context">The operation context.</param>
        public SubscribeMessageOperation(OperationContext context)
            : base(context, "SubscribeMessageOperation.V1")
        {
        }

        /// <summary>
        /// Run the operation.
        /// </summary>
        /// <param name="incomingMessage">The incoming message.</param>
        /// <returns>The operation response.</returns>
        public Task<OperationResponse<bool>> Run(Message incomingMessage)
        {
            this.checkpointData = new SubscribeMessageOperationCheckpointData(this.Context.Account.AccountId);
            return this.TransitionToProcessMessage(incomingMessage);
        }

        /// <summary>
        /// Resumes the operation.
        /// </summary>
        /// <param name="serializedCheckpointData">The serialized checkpoint data.</param>
        /// <returns>The operation response.</returns>
        protected override Task<OperationResponse<bool>> Resume(string serializedCheckpointData)
        {
            this.checkpointData = JsonConvert.DeserializeObject<SubscribeMessageOperationCheckpointData>(serializedCheckpointData);

            switch (this.checkpointData.State)
            {
                case GenericOperationState.ProcessingMessage:
                    return this.EnterProcessMessage();
                case GenericOperationState.UpdatingAccount:
                    return this.EnterUpdateAccount();
            }

            throw new NotImplementedException();
        }

        private Task<OperationResponse<bool>> TransitionToProcessMessage(Message incomingMessage)
        {
            this.checkpointData.State = GenericOperationState.ProcessingMessage;
            this.checkpointData.IncomingMessage = incomingMessage;
            return this.DelayProcessingCheckpoint(
                TimeSpan.Zero,
                this.checkpointData,
                HttpStatusCode.Accepted,
                true);
        }

        private async Task<OperationResponse<bool>> EnterProcessMessage()
        {
            bool alreadySubscribed = false;
            Message outgoingMessage;

            if (this.Context.Account.Status == AccountStatus.Subscribed &&
                string.IsNullOrWhiteSpace(this.Context.Account.ZipCode) == false)
            {
                outgoingMessage =
                    Message.CreateAlreadySubscribedWithZipCode(this.Context.Account.PhoneNumber, this.Context.Account.ZipCode);
                alreadySubscribed = true;
            }
            else
            {
                outgoingMessage =
                    Message.CreateSubscriptionConfirmed(this.Context.Account.PhoneNumber);
            }

            await this.Bag.MessageClient.SendMessage(outgoingMessage);
            return await this.TransitionToUpdateAccount(outgoingMessage, alreadySubscribed);
        }

        private async Task<OperationResponse<bool>> TransitionToUpdateAccount(Message outgoingMessage, bool alreadySubscribed)
        {
            this.checkpointData.OutgoingMessage = outgoingMessage;
            this.checkpointData.AccountAlreadySubscribed = alreadySubscribed;
            this.checkpointData.State = GenericOperationState.UpdatingAccount;
            this.checkpointData.StatusVersion = this.Context.Account.StatusVersion;
            this.checkpointData.IncomingMessageId = Guid.NewGuid().ToString();
            this.checkpointData.OutgoingMessageId = Guid.NewGuid().ToString();

            return
                await this.CreateOrUpdateCheckpoint(this.checkpointData) ??
                await this.EnterUpdateAccount();
        }

        private async Task<OperationResponse<bool>> EnterUpdateAccount()
        {
            if (this.checkpointData.AccountAlreadySubscribed == false &&
                //// make sure no other operation updated the status
                this.checkpointData.StatusVersion == this.Context.Account.StatusVersion)
            {
                this.Context.Account.Status = AccountStatus.Subscribed;
                this.Context.Account.StatusVersion++;
                this.Context.Account.ZipCode = null;
                this.Context.Account.ConversationContext = ConversationContext.SubscriptionConfirmed;

                MessageEntity incomingEntity = this.checkpointData.IncomingMessage.ToEntity(
                    this.Context.Account.AccountId,
                    this.checkpointData.IncomingMessageId,
                    MessageDirection.Incoming,
                    MessageStatus.Responded);
                TryAddMessageEntity(this.Context.Account, incomingEntity);

                MessageEntity outgoingEntity = this.checkpointData.OutgoingMessage.ToEntity(
                    this.Context.Account.AccountId,
                    this.checkpointData.OutgoingMessageId,
                    MessageDirection.Outgoing,
                    MessageStatus.Sent);
                TryAddMessageEntity(this.Context.Account, outgoingEntity);

                await this.Bag.AccountStore.Update(this.Context.Account, this.Context.CancellationToken);
            }

            return await this.CompleteCheckpoint(this.checkpointData, HttpStatusCode.OK, true);
        }
    }
}
