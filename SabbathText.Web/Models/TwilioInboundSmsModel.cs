using System.ComponentModel.DataAnnotations;

namespace SabbathText.Web.Models
{
    public class TwilioInboundSmsModel
    {
        [Required, MaxLength(64)]
        public string MessageSid { get; set; }

        [Required, MaxLength(64)]
        public string From { get; set; }

        [Required, MaxLength(64)]
        public string To { get; set; }

        [Required, MaxLength(1024)]
        public string Body { get; set; }
    }
}