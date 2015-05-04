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
        protected Task<AccountEntity> GetOrCreateAccount(string phoneNumber, CancellationToken token)
        {
            AccountEntity account = new AccountEntity
            {
                AccountId = AccountEntity.GetAccountId(phoneNumber),
                CreationTime = Clock.UtcNow,
                PhoneNumber = phoneNumber,
                Status = AccountStatus.BrandNew,
            };

            GoodieBag bag = GoodieBag.Create();
            return bag.AccountStore.InsertOrGet(account, token);
        }

        protected async Task<OperationContext> CreateContext(string phoneNumber)
        {
            GoodieBag bag = GoodieBag.Create();
            CancellationToken token = new CancellationTokenSource(bag.Settings.OperationTimeout).Token;
            AccountEntity account = await this.GetOrCreateAccount(phoneNumber, token);            

            return new OperationContext
            {
                Account = account,
                AccountStore = bag.AccountStore,
                CancellationToken = token,
                Compensation = bag.CompensationClient,
                LocationStore = bag.LocationStore,
                MessageClient = bag.MessageClient,
                MessageStore = bag.MessageStore,
                Settings = bag.Settings,
                TrackingId = Guid.NewGuid().ToString(),
                ZipCodeAccountIdIndices = bag.ZipCodeAccountIdIndices,
            };
        }
    }
}