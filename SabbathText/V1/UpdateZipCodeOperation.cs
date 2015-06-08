namespace SabbathText.V1
{
    using System;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SabbathText.Entities;
    using SabbathText.Location.V1;

    /// <summary>
    /// The operation to update an account ZIP code.
    /// </summary>
    public class UpdateZipCodeOperation : BaseOperation<bool>
    {
        private static readonly Regex ZipMessageRegex = new Regex(@"^Zip(?:Code)?\s*(?<ZipCode>\d+)$", RegexOptions.IgnoreCase);

        private UpdateZipCodeOperationCheckpointData checkpointData;

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="context">The operation context.</param>
        public UpdateZipCodeOperation(OperationContext context)
            : base(context, "UpdateZipCodeOperation.V1")
        {
        }

        /// <summary>
        /// Starts the operation.
        /// </summary>
        /// <param name="incomingMessage">The incoming message.</param>
        /// <returns>The operation response.</returns>
        public Task<OperationResponse<bool>> Run(Message incomingMessage)
        {
            this.checkpointData = new UpdateZipCodeOperationCheckpointData(this.Context.Account.AccountId);
            return this.TransitionToProcessMessage(incomingMessage);
        }
        
        /// <summary>
        /// Resumes the operation.
        /// </summary>
        /// <param name="serializedCheckpointData">The serialized operation data.</param>
        /// <returns>The operation response.</returns>
        protected override Task<OperationResponse<bool>> Resume(string serializedCheckpointData)
        {
            this.checkpointData = JsonConvert.DeserializeObject<UpdateZipCodeOperationCheckpointData>(serializedCheckpointData);

            switch (this.checkpointData.State)
            {
                case RespondingMessageOperationState.ProcessingMessage:
                    return this.EnterProcessMessage();
            }

            throw new NotImplementedException();
        }

        private async Task<OperationResponse<bool>> TransitionToProcessMessage(Message incomingMessage)
        {
            this.checkpointData.State = RespondingMessageOperationState.ProcessingMessage;
            this.checkpointData.IncomingMessage = incomingMessage;
            this.checkpointData.CurrentZipCode = this.Context.Account.ZipCode;
            this.checkpointData.OutgoingMessageId = Guid.NewGuid().ToString();

            return
                await this.SetCheckpoint(this.checkpointData) ??
                await this.EnterProcessMessage();
        }

        private async Task<OperationResponse<bool>> EnterProcessMessage()
        {
            Message outgoingMessage = null;
            string body = this.checkpointData.IncomingMessage.Body.ExtractAlphaNumericSpace().Trim();
            
            Match match = ZipMessageRegex.Match(body);
            if (match.Success == false ||
                match.Groups == null ||
                match.Groups["ZipCode"] == null ||
                string.IsNullOrWhiteSpace(match.Groups["ZipCode"].Value))
            {
                outgoingMessage = Message.CreateUpdateZipInstruction(this.Context.Account.PhoneNumber);
            }
            else
            {
                string zipCode = match.Groups["ZipCode"].Value;
                LocationInfo location = LocationInfo.FromZipCode(zipCode);

                if (location == null)
                {
                    outgoingMessage = Message.CreateLocationNotFound(this.Context.Account.PhoneNumber, zipCode);
                }
                else if (this.checkpointData.CurrentZipCode == this.Context.Account.ZipCode)
                {
                    // the ZIP code has not been updated
                    this.Context.Account.ZipCode = zipCode;
                    await this.Bag.AccountStore.Update(this.Context.Account, this.Context.CancellationToken);

                    if (this.Context.Account.Status != AccountStatus.Subscribed)
                    {
                        outgoingMessage = Message.CreateSubscriptionRequired(this.Context.Account.PhoneNumber);
                    }
                    else
                    {
                        outgoingMessage = Message.CreateSubscribedForLocation(this.Context.Account.PhoneNumber, location);
                    }
                }
            }

            if (outgoingMessage != null)
            {
                await this.Bag.MessageClient.SendMessage(outgoingMessage, this.checkpointData.OutgoingMessageId, this.Context.CancellationToken);
            }

            return await this.TranstitionToUpdateAccount(outgoingMessage);
        }
        
        private async Task<OperationResponse<bool>> TranstitionToUpdateAccount(Message outgoingMessage)
        {
            this.checkpointData.State = RespondingMessageOperationState.UpdatingAccount;
            this.checkpointData.OutgoingMessage = outgoingMessage;
            this.checkpointData.IncomingMessageId = Guid.NewGuid().ToString();

            return
                await this.SetCheckpoint(this.checkpointData) ??
                await this.EnterUpdateAccount();
        }

        private async Task<OperationResponse<bool>> EnterUpdateAccount()
        {
            MessageEntity incomingMessageEntity = this.checkpointData.IncomingMessage.ToEntity(
                this.Context.Account.AccountId,
                this.checkpointData.IncomingMessageId,
                MessageDirection.Incoming,
                this.checkpointData.OutgoingMessage == null ? MessageStatus.Received : MessageStatus.Responded);
            bool incomingMsgAdded = TryAddMessageEntity(this.Context.Account, incomingMessageEntity);

            bool outgoingMsgAdded = false;
            if (this.checkpointData.OutgoingMessage != null)
            {
                MessageEntity outgoingMessageEntity = this.checkpointData.OutgoingMessage.ToEntity(
                    this.Context.Account.AccountId,
                    this.checkpointData.OutgoingMessageId,
                    MessageDirection.Outgoing,
                    MessageStatus.Sent);
                outgoingMsgAdded = TryAddMessageEntity(this.Context.Account, outgoingMessageEntity);
            }

            if (incomingMsgAdded || outgoingMsgAdded)
            {
                await this.Bag.AccountStore.Update(this.Context.Account, this.Context.CancellationToken);
            }

            return await this.CompleteCheckpoint(this.checkpointData, HttpStatusCode.OK, true);
        }
    }
}
