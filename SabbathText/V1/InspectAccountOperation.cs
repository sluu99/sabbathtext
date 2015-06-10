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

        private Task<OperationResponse<bool>> TransitionToCheckSabbath()
        {
            this.checkpointData.OperationState = InspectAccountOperationState.CheckingSabbath;
            this.checkpointData.SabbathMessageId = this.Context.Account.AccountId + "_SabbathText_{0:yyyyMMdd}".InvariantFormat(Clock.UtcNow);
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
                return await this.TransitionToCheckAnnouncements();
            }

            LocationInfo locationInfo = LocationInfo.FromZipCode(this.Context.Account.ZipCode);
            DateTime accountTime = locationInfo.LocalTime;

            if (locationInfo.IsSabbath() == false)
            {
                return await this.TransitionToCheckAnnouncements();
            }

            string verseNumber = this.GetBibleVerse();
            string verseContent = DomainData.BibleVerses[verseNumber];
            
            Message sabbathMessage = Message.CreateSabbathText(this.Context.Account.PhoneNumber, verseNumber, verseContent);
            if (await this.Bag.MessageClient.SendMessage(sabbathMessage, this.checkpointData.SabbathMessageId, this.Context.CancellationToken))
            {
                this.Context.Account.LastSabbathTextTime = Clock.UtcNow;
                await this.Bag.AccountStore.Update(this.Context.Account, this.Context.CancellationToken);
            }

            return await this.TransitionToStoreSabbathText(sabbathMessage);
        }

        private async Task<OperationResponse<bool>> TransitionToStoreSabbathText(Message sabbathMessage)
        {
            this.checkpointData.OperationState = InspectAccountOperationState.StoringSabbathText;
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

            return await this.TransitionToCheckAnnouncements();
        }

        private async Task<OperationResponse<bool>> TransitionToCheckAnnouncements()
        {
            this.checkpointData.OperationState = InspectAccountOperationState.CheckingAnnouncements;

            DateTime timeForNextAnnouncement = this.Context.Account.LastAnnouncementTextTime + this.Bag.Settings.AnnouncementTextGap;
            if (timeForNextAnnouncement <= Clock.UtcNow)
            {
                foreach (var announcement in DomainData.Announcements)
                {
                    if (this.Context.Account.SentAnnouncements.Contains(announcement.AnnouncementId) == false &&
                        announcement.IsEligible(this.Context.Account))
                    {
                        this.checkpointData.SelectedAnnouncementId = announcement.AnnouncementId;
                        break;
                    }
                }
            }           

            return
                await this.SetCheckpoint(this.checkpointData) ??
                await this.EnterCheckAnnouncements();
        }

        private async Task<OperationResponse<bool>> EnterCheckAnnouncements()
        {
            if (string.IsNullOrWhiteSpace(this.checkpointData.SelectedAnnouncementId))
            {
                return await this.TransitionToArchiveMessages();
            }

            if (this.Context.Account.SentAnnouncements.Contains(this.checkpointData.SelectedAnnouncementId))
            {
                // someone else could've sent out this announcement between transition & enter
                return await this.TransitionToArchiveMessages();
            }

            string trackingId = this.Context.Account.AccountId + "_" + this.checkpointData.SelectedAnnouncementId;
            Message announcementMessage = Message.CreateAnnouncement(
                this.Context.Account.PhoneNumber,
                DomainData.Announcements.First(a => a.AnnouncementId == this.checkpointData.SelectedAnnouncementId).Content);
            
            if (await this.Bag.MessageClient.SendMessage(announcementMessage, trackingId, this.Context.CancellationToken))
            {
                this.Context.Account.LastAnnouncementTextTime = Clock.UtcNow;
                this.Context.Account.SentAnnouncements.Add(this.checkpointData.SelectedAnnouncementId);
                await this.Bag.AccountStore.Update(this.Context.Account, this.Context.CancellationToken);
            }

            return await TransitionToStoreAnnouncementText(announcementMessage);
        }

        private async Task<OperationResponse<bool>> TransitionToStoreAnnouncementText(Message announcementMessage)
        {
            this.checkpointData.AnnouncementMessage = announcementMessage;
            this.checkpointData.OperationState = InspectAccountOperationState.StoringAnnouncementText;

            return
                await this.SetCheckpoint(this.checkpointData) ??
                await this.EnterStoreAnnouncementText();
        }

        private async Task<OperationResponse<bool>> EnterStoreAnnouncementText()
        {
            if (this.checkpointData.AnnouncementMessage == null)
            {
                return await this.TransitionToArchiveMessages();
            }

            MessageEntity messageEntity = this.checkpointData.AnnouncementMessage.ToEntity(
                this.Context.Account.AccountId,
                this.Context.Account.AccountId + "_" + this.checkpointData.SelectedAnnouncementId,
                MessageDirection.Outgoing,
                MessageStatus.Sent);
            bool messageAdded = TryAddMessageEntity(this.Context.Account, messageEntity);

            if (messageAdded)
            {
                await this.Bag.AccountStore.Update(this.Context.Account, this.Context.CancellationToken);
            }

            return await this.TransitionToArchiveMessages();
        }

        private Task<OperationResponse<bool>> TransitionToArchiveMessages()
        {
            // This state is idempotent. We don't need to update the checkpoint
            this.checkpointData.OperationState = InspectAccountOperationState.ArchivingMessages;

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
            if (this.IsCancelling)
            {
                // we don't have any compensation
                return this.CompleteCheckpoint(null, HttpStatusCode.InternalServerError, false);
            }

            this.checkpointData = JsonConvert.DeserializeObject<InspectAccountOperationCheckpointData>(serializedCheckpointData);

            switch (this.checkpointData.OperationState)
            {
                case InspectAccountOperationState.CheckingSabbath:
                    return this.EnterCheckSabbath();
                case InspectAccountOperationState.StoringSabbathText:
                    return this.EnterStoreSabbathText();
                case InspectAccountOperationState.CheckingAnnouncements:
                    return this.EnterCheckAnnouncements();
                case InspectAccountOperationState.StoringAnnouncementText:
                    return this.EnterStoreAnnouncementText();
                case InspectAccountOperationState.ArchivingMessages:
                    return this.EnterArchiveMessages();
            }

            throw new NotImplementedException();
        }
    }
}
