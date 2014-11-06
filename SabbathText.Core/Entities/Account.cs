using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace SabbathText.Core.Entities
{
    public class Account : TableEntity
    {
        public Account()
        {
            this.CreationTime = Clock.MinValue;
            this.LastSabbathMessageTime = Clock.MinValue;
        }

        public string AccountId { get; set; }
        public DateTime CreationTime { get; set; }        
        public string PhoneNumber { get; set; }
        public string ZipCode { get; set; }
        public string Status { get; set; }
        public DateTime LastSabbathMessageTime { get; set; }
    }
}
