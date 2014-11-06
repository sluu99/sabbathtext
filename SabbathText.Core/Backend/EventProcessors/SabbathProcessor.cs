using System.Threading.Tasks;

namespace SabbathText.Core.Backend.EventProcessors
{
    public class SabbathProcessor : AccountBasedProcessor
    {
        public SabbathProcessor() : base(subscriberRequired: true, skipRecordMessage: true)
        {
        }

        protected override Task<Entities.TemplatedMessage> ProcessMessageWithAccount(Entities.Message message, Entities.Account account)
        {
            return Task.FromResult(MessageFactory.CreateHappySabbath(account.PhoneNumber));
        }
    }
}
