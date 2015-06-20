namespace SabbathText.Compensation.V1
{
    using System.Threading;
using System.Threading.Tasks;

    /// <summary>
    /// This class handles any checkpoints picked up by the compensation processor
    /// </summary>
    public abstract class CheckpointHandler
    {
        /// <summary>
        /// Finishes the checkpoint, whether pushing it to completion or canceling it
        /// </summary>
        /// <param name="checkpoint">The checkpoint</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>Whether the checkpoint got processed</returns>
        public abstract Task<bool> Finish(Checkpoint checkpoint, CancellationToken cancellationToken);
    }
}
