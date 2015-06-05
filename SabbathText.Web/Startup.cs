[assembly: Microsoft.Owin.OwinStartup(typeof(SabbathText.Web.Startup))]

namespace SabbathText.Web
{
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;
    using Microsoft.AspNet.Identity;
    using Microsoft.Owin;
    using Microsoft.Owin.Security.Cookies;
    using Owin;

    /// <summary>
    /// Configurations for application startup
    /// </summary>    
    public class Startup
    {
        /// <summary>
        /// Registers application code bundles.
        /// For more information on bundling, visit <c>http://go.microsoft.com/fwlink/?LinkId=301862</c>
        /// </summary>
        /// <param name="bundles">The bundles.</param>
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
            BundleTable.EnableOptimizations = true;
        }

        /// <summary>
        /// Registers global filters for the controllers.
        /// </summary>
        /// <param name="filters">The filter collection.</param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        /// <summary>
        /// Registers application routes
        /// </summary>
        /// <param name="routes">The route collection of the web application.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.LowercaseUrls = true;
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapMvcAttributeRoutes();            
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional });
        }

        /// <summary>
        /// Registers Web API routes.
        /// </summary>
        /// <param name="config">The HTTP configuration.</param>
        public static void RegisterApiRoutes(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });
        }

        /// <summary>
        /// Configures <c>Owin</c> authentication.
        /// For more information, visit <c>http://go.microsoft.com/fwlink/?LinkId=301864</c>
        /// </summary>
        /// <param name="app">The app builder.</param>
        public static void ConfigureAuth(IAppBuilder app)
        {
            EnvironmentSettings settings = EnvironmentSettings.Create();

            app.UseCookieAuthentication(
                new CookieAuthenticationOptions
                {
                    AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                    LoginPath = new PathString("/auth/login"),
                    LogoutPath = new PathString("/auth/logout"),
                });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            if (settings.UseGoogleAuthentication)
            {
                app.UseGoogleAuthentication(
                    settings.GoogleClientId,
                    settings.GoogleClientSecret);
            }
        }

        /// <summary>
        /// This method is invoked by <c>Owin</c> automatically.
        /// </summary>
        /// <param name="app">The <c>Owin</c> app.</param>
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}