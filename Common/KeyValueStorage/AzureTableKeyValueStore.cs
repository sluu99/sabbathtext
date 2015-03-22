namespace KeyValueStorage
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    /// <summary>
    /// Key value store backed by azure table    
    /// </summary>
    /// <typeparam name="T">The key value entity type</typeparam>
    public class AzureTableKeyValueStore<T> : KeyValueStore<T>
        where T : KeyValueEntity
    {
        /// <summary>
        /// The Azure storage connection string
        /// </summary>
        private string connectionString;

        /// <summary>
        /// The Azure table name
        /// </summary>
        private string tableName;

        /// <summary>
        /// The Azure table client
        /// </summary>
        private CloudTable cloudTable;

        /// <summary>
        /// Initializes a new instance of this class
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <param name="tableName">The table name</param>
        public AzureTableKeyValueStore(string connectionString, string tableName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string is required", "connectionString");
            }

            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Table name is required", "tableName");
            }

            this.tableName = tableName;
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Gets an entity
        /// </summary>
        /// <param name="partitionKey">The partition key</param>
        /// <param name="rowKey">The row key</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The entity, or null if it does not exist</returns>
        public override async Task<T> Get(string partitionKey, string rowKey, CancellationToken cancellationToken)
        {
            this.ThrowIfNullKeys(partitionKey, rowKey);

            TableOperation getOperation = TableOperation.Retrieve<AzureTableKeyValueEntity>(partitionKey, rowKey);
            TableResult result = await this.cloudTable.ExecuteAsync(getOperation, cancellationToken);

            if (result.HttpStatusCode == 404)
            {
                return null;
            }

            AzureTableKeyValueEntity rawEntity = result.Result as AzureTableKeyValueEntity;

            if (rawEntity == null)
            {
                return null;
            }

            T entity = JsonConvert.DeserializeObject<T>(rawEntity.EntityData);
            entity.Timestamp = rawEntity.Timestamp.UtcDateTime;
            entity.ETag = rawEntity.ETag;

            return entity;
        }

        /// <summary>
        /// Insert an entity into the azure table
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The insert task</returns>
        public override async Task Insert(T entity, CancellationToken cancellationToken)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            this.ThrowIfNullKeys(entity.PartitionKey, entity.RowKey);

            AzureTableKeyValueEntity rawEntity = new AzureTableKeyValueEntity
            {
                PartitionKey = entity.PartitionKey,
                RowKey = entity.RowKey,
                EntityData = JsonConvert.SerializeObject(entity),
            };

            TableOperation insertOperation = TableOperation.Insert(rawEntity);
            try
            {
                await this.cloudTable.ExecuteAsync(insertOperation, cancellationToken);
            }
            catch (StorageException se)
            {
                if (se.RequestInformation.HttpStatusCode == 409 /* conflict */ &&
                    "EntityAlreadyExists".Equals(se.RequestInformation.ExtendedErrorInformation.ErrorCode, StringComparison.InvariantCulture))
                {
                    throw new DuplicateKeyException();
                }

                throw;
            }

            entity.ETag = rawEntity.ETag;
            entity.Timestamp = rawEntity.Timestamp.UtcDateTime;
        }

        /// <summary>
        /// Updates an entity
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The update task</returns>
        public override async Task Update(T entity, CancellationToken cancellationToken)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            this.ThrowIfNullKeys(entity.PartitionKey, entity.RowKey);

            AzureTableKeyValueEntity rawEntity = new AzureTableKeyValueEntity
            {
                PartitionKey = entity.PartitionKey,
                RowKey = entity.RowKey,
                EntityData = JsonConvert.SerializeObject(entity),
                ETag = entity.ETag,
                Timestamp = entity.Timestamp,
            };

            TableOperation updateOperation = TableOperation.Replace(rawEntity);

            try
            {
                await this.cloudTable.ExecuteAsync(updateOperation, cancellationToken);
            }
            catch (StorageException se)
            {
                if (se.RequestInformation.HttpStatusCode == 404 &&
                    "ResourceNotFound".Equals(se.RequestInformation.ExtendedErrorInformation.ErrorCode, StringComparison.InvariantCulture))
                {
                    throw new EntityNotFoundException();
                }

                if (se.RequestInformation.HttpStatusCode == 412 /* precondition failed */ &&
                    ("UpdateConditionNotSatisfied".Equals(se.RequestInformation.ExtendedErrorInformation.ErrorCode, StringComparison.InvariantCulture) ||
                     "ConditionNotMet".Equals(se.RequestInformation.ExtendedErrorInformation.ErrorCode, StringComparison.InvariantCulture)))
                {
                    throw new ETagMismatchException();
                }

                throw;
            }

            entity.ETag = rawEntity.ETag;
            entity.Timestamp = rawEntity.Timestamp.UtcDateTime;
        }

        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The delete task</returns>
        public override async Task Delete(T entity, CancellationToken cancellationToken)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            this.ThrowIfNullKeys(entity.PartitionKey, entity.RowKey);

            AzureTableKeyValueEntity rawEntity = new AzureTableKeyValueEntity
            {
                PartitionKey = entity.PartitionKey,
                RowKey = entity.RowKey,
                ETag = entity.ETag,
                Timestamp = entity.Timestamp,
            };

            TableOperation deleteOperation = TableOperation.Delete(rawEntity);

            try
            {
                await this.cloudTable.ExecuteAsync(deleteOperation, cancellationToken);
            }
            catch (StorageException se)
            {
                if (se.RequestInformation.HttpStatusCode == 404 &&
                    "ResourceNotFound".Equals(se.RequestInformation.ExtendedErrorInformation.ErrorCode, StringComparison.InvariantCulture))
                {
                    throw new EntityNotFoundException();
                }

                if (se.RequestInformation.HttpStatusCode == 412 /* precondition failed */ &&
                    ("UpdateConditionNotSatisfied".Equals(se.RequestInformation.ExtendedErrorInformation.ErrorCode, StringComparison.InvariantCulture) ||
                     "ConditionNotMet".Equals(se.RequestInformation.ExtendedErrorInformation.ErrorCode, StringComparison.InvariantCulture)))
                {
                    throw new ETagMismatchException();
                }

                throw;
            }
        }

        /// <summary>
        /// Initializes the Azure table
        /// </summary>
        public override void Init()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            this.cloudTable = tableClient.GetTableReference(this.tableName);
            this.cloudTable.CreateIfNotExists();
        }
    }
}
