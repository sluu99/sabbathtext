using SabbathText.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend.InboundProcessors
{
    public class UnknownMessageProcessor : AccountBasedProcessor
    {
        public UnknownMessageProcessor() : base(subscriberRequired: false, skipRecordMessage: true)
        {
        }

        protected override Task<Entities.TemplatedMessage> ProcessMessageWithAccount(Entities.Message message, Entities.Account account)
        {
            if (account.Status == AccountStatus.Subscribed)
            {
                if (IsZipCode(message.Body))
                {
                    return Task.FromResult(new MessageFactory().CreateDidYouTextZipCode(account.PhoneNumber, message.Body.Trim()));
                }
            }

            throw new NotImplementedException();
        }
        
        private static bool IsZipCode(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return false;
            }

            str = str.Trim();
            
            int n;
            if (str.Length == 5 && int.TryParse(str, out n))
            {
                return true;
            }

            return false;
        }
    }
}
