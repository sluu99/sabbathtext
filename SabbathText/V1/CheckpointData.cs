namespace SabbathText.V1
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// This class represents the generic operation specific data for each checkpoint
    /// </summary>
    public class CheckpointData
    {
        /// <summary>
        /// Creates a new instance of the checkpoint data.
        /// </summary>
        /// <param name="accountId">The account ID associated with the checkpoint.</param>
        public CheckpointData(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
            {
                throw new ArgumentException("The account ID is required", "accountId");
            }

            this.AccountId = accountId;
        }

        /// <summary>
        /// Gets or sets the account ID associated with the checkpoint.
        /// </summary>
        public string AccountId { get; set; }
        
        /// <summary>
        /// Gets or sets the time when the checkpoint can be processed
        /// </summary>
        public DateTime? ProcessAfter { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the checkpoint was handed off to compensation processing.
        /// </summary>
        public bool IsHandOffProcessing { get; set; }
    }

    /// <summary>
    /// Operation specific  data
    /// </summary>
    /// <typeparam name="TResponse">The response data type</typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "A generic variation of the base class")]
    public class CheckpointData<TResponse> : CheckpointData
    {
        /// <summary>
        /// Creates a new instance of the checkpoint data.
        /// </summary>
        /// <param name="accountId">The account ID associated with the checkpoint.</param>
        public CheckpointData(string accountId)
            : base(accountId)
        {
        }

        /// <summary>
        /// Gets or sets the operation response
        /// </summary>
        public OperationResponse<TResponse> Response { get; set; }
    }
}
