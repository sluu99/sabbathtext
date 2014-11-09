using SabbathText.Core;
using SabbathText.Core.Entities;
using SabbathText.Web.Models;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SabbathText.Web.Controllers
{
    public class StatsController : Controller
    {
        static readonly TimeSpan StatsCacheTime = TimeSpan.FromMinutes(5);
        static StatsModel Stats = new StatsModel();

        public StatsController()
        {
            this.DataProvider = new AzureDataProvider();
        }

        // GET: Stats
        public async Task<ActionResult> Index()
        {
            await this.RefreshStats();
            return View(Stats);
        }

        public IDataProvider DataProvider { get; set; }

        private async Task RefreshStats()
        {
            DateTime now = DateTime.UtcNow;

            if (Stats.StatsUpdatedOn + StatsCacheTime > now)
            {
                return;
            }

            Stats = new StatsModel()
            {
                StatsUpdatedOn = now,
                AccountCount = await this.GetCount(KeyValue.AccountCount),
                SubscriberCount = await this.GetCount(KeyValue.SubscriberCount),
                PoisonMessageCount = await this.DataProvider.CountPoisonMessages(),
            };
        }

        private async Task<long> GetCount(string key)
        {
            KeyValue kv = await this.DataProvider.GetKeyValue(key);
            if (kv == null)
            {
                return 0;
            }

            long count = 0;
            long.TryParse(kv.Value, out count);

            return count;
        }
    }
}