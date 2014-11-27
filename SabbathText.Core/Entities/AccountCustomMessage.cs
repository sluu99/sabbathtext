using Microsoft.WindowsAzure.Storage.Table;

namespace SabbathText.Core.Entities
{
    public class AccountCustomMessage : TableEntity
    {
        public string AccountId { get; set; }
        public string ScheduleId { get; set; }
    }
}
