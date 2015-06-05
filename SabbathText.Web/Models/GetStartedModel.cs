namespace SabbathText.Web.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Controller model for get started page
    /// </summary>
    public class GetStartedModel
    {
        /// <summary>
        /// Gets or sets the phone number
        /// </summary>
        [Required, MaxLength(128)]
        public string PhoneNumber { get; set; }
    }
}