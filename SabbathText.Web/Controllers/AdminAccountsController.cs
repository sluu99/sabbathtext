namespace SabbathText.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// The controller for administrating accounts
    /// </summary>
    public class AdminAccountsController : BaseController
    {
        /// <summary>
        /// Home page for managing accounts
        /// </summary>
        /// <returns>The action result</returns>
        public ActionResult Index()
        {
            return this.View();
        }
    }
}