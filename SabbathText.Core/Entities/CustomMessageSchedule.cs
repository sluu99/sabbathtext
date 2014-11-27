using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace SabbathText.Core.Entities
{
    public class CustomMessageSchedule : TableEntity
    {
        public string ScheduleId { get; set; }
        public string Body { get; set; }
        public DateTime ScheduleDate { get; set; }
        public long SecondsOffset { get; set; }
        public TimeOffsetType OffsetFrom { get; set; }
        public long GracePeriod { get; set; }
    }
}
