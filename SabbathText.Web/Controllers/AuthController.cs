namespace SabbathText.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin.Security;

    /// <summary>
    /// Controller used for authorization
    /// </summary>
    [RoutePrefix("auth")]
    public class AuthController : BaseController
    {
        private const string DevelopmentLoginProvider = "Development";
        private const string DevelopmentUserName = "Dev";
        private const string DevelopmentIdentityName = "dev@dev.local";

        private IAuthenticationManager AuthManager
        {
            get
            {
                return this.HttpContext.GetOwinContext().Authentication;
            }
        }

        /// <summary>
        /// Shows the login page.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns>The action result.</returns>
        [Route("login"), HttpGet, AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = "/";
            }

            if (this.HttpContext.User.Identity.IsAuthenticated)
            {
                return this.RedirectToLocal(returnUrl);
            }

            this.ViewBag.ReturnUrl = returnUrl;
            return this.View("Login");
        }

        /// <summary>
        /// Handles external login
        /// </summary>
        /// <param name="provider">The login provider.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns>The action result.</returns>
        [Route("external-login"), HttpPost, AllowAnonymous]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = "/";
            }

            if (this.HttpContext.User.Identity.IsAuthenticated)
            {
                return this.RedirectToLocal(returnUrl);
            }

            if (GoodieBag.Create().Settings.UseDevelopmentAuthentication && DevelopmentLoginProvider.Equals(provider, StringComparison.OrdinalIgnoreCase))
            {
                this.SignIn(DevelopmentUserName, DevelopmentIdentityName, DevelopmentLoginProvider);
                return this.RedirectToLocal(returnUrl);
            }

            return new ChallengeResult(
                provider,
                this.Url.Action("ExternalLoginCallback", "Auth", new { ReturnUrl = returnUrl }),
                userId: null);
        }

        /// <summary>
        /// The external login callback.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns>The action result.</returns>
        [Route("external-login-callback"), HttpGet, AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = "/";
            }

            if (this.HttpContext.User.Identity.IsAuthenticated)
            {
                return this.RedirectToLocal(returnUrl);
            }

            ExternalLoginInfo loginInfo = await this.AuthManager.GetExternalLoginInfoAsync();
            if (loginInfo == null || string.IsNullOrWhiteSpace(loginInfo.Email) || loginInfo.Login == null)
            {
                return this.RedirectToAction("Login");
            }

            string name = loginInfo.DefaultUserName;

            ClaimsIdentity externalIdentity =
                await this.AuthManager.GetExternalIdentityAsync(DefaultAuthenticationTypes.ExternalCookie);
            if (externalIdentity != null && externalIdentity.Claims != null && externalIdentity.Claims.Any())
            {
                var nameClaim = externalIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                if (nameClaim != null)
                {
                    name = nameClaim.Value;
                }
            }

            this.SignIn(name, loginInfo.Email, loginInfo.Login.LoginProvider);

            return this.RedirectToLocal(returnUrl);
        }

        /// <summary>
        /// Logs out
        /// </summary>
        /// <returns>The action result.</returns>
        [Route("logout"), HttpPost]
        public ActionResult Logout()
        {
            this.AuthManager.SignOut();
            return this.RedirectToAction("Index", "Home");
        }

        private void SignIn(string name, string email, string loginProvider)
        {
            this.AuthManager.SignOut();
            ClaimsIdentity identity = this.CreateIdentity(name, email, loginProvider);
            this.AuthManager.SignIn(identity);
        }

        private ClaimsIdentity CreateIdentity(string name, string email, string loginProvider)
        {
            const string IdentityProvideeClaimName = "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider";
            Claim[] claims = new Claim[]
            {
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.NameIdentifier, email),
                new Claim(IdentityProvideeClaimName, loginProvider),
            };

            return new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }
            else
            {
                return this.RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            private const string XsrfKey = "XsrfId";

            /// <summary>
            /// Creates a new instance of this class.
            /// </summary>
            /// <param name="provider">The login provider.</param>
            /// <param name="redirectUri">The redirect URL.</param>
            /// <param name="userId">The user ID.</param>
            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                this.LoginProvider = provider;
                this.RedirectUri = redirectUri;
                this.UserId = userId;
            }

            public string LoginProvider { get; set; }

            public string RedirectUri { get; set; }

            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = this.RedirectUri };
                if (this.UserId != null)
                {
                    properties.Dictionary[XsrfKey] = this.UserId;
                }

                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, this.LoginProvider);
            }
        }
    }
}