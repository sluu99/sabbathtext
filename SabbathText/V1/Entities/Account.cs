namespace SabbathText.V1.Entities
{
    using System;
    using System.Collections.Generic;
    using KeyValueStorage;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Account statuses
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AccountStatus
    {
        /// <summary>
        /// The account is brand new
        /// </summary>
        BrandNew,

        /// <summary>
        /// The account has subscribed
        /// </summary>
        Subscribed,
        
        /// <summary>
        /// The account has unsubscribed
        /// </summary>
        Unsubscribed,
    }

    /// <summary>
    /// The account entity
    /// </summary>
    public class Account : KeyValueEntity
    {
        /// <summary>
        /// Initializes a new instance of this class
        /// </summary>
        public Account()
        {
            this.RecentlySentVerses = new List<string>();
        }

        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Gets or sets the account creation time
        /// </summary>
        public DateTime CreationTime { get; set; }        
        
        /// <summary>
        /// Gets or sets the phone number
        /// </summary>
        public string PhoneNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the zip code
        /// </summary>
        public string ZipCode { get; set; }
        
        /// <summary>
        /// Gets or sets the account status
        /// </summary>
        public AccountStatus Status { get; set; }
        
        /// <summary>
        /// Gets or sets the last sabbath message time
        /// </summary>
        public DateTime LastSabbathMessageTime { get; set; }
        
        /// <summary>
        /// Gets or sets the cycle key
        /// </summary>
        public string CycleKey { get; set; }

        /// <summary>
        /// Gets or sets the recently sent verses
        /// </summary>
        public List<string> RecentlySentVerses { get; set; }
    }
}
