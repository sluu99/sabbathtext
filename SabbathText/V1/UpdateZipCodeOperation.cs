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
                case GenericOperationState.ProcessingMessage:
                    return this.EnterProcessMessage();
            }

            throw new NotImplementedException();
        }

        private Task<OperationResponse<bool>> TransitionToProcessMessage(Message incomingMessage)
        {
            this.checkpointData.IncomingMessage = incomingMessage;
            this.checkpointData.State = GenericOperationState.ProcessingMessage;
            this.checkpointData.CurrentZipCode = this.Context.Account.ZipCode;

            return this.DelayProcessingCheckpoint(this.checkpointData, HttpStatusCode.Accepted, true);
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
                    await this.IndexZipCode();

                    outgoingMessage = Message.CreateZipCodeUpdated(this.Context.Account.PhoneNumber, location);
                }
            }

            if (outgoingMessage != null)
            {
                await this.Bag.MessageClient.SendMessage(outgoingMessage);
            }

            return await this.TranstitionToUpdateAccount(outgoingMessage);
        }

        private async Task IndexZipCode()
        {
            LocationEntity location = new LocationEntity { ZipCode = this.Context.Account.ZipCode };
            await this.Bag.LocationStore.InsertOrGet(location, this.Context.CancellationToken);

            ZipCodeAccountIdIndex index = new ZipCodeAccountIdIndex
            {
                ZipCode = this.Context.Account.ZipCode,
                AccountId = this.Context.Account.AccountId,
            };

            await this.Bag.ZipCodeAccountIdIndices.InsertOrGet(index, this.Context.CancellationToken);
        }

        private async Task<OperationResponse<bool>> TranstitionToUpdateAccount(Message outgoingMessage)
        {
            this.checkpointData.State = GenericOperationState.UpdatingAccount;
            this.checkpointData.OutgoingMessage = outgoingMessage;
            this.checkpointData.IncomingMessageId = Guid.NewGuid().ToString();
            this.checkpointData.OutgoingMessageId = Guid.NewGuid().ToString();

            return
                await this.CreateOrUpdateCheckpoint(this.checkpointData) ??
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
