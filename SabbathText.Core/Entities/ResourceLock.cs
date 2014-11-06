using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace SabbathText.Core.Entities
{
    public class ResourceLock : TableEntity
    {
        public ResourceLock()
        {
            this.LockedOn = Clock.MinValue;
            this.LockExpiresOn = Clock.MinValue;
        }

        public string ResourceId { get; set; }
        public string LockKey { get; set; }
        public DateTime LockedOn { get; set; }
        public DateTime LockExpiresOn { get; set; }
    }
}
