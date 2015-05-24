namespace SabbathText.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using NodaTime;
    using SabbathText.Entities;
    using SabbathText.Location.V1;

    /// <summary>
    /// This operation performs inspection on an account.
    /// </summary>
    public class InspectAccountOperation : BaseOperation<bool>
    {
        private static readonly Random Rand = new Random(Environment.TickCount);
        private InspectAccountOperationCheckpointData checkpointData;

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="context">The operation context.</param>
        public InspectAccountOperation(OperationContext context)
            : base(context, "InspectAccountOperation.V1")
        {
        }

        /// <summary>
        /// Runs the operation
        /// </summary>
        /// <returns>The operation response.</returns>
        public Task<OperationResponse<bool>> Run()
        {
            this.checkpointData = new InspectAccountOperationCheckpointData(this.Context.Account.AccountId);
            return this.TransitionToCheckSabbath();
        }

        private static void SelectBibleVerse(IEnumerable<string> recentVerses, out string verseNumber, out string verseContent)
        {
            string[] allVerses = null;

            if (recentVerses != null)
            {
                allVerses = DomainData.BibleVerses.Keys.Except(recentVerses.Take(DomainData.BibleVerses.Count - 1)).ToArray();
            }
            else
            {
                allVerses = DomainData.BibleVerses.Keys.ToArray();
            }

            verseNumber = allVerses[Rand.Next(0, allVerses.Length)];
            verseContent = DomainData.BibleVerses[verseNumber];
        }

        private Task<OperationResponse<bool>> TransitionToCheckSabbath()
        {
            this.checkpointData.State = InspectAccountOperationState.CheckingSabbath;
            this.checkpointData.SabbathMessageId = Guid.NewGuid().ToString();
            return this.HandOffCheckpoint(
                TimeSpan.Zero,
                this.checkpointData,
                HttpStatusCode.Accepted,
                true);
        }

        private async Task<OperationResponse<bool>> EnterCheckSabbath()
        {
            TimeSpan timeSinceLastSabbathText = Clock.UtcNow - this.Context.Account.LastSabbathTextTime;

            if (this.Context.Account.Status != Entities.AccountStatus.Subscribed ||
                timeSinceLastSabbathText < this.Bag.Settings.SabbathTextGap ||
                string.IsNullOrWhiteSpace(this.Context.Account.ZipCode))
            {
                // Don't need to check for Sabbath
                return await this.TransitionToArchiveMessages();
            }

            LocationInfo locationInfo = LocationInfo.FromZipCode(this.Context.Account.ZipCode);
            DateTime accountTime = locationInfo.LocalTime;

            if (accountTime.DayOfWeek != DayOfWeek.Friday && accountTime.DayOfWeek != DayOfWeek.Saturday)
            {
                // Sabbath is only between Friday & Saturday
                return await this.TransitionToArchiveMessages();
            }

            // it is now either Friday or Saturday
            // find the sun down time on Friday
            DateTime friday = accountTime.Date;
            if (friday.DayOfWeek == DayOfWeek.Saturday)
            {
                friday = friday.AddDays(-1);
            }

            TimeInfo timeInfo = TimeInfo.Create(this.Context.Account.ZipCode, friday);

            if (timeInfo.SunSetUtc > Clock.UtcNow)
            {
                // Sabbath has not started yet
                return await this.TransitionToArchiveMessages();
            }

            DateTime sabbathTextEndTime = timeInfo.SunSetUtc + this.Bag.Settings.SabbathTextGracePeriod;
            if (Clock.UtcNow > sabbathTextEndTime)
            {
                // we have passed the Sabbath text grace period
                return await this.TransitionToArchiveMessages();
            }

            string verseNumber;
            string verseContent;
            SelectBibleVerse(this.Context.Account.RecentVerses, out verseNumber, out verseContent);

            this.Context.Account.RecentVerses.Add(verseNumber);
            if (this.Context.Account.RecentVerses.Count == DomainData.BibleVerses.Count)
            {
                // This account has seen all the Bible verses we have.
                // We'll remove the oldest *five* verses.
                // We can just remove one oldest one, but that would put the account into a circle of verses in order.
                // By removing five verses, the account can still have somewhat random verses after the first cycle
                this.Context.Account.RecentVerses.RemoveRange(0, 5);
            }

            // update the account Sabbath text time first
            // so that if the operation fails later, we won't be spamming the user on each retry
            // (Twilio really needs to start supporting idempotent calls)
            this.Context.Account.LastSabbathTextTime = Clock.UtcNow;            
            await this.Bag.AccountStore.Update(this.Context.Account, this.Context.CancellationToken);

            Message sabbathMessage = Message.CreateSabbathText(this.Context.Account.PhoneNumber, verseNumber, verseContent);
            await this.Bag.MessageClient.SendMessage(sabbathMessage, this.checkpointData.SabbathMessageId);

            return await this.TransitionToStoreSabbathText(sabbathMessage);
        }

        private async Task<OperationResponse<bool>> TransitionToStoreSabbathText(Message sabbathMessage)
        {
            this.checkpointData.State = InspectAccountOperationState.StoringSabbathText;
            this.checkpointData.SabbathMesage = sabbathMessage;

            return
                await this.SetCheckpoint(this.checkpointData) ??
                await this.EnterStoreSabbathText();
        }

        private async Task<OperationResponse<bool>> EnterStoreSabbathText()
        {
            MessageEntity messageEntity = this.checkpointData.SabbathMesage.ToEntity(
                this.Context.Account.AccountId,
                this.checkpointData.SabbathMessageId,
                MessageDirection.Outgoing,
                MessageStatus.Sent);

            if (TryAddMessageEntity(this.Context.Account, messageEntity))
            {
                await this.Bag.AccountStore.Update(this.Context.Account, this.Context.CancellationToken);
            }

            return await this.TransitionToArchiveMessages();
        }

        private Task<OperationResponse<bool>> TransitionToArchiveMessages()
        {
            // This state is idempotent. We don't need to update the checkpoint
            this.checkpointData.State = InspectAccountOperationState.ArchivingMessages;

            return this.EnterArchiveMessages();
        }

        private async Task<OperationResponse<bool>> EnterArchiveMessages()
        {
            bool messagesRemoved = false;
            while (
                this.Context.Account.RecentMessages != null &&
                this.Context.Account.RecentMessages.Count > this.Bag.Settings.RecentMessageThreshold)
            {
                await this.Bag.MessageStore.InsertOrGet(this.Context.Account.RecentMessages[0], this.Context.CancellationToken);
                this.Context.Account.RecentMessages.RemoveAt(0);
                messagesRemoved = true;
            }

            if (messagesRemoved)
            {
                await this.Bag.AccountStore.Update(this.Context.Account, this.Context.CancellationToken);
            }

            return await this.CompleteCheckpoint(this.checkpointData, HttpStatusCode.Accepted, true);
        }

        /// <summary>
        /// Resumes this operation.
        /// </summary>
        /// <param name="serializedCheckpointData">The serialized checkpoint data.</param>
        /// <returns>The operation response.</returns>
        protected override Task<OperationResponse<bool>> Resume(string serializedCheckpointData)
        {
            this.checkpointData = JsonConvert.DeserializeObject<InspectAccountOperationCheckpointData>(serializedCheckpointData);

            switch (this.checkpointData.State)
            {
                case InspectAccountOperationState.CheckingSabbath:
                    return this.EnterCheckSabbath();
                case InspectAccountOperationState.StoringSabbathText:
                    return this.EnterStoreSabbathText();
                case InspectAccountOperationState.ArchivingMessages:
                    return this.EnterArchiveMessages();
            }

            throw new NotImplementedException();
        }
    }
}
