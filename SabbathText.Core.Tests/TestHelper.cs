using SabbathText.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabbathText.Core.Tests
{
    public class TestHelper
    {
        static Random Rand = new Random();

        public static Account GenerateAccount()
        {
            return new Account()
            {
                AccountId = Guid.NewGuid().ToString(),
                CreationTime = Clock.UtcNow,
                PhoneNumber = "+1" + GenerateDigits(10),
                Status = AccountStatus.Subscribed,
                ZipCode = GenerateDigits(5),
            };
        }

        public static Message CreateInboundMessage(string sender, string body)
        {
            return new Message
            {
                Body = body,
                CreationTime = Clock.UtcNow,
                ExternalId = Guid.NewGuid().ToString(),
                MessageId = Guid.NewGuid().ToString(),
                Sender = sender,
            };
        }

        static string GenerateDigits(int n)
        {
            StringBuilder sb = new StringBuilder(n);
            
            for (int i = 0; i < n; i++)
            {
                sb.Append(Rand.Next(0, 10));
            }

            return sb.ToString();
        }
    }
}
