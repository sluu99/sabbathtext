using Microsoft.WindowsAzure.Storage.Table;

namespace SabbathText.Core.Entities
{
    public class KeyValue : TableEntity
    {
        public const string SubscriberCount = "SubscriberCount";
        public const string AccountCount = "AccountCount";

        public string Key { get; set; }
        public string Value { get; set; }
    }
}
