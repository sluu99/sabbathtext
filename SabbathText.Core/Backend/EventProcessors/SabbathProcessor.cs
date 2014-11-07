using SabbathText.Core.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

namespace SabbathText.Core.Backend.EventProcessors
{
    public class SabbathProcessor : AccountBasedProcessor
    {
        static readonly TimeSpan SabbathMessageGap = TimeSpan.FromDays(5);
        static readonly Random Rand = new Random();

        public SabbathProcessor()
            : base(subscriberRequired: true, skipRecordMessage: true)
        {
        }

        protected override async Task<Entities.TemplatedMessage> ProcessMessageWithAccount(Entities.Message message, Entities.Account account)
        {
            TimeSpan timeSinceLastSabbathMessage = Clock.UtcNow - account.LastSabbathMessageTime;

            if (timeSinceLastSabbathMessage < SabbathMessageGap)
            {
                Trace.TraceInformation("Time since last Sabbath message for account {0} is {1}. Skipped!", account.AccountId.Mask(4), timeSinceLastSabbathMessage);
                return null;
            }

            account.LastSabbathMessageTime = Clock.UtcNow;

            await this.DataProvider.UpdateAccount(account);

            string[] verseNumbers = BibleVerses.Keys.ToArray();
            string randomVerse = verseNumbers[Rand.Next(0, verseNumbers.Length)];
            string verseContent = BibleVerses[randomVerse];

            return new MessageFactory().CreateHappySabbath(account.PhoneNumber, randomVerse, verseContent);
        }

        static readonly Dictionary<string, string> BibleVerses = new Dictionary<string, string>
        {
            { "1 Peter 5:7", "casting all your anxiety on Him, because He cares for you." },
            { "2 Corinthians 5:17", "Therefore if anyone is in Christ, he is a new creature; the old things passed away; behold, new things have come." },
            { "2 Timothy 1:7", "For God has not given us a spirit of timidity, but of power and love and discipline." },
            { "Exodus 20:8", "Remember the Sabbath day, to keep it holy." },
            { "Colossians 3:12", "Put on then, as God's chosen ones, holy and beloved, compassionate hearts, kindness, humility, meekness, and patience" },
            { "Hebrews 11:1", "Now faith is confidence in what we hope for and assurance about what we do not see." },
            { "James 1:2-3", "Consider it all joy, my brethren, when you encounter various trials, knowing that the testing of your faith produces endurance." },  
            { "Matthew 11:28", "Come to Me, all who are weary and heavy-laden, and I will give you rest." },
            { "Phil 4:6", "Do not be anxious about anything, but in everything, by prayer and petition, with thanksgiving, present your requests to God." },
            { "Philippians 4:7", "And the peace of God, which transcends all understanding, will guard your hearts and your minds in Christ Jesus." },
            { "Psalm 37:4", "Delight yourself in the LORD ; And He will give you the desires of your heart." },
            { "Psalm 118:24", "This is the day the Lord has made; We will rejoice and be glad in it." },
        };
    }
}
