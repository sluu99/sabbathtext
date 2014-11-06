using SabbathText.Core.Entities;
using System;

namespace SabbathText.Core
{
    public class MessageFactory
    {
        public static TemplatedMessage CreateSubscriberGreetings(string recipient)
        {
            return Create(
                MessageTemplate.SubscriberGreetings,
                null,
                recipient,
                "Welcome back! Remember, you can always visit http://www.SabbathText.com for more information!"
            );
        }

        public static TemplatedMessage CreateGeneralGreetings(string recipient)
        {
            return Create(
                MessageTemplate.GeneralGreetings,
                null,
                recipient,
                "Greetings from SabbathText.com! Text \"subscribe\" to get started!"
            );
        }

        public static TemplatedMessage CreateSubscribedMissingZipCode(string recipient)
        {
            return Create(
                MessageTemplate.SubscribedMissingZipCode,
                null,
                recipient,
                "Thank you for subscribing! Text us your ZIP code so we can find out when Sabbath starts. For example, \"Zip 12345\""
            );
        }

        public static TemplatedMessage CreateSubscribedConfirmZipCode(string recipient, string zipCode)
        {
            string body = string.Format(
                "Thank you for subscribing! We currently have {0} as your ZIP code. Text \"Zip <your ZIP code>\" to update!",
                zipCode
            );

            return Create(MessageTemplate.SubscribedConfirmZipCode, null, recipient, body);
        }
        
        public static TemplatedMessage CreateConfirmZipCodeUpdate(string recipient, string zipCode, string locationName, DateTime sabbath)
        {
            string body = string.Format(
                "Your location is updated to \"{0}.\" Sabbath starts around {1:h:mm} on {1:m}. Expect a message around that time!",
                locationName,
                sabbath
            );
            return Create(MessageTemplate.ConfirmZipCodeUpdate, null, recipient, body);
        }

        public static TemplatedMessage CreateSubscriberRequired(string recipient)
        {
            return Create(
                MessageTemplate.SubscriberRequired,
                null,
                recipient,
                "Please text \"subscribe\" to get started!"
            );
        }

        public static TemplatedMessage CreateBadRequest(string recipient, string messaage)
        {
            return Create(MessageTemplate.BadRequest, null, recipient, messaage);
        }

        public static TemplatedMessage Create(MessageTemplate template, string sender, string recipient, string body)
        {
            return new TemplatedMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                CreationTime = Clock.UtcNow,
                Sender = sender,
                Recipient = recipient,
                Body = body,
                Template = template,
            };
        }
    }
}