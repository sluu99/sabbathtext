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
            if (Clock.UtcNow.DayOfWeek == DayOfWeek.Saturday)
            {
                // we want to use the Friday (when Sabbath starts) in the message ID.
                // if today is not Sabbath (not Friday/Saturday) this ID won't be used.
                this.checkpointData.SabbathMessageId = this.Context.Account.AccountId + "_SabbathText_{0:yyyyMMdd}".InvariantFormat(Clock.UtcNow.AddDays(-1));
            }

            return this.HandOffCheckpoint(
                TimeSpan.Zero,
                this.checkpointData,
                HttpStatusCode.Accepted,
                true);
        }

        private async Task<OperationResponse<bool>> EnterCheckSabbath()
        {
            if (this.Context.Account.Status == AccountStatus.Subscribed &&
                string.IsNullOrWhiteSpace(this.Context.Account.ZipCode) == false)
            {
                OperationContext sabbathMsgOpContext = new OperationContext
                {
                    Account = this.Context.Account,
                    CancellationToken = this.Context.CancellationToken,
                    TrackingId = this.checkpointData.SabbathMessageId,
                };

                SabbathMessageOperation sabbathMsgOp = new SabbathMessageOperation(sabbathMsgOpContext);

                LocationInfo location = LocationInfo.FromZipCode(this.Context.Account.ZipCode);
                if (location.IsSabbath())
                {
                    // since we are using the same tracking for the same Sabbath period, running again won't hurt
                    await sabbathMsgOp.Run();
                }
                else if (location.LocalTime.DayOfWeek == DayOfWeek.Friday)
                {
                    // it's Friday and not Sabbath yet, so it means Sabbath has not started yet
                    TimeInfo fridayTimeInfo = TimeInfo.Create(location.ZipCode, location.LocalTime.Date);
                    TimeSpan timeSpanUntilSabbath = fridayTimeInfo.SunSetUtc - Clock.UtcNow;
                    if (timeSpanUntilSabbath < TimeSpan.Zero)
                    {
                        timeSpanUntilSabbath = TimeSpan.Zero;
                    }

                    if (timeSpanUntilSabbath <= this.Bag.Settings.RunnerFrequency)
                    {
                        // Sabbath begins before the next runner iteration
                        await sabbathMsgOp.RunDelayed(timeSpanUntilSabbath);
                    }
                }
            }

            return await this.TransitionToCheckAnnouncements();
        }

        private async Task<OperationResponse<bool>> TransitionToCheckAnnouncements()
        {
            this.checkpointData.OperationState = InspectAccountOperationState.CheckingAnnouncements;

            DateTime timeForNextAnnouncement = this.Context.Account.LastAnnouncementTextTime + this.Bag.Settings.AnnouncementTextGap;

            foreach (var announcement in DomainData.Announcements)
            {
                if (this.Context.Account.SentAnnouncements.Contains(announcement.AnnouncementId) == false &&
                    (announcement.OverrideAnnouncementGap || timeForNextAnnouncement <= Clock.UtcNow) &&
                    announcement.IsEligible(this.Context.Account))
                {
                    this.checkpointData.SelectedAnnouncementId = announcement.AnnouncementId;
                    break;
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
