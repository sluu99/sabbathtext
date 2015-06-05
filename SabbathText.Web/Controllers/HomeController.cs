namespace SabbathText.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// The landing page of the web application
    /// </summary>
    public class HomeController : BaseController
    {
        /// <summary>
        /// The index page
        /// </summary>
        /// <returns>The index view.</returns>
        public ActionResult Index()
        {
            return this.View();
        }
    }
}