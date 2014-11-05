using SabbathText.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend.InboundProcessors
{
    public abstract class AccountBasedProcessor : IProcessor
    {
        public AccountBasedProcessor()
        {
            this.DataProvider = new AzureDataProvider();
        }

        public IDataProvider DataProvider { get; set; }

        protected abstract Task<TemplatedMessage> ProcessMessageWithAccount(Message message, Account account);

        public async Task<TemplatedMessage> ProcessMessage(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (string.IsNullOrWhiteSpace(message.Sender))
            {
                throw new ArgumentException("Message sender cannot be null or white space");
            }

            Account account = await this.DataProvider.GetAccountByPhoneNumber(message.Sender);

            if (account == null)
            {
                await this.DataProvider.CreateAccountWithPhoneNumber(message.Sender);
                account = await this.DataProvider.GetAccountByPhoneNumber(message.Sender);

                if (account == null)
                {
                    throw new ApplicationException("Unknown exception. Cannot create account");
                }
            }

            await this.DataProvider.RecordMessage(account.AccountId, message);

            return await this.ProcessMessageWithAccount(message, account);

        }
    }
}
