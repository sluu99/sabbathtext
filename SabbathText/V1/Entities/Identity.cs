namespace SabbathText.V1.Entities
{
    using KeyValueStorage;

    /// <summary>
    /// Identity types
    /// </summary>
    public enum IdentityType
    {
        /// <summary>
        /// Identity by the hashed phone number
        /// </summary>
        PhoneHash,
    }

    /// <summary>
    /// An identity references an account
    /// </summary>
    public class Identity : KeyValueEntity
    {
        /// <summary>
        /// Gets or sets the identity type
        /// </summary>
        public IdentityType Type { get; set; }

        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public string AccountId { get; set; }
    }
}
