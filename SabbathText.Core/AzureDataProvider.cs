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
        public const string AccountTable = "accounts";
        public const string MessageTable = "messages";
        public const string LocationByZipTable = "locationbyzip";

        CloudStorageAccount account = null;
        CloudTableClient client = null;

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
            return await this.GetEntity<Account>(AccountTable, phoneNumber, phoneNumber);
        }

        public Task CreateAccountWithPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException("phoneNumber", "Phone number cannot be null or white space");
            }

            phoneNumber = phoneNumber.Trim();

            Account account = new Account
            {
                PartitionKey = phoneNumber,
                RowKey = phoneNumber,
                AccountId = phoneNumber,
                PhoneNumber = phoneNumber,
                CreationTime = Clock.UtcNow,
                Status = AccountStatus.BrandNew,
            };

            return this.InsertEntity(AccountTable, account);
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

            if (location != null)
            {
                return location;
            }

            // not found in the DB
            location = await this.LocationProvider.GetLocationByZipCode(zipCode);

            if (location != null)
            {
                location.PartitionKey = location.ZipCode;
                location.RowKey = location.ZipCode;

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
