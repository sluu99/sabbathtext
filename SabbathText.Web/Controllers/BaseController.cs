namespace SabbathText.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using SabbathText.Entities;
    using SabbathText.V1;

    /// <summary>
    /// The base controller class for the web project
    /// </summary>
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// Gets or creates an account
        /// </summary>
        /// <param name="phoneNumber">The account phone number</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The account</returns>
        protected Task<AccountEntity> GetOrCreateAccount(string phoneNumber, CancellationToken token)
        {
            AccountEntity account = new AccountEntity
            {
                AccountId = AccountEntity.GetAccountIdByPhoneNumber(phoneNumber),
                CreationTime = Clock.UtcNow,
                PhoneNumber = phoneNumber,
                Status = AccountStatus.BrandNew,
            };

            GoodieBag bag = GoodieBag.Create();
            return bag.AccountStore.InsertOrGet(account, token);
        }

        /// <summary>
        /// Creates an operation context, with the account identified by a phone number.
        /// </summary>
        /// <param name="phoneNumber">The account phone number.</param>
        /// <returns>An operation context instance.</returns>
        protected async Task<OperationContext> CreateContext(string phoneNumber)
        {
            GoodieBag bag = GoodieBag.Create();
            CancellationToken token = new CancellationTokenSource(bag.Settings.OperationTimeout).Token;
            AccountEntity account = await this.GetOrCreateAccount(phoneNumber, token);            

            return new OperationContext
            {
                Account = account,
                CancellationToken = token,
                TrackingId = Guid.NewGuid().ToString(),
            };
        }

        /// <summary>
        /// Gets the default cancellation
        /// </summary>
        /// <returns>A cancellation token</returns>
        protected CancellationToken GetCancellationToken()
        {
            return new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;
        }
    }
}