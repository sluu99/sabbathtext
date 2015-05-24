namespace SabbathText.Web.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using SabbathText.V1;
    using SabbathText.Web.Models;

    /// <summary>
    /// A controller to communicate with the <c>Twilio</c> service.
    /// </summary>
    public class TwilioController : BaseController
    {
        /// <summary>
        /// The method will be the callback when <c>Twilio</c> receives a text message.
        /// </summary>
        /// <param name="key">The authentication key.</param>
        /// <param name="model">The message model.</param>
        /// <returns>An action result.</returns>
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

            if ((int)response.StatusCode / 100 == 2)
            {
                // successful response
                string content = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?><Response></Response>";
                return this.Content(content, "text/xml");
            }
            else
            {
                return new HttpStatusCodeResult(response.StatusCode);
            }
        }
    }
}