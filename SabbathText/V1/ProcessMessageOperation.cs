namespace SabbathText.V1
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// This operation processes an incoming message
    /// </summary>
    public class ProcessMessageOperation : BaseOperation<bool>
    {
        private ProcessMessageCheckpointData checkpointData;

        /// <summary>
        /// Creates a new instance of this operation
        /// </summary>
        /// <param name="context">The operation context</param>
        public ProcessMessageOperation(OperationContext context)
            : base(context, "ProcessMessageOperation.V1")
        {
        }

        /// <summary>
        /// Processes a message
        /// </summary>
        /// <param name="message">The message to be processed</param>
        /// <returns>The operation response</returns>
        public Task<OperationResponse<bool>> Run(Message message)
        {
            string phoneNumber = message.Recipient.ExtractUSPhoneNumber();

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return Task.FromResult(new OperationResponse<bool>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorCode = CommonErrorCodes.InvalidInput,
                    ErrorDescription = "The recipient must be a valid US phone number",
                });
            }

            this.checkpointData = new ProcessMessageCheckpointData
            {
                AccountId = this.GetAccountId(phoneNumber),
                Message = message,
            };

            return this.TransitionToMarkedForProcessing();
        }

        private Task<OperationResponse<bool>> TransitionToMarkedForProcessing()
        {
            return this.DelayProcessing(
                this.checkpointData,
                HttpStatusCode.Accepted,
                true /* response data */);
        }

        /// <summary>
        /// Resumes the operation
        /// </summary>
        /// <param name="serializedCheckpointData">The serialized checkpoint data</param>
        /// <returns>The operation response</returns>
        protected override Task<OperationResponse<bool>> Resume(string serializedCheckpointData)
        {
            throw new NotImplementedException();
        }

        private class ProcessMessageCheckpointData : CheckpointData<bool>
        {
            /// <summary>
            /// Gets or sets the message to be processed
            /// </summary>
            public Message Message { get; set; }

            /// <summary>
            /// Gets or sets the account phone number
            /// </summary>
            public string PhoneNumber { get; set; }
        }
    }
}
