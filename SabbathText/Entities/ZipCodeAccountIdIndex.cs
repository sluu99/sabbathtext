namespace SabbathText.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using KeyValueStorage;

    /// <summary>
    /// This entity indexes accounts by zip code.
    /// </summary>
    public class ZipCodeAccountIdIndex : KeyValueEntity
    {
        /// <summary>
        /// Gets or sets the ZIP code.
        /// </summary>
        public string ZipCode { get; set; }

        /// <summary>
        /// Gets or sets the account ID.
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Gets the partition key.
        /// </summary>
        public override string PartitionKey
        {
            get
            {
                return this.ZipCode;
            }
        }

        /// <summary>
        /// Gets the row key.
        /// </summary>
        public override string RowKey
        {
            get
            {
                return this.AccountId;
            }
        }
    }
}
