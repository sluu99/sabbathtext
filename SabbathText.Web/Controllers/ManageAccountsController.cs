namespace SabbathText.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using KeyValueStorage;
    using SabbathText.Entities;
    using SabbathText.Web.Models;

    /// <summary>
    /// The controller for administrating accounts
    /// </summary>
    [Authorize]
    [RoutePrefix("admin/accounts")]
    public class ManageAccountsController : BaseController
    {
        /// <summary>
        /// Home page for managing accounts
        /// </summary>
        /// <param name="continuationToken">The continuation token</param>
        /// <returns>The action result</returns>
        [Route, HttpGet]
        public async Task<ActionResult> Index(string continuationToken)
        {
            const int Take = 25;

            if (string.IsNullOrWhiteSpace(continuationToken))
            {
                continuationToken = null;
            }
            else
            {
                continuationToken = continuationToken.FromBase64();
            }

            GoodieBag bag = GoodieBag.Create();
            PagedResult<AccountEntity> page = await bag.AccountStore.ReadAllPartitions(
                Take,
                continuationToken,
                this.GetCancellationToken());

            return this.View(
                new AccountListModel
                {
                    Accounts = page.Entities,
                    ContinuationToken = page.ContinuationToken.ToBase64(),
                });
        }

        /// <summary>
        /// View the details of an account
        /// </summary>
        /// <param name="accountId">The account ID</param>
        /// <returns>The action result</returns>
        [Route("{accountId}"), HttpGet]
        public async Task<ActionResult> Details(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            GoodieBag bag = GoodieBag.Create();
            AccountEntity account = await bag.AccountStore.Get(AccountEntity.GetReferenceById(accountId), this.GetCancellationToken());
            if (account == null)
            {
                return this.HttpNotFound();
            }

            return this.View(account);
        }
    }
}