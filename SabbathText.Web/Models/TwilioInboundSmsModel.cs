namespace SabbathText.Web.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The model for incoming SMS messages from <c>Twilio</c>
    /// </summary>
    public class TwilioInboundSmsModel
    {
        /// <summary>
        /// Gets or sets the external message ID
        /// </summary>
        [Required, MaxLength(64)]
        public string MessageSid { get; set; }

        /// <summary>
        /// Gets or sets the phone number of the sender
        /// </summary>
        [Required, MaxLength(64)]
        public string From { get; set; }

        /// <summary>
        /// Gets or sets the recipient phone number
        /// </summary>
        [Required, MaxLength(64)]
        public string To { get; set; }

        /// <summary>
        /// Gets or sets the message body
        /// </summary>
        [Required, MaxLength(1024)]
        public string Body { get; set; }
    }
}