namespace SabbathText.Web.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using SabbathText.V1;
    using SabbathText.Web.Models;

    public class TwilioController : BaseController
    {
        [HttpPost, ValidateInput(false)]
        public async Task<ActionResult> Sms(string key, TwilioInboundSmsModel model)
        {
            GoodieBag bag = GoodieBag.Create();
            string[] acceptedTokens = 
            {
                bag.Settings.IncomingMessagePrimaryToken,
                bag.Settings.IncomingMessageSecondaryToken,
            };
            
            if (string.IsNullOrWhiteSpace(key) || acceptedTokens.Contains(key) == false)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Cannot verify access key");
            }

            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid inbound message");
            }

            string phoneNumber = model.From.ExtractUSPhoneNumber();

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return new HttpStatusCodeResult(501 /* not supported */, "Only US numbers are supported");
            }

            Message incomingMessage = new Message
            {
                Body = model.Body,
                Sender = phoneNumber,
                ExternalId = model.MessageSid,
                Recipient = bag.Settings.ServicePhoneNumber,
                Template = Entities.MessageTemplate.FreeForm,
            };

            OperationContext context = await this.CreateContext(phoneNumber);
            if (string.IsNullOrWhiteSpace(model.MessageSid) == false)
            {
                // use the external message ID as tracking ID
                context.TrackingId = model.MessageSid;
            }

            MessageProcessor processor = new MessageProcessor();
            OperationResponse<bool> response = await processor.Process(context, incomingMessage);

            string content = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?><Response></Response>";
            return this.Content(content, "text/xml");
        }
    }
}