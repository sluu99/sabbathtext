namespace SabbathText.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// Controller for admins
    /// </summary>
    [Authorize]
    public class AdminController : BaseController
    {
        /// <summary>
        /// The index page
        /// </summary>
        /// <returns>The action result</returns>
        public ActionResult Index()
        {
            return View();
        }
    }
}