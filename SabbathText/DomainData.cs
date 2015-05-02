namespace SabbathText
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This class contains the different domain data fields
    /// </summary>
    public static class DomainData
    {
        /// <summary>
        /// Bible verses
        /// </summary>
        public static readonly ReadOnlyDictionary<string, string> BibleVerses = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>
            {
                { "1 Corinthians 10:31", "Whether, then, you eat or drink or whatever you do, do all to the glory of God." },
                { "1 Chronicles 16:11", "Seek the Lord and His strength; Seek His face continually." },
                { "1 John 4:19", "We love, because He first loved us." },
                { "1 John 4:21", "And this commandment we have from Him, that the one who loves God should love his brother also." },
                { "1 Peter 5:7", "Cast all your anxiety on Him, because He cares for you." },            
                { "1 Thessalonians 5:16-18", "Rejoice always; pray without ceasing; in everything give thanks; for this is God’s will for you in Christ Jesus." },                        
                { "2 Corinthians 5:17", "Therefore if anyone is in Christ, he is a new creature; the old things passed away; behold, new things have come." },
                { "2 Timothy 1:7", "For God has not given us a spirit of timidity, but of power and love and discipline." },
                { "Exodus 20:8", "Remember the Sabbath day, to keep it holy." },
                { "Colossians 3:12", "Put on then, as God's chosen ones, holy and beloved, compassionate hearts, kindness, humility, meekness, and patience" },
                { "Hebrews 11:1", "Now faith is confidence in what we hope for and assurance about what we do not see." },
                { "James 1:2-3", "Consider it all joy, my brethren, when you encounter various trials, knowing that the testing of your faith produces endurance." },
                { "Luke 6:36", "Be merciful, just as your Father is merciful." },
                { "Matthew 6:34", "So do not worry about tomorrow; for tomorrow will care for itself. Each day has enough trouble of its own." },
                { "Matthew 11:28", "Come to Me, all who are weary and heavy-laden, and I will give you rest." },            
                { "Phil 4:6", "Do not be anxious about anything, but in everything, by prayer and petition, with thanksgiving, present your requests to God." },
                { "Philippians 4:7", "And the peace of God, which transcends all understanding, will guard your hearts and your minds in Christ Jesus." },
                { "Philippians 4:13", "I can do all things through Him who strengthens me." },
                { "Proverbs 18:10", "The name of the Lord is a strong tower; The righteous runs into it and is safe." },
                { "Psalm 29:11", "The Lord gives strength to His people; the Lord blesses His people with peace." },
                { "Psalm 37:4", "Delight yourself in the LORD; And He will give you the desires of your heart." },
                { "Psalm 46:1", "God is our refuge and strength, an ever-present help in trouble." },
                { "Psalm 56:3", "When I am afraid, I will put my trust in You." },
                { "Psalm 118:24", "This is the day the Lord has made; We will rejoice and be glad in it." },
                { "Romans 8:1", "Therefore there is now no condemnation for those who are in Christ Jesus." },
                { "Romans 12:12", "Be joyful in hope, patient in affliction, faithful in prayer." },
                { "Romans 12:13", "Share with the Lord’s people who are in need. Practice hospitality." },
            });
    }
}
