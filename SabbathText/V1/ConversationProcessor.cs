namespace SabbathText.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using SabbathText.Entities;

    /// <summary>
    /// Processes a message in the context of a conversation.
    /// </summary>
    public class ConversationProcessor
    {
        private static readonly Regex ZipMessageRegex = new Regex(@"^Zip(?:Code)?\s*(?<ZipCode>\d+)$", RegexOptions.IgnoreCase);

        /// <summary>
        /// Process a message.
        /// </summary>
        /// <param name="account">The account to process the message for.</param>
        /// <param name="message">The message content.</param>
        /// <returns>A response message.</returns>
        public Message Process(AccountEntity account, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Message cannot be null or white space", "message");
            }

            message = message.ExtractAlphaNumericSpace().Trim();

            if ("subscribe".OicEquals(message))
            {
                return this.ProcessSubscribe(account, message);
            }

            if (this.IsZipCode(message) && account.ConversationContext == ConversationContext.SubscriptionConfirmed)
            {
                return this.ProcessZipCode(account, message);
            }

            Match zipMatch = ZipMessageRegex.Match(message);
            if (zipMatch != null && zipMatch.Success && string.IsNullOrWhiteSpace(zipMatch.Groups["ZipCode"].Value) == false)
            {
                return this.ProcessZipCode(account, zipMatch.Groups["ZipCode"].Value);
            }

            return Message.CreateNotUnderstandable(account.PhoneNumber);
        }

        private Message ProcessZipCode(AccountEntity account, string zipCode)
        {
            throw new NotImplementedException();
        }

        private bool IsPositiveMessage(string message)
        {
            return
                "yes".OicEquals(message) ||
                "yep".OicEquals(message) ||
                "sure".OicEquals(message) ||
                "ok".OicEquals(message) ||
                "certainly".OicEquals(message) ||
                "of course".OicEquals(message) ||
                "for sure".OicEquals(message);
        }

        private bool IsZipCode(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return false;
            }

            str = str.Trim();

            int n;
            if (str.Length == 5 && int.TryParse(str, out n))
            {
                return true;
            }

            return false;
        }

        private Message ProcessSubscribe(AccountEntity account, string message)
        {
            if (account.Status == AccountStatus.Subscribed &&
                string.IsNullOrWhiteSpace(account.ZipCode) == false)
            {
                account.ConversationContext = ConversationContext.AlreadySubscribedWithZipCode;
                return Message.CreateAlreadySubscribedWithZipCode(
                    account.PhoneNumber,
                    account.ZipCode);
            }

            account.Status = AccountStatus.Subscribed;
            account.ConversationContext = ConversationContext.SubscriptionConfirmed;
            return Message.CreateSubscriptionConfirmed(account.PhoneNumber);
        }
    }
}
