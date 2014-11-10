using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace SabbathText.Core.Entities
{
    public class LocationTimeInfo : TableEntity
    {
        public LocationTimeInfo()
        {
            this.InfoDate = Clock.MinValue;
            this.Sunset = Clock.MinValue;
        }

        public string ZipCode { get; set; }
        public DateTime InfoDate { get; set; }
        public DateTime Sunrise { get; set; }
        public DateTime Sunset { get; set; }
    }
}
