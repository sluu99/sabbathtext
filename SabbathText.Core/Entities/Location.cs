using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace SabbathText.Core.Entities
{
    public class Location : TableEntity
    {
        public static readonly Location TestLocation = new Location
        {
            ZipCode = "00000",
            LocationName = "Test Site",
            Country = "Republic of Bananas",
            Region = "Mall-The",
            Longitude = -500,
            Latitude = -500,
            TimeZoneOffset = 0, 
            CreationTime = Clock.MaxValue,
            UpdateTime = Clock.MaxValue,
        };

        public Location()
        {
            this.CreationTime = Clock.MinValue;
            this.UpdateTime = Clock.MinValue;
        }

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
