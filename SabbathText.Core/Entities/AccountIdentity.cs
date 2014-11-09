using Microsoft.WindowsAzure.Storage.Table;

namespace SabbathText.Core.Entities
{
    public class AccountIdentity : TableEntity
    {
        public string AccountId { get; set; }
    }
}
