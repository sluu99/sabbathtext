namespace SabbathText.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using KeyValueStorage;

    /// <summary>
    /// This entity represents a location that one or more accounts are using.
    /// </summary>
    public class LocationEntity : KeyValueEntity
    {
        /// <summary>
        /// Gets or sets the location ZIP Code.
        /// </summary>
        public string ZipCode { get; set; }

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
        /// Gets the row key
        /// </summary>
        public override string RowKey
        {
            get
            {
                return this.ZipCode;
            }
        }
    }
}
