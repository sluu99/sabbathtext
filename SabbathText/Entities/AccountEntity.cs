namespace SabbathText.Entities
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
    public class AccountEntity : KeyValueEntity
    {
        /// <summary>
        /// Initializes a new instance of this class
        /// </summary>
        public AccountEntity()
        {
            this.RecentMessages = new List<MessageEntity>();
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
        /// Gets or sets the list of recent messages
        /// </summary>
        public List<MessageEntity> RecentMessages { get; set; }

        /// <summary>
        /// Gets the account ID from a phone number
        /// </summary>
        /// <param name="phoneNumber">The phone number</param>
        /// <returns>The account ID</returns>
        public static string GetAccountId(string phoneNumber)
        {
            return ("PhoneNumber:" + phoneNumber).Sha256();
        }
    }
}
