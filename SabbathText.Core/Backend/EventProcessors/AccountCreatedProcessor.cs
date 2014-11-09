using SabbathText.Core.Entities;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend.EventProcessors
{
    public class AccountCreatedProcessor : AccountBasedProcessor
    {
        public AccountCreatedProcessor() : base(subscriberRequired: false, skipRecordMessage: true)
        {
        }

        protected override async Task<Entities.TemplatedMessage> ProcessMessageWithAccount(Entities.Message message, Entities.Account account)
        {
            KeyValue kv = await this.DataProvider.GetKeyValue(KeyValue.AccountCount);

            if (kv == null)
            {
                kv = new KeyValue
                {
                    Key = KeyValue.AccountCount,
                    Value = "0",
                };
            }

            int count = 0;
            int.TryParse(kv.Value, out count);

            count += 1;
            kv.Value = count.ToString();

            await this.DataProvider.PutKeyValue(kv);

            return null;
        }
    }
}
