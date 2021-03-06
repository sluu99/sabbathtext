﻿namespace SabbathText.V1
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using SabbathText.Entities;

    /// <summary>
    /// An operation which sends the user a greeting message
    /// </summary>
    public class GreetUserOperation : BaseOperation<bool>
    {
        /// <summary>
        /// All ready greeted user
        /// </summary>
        public const string UserHasBeenGreetedError = "AlradyGreetedUser";

        private GreetUserOperationCheckpointData checkpointData;

        /// <summary>
        /// Creates a new instance of the operation
        /// </summary>
        /// <param name="context">The operation context</param>
        public GreetUserOperation(OperationContext context)
            : base(context, "GreetUserOperation.V1")
        {
        }

        /// <summary>
        /// Starts the operation
        /// </summary>
        /// <returns>The operation response</returns>
        public Task<OperationResponse<bool>> Run()
        {
            this.checkpointData = new GreetUserOperationCheckpointData(this.Context.Account.AccountId);

            return this.TransitionToSendingMessage();
        }

        /// <summary>
        /// Resumes the operation
        /// </summary>
        /// <param name="serializedCheckpointData">The serialized operation checkpoint data</param>
        /// <returns>The operation response</returns>
        protected override Task<OperationResponse<bool>> Resume(string serializedCheckpointData)
        {
            this.checkpointData = JsonConvert.DeserializeObject<GreetUserOperationCheckpointData>(serializedCheckpointData);

            switch (this.checkpointData.OperationState)
            {
                case ServiceMessageOperationState.SendingMessage:
                    return this.EnterSendingMessage();
                case ServiceMessageOperationState.UpdatingAccount:
                    return this.EnterUpdatingAccount();
            }

            throw new NotImplementedException();
        }

        private Task<OperationResponse<bool>> TransitionToSendingMessage()
        {
            if (this.Context.Account.HasBeenGreeted)
            {
                return Task.FromResult(new OperationResponse<bool>
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    Data = false,
                    ErrorCode = UserHasBeenGreetedError,
                });
            }

            this.checkpointData.MessageEntityId = Guid.NewGuid().ToString();
            this.checkpointData.OperationState = ServiceMessageOperationState.SendingMessage;
            return this.HandOffCheckpoint(
                TimeSpan.Zero,
                this.checkpointData,
                HttpStatusCode.Accepted,
                responseData: true);
        }

        private async Task<OperationResponse<bool>> EnterSendingMessage()
        {
            Message message = Message.CreateGreetingMessage(this.Context.Account.PhoneNumber);
            await this.Bag.MessageClient.SendMessage(message, this.checkpointData.MessageEntityId, this.Context.CancellationToken);

            return await this.TransitionToUpdatingAccount(message);
        }

        private async Task<OperationResponse<bool>> TransitionToUpdatingAccount(Message message)
        {
            this.checkpointData.Message = message;
            this.checkpointData.OperationState = ServiceMessageOperationState.UpdatingAccount;            

            return
                await this.SetCheckpoint(this.checkpointData) ??
                await this.EnterUpdatingAccount();
        }

        private async Task<OperationResponse<bool>> EnterUpdatingAccount()
        {
            MessageEntity messageEntity = new MessageEntity
            {
                AccountId = this.Context.Account.AccountId,
                Body = this.checkpointData.Message.Body,
                Direction = MessageDirection.Outgoing,
                MessageId = this.checkpointData.MessageEntityId,
                MessageTimestamp = this.checkpointData.Message.Timestamp,
                Recipient = this.Context.Account.PhoneNumber,
                Status = MessageStatus.Sent,
                Template = MessageTemplate.Greetings,
            };

            TryAddMessageEntity(this.Context.Account, messageEntity);

            this.Context.Account.HasBeenGreeted = true;
            await this.Bag.AccountStore.Update(this.Context.Account, this.Context.CancellationToken);

            return await this.CompleteCheckpoint(this.checkpointData, HttpStatusCode.OK, responseData: true);
        }
    }
}