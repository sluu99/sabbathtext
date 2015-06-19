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
    /// An operation to send out a Bible verse
    /// </summary>
    public class BibleVerseOperation : BaseOperation<bool>
    {
        private BibleVerseOperationCheckpointData checkpointData;

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="context">The operation context</param>
        public BibleVerseOperation(OperationContext context)
            : base(context, "BibleVerseOperation.V1")
        {
        }

        /// <summary>
        /// Runs the operation
        /// </summary>
        /// <param name="incomingMessage">The incoming message</param>
        /// <returns>The operation response</returns>
        public Task<OperationResponse<bool>> Run(Message incomingMessage)
        {
            if (incomingMessage == null)
            {
                throw new ArgumentNullException("incomingMessage");
            }

            this.checkpointData = new BibleVerseOperationCheckpointData(this.Context.Account.AccountId);
            return this.TransitionToProcessMessage(incomingMessage);
        }

        private async Task<OperationResponse<bool>> TransitionToProcessMessage(Message incomingMessage)
        {
            this.checkpointData.OperationState = RespondingMessageOperationState.ProcessingMessage;
            this.checkpointData.IncomingMessageId = Guid.NewGuid().ToString();
            this.checkpointData.IncomingMessage = incomingMessage;

            return
                await this.SetCheckpoint(this.checkpointData) ??
                await this.EnterProcessMessage();
        }

        private Task<OperationResponse<bool>> EnterProcessMessage()
        {
            // don't really have much to process
            return this.TransitionToSendResponse();
        }

        private async Task<OperationResponse<bool>> TransitionToSendResponse()
        {
            this.checkpointData.OperationState = RespondingMessageOperationState.SendingResponse;
            this.checkpointData.OutgoingMessageId = Guid.NewGuid().ToString();

            return
                await this.SetCheckpoint(this.checkpointData) ??
                await this.EnterSendResponse();
        }

        private async Task<OperationResponse<bool>> EnterSendResponse()
        {
            string selectedBibleVerse = await this.ReserveBibleVerse(this.checkpointData.OutgoingMessageId);
            
            Message outgoingMessage = Message.CreateBibleVerse(
                this.Context.Account.PhoneNumber,
                selectedBibleVerse,
                DomainData.BibleVerses[selectedBibleVerse]);

            await this.Bag.MessageClient.SendMessage(
                outgoingMessage,
                this.checkpointData.OutgoingMessageId,
                this.Context.CancellationToken);

            return await this.TransitionToUpdateAccount(outgoingMessage);
        }

        private async Task<OperationResponse<bool>> TransitionToUpdateAccount(Message outgoingMessage)
        {
            this.checkpointData.OperationState = RespondingMessageOperationState.UpdatingAccount;
            this.checkpointData.OutgoingMessage = outgoingMessage;

            return
                await this.SetCheckpoint(this.checkpointData) ??
                await this.EnterUpdateAccount();
        }

        private async Task<OperationResponse<bool>> EnterUpdateAccount()
        {
            bool reservedBibleVerseRemoved = false;
            if (this.Context.Account.ReservedBibleVerse.ContainsKey(this.checkpointData.OutgoingMessageId))
            {
                this.Context.Account.ReservedBibleVerse.Remove(this.checkpointData.OutgoingMessageId);
                reservedBibleVerseRemoved = true;
            }

            bool messageAdded = this.AddProcessedMessages(
                this.checkpointData.IncomingMessageId,
                this.checkpointData.IncomingMessage,
                this.checkpointData.OutgoingMessageId,
                this.checkpointData.OutgoingMessage);

            if (messageAdded || reservedBibleVerseRemoved)
            {
                await this.Bag.AccountStore.Update(this.Context.Account, this.Context.CancellationToken);
            }

            return await this.CompleteCheckpoint(this.checkpointData, HttpStatusCode.OK, true);
        }

        /// <summary>
        /// Resumes the operation
        /// </summary>
        /// <param name="serializedCheckpointData">The serialized checkpoint data</param>
        /// <returns>The operation response</returns>
        protected override Task<OperationResponse<bool>> Resume(string serializedCheckpointData)
        {
            this.checkpointData = JsonConvert.DeserializeObject<BibleVerseOperationCheckpointData>(serializedCheckpointData);

            switch (this.checkpointData.OperationState)
            {
                case RespondingMessageOperationState.ProcessingMessage:
                    return this.EnterProcessMessage();
                case RespondingMessageOperationState.SendingResponse:
                    return this.EnterSendResponse();
                case RespondingMessageOperationState.UpdatingAccount:
                    return this.EnterUpdateAccount();
                default:
                    throw new InvalidOperationException("Cannot resume with the state {0}".InvariantFormat(this.checkpointData.OperationState));
            }
        }
    }
}
