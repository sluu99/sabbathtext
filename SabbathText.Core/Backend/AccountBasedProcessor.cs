using SabbathText.Core.Entities;
using System;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend
{
    public abstract class AccountBasedProcessor : IProcessor
    {
        private bool subscriberRequired = false;
        private bool skipRecordMessage = false;

        public AccountBasedProcessor() : this(false, false)
        {
        }

        public AccountBasedProcessor(bool subscriberRequired) : this(subscriberRequired, false)
        {
        }
        
        public AccountBasedProcessor(bool subscriberRequired, bool skipRecordMessage) 
        {
            this.DataProvider = new AzureDataProvider();
            this.EventQueue = new MessageQueue(MessageQueue.EventMessageQueue);
            this.subscriberRequired = subscriberRequired;
            this.skipRecordMessage = skipRecordMessage;
        }

        public IDataProvider DataProvider { get; set; }
        public MessageQueue EventQueue { get; set; }

        /// <summary>
        /// Start a new account cycle. This will changes the account cycle key so that other cycles will not reschedule themselves.
        /// </summary>
        protected async Task ResetAccountCycle(Account account, TimeSpan timeUntilNextCycle)
        {
            account.CycleKey = Guid.NewGuid().ToString();
            account.NextCycleTime = Clock.UtcNow + timeUntilNextCycle;

            await this.EventQueue.AddMessage(EventMessage.Create(account.PhoneNumber, EventType.AccountCycle, account.CycleKey), timeUntilNextCycle);
            await this.DataProvider.UpdateAccount(account);
        }

        protected abstract Task<TemplatedMessage> ProcessMessageWithAccount(Message message, Account account);

        public virtual async Task<TemplatedMessage> ProcessMessage(Message message)
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
                else
                {
                    await this.EventQueue.AddMessage(EventMessage.Create(message.Sender, EventType.AccountCreated, string.Empty));
                }
            }

            if (!this.skipRecordMessage)
            {
                await this.DataProvider.RecordMessage(account.AccountId, message);
            }
                        
            if (this.subscriberRequired && account.Status == AccountStatus.BrandNew)
            {
                return new MessageFactory().CreateSubscriberRequired(message.Sender);
            }            
            
            if (account.Status == AccountStatus.Unsubscribed)
            {
                return null;
            }
            
            return await this.ProcessMessageWithAccount(message, account);
        }
    }
}
