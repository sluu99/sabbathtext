namespace SabbathText.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SabbathText.Entities;

    /// <summary>
    /// An operation to send Sabbath texts
    /// </summary>
    public class SendSabbathTextOperation : BaseOperation<bool>
    {
        private static readonly Random Rand = new Random(Environment.TickCount);
        private SendSabbathTextOperationCheckpointData checkpointData;

        /// <summary>
        /// Creates a new instance of the operation
        /// </summary>
        /// <param name="context">The operation context</param>
        public SendSabbathTextOperation(OperationContext context)
            : base(context, "SendSabbathTextOperation.V1")
        {
        }

        /// <summary>
        /// Run this operation
        /// </summary>
        /// <returns>The operation response</returns>
        public Task<OperationResponse<bool>> Run()
        {
            this.checkpointData = new SendSabbathTextOperationCheckpointData(this.Context.Account.AccountId);
            return this.TransitionToSendMessage(startTime: null); // run immediately
        }

        /// <summary>
        /// Run this operation
        /// </summary>
        /// <param name="startTime">The start time for the operation</param>
        /// <returns>The operation response</returns>
        public Task<OperationResponse<bool>> RunDelay(DateTime startTime)
        {
            this.checkpointData = new SendSabbathTextOperationCheckpointData(this.Context.Account.AccountId);
            return this.TransitionToSendMessage(startTime);
        }

        private async Task<OperationResponse<bool>> TransitionToSendMessage(DateTime? startTime)
        {
            this.checkpointData.OperationState = ServiceMessageOperationState.SendingMessage;
            this.checkpointData.MessageId = Guid.NewGuid().ToString();

            if (startTime != null)
            {
                TimeSpan delay = startTime.Value - Clock.UtcNow;
                return await this.HandOffCheckpoint(
                    delay,
                    checkpointData,
                    HttpStatusCode.Accepted,
                    true);
            }
            else
            {
                return
                    await this.SetCheckpoint(this.checkpointData) ??
                    await this.EnterSendMessage();
            }
        }

        private async Task<OperationResponse<bool>> EnterSendMessage()
        {
            if (this.Context.Account.RecentVerses.Count == DomainData.BibleVerses.Count)
            {
                // This account has seen all the Bible verses we have.
                // We'll remove a random one from the first half
                int indexToRemove = Rand.Next(this.Context.Account.RecentVerses.Count / 2);
                this.Context.Account.RecentVerses.RemoveAt(indexToRemove);
            }

            string verseNumber;
            string verseContent;
            SelectBibleVerse(this.Context.Account.RecentVerses, out verseNumber, out verseContent);

            this.Context.Account.RecentVerses.Add(verseNumber);
            this.Context.Account.LastSabbathTextTime = Clock.UtcNow;

            await this.Bag.AccountStore.Update(this.Context.Account, this.Context.CancellationToken);

            Message sabbathMessage = Message.CreateSabbathText(this.Context.Account.PhoneNumber, verseNumber, verseContent);
            await this.Bag.MessageClient.SendMessage(sabbathMessage, this.checkpointData.MessageId, this.Context.CancellationToken);

            return await this.TransitionToUpdateAccount(sabbathMessage);
        }

        private async Task<OperationResponse<bool>> TransitionToUpdateAccount(Message sabbathMessage)
        {
            this.checkpointData.OperationState = ServiceMessageOperationState.UpdatingAccount;
            this.checkpointData.Message = sabbathMessage;

            return
                await this.SetCheckpoint(this.checkpointData) ??
                await this.EnterUpdateAccount();
        }

        private async Task<OperationResponse<bool>> EnterUpdateAccount()
        {
            MessageEntity messageEntity = this.checkpointData.Message.ToEntity(
                this.Context.Account.AccountId,
                this.checkpointData.MessageId,
                MessageDirection.Outgoing,
                MessageStatus.Sent);

            if (TryAddMessageEntity(this.Context.Account, messageEntity))
            {
                await this.Bag.AccountStore.Update(this.Context.Account, this.Context.CancellationToken);
            }

            return await this.CompleteCheckpoint(
                this.checkpointData,
                HttpStatusCode.OK,
                true);
        }

        /// <summary>
        /// Resumes the operation
        /// </summary>
        /// <param name="serializedCheckpointData">The checkpoint data</param>
        /// <returns>The operation response</returns>
        protected override Task<OperationResponse<bool>> Resume(string serializedCheckpointData)
        {
            this.checkpointData = JsonConvert.DeserializeObject<SendSabbathTextOperationCheckpointData>(serializedCheckpointData);

            switch (this.checkpointData.OperationState)
            {
                case ServiceMessageOperationState.SendingMessage:
                    return this.EnterSendMessage();
                case ServiceMessageOperationState.UpdatingAccount:
                    return this.EnterUpdateAccount();
                default:
                    throw new InvalidOperationException();
            }
        }

        private static void SelectBibleVerse(IEnumerable<string> recentVerses, out string verseNumber, out string verseContent)
        {
            string[] allVerses = null;

            if (recentVerses != null)
            {
                allVerses = DomainData.BibleVerses.Keys.Except(recentVerses).ToArray();
            }
            else
            {
                allVerses = DomainData.BibleVerses.Keys.ToArray();
            }

            verseNumber = allVerses[Rand.Next(0, allVerses.Length)];
            verseContent = DomainData.BibleVerses[verseNumber];
        }
    }
}
