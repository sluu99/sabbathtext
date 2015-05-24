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
            this.RecentVerses = new List<string>();
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
        /// Gets or sets the recent Bible verses sent to the user
        /// </summary>
        public List<string> RecentVerses { get; set; }

        /// <summary>
        /// Gets or sets the account conversation context.
        /// </summary>
        public ConversationContext ConversationContext { get; set; }

        /// <summary>
        /// Gets or sets the authentication key for this account.
        /// </summary>
        public string AuthKey { get; set; }

        /// <summary>
        /// Gets or sets when the authentication key expires
        /// </summary>
        public DateTime AuthKeyExpiration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has been greeted.
        /// </summary>
        public bool HasBeenGreeted { get; set; }

        /// <summary>
        /// Gets the partition key.
        /// </summary>
        public override string PartitionKey
        {
            get
            {
                return GetPartitionKey(this.AccountId);
            }
        }

        /// <summary>
        /// Gets the row key.
        /// </summary>
        public override string RowKey
        {
            get { return GetRowKey(this.AccountId); }
        }

        /// <summary>
        /// Gets or sets last time a Sabbath text was sent to this account.
        /// </summary>
        public DateTime LastSabbathTextTime { get; set; }

        /// <summary>
        /// Gets the account ID from a phone number
        /// </summary>
        /// <param name="phoneNumber">The phone number</param>
        /// <returns>The account ID</returns>
        public static string GetAccountIdByPhoneNumber(string phoneNumber)
        {
            return ("PhoneNumber:" + phoneNumber).Sha1();
        }

        /// <summary>
        /// Gets the reference to an account entity.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <returns>The entity reference.</returns>
        public static EntityReference GetReferenceByPhoneNumber(string phoneNumber)
        {
            return GetReferenceById(GetAccountIdByPhoneNumber(phoneNumber));
        }

        /// <summary>
        /// Gets the reference to an account entity.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <returns>The entity reference.</returns>
        public static EntityReference GetReferenceById(string accountId)
        {
            return new EntityReference
            {
                PartitionKey = GetPartitionKey(accountId),
                RowKey = GetRowKey(accountId),
            };
        }

        private static string GetPartitionKey(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
            {
                return null;
            }

            return accountId.Substring(0, 2).ToUpperInvariant();
        }

        private static string GetRowKey(string accountId)
        {
            return accountId;
        }
    }
}
