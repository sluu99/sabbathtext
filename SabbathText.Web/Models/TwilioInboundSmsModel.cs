using System.ComponentModel.DataAnnotations;

namespace SabbathText.Web.Models
{
    public class TwilioInboundSmsModel
    {
        [Required]
        public string MessageSid { get; set; }

        [Required]
        public string From { get; set; }

        [Required]
        public string To { get; set; }

        [Required]
        public string Body { get; set; }
    }
}