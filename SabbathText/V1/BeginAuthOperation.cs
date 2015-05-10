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
    /// An operation to begin the authentication process for an account
    /// </summary>
    public class BeginAuthOperation : BaseOperation<bool>
    {
        private BeginAuthOperationCheckpointData checkpointData;

        /// <summary>
        /// Creates a new instance of this operation
        /// </summary>
        /// <param name="context">The operation context.</param>
        public BeginAuthOperation(OperationContext context)
            : base(context, "BeginAuthOperation.V1")
        {
        }

        /// <summary>
        /// Runs the operation
        /// </summary>
        /// <param name="incomingMessage">The incoming message.</param>
        /// <returns>The operation response</returns>
        public Task<OperationResponse<bool>> Run(Message incomingMessage)
        {
            this.checkpointData = new BeginAuthOperationCheckpointData(this.Context.Account.AccountId);
            return this.TransitionToProcessMessage(incomingMessage);
        }

        /// <summary>
        /// Resumes the operation
        /// </summary>
        /// <param name="serializedCheckpointData">The serialized checkpoint data.</param>
        /// <returns>The operation response.</returns>
        protected override Task<OperationResponse<bool>> Resume(string serializedCheckpointData)
        {
            this.checkpointData = JsonConvert.DeserializeObject<BeginAuthOperationCheckpointData>(serializedCheckpointData);

            if (this.checkpointData.OperationState == GenericOperationState.ProcessingMessage)
            {
                // Woke up from delay processing
                return this.EnterProcessMessage();
            }

            // We don't retry with this operation.
            // If we failed at the beginning, a retry may expose the user to unwanted time windows of authentication.
            return Task.FromResult<OperationResponse<bool>>(
                new OperationResponse<bool>
                {
                    Data = false,
                    StatusCode = HttpStatusCode.NoContent,
                });
        }

        private Task<OperationResponse<bool>> TransitionToProcessMessage(Message incomingMessage)
        {
            this.checkpointData.OperationState = GenericOperationState.ProcessingMessage;
            this.checkpointData.IncomingMessage = incomingMessage;
            return this.DelayProcessingCheckpoint(TimeSpan.Zero, this.checkpointData, HttpStatusCode.Accepted, true);
        }

        private async Task<OperationResponse<bool>> EnterProcessMessage()
        {
            string authKey = AuthKey.Create();
            this.Context.Account.AuthKey = authKey;
            this.Context.Account.AuthKeyExpiration = Clock.UtcNow + this.Bag.Settings.AuthKeyLifeSpan;
            await this.Bag.AccountStore.Update(this.Context.Account, this.Context.CancellationToken);

            return await this.TransitionToSendMessage(authKey);
        }

        private async Task<OperationResponse<bool>> TransitionToSendMessage(string authKey)
        {
            this.checkpointData.OperationState = GenericOperationState.SendingResponse;
            this.checkpointData.AuthKey = authKey;

            return
                await this.CreateOrUpdateCheckpoint(this.checkpointData) ??
                await this.EnterSendMessage();
        }

        private async Task<OperationResponse<bool>> EnterSendMessage()
        {
            Message outgoingMessage = null;

            if (this.Context.Account.AuthKey == this.checkpointData.AuthKey &&
                this.Context.Account.AuthKeyExpiration > Clock.UtcNow)
            {
                // auth key has not changed and has not expired yet
                outgoingMessage = Message.CreateAuthKeyCreated(
                    this.Context.Account.PhoneNumber,
                    this.checkpointData.AuthKey,
                    (int)(this.Context.Account.AuthKeyExpiration - Clock.UtcNow).TotalMinutes);

                await this.Bag.MessageClient.SendMessage(outgoingMessage);
            }

            return await this.TransitionToUpdateAccount(outgoingMessage);
        }

        private async Task<OperationResponse<bool>> TransitionToUpdateAccount(Message outgoingMessage)
        {
            this.checkpointData.OperationState = GenericOperationState.UpdatingAccount;
            this.checkpointData.OutgoingMessage = outgoingMessage;
            this.checkpointData.IncomingMessageId = Guid.NewGuid().ToString();
            this.checkpointData.OutgoingMessageId = Guid.NewGuid().ToString();

            return
                await this.CreateOrUpdateCheckpoint(this.checkpointData) ??
                await this.EnterUpdateAccountMessages();
        }

        private async Task<OperationResponse<bool>> EnterUpdateAccountMessages()
        {
            await this.AddProcessedMessages(
                this.checkpointData.IncomingMessageId,
                this.checkpointData.IncomingMessage,
                this.checkpointData.OutgoingMessageId,
                this.checkpointData.OutgoingMessage);

            return await this.CompleteCheckpoint(this.checkpointData, HttpStatusCode.Created, true);
        }        
    }
}
