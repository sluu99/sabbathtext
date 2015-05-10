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
        /// <summary>
        /// Finishes an operation based on the checkpoint.
        /// </summary>
        /// <param name="checkpoint">The checkpoint.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A TPL Task.</returns>
        public override async Task Finish(Checkpoint checkpoint, CancellationToken cancellationToken)
        {
            GoodieBag bag = GoodieBag.Create();

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
                await bag.CompensationClient.UpdateCheckpoint(checkpoint, cancellationToken);
            }

            CheckpointData checkpointData = JsonConvert.DeserializeObject<CheckpointData>(checkpoint.CheckpointData);
            AccountEntity account =
                await bag.AccountStore.Get(AccountEntity.GetReferenceById(checkpoint.AccountId), cancellationToken);
            OperationContext context = new OperationContext
            {
                Account = account,
                CancellationToken = cancellationToken,
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

                case "UpdateZipCodeOperation.V1":
                    {
                        UpdateZipCodeOperation operation = new UpdateZipCodeOperation(context);
                        await operation.Resume(checkpoint);
                        break;
                    }

                case "BeginAuthOperation.V1":
                    {
                        BeginAuthOperation operation = new BeginAuthOperation(context);
                        await operation.Resume(checkpoint);
                        break;
                    }

                default:
                    throw new NotSupportedException("{0} is not handled for compensation.".InvariantFormat(checkpoint.OperationType));
            }
        }
    }
}
