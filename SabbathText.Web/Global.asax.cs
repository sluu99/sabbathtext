namespace SabbathText.Web
{
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;

    /// <summary>
    /// The application entry point
    /// </summary>
    public class MvcApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// This method will be called with the application starts up
        /// </summary>
        protected void Application_Start()
        {
            GoodieBag.Initialize(EnvironmentSettings.Create());
            AreaRegistration.RegisterAllAreas();
            Startup.RegisterGlobalFilters(GlobalFilters.Filters);
            Startup.RegisterRoutes(RouteTable.Routes);
            Startup.RegisterBundles(BundleTable.Bundles);            
        }
    }
}
