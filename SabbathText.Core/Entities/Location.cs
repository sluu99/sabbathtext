using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace SabbathText.Core.Entities
{
    public class Location : TableEntity
    {
        public string ZipCode { get; set; }
        public string LocationName { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double TimeZoneOffset { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
