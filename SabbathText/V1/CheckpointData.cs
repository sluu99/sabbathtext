﻿namespace SabbathText.V1
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
                throw new ArgumentException("The account ID is requireD", "accountId");
            }

            this.AccountId = accountId;
        }

        /// <summary>
        /// Gets or sets the account ID associated with the checkpoint.
        /// </summary>
        public string AccountId { get; set; }
    }

    /// <summary>
    /// Operation specific  data
    /// </summary>
    /// <typeparam name="TResponse">The response data type</typeparam>
    /// <typeparam name="TState">The operation enumeration type</typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "A generic variation of the base class")]
    public class CheckpointData<TResponse, TState> : CheckpointData
        where TState : struct, IConvertible /* enum */
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

        /// <summary>
        /// Gets or sets the operation state
        /// </summary>
        public TState OperationState { get; set; }
    }
}
