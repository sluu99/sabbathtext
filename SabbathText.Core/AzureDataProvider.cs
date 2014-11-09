using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SabbathText.Core.Entities;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SabbathText.Core
{
    public class AzureDataProvider : IDataProvider
    {
        public const string AccountByIdentityTable = "accountbyidentity";
        public const string AccountTable = "accounts";
        public const string MessageTable = "messages";
        public const string LocationByZipTable = "locationbyzip";
        public const string PoisonMessageTable = "poisonmessages";
        public const string KeyValueTable = "keyvalues";

        CloudStorageAccount account = null;
        CloudTableClient client = null;
        readonly TimeSpan locationCacheTime = TimeSpan.FromDays(7);
        readonly TimeSpan resourceLockDuration = TimeSpan.FromSeconds(60);

        public AzureDataProvider()
        {
            string connectionString = Environment.GetEnvironmentVariable("ST_TABLE_CONN_STR");
            this.account = CloudStorageAccount.DevelopmentStorageAccount;

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                this.account = CloudStorageAccount.Parse(connectionString);
            }

            this.client = this.account.CreateCloudTableClient();
            this.LocationProvider = new WwoLocationProvider();
        }

        public ILocationProvider LocationProvider { get; set; }

        public Task CreateTableIfNotExists(string tableName)
        {
            CloudTable table = this.client.GetTableReference(tableName);
            return table.CreateIfNotExistsAsync();
        }

        public async Task<Account> GetAccountByPhoneNumber(string phoneNumber)
        {
            string identityPartitionKey;
            string identityRowKey;
            this.GetIdentityKeysFromPhoneNumber(phoneNumber, out identityPartitionKey, out identityRowKey);

            AccountIdentity identity = await this.GetEntity<AccountIdentity>(AccountByIdentityTable, identityPartitionKey, identityRowKey);

            if (identity == null)
            {
                return null;
            }

            return await this.GetEntity<Account>(AccountTable, identity.AccountId, identity.AccountId);
        }

        public async Task CreateAccountWithPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException("phoneNumber", "Phone number cannot be null or white space");
            }

            phoneNumber = phoneNumber.Trim();
            string accountId = Guid.NewGuid().ToString();

            // create the account first
            Account account = new Account
            {
                PartitionKey = accountId,
                RowKey = accountId,
                AccountId = accountId,
                PhoneNumber = phoneNumber,
                CreationTime = Clock.UtcNow,
                Status = AccountStatus.BrandNew,
            };

            await this.InsertEntity(AccountTable, account);
            
            // create an identity referencing the account
            string identityPartitionKey;
            string identityRowKey;
            this.GetIdentityKeysFromPhoneNumber(phoneNumber, out identityPartitionKey, out identityRowKey);

            AccountIdentity identity = new AccountIdentity
            {
                PartitionKey = identityPartitionKey,
                RowKey = identityRowKey,
                AccountId = accountId,
            };

            await this.InsertEntity(AccountByIdentityTable, identity);
        }

        public Task RecordMessage(string accountId, Message message)
        {
            message.PartitionKey = accountId;
            message.RowKey = message.MessageId;

            CloudTable table = this.client.GetTableReference(MessageTable);
            TableOperation operation = TableOperation.InsertOrReplace(message);
            return table.ExecuteAsync(operation);
        }

        public Task UpdateAccount(Account account)
        {
            return this.UpdateEntity(AccountTable, account);
        }

        public async Task<Location> GetLocationByZipCode(string zipCode)
        {
            // look into the cache first
            if (string.IsNullOrWhiteSpace(zipCode))
            {
                return null;
            }
            zipCode = zipCode.Trim();

            Location location = await this.GetEntity<Location>(LocationByZipTable, zipCode, zipCode);

            if (location != null && (Clock.UtcNow - location.UpdateTime) < this.locationCacheTime)
            {
                return location;
            }

            // not found in the DB or cache expired
            location = await this.LocationProvider.GetLocationByZipCode(zipCode);

            if (location != null)
            {
                location.PartitionKey = location.ZipCode;
                location.RowKey = location.ZipCode;
                location.CreationTime = Clock.UtcNow;
                location.UpdateTime = Clock.UtcNow;

                try
                {
                    await this.UpsertEntity(LocationByZipTable, location);
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("Failed to cache location {0}. Exception message: {1}", zipCode, ex.Message);
                }
            }

            return location;
        }

        public Task RecordPoisonMessage(string queueName, string message)
        {
            string key = Guid.NewGuid().ToString();

            PoisonMessage poison = new PoisonMessage
            {
                PartitionKey = key,
                RowKey = key,
                QueueName = queueName,
                Body = message,
            };

            return this.InsertEntity(PoisonMessageTable, poison);
        }

        public Task<KeyValue> GetKeyValue(string key)
        {
            key = key.Trim().ToLowerInvariant();
            return this.GetEntity<KeyValue>(KeyValueTable, key, key);
        }

        public Task PutKeyValue(KeyValue keyValue)
        {
            keyValue.PartitionKey = keyValue.Key.Trim().ToLowerInvariant();
            keyValue.RowKey = keyValue.PartitionKey;

            if (keyValue.ETag == null)
            {
                return this.InsertEntity(KeyValueTable, keyValue);
            }
            else
            {
                return this.UpdateEntity(KeyValueTable, keyValue);
            }
        }

        private void GetIdentityKeysFromPhoneNumber(string phoneNumber, out string partitionKey, out string rowKey)
        {
            string hash = string.Format("phone_{0}", phoneNumber).Sha256();
            partitionKey = hash.Substring(0, 32);
            rowKey = hash.Substring(32, 32);
        }

        private Task UpsertEntity<T>(string tableName, T entity)
            where T : class, ITableEntity
        {
            CloudTable table = this.client.GetTableReference(tableName);
            TableOperation operation = TableOperation.InsertOrReplace(entity);
            return table.ExecuteAsync(operation);
        }

        private Task InsertEntity<T>(string tableName, T entity)
            where T : class, ITableEntity
        {
            CloudTable table = this.client.GetTableReference(tableName);
            TableOperation operation = TableOperation.Insert(entity);
            return table.ExecuteAsync(operation);
        }

        private Task DeleteEntity<T>(string tableName, T entity)
            where T : class, ITableEntity
        {
            CloudTable table = this.client.GetTableReference(tableName);
            TableOperation operation = TableOperation.Delete(entity);
            return table.ExecuteAsync(operation);
        }

        private Task UpdateEntity<T>(string tableName, T entity)
            where T : class, ITableEntity
        {
            CloudTable table = this.client.GetTableReference(tableName);
            TableOperation operation = TableOperation.Replace(entity);
            return table.ExecuteAsync(operation);
        }

        private async Task<T> GetEntity<T>(string tableName, string partitionKey, string rowKey)
            where T : class, ITableEntity
        {
            CloudTable table = this.client.GetTableReference(tableName);
            TableOperation operation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            TableResult result = await table.ExecuteAsync(operation);

            if (result.Result != null)
            {
                return (T)result.Result;
            }

            return null;
        }        
    }
}
