using SabbathText.Core;
using SabbathText.Core.Entities;
using SabbathText.Web.Models;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SabbathText.Web.Controllers
{
    public class GetStartedController : Controller
    {
        public IDataProvider DataProvider { get; set; }
        public MessageQueue EventQueue { get; set; }

        public GetStartedController()
        {
            this.DataProvider = new AzureDataProvider();
            this.EventQueue = new MessageQueue(MessageQueue.EventMessageQueue);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(GetStartedModel model)
        {
            if (!ModelState.IsValid)
            {
                return this.View("Invalid", model);
            }

            string phoneNumber = PhoneUtility.ExtractUSPhoneNumber(model.PhoneNumber);

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return this.View("Invalid", model);
            }

            model.PhoneNumber = phoneNumber;

            Account account = await this.DataProvider.GetAccountByPhoneNumber(phoneNumber);

            if (account != null)            
            {                
                return this.View("TextUs", model);
            }

            // create the account
            await this.DataProvider.CreateAccountWithPhoneNumber(phoneNumber);
            await this.EventQueue.AddMessage(EventMessage.Create(phoneNumber, EventType.AccountCreated, string.Empty));
            account = await this.DataProvider.GetAccountByPhoneNumber(phoneNumber);

            // queue an event to send out a greeting to this account
            await this.EventQueue.AddMessage(EventMessage.Create(account.PhoneNumber, EventType.GreetingsRequested, string.Empty));
                        
            return this.View("Success", model);
        }
    }
}