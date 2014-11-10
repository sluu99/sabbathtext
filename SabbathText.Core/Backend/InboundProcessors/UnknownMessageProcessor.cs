using SabbathText.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend.InboundProcessors
{
    public class UnknownMessageProcessor : AccountBasedProcessor
    {
        public static readonly Regex UnknownMessageRegex = new Regex(".*");

        public UnknownMessageProcessor() : base(subscriberRequired: false, skipRecordMessage: true)
        {
        }

        protected override Task<Entities.TemplatedMessage> ProcessMessageWithAccount(Entities.Message message, Entities.Account account)
        {
            if (account.Status == AccountStatus.Subscribed)
            {
                string body = message.Body.ExtractAlphaNumericSpace().Trim();
                if (IsZipCode(body))
                {
                    return Task.FromResult(new MessageFactory().CreateDidYouTextZipCode(account.PhoneNumber, body));
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
