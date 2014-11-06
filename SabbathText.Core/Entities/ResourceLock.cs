using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace SabbathText.Core.Entities
{
    public class ResourceLock : TableEntity
    {
        public string ResourceId { get; set; }
        public string LockKey { get; set; }
        public DateTime LockedOn { get; set; }
        public DateTime LockExpiresOn { get; set; }
    }
}
