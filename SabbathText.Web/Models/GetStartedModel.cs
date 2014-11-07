using System.ComponentModel.DataAnnotations;

namespace SabbathText.Web.Models
{
    public class GetStartedModel
    {
        [Required, MaxLength(128)]
        public string PhoneNumber { get; set; }
    }
}