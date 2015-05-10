namespace SabbathText.Web.Controllers
{
    using System;
    using System.Web.Mvc;
    using SabbathText.Web.Models;

    /// <summary>
    /// A page to view statistics
    /// </summary>
    public class StatsController : BaseController
    {
        private static readonly TimeSpan StatsCacheTime = TimeSpan.FromMinutes(5);
        private static StatsModel stats = new StatsModel();

        /// <summary>
        /// The index page
        /// </summary>
        /// <returns>The index view.</returns>
        public ActionResult Index()
        {
            return View(stats);
        }
    }
}