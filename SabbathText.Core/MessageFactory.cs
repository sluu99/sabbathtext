using SabbathText.Core.Entities;
using System;

namespace SabbathText.Core
{
    public class MessageFactory
    {
        private string locale;

        public MessageFactory() : this("en-US")
        {
        }

        public MessageFactory(string locale)
        {
            this.locale = locale;
        }

        public TemplatedMessage CreateSubscriberGreetings(string recipient)
        {
            return Create(
                MessageTemplate.SubscriberGreetings,
                null,
                recipient,
                "Welcome back! Remember, you can always visit http://www.SabbathText.com for more information!"
            );
        }

        public TemplatedMessage CreateGeneralGreetings(string recipient)
        {
            return Create(
                MessageTemplate.GeneralGreetings,
                null,
                recipient,
                "Greetings from SabbathText.com! Text \"subscribe\" to get started. (Messaging rates may apply. Text \"STOP\" to mute at anytime)"
            );
        }

        public TemplatedMessage CreateSubscribedMissingZipCode(string recipient)
        {
            return Create(
                MessageTemplate.SubscribedMissingZipCode,
                null,
                recipient,
                "Thank you for subscribing! For the sunset time, text us your ZIP code and we'll send you a Bible text to start each Sabbath. For example, \"Zip 12345\""
            );
        }

        public TemplatedMessage CreateSubscribedConfirmZipCode(string recipient, string zipCode)
        {
            string body = string.Format(
                "Thank you for subscribing! We currently have {0} as your ZIP code. Text \"Zip <your ZIP code>\" to update!",
                zipCode
            );

            return Create(MessageTemplate.SubscribedConfirmZipCode, null, recipient, body);
        }
        
        public TemplatedMessage CreateCannotFindZipCode(string recipient, string zipCode)
        {
            string body = string.Format("Cannot find your location \"{0}\". Please double check the ZIP code and try again!", zipCode);

            return Create(MessageTemplate.BadRequest, null, recipient, body);
        }

        public TemplatedMessage CreateConfirmZipCodeUpdate(string recipient, string zipCode, string locationName, string regionName, DateTime sabbath)
        {
            if (!string.IsNullOrWhiteSpace(regionName))
            {
                locationName += "/" + regionName;
            }

            string body = string.Format(
                "Your location is set to {0}. Sabbath starts around {1:h:mm tt} on {1:m}. Expect a message around that time!",
                locationName,
                sabbath
            );
            return Create(MessageTemplate.ConfirmZipCodeUpdate, null, recipient, body);
        }

        public TemplatedMessage CreateSubscriberRequired(string recipient)
        {
            return Create(
                MessageTemplate.SubscriberRequired,
                null,
                recipient,
                "Please text \"subscribe\" to get started!"
            );
        }

        public TemplatedMessage CreateHappySabbath(string recipient, string bibleVerseNumber, string bibleVerseContent)
        {
            string body = string.Format("Happy Sabbath!\r\n\"{0}\" -- {1}", bibleVerseContent, bibleVerseNumber);

            return Create(
                MessageTemplate.HappySabbath,
                null,
                recipient,
                body
            );
        }

        public TemplatedMessage CreateBadRequest(string recipient, string messaage)
        {
            return Create(MessageTemplate.BadRequest, null, recipient, messaage);
        }

        public TemplatedMessage CreateDidYouTextZipCode(string recipient, string zipCode)
        {
            string body = string.Format(
                "\"{0}\" looks like a ZIP code. Please text \"Zip {0}\" (with the word \"zip\") if you wish to set a location.",
                zipCode.Trim()
            );

            return Create(MessageTemplate.DidYouTextZipCode, null, recipient, body);
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