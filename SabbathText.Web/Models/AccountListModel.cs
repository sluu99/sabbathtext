namespace SabbathText.Web.Models
{
    using System.Collections.Generic;
    using SabbathText.Entities;

    /// <summary>
    /// A model for the account list view
    /// </summary>
    public class AccountListModel
    {
        /// <summary>
        /// Gets or sets the accounts
        /// </summary>
        public IEnumerable<AccountEntity> Accounts { get; set; }

        /// <summary>
        /// Gets or sets the continuation token
        /// </summary>
        public string ContinuationToken { get; set; }
    }
}