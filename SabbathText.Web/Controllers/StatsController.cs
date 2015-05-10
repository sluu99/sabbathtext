using SabbathText.Web.Models;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SabbathText.Web.Controllers
{
    public class StatsController : BaseController
    {
        static readonly TimeSpan StatsCacheTime = TimeSpan.FromMinutes(5);
        static StatsModel Stats = new StatsModel();

        // GET: Stats
        public ActionResult Index()
        {
            return View(Stats);
        }
    }
}