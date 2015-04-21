namespace SabbathText.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Checkpoint data for <see cref="UpdateZipCodeOperation"/>
    /// </summary>
    public class UpdateZipCodeOperationCheckpointData : CheckpointData<bool>
    {
        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        public UpdateZipCodeOperationCheckpointData(string accountId)
            : base(accountId)
        {
        }
    }
}
