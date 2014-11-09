using Microsoft.WindowsAzure.Storage.Table;

namespace SabbathText.Core.Entities
{
    public class KeyValue : TableEntity
    {
        public const string SubscriberCount = "SubscriberCount";

        public string Key { get; set; }
        public string Value { get; set; }
    }
}
