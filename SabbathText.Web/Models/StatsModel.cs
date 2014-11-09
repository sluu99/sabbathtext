using System;

namespace SabbathText.Web.Models
{
    public class StatsModel
    {
        public long AccountCount { get; set; }
        public long SubscriberCount { get; set; }
        public int PoisonMessageCount { get; set; }
        public DateTime StatsUpdatedOn { get; set; }
    }
}