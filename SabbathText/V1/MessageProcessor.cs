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
    public class MessageProcessor
    {
        /// <summary>
        /// Process a message.
        /// </summary>
        /// <param name="context">The operation context.</param>
        /// <param name="message">The message .</param>
        /// <returns>A response message.</returns>
        public async Task<OperationResponse<bool>> Process(OperationContext context, Message message)
        {
            GoodieBag bag = GoodieBag.Create();
            DateTime startTime = Clock.UtcNow;

            try
            {                
                OperationResponse<bool> response = await this.InternalProcess(context, message);
                bag.TelemetryTracker.MessageProcessed(message.Sender, message.Body, Clock.UtcNow - startTime);

                return response;
            }
            catch (Exception ex)
            {
                bag.TelemetryTracker.ProcessMessageException(ex, message.Sender, message.Body, Clock.UtcNow - startTime);
                throw;
            }            
        }

        private Task<OperationResponse<bool>> InternalProcess(OperationContext context, Message message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.Body))
            {
                return Task.FromResult(
                    new OperationResponse<bool>()
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ErrorCode = CommonErrorCodes.InvalidInput,
                    });
            }

            string body = message.Body.ExtractAlphaNumericSpace().Trim();

            if ("subscribe".OicEquals(body))
            {
                SubscribeMessageOperation subscribe = new SubscribeMessageOperation(context);
                return subscribe.Run(message);
            }
            else if (body.ToLowerInvariant().StartsWith("zip", StringComparison.OrdinalIgnoreCase))
            {
                UpdateZipCodeOperation updateZipCode = new UpdateZipCodeOperation(context);
                return updateZipCode.Run(message);
            }
            else if (
                "bible verse".OicEquals(body) ||
                "verse".OicEquals(body) || 
                "bibleverse".OicEquals(body) ||
                "bible text".OicEquals(body))
            {
                BibleVerseOperation bibleVerseOperation = new BibleVerseOperation(context);
                return bibleVerseOperation.Run(message);
            }
                        
            return Task.FromResult(
                new OperationResponse<bool>
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ErrorCode = CommonErrorCodes.UnrecognizedIncomingMessage,
                    ErrorDescription = "Cannot process message with the content '{0}'".InvariantFormat(body),
                });
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
    }
}
