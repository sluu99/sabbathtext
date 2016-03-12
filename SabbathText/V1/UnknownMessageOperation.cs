using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabbathText.V1
{
    public class UnknownMessageOperation : BaseOperation<bool>
    {
        public UnknownMessageOperation(OperationContext context)
            : base(context, "UnknownMessageOperation.V1")
        {
        }

        public async Task<OperationResponse<bool>> Run(Message message)
        {
            var adminSmsNumber = Environment.GetEnvironmentVariable("ADMIN_SMS");
            adminSmsNumber = adminSmsNumber.ExtractUSPhoneNumber();

            if (string.IsNullOrWhiteSpace(adminSmsNumber))
            {
                var notificationMessage = new Message
                {
                    Body = "Unknown message from {0}: {1}".InvariantFormat(message.Sender, message.Body),
                    Recipient = adminSmsNumber,
                    Template = Entities.MessageTemplate.FreeForm,
                    Timestamp = Clock.UtcNow,
                };

                await this.Bag.MessageClient.SendMessage(notificationMessage, this.Context.TrackingId, this.Context.CancellationToken);
            }

            return new OperationResponse<bool>
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                ErrorCode = CommonErrorCodes.UnrecognizedIncomingMessage,
                ErrorDescription = "Cannot process message with the content '{0}'".InvariantFormat(message.Body),
            };
        }

        protected override Task<OperationResponse<bool>> Resume(string serializedCheckpointData)
        {
            throw new NotImplementedException();
        }
    }
}
