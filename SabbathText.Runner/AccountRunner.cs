namespace SabbathText.Runner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using KeyValueStorage;
    using SabbathText.Entities;
    using SabbathText.V1;

    /// <summary>
    /// This class invokes the AccountInspect operation for all the accounts in the system
    /// </summary>
    public class AccountRunner : Worker
    {
        private static readonly TimeSpan IterationDelay = TimeSpan.FromSeconds(5);
        private static readonly char[] HexCharacters = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        private DateTime nextRun = DateTime.MinValue;

        /// <summary>
        /// Runs a worker iteration
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The delay after the iteration.</returns>
        public override async Task<TimeSpan> RunIteration(CancellationToken cancellationToken)
        {
            if (Clock.UtcNow >= nextRun)
            {
                // time to run again
                DateTime startTime = Clock.UtcNow;
                await IterateAccounts(this.InspectAccount, cancellationToken);
                nextRun = startTime.Add(GoodieBag.Create().Settings.RunnerFrequency);
            }
            
            Trace.TraceInformation("Next run = {0} {1}. Now = {2}", nextRun, nextRun.Kind, Clock.UtcNow);
            
            return IterationDelay;
        }

        private static async Task IterateAccounts(Func<AccountEntity, CancellationToken, Task> toRun, CancellationToken cancellationToken)
        {
            const int PageSize = 50;
            GoodieBag bag = GoodieBag.Create();

            foreach (string accountPartition in GetAllAccountPartitions())
            {
                cancellationToken.ThrowIfCancellationRequested();
                string continuationToken = null;

                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    PagedResult<AccountEntity> page = await bag.AccountStore.ReadPartition(accountPartition, PageSize, continuationToken, cancellationToken);

                    if (page.Entities == null || page.Entities.Any() == false)
                    {
                        break;
                    }

                    continuationToken = page.ContinuationToken;

                    foreach (AccountEntity account in page.Entities)
                    {
                        await toRun(account, cancellationToken);
                    }
                }
                while (continuationToken != null);
            }
        }

        private static IEnumerable<string> GetAllAccountPartitions()
        {
            foreach (char c1 in HexCharacters)
            {
                foreach (char c2 in HexCharacters)
                {
                    yield return new string(new char[2] { c1, c2 });
                }
            }
        }

        private Task InspectAccount(AccountEntity account, CancellationToken cancellationToken)
        {
            Trace.TraceInformation("Inspecting account {0}", account.AccountId);

            OperationContext context = new OperationContext
            {
                Account = account,
                CancellationToken = cancellationToken,
                TrackingId = Guid.NewGuid().ToString(),
            };

            InspectAccountOperation operation = new InspectAccountOperation(context);
            return operation.Run();
        }
    }
}
