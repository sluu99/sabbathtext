namespace SabbathText.V1
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using KeyValueStorage;
    using Newtonsoft.Json;
    using SabbathText.Compensation.V1;
    using SabbathText.Entities;

    /// <summary>
    /// This class has the logic to push operation checkpoints to completion or cancellation.
    /// </summary>
    public class OperationCheckpointHandler : CheckpointHandler
    {
        private KeyValueStore<AccountEntity> accountStore;
        private KeyValueStore<MessageEntity> messageStore;
        private MessageClient messageClient;
        private CompensationClient compensation;

        /// <summary>
        /// Creates a new instance of this handler class.
        /// </summary>
        /// <param name="accountStore">The account store.</param>
        /// <param name="messageStore">The message store.</param>
        /// <param name="messageClient">The message client.</param>
        /// <param name="compensationClient">The compensation client.</param>
        public OperationCheckpointHandler(
            KeyValueStore<AccountEntity> accountStore,
            KeyValueStore<MessageEntity> messageStore,
            MessageClient messageClient,
            CompensationClient compensationClient)
        {
            this.accountStore = accountStore;
            this.messageStore = messageStore;
            this.messageClient = messageClient;
            this.compensation = compensationClient;
        }

        /// <summary>
        /// Finishes an operation based on the checkpoint.
        /// </summary>
        /// <param name="checkpoint">The checkpoint.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A TPL Task.</returns>
        public override async Task Finish(Checkpoint checkpoint, CancellationToken cancellationToken)
        {
            if (checkpoint == null)
            {
                throw new ArgumentNullException("checkpoint");
            }

            if (checkpoint.Status == CheckpointStatus.Completed || checkpoint.Status == CheckpointStatus.Cancelled)
            {
                return;
            }

            if (checkpoint.Status == CheckpointStatus.InProgress)
            {
                // the operation failed mid way
                // we want to cancel
                checkpoint.Status = CheckpointStatus.Cancelling;
                await this.compensation.UpdateCheckpoint(checkpoint, cancellationToken);
            }

            CheckpointData checkpointData = JsonConvert.DeserializeObject<CheckpointData>(checkpoint.CheckpointData);
            AccountEntity account = new AccountEntity
            {
                AccountId = checkpointData.AccountId,
            };
            account = await this.accountStore.Get(account.PartitionKey, account.RowKey);
            OperationContext context = new OperationContext
            {
                Account = account,
                AccountStore = this.accountStore,
                CancellationToken = cancellationToken,
                Compensation = this.compensation,
                MessageClient = this.messageClient,
                MessageStore = this.messageStore,
                TrackingId = checkpoint.TrackingId,
            };

            switch (checkpoint.OperationType)
            {
                case "GreetUserOperation.V1":
                    {
                        GreetUserOperation operation = new GreetUserOperation(context);
                        await operation.Resume(checkpoint);
                        break;
                    }

                case "SubscribeMessageOperation.V1":
                    {
                        SubscribeMessageOperation operation = new SubscribeMessageOperation(context);
                        await operation.Resume(checkpoint);
                        break;
                    }

                default:
                    throw new NotSupportedException("{0} is not handled for compensation.".InvariantFormat(checkpoint.OperationType));
            }
        }
    }
}
