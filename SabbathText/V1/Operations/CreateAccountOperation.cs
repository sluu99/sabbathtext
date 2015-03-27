namespace SabbathText.V1.Operations
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using KeyValueStorage;
    using Newtonsoft.Json;
    using SabbathText.Compensation;
    using SabbathText.Entities;

    /// <summary>
    /// This operation creates an account
    /// </summary>
    public class CreateAccountOperation : BaseOperation<Account>
    {
        private const string AccountCreatedPhoneMismatchErrorCode = "AccountCreatedPhoneMismatch";
        private const string AccountCreatedPhoneMismatchErrorDescription = "This account was created with a different phone number";

        private CreateAccountCheckpointData checkpointData;

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="context">The operation context</param>
        public CreateAccountOperation(OperationContext context)
            : base(context, "CreateAccountOperation.V1")
        {
        }

        private enum CreateAccountCheckpoint
        {
            CreatingIdentity,
            CreatingAccount,
        }

        /// <summary>
        /// Creates an account with a phone number
        /// </summary>
        /// <param name="phoneNumber">The phone number</param>
        /// <returns>The account</returns>
        public Task<OperationResponse<Account>> Run(string phoneNumber)
        {
            phoneNumber = phoneNumber.ExtractUSPhoneNumber();

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return Task.FromResult(new OperationResponse<Account>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorCode = CommonErrorCodes.InvalidInput,
                    ErrorDescription = "Invalid phone number {0}".InvariantFormat(phoneNumber),
                });
            }

            this.checkpointData = new CreateAccountCheckpointData
            {
                PhoneNumber = phoneNumber,
            };

            return this.TransitionToCreateIdentity();
        }
        
        /// <summary>
        /// Resumes the current operation
        /// </summary>
        /// <param name="serializedCheckpointData">The serialized checkpoint data</param>
        /// <returns>The operation response</returns>
        protected override Task<OperationResponse<Account>> Resume(string serializedCheckpointData)
        {
            this.checkpointData = JsonConvert.DeserializeObject<CreateAccountCheckpointData>(serializedCheckpointData);

            switch (this.checkpointData.State)
            {
                case CreateAccountCheckpoint.CreatingIdentity:
                    return this.EnterCreateIdentity();
                case CreateAccountCheckpoint.CreatingAccount:
                    return this.EnterCreateAccount();
                default:
                    throw new NotImplementedException();
            }
        }

        private async Task<OperationResponse<Account>> TransitionToCreateIdentity()
        {
            this.checkpointData.State = CreateAccountCheckpoint.CreatingIdentity;            
            return await this.EnterCreateIdentity();
        }

        private async Task<OperationResponse<Account>> EnterCreateIdentity()
        {
            string identityHash = ("PhoneHash:" + this.checkpointData.PhoneNumber).Sha256();
            Identity identity = new Identity
            {
                Type = IdentityType.PhoneHash,
                PartitionKey = identityHash,
                RowKey = identityHash,
                AccountId = this.checkpointData.AccountId,
            };

            identity = await this.Context.IdentityStore.InsertOrGet(identity, this.Context.CancellationToken);
            this.checkpointData.AccountId = identity.AccountId;

            return await this.TransitionToCreateAccount();
        }

        private async Task<OperationResponse<Account>> TransitionToCreateAccount()
        {
            this.checkpointData.State = CreateAccountCheckpoint.CreatingAccount;
            
            return
                await this.CreateOrUpdateCheckpoint(this.checkpointData.AccountId, this.checkpointData) ??
                await this.EnterCreateAccount();
        }

        private async Task<OperationResponse<Account>> EnterCreateAccount()
        {
            Account account = new Account
            {
                PartitionKey = this.checkpointData.AccountId,
                RowKey = this.checkpointData.AccountId,
                AccountId = this.checkpointData.AccountId,
                CreationTrackingId = this.Context.TrackingId,
                CreationTime = Clock.UtcNow,
                Status = AccountStatus.BrandNew,
                PhoneNumber = this.checkpointData.PhoneNumber,
            };

            account = await this.Context.AccountStore.InsertOrGet(account, this.Context.CancellationToken);

            if (this.checkpointData.PhoneNumber != account.PhoneNumber)
            {
                return await this.Complete(
                    this.checkpointData.AccountId, /* partition key */
                    this.checkpointData,
                    HttpStatusCode.Conflict,
                    null, /* response data */
                    AccountCreatedPhoneMismatchErrorCode,
                    AccountCreatedPhoneMismatchErrorDescription);
            }

            return await this.Complete(
                this.checkpointData.AccountId, /* partition key */
                this.checkpointData,
                HttpStatusCode.Created, 
                account, /* response data */
                errorCode: null,
                errorDescription: null);
        }

        private class CreateAccountCheckpointData : CheckpointData<Account>
        {
            /// <summary>
            /// Gets or sets the checkpoint
            /// </summary>
            public CreateAccountCheckpoint State { get; set; }

            /// <summary>
            /// Gets or sets the phone number
            /// </summary>
            public string PhoneNumber { get; set; }

            /// <summary>
            /// Gets or sets the account ID
            /// </summary>
            public string AccountId { get; set; }
        }
    }
}
