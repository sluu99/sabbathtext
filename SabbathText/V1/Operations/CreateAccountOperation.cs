namespace SabbathText.V1.Operations
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using KeyValueStorage;
    using SabbathText.Compensation;
    using SabbathText.V1.Entities;

    /// <summary>
    /// This operation creates an account
    /// </summary>
    public class CreateAccountOperation : BaseOperation<Account>
    {
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
            Started,
            CreatingIdentity,
            CreatingAccount,
        }

        /// <summary>
        /// Creates an account with a phone number
        /// </summary>
        /// <param name="phoneNumber">The phone number</param>
        /// <returns>The account</returns>
        public Task<OperationResponse<Account>> CreateWithPhoneNumber(string phoneNumber)
        {
            phoneNumber = phoneNumber.ExtractUSPhoneNumber();

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return Task.FromResult(new OperationResponse<Account>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorDescription = "Invalid phone number {0}".InvariantFormat(phoneNumber),
                });
            }

            this.checkpointData = new CreateAccountCheckpointData
            {
                PhoneNumber = phoneNumber,
            };

            return this.TransitionToCreateIdentity();
        }

        private async Task<OperationResponse<Account>> TransitionToCreateIdentity()
        {
            this.checkpointData.Checkpoint = CreateAccountCheckpoint.CreatingIdentity;
            this.checkpointData.AccountId = Guid.NewGuid().ToString();

            OperationResponse<Account> response =
                await this.CreateOrUpdateCheckpoint(this.checkpointData.AccountId, this.checkpointData);
            if (response != null)
            {
                return response;
            }

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

            if (identity.AccountId != this.checkpointData.AccountId)
            {
                return await this.Complete(
                    this.checkpointData.AccountId, /* partition key */
                    this.checkpointData,
                    HttpStatusCode.Conflict,
                    null, /* response data */
                    "PhoneNumberUsed",
                    "A different operation already used this phone number to create an account");
            }

            return await this.TransitionToCreateAccount();
        }

        private async Task<OperationResponse<Account>> TransitionToCreateAccount()
        {
            this.checkpointData.Checkpoint = CreateAccountCheckpoint.CreatingAccount;
            
            OperationResponse<Account> response =
                await this.CreateOrUpdateCheckpoint(this.checkpointData.AccountId, this.checkpointData);
            if (response != null)
            {
                return response;
            }

            return await this.EnterCreateAccount();
        }

        private async Task<OperationResponse<Account>> EnterCreateAccount()
        {
            Account account = new Account
            {
                PartitionKey = this.checkpointData.AccountId,
                RowKey = this.checkpointData.AccountId,
                AccountId = this.checkpointData.AccountId,
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
                    "AccountCreatedPhoneMismatch",
                    "A diffent operation created the account with a different phone number");
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
            public CreateAccountCheckpoint Checkpoint { get; set; }

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
