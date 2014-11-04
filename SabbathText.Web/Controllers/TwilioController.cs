using SabbathText.Core;
using SabbathText.Core.Entities;
using SabbathText.Web.Models;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SabbathText.Web.Controllers
{
    public class TwilioController : Controller
    {
        public TwilioController()
        {
            this.TwilioInboundKeyPrimary = Environment.GetEnvironmentVariable("ST_TWILIO_INBOUND_KEY_PRIMARY") ?? Guid.NewGuid().ToString();
            this.TwilioInboundKeySecondary = Environment.GetEnvironmentVariable("ST_TWILIO_INBOUND_KEY_SECONDARY") ?? Guid.NewGuid().ToString();
        }

        public string TwilioInboundKeyPrimary { get; set; }
        public string TwilioInboundKeySecondary { get; set; }
        
        [HttpPost]
        public async Task<ActionResult> Sms(string key, TwilioInboundSmsModel model)
        {
            if (string.IsNullOrWhiteSpace(key) ||
                !(string.Equals(this.TwilioInboundKeyPrimary, key) || string.Equals(this.TwilioInboundKeySecondary, key)))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Cannot verify access key");
            }

            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid inbound message");
            }

            Message message = new Message
            {
                MessageId = Guid.NewGuid().ToString(),
                ExternalId = model.MessageSid,
                From = model.From,
                To = model.To,
                Body = model.Body,
                CreateOn = Clock.UtcNow,
            };

            MessageQueue manager = new MessageQueue();
            await manager.QueueInboundMessage(message);

            string content = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?><Response></Response>";
            return this.Content(content, "text/xml");
        }
    }
}