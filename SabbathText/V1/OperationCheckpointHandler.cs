namespace SabbathText.V1
{
    using System;
    using System.Diagnostics;
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
        /// <returns>Whether the checkpoint got processed</returns>
        public override async Task<bool> Finish(Checkpoint checkpoint, CancellationToken cancellationToken)
        {
            GoodieBag bag = GoodieBag.Create();

            if (checkpoint == null)
            {
                throw new ArgumentNullException("checkpoint");
            }

            if (checkpoint.Status == CheckpointStatus.Completed || checkpoint.Status == CheckpointStatus.Cancelled)
            {
                // consider the checkpoint processed
                return true;
            }

            CheckpointData checkpointData = null;            
            if (checkpoint.CheckpointData != null)
            {
                checkpointData = JsonConvert.DeserializeObject<CheckpointData>(checkpoint.CheckpointData);
            }

            if (checkpointData != null && checkpointData.ProcessAfter != null && checkpointData.ProcessAfter > Clock.UtcNow)
            {
                bag.TelemetryTracker.ExtendingCheckpoint(
                    checkpoint.PartitionKey,
                    checkpoint.RowKey,
                    checkpoint.OperationType,
                    checkpointData.ProcessAfter.Value);

                if (checkpoint.QueueMessage != null)
                {
                    await bag.CompensationClient.ExtendMessageTimeout(
                        checkpoint.QueueMessage,
                        checkpointData.ProcessAfter.Value - Clock.UtcNow,
                        cancellationToken);
                }

                return false;
            }

            if (checkpoint.Status == CheckpointStatus.InProgress)
            {
                if (checkpointData == null || checkpointData.IsHandOffProcessing == false)
                {
                    bag.TelemetryTracker.CancellingCheckpoint(checkpoint.PartitionKey, checkpoint.RowKey, checkpoint.OperationType);

                    // the operation failed mid way (not hand off processing)
                    // we want to cancel
                    checkpoint.Status = CheckpointStatus.Cancelling;
                    await bag.CompensationClient.UpdateCheckpoint(checkpoint, cancellationToken);
                }
            }
            
            try
            {
                bag.TelemetryTracker.ProcessingCheckpoint(checkpoint.PartitionKey, checkpoint.RowKey, checkpoint.OperationType);
                
                DateTime startTime = Clock.UtcNow;
                await ResumeOperation(checkpoint, cancellationToken, bag);
                DateTime endTime = Clock.UtcNow;

                bag.TelemetryTracker.CompletedCheckpoint(checkpoint.PartitionKey, checkpoint.RowKey, checkpoint.OperationType, endTime - startTime);

                return true;
            }
            catch (Exception ex)
            {
                bag.TelemetryTracker.ProcessCheckpointException(ex, checkpoint.PartitionKey, checkpoint.RowKey, checkpoint.OperationType);
            }

            return false;
        }

        private static async Task ResumeOperation(Checkpoint checkpoint, CancellationToken cancellationToken, GoodieBag bag)
        {
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

                case "InspectAccountOperation.V1":
                    {
                        InspectAccountOperation operation = new InspectAccountOperation(context);
                        await operation.Resume(checkpoint);
                        break;
                    }
                    
                case "BibleVerseOperation.V1":
                    {
                        BibleVerseOperation operation = new BibleVerseOperation(context);
                        await operation.Resume(checkpoint);
                        break;
                    }

                case "SabbathMessageOperation.V1":
                    {
                        SabbathMessageOperation operation = new SabbathMessageOperation(context);
                        await operation.Resume(checkpoint);
                        break;
                    }

                default:
                    throw new NotSupportedException("{0} is not handled for compensation.".InvariantFormat(checkpoint.OperationType));
            }
        }
    }
}
