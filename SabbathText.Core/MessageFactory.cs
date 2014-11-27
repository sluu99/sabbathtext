using SabbathText.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SabbathText.Core
{
    public class MessageFactory
    {
        static readonly Random Rand = new Random();

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

        public TemplatedMessage CreateConfirmZipCodeUpdate(string recipient, string zipCode, string locationName, string regionName, DateTime sabbath, TimeSpan timeUntilSabbath)
        {
            if (!string.IsNullOrWhiteSpace(regionName))
            {
                locationName += ", " + regionName;
            }

            string sabbathTime;

            string body;

            if (timeUntilSabbath < TimeSpan.Zero) // Sabbath already started
            {
                body = string.Format("Your location is set to {0}. It is now the Sabbath!", locationName);
            }
            else
            {
                if (timeUntilSabbath.TotalMinutes <= 15) // Sabbath starts within 15 mins
                {
                    sabbathTime = string.Format("in {0:0} minutes", timeUntilSabbath.TotalMinutes);
                }
                else
                {
                    sabbathTime = string.Format("around {0:h:mm tt} on {0:m}", sabbath);
                }

                body = string.Format(
                    "Your location is set to {0}. Sabbath starts {1}. Expect a message around that time!",
                    locationName,
                    sabbathTime
                );
            }
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

        public TemplatedMessage CreateHappySabbath(string recipient)
        {
            string[] verseNumbers = BibleVerses.Keys.ToArray();
            string randomVerse = verseNumbers[Rand.Next(0, verseNumbers.Length)];
            string verseContent = BibleVerses[randomVerse];

            string body = string.Format("Happy Sabbath!\r\n\"{0}\" -- {1}", verseContent, randomVerse);

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

        public TemplatedMessage CreateCustomMessage(string recipient, string body)
        {
            return Create(MessageTemplate.CustomMessage, null, recipient, body);
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

        static readonly Dictionary<string, string> BibleVerses = new Dictionary<string, string>
        {
            { "1 Corinthians 10:31", "Whether, then, you eat or drink or whatever you do, do all to the glory of God." },
            { "1 Chronicles 16:11", "Seek the Lord and His strength; Seek His face continually." },
            { "1 John 4:19", "We love, because He first loved us." },
            { "1 Peter 5:7", "Cast all your anxiety on Him, because He cares for you." },            
            { "1 Thessalonians 5:16-18", "Rejoice always; pray without ceasing; in everything give thanks; for this is God’s will for you in Christ Jesus." },                        
            { "2 Corinthians 5:17", "Therefore if anyone is in Christ, he is a new creature; the old things passed away; behold, new things have come." },
            { "2 Timothy 1:7", "For God has not given us a spirit of timidity, but of power and love and discipline." },
            { "Exodus 20:8", "Remember the Sabbath day, to keep it holy." },
            { "Colossians 3:12", "Put on then, as God's chosen ones, holy and beloved, compassionate hearts, kindness, humility, meekness, and patience" },
            { "Hebrews 11:1", "Now faith is confidence in what we hope for and assurance about what we do not see." },
            { "James 1:2-3", "Consider it all joy, my brethren, when you encounter various trials, knowing that the testing of your faith produces endurance." },  
            { "Matthew 11:28", "Come to Me, all who are weary and heavy-laden, and I will give you rest." },
            { "Phil 4:6", "Do not be anxious about anything, but in everything, by prayer and petition, with thanksgiving, present your requests to God." },
            { "Philippians 4:7", "And the peace of God, which transcends all understanding, will guard your hearts and your minds in Christ Jesus." },
            { "Philippians 4:13", "I can do all things through Him who strengthens me." },
            { "Proverbs 18:10", "The name of the Lord is a strong tower; The righteous runs into it and is safe." },
            { "Psalm 37:4", "Delight yourself in the LORD; And He will give you the desires of your heart." },
            { "Psalm 46:1", "God is our refuge and strength, an ever-present help in trouble." },
            { "Psalm 56:3", "When I am afraid, I will put my trust in You." },
            { "Psalm 118:24", "This is the day the Lord has made; We will rejoice and be glad in it." },
            { "Romans 8:1", "Therefore there is now no condemnation for those who are in Christ Jesus." },
            { "Romans 12:12", "Be joyful in hope, patient in affliction, faithful in prayer." },
            { "Romans 12:13", "Share with the Lord’s people who are in need. Practice hospitality." },
        };
    }
}