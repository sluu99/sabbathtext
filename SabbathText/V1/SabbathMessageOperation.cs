namespace SabbathText.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using SabbathText.Entities;

    /// <summary>
    /// An operation to send out Sabbath messages.
    /// </summary>
    public class SabbathMessageOperation : BaseOperation<bool>
    {
        private SabbathMessageOperationCheckpointData checkpointData;

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="context">The operation context.</param>
        public SabbathMessageOperation(OperationContext context)
            : base(context, "SabbathMessageOperation.V1")
        {
        }

        /// <summary>
        /// Runs the operation.
        /// </summary>
        /// <returns>The operation response.</returns>
        public Task<OperationResponse<bool>> Run()
        {
            this.checkpointData = new SabbathMessageOperationCheckpointData(this.Context.Account.AccountId);
            return this.TransitionToSendMessage(delay: TimeSpan.Zero);
        }

        /// <summary>
        /// Runs the operation with a delay.
        /// </summary>
        /// <param name="delay">The delay of the operation.</param>
        /// <returns>The operation response.</returns>
        public Task<OperationResponse<bool>> RunDelayed(TimeSpan delay)
        {
            this.checkpointData = new SabbathMessageOperationCheckpointData(this.Context.Account.AccountId);
            return this.TransitionToSendMessage(delay);
        }

        private async Task<OperationResponse<bool>> TransitionToSendMessage(TimeSpan delay)
        {
            this.checkpointData.OperationState = ServiceMessageOperationState.SendingMessage;

            if (delay == TimeSpan.Zero)
            {
                return
                    await this.SetCheckpoint(this.checkpointData) ??
                    await this.EnterSendMessage();
            }
            else
            {
                return await this.HandOffCheckpoint(
                    delay,
                    this.checkpointData,
                    HttpStatusCode.Accepted,
                    true);
            }
        }

        private async Task<OperationResponse<bool>> EnterSendMessage()
        {
            string bibleVerse = await this.ReserveBibleVerse(this.Context.TrackingId);
            string verseContent = DomainData.BibleVerses[bibleVerse];
            Message sabbathTextMessage = Message.CreateSabbathText(this.Context.Account.PhoneNumber, bibleVerse, verseContent);

            await this.Bag.MessageClient.SendMessage(sabbathTextMessage, this.Context.TrackingId, this.Context.CancellationToken);
            
            return await this.TransitionToUpdateAccount(sabbathTextMessage);
        }

        private async Task<OperationResponse<bool>> TransitionToUpdateAccount(Message sabbathTextMessage)
        {
            this.checkpointData.OperationState = ServiceMessageOperationState.UpdatingAccount;
            this.checkpointData.OutgoingMessage = sabbathTextMessage;

            return
                await this.SetCheckpoint(this.checkpointData) ??
                await this.EnterUpdateAccount();
        }

        private async Task<OperationResponse<bool>> EnterUpdateAccount()
        {
            bool reservedBibleVerseRemoved = false;
            if (this.Context.Account.ReservedBibleVerse.ContainsKey(this.Context.TrackingId))
            {
                this.Context.Account.ReservedBibleVerse.Remove(this.Context.TrackingId);
                reservedBibleVerseRemoved = true;
            }

            MessageEntity messageEntity = this.checkpointData.OutgoingMessage.ToEntity(
                this.Context.Account.AccountId,
                this.Context.TrackingId, /* messageId */
                MessageDirection.Outgoing,
                MessageStatus.Sent);
            bool messageAdded = TryAddMessageEntity(this.Context.Account, messageEntity);

            if (reservedBibleVerseRemoved || messageAdded)
            {
                await this.Bag.AccountStore.Update(this.Context.Account, this.Context.CancellationToken);
            }

            return await this.CompleteCheckpoint(this.checkpointData, HttpStatusCode.OK, true);
        }

        /// <summary>
        /// Resumes the operation.
        /// </summary>
        /// <param name="serializedCheckpointData">The serialized checkpoint data.</param>
        /// <returns>The operation response.</returns>
        protected override Task<OperationResponse<bool>> Resume(string serializedCheckpointData)
        {
            throw new NotImplementedException();
        }
    }
}
