namespace KeyValueStorage
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Bson;

    /// <summary>
    /// Key value store backed by azure table    
    /// </summary>
    /// <typeparam name="T">The key value entity type</typeparam>
    public class KeyValueStore<T>
        where T : KeyValueEntity
    {
        /// <summary>
        /// The Azure table client
        /// </summary>
        private CloudTable cloudTable;

        /// <summary>
        /// Initializes the underlying Azure storage
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <param name="tableName">The table name</param>
        public void InitAzureTable(string connectionString, string tableName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string is required", "connectionString");
            }

            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Table name is required", "tableName");
            }

            CloudStorageAccount account = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = account.CreateCloudTableClient();
            this.cloudTable = tableClient.GetTableReference(tableName);
            this.cloudTable.CreateIfNotExists();
        }
        
        /// <summary>
        /// Gets an entity
        /// </summary>
        /// <param name="partitionKey">The partition key</param>
        /// <param name="rowKey">The row key</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The entity, or null if it does not exist</returns>
        public virtual async Task<T> Get(string partitionKey, string rowKey, CancellationToken cancellationToken)
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

            T entity = this.Deserialize<T>(rawEntity.EntityData);
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
        public virtual async Task Insert(T entity, CancellationToken cancellationToken)
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
                EntityData = this.Serialize(entity),
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
        public virtual async Task Update(T entity, CancellationToken cancellationToken)
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
                EntityData = this.Serialize(entity),
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
        public virtual async Task Delete(T entity, CancellationToken cancellationToken)
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
        /// Inserts or gets an entity from the store
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The entity itself</returns>
        public virtual async Task<T> InsertOrGet(T entity, CancellationToken cancellationToken)
        {
            try
            {
                await this.Insert(entity, cancellationToken);
                return entity;
            }
            catch (DuplicateKeyException)
            {
            }

            entity = await this.Get(entity.PartitionKey, entity.RowKey, cancellationToken);
            if (entity == null)
            {
                throw new EntityNotFoundException();
            }

            return entity;
        }

        /// <summary>
        /// Throws ArgumentNullExceptions when either key is null or empty
        /// </summary>
        /// <param name="partitionKey">The partition key</param>
        /// <param name="rowKey">The row key</param>
        protected void ThrowIfNullKeys(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey))
            {
                throw new ArgumentNullException("partitionKey");
            }

            if (string.IsNullOrEmpty(rowKey))
            {
                throw new ArgumentNullException("rowKey");
            }
        }

        private byte[] Serialize(object obj)
        {
            using (MemoryStream stream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(stream))
            using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(jsonWriter, obj);

                jsonWriter.Flush();
                stream.Flush();
                stream.Position = 0;

                return this.Compress(stream);
            }
        }

        private byte[] Compress(MemoryStream uncompressedStream)
        {
            uncompressedStream.Position = 0;

            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory, CompressionLevel.Optimal, true))
                {
                    uncompressedStream.CopyTo(gzip);
                }

                return memory.ToArray();
            }
        }

        private void Decompressed(MemoryStream compressedStream, MemoryStream outStream)
        {
            compressedStream.Position = 0;
            using (GZipStream gzip = new GZipStream(compressedStream, CompressionMode.Decompress, true))
            {
                gzip.CopyTo(outStream);
            }
        }

        private TData Deserialize<TData>(byte[] data)
        {
            using (MemoryStream compressedStream = new MemoryStream(data))
            using (MemoryStream stream = new MemoryStream())
            using (StreamReader reader = new StreamReader(stream))
            using (JsonTextReader jsonReader = new JsonTextReader(reader))
            {
                this.Decompressed(compressedStream, stream);
                stream.Position = 0;

                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize<TData>(jsonReader);
            }
        }
    }
}
