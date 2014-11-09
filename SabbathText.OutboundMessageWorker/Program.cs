using SabbathText.Core;
using SabbathText.Core.Backend;
using SabbathText.Core.Entities;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SabbathText.OutboundMessageWorker
{
    class Program
    {
        static IMessageSender sender;
        static IDataProvider dataProvider;
        static bool missingTwilio = false;

        static void Main(string[] args)
        {
            SabbathText.Core.Common.Setup();
            SabbathText.Core.Common.SetupStorage();

            if (Environment.GetEnvironmentVariable("ST_TWILIO_ACCOUNT") == null ||
                Environment.GetEnvironmentVariable("ST_TWILIO_TOKEN") == null ||
                Environment.GetEnvironmentVariable("ST_TWILIO_PHONE_NUMBER") == null)
            {
                Trace.TraceWarning("Missing Twilio credentials");
                Program.missingTwilio = true;
            }

            Program.sender = new TwilioMessageSender();
            Program.dataProvider = new AzureDataProvider();

            Supervisor supervisor = new AzureWebJobSupervisor(MessageQueue.OutboundMessageQueue);
            supervisor.Start(Program.SendMessage).Wait();
        }

        async static Task<bool> SendMessage(Message message)
        {
            if (!Program.missingTwilio)
            {
                message.ExternalId = await Program.sender.Send(message);
            }

            string accountId = message.Recipient;

            Account account = await Program.dataProvider.GetAccountByPhoneNumber(message.Recipient);
                        
            if (account == null)
            {
                Trace.TraceWarning("Cannot find account for message {0}", message.MessageId);
            }
            else
            {
                accountId = account.AccountId;
            }
            
            await Program.dataProvider.RecordMessage(accountId, message);
            return true;
        }
    }
}
