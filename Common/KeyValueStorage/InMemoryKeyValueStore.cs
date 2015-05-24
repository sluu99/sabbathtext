namespace KeyValueStorage
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// This class provides an in-memory key value store implementation
    /// </summary>
    /// <typeparam name="T">The key value entity type</typeparam>
    public class InMemoryKeyValueStore<T> : KeyValueStore<T>
        where T : KeyValueEntity
    {
        /// <summary>
        /// The internal storage
        /// </summary>
        private SortedDictionary<string, string> entities;

        /// <summary>
        /// The SHA265 hashing provider
        /// </summary>
        private SHA256 sha256;

        /// <summary>
        /// Gets an entity
        /// </summary>
        /// <param name="partitionKey">The partition key</param>
        /// <param name="rowKey">The row key</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The entity, or null if not found</returns>
        public override Task<T> Get(string partitionKey, string rowKey, CancellationToken cancellationToken)
        {
            string key = this.HashEntityKeys(partitionKey, rowKey);

            if (!this.entities.ContainsKey(key))
            {
                return Task.FromResult<T>(null);
            }

            lock (this.entities)
            {
                if (!this.entities.ContainsKey(key))
                {
                    return Task.FromResult<T>(null);
                }

                return Task.FromResult(JsonConvert.DeserializeObject<T>(this.entities[key]));
            }
        }

        /// <summary>
        /// Reads the entities from a particular partition.
        /// </summary>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="take">The number of entities to read.</param>
        /// <param name="continuationToken">The continuation token. Specify null for the first time.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The paged result.</returns>
        public override Task<PagedResult<T>> ReadPartition(string partitionKey, int take, string continuationToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(partitionKey))
            {
                throw new ArgumentException("The partition key is required", "partitionKey");
            }

            if (take < 1 || take > 100)
            {
                throw new ArgumentException("Take must be between 1 and 100, inclusive", "take");
            }

            string partitionKeyPrefix = this.Hash(partitionKey) + "::";

            int skip = 0;
            if (continuationToken != null && int.TryParse(continuationToken, out skip) == false)
            {
                throw new ArgumentException("Invalid continuation token", "continuationToken");
            }

            PagedResult<T> pagedResult = new PagedResult<T>(take);
            pagedResult.Entities.AddRange(
                this.entities
                .Where(kv => kv.Key.StartsWith(partitionKeyPrefix))
                .Skip(skip)
                .Take(take)
                .Select(kv => JsonConvert.DeserializeObject<T>(kv.Value)));

            if (pagedResult.Entities.Count == take)
            {
                pagedResult.ContinuationToken = (skip + pagedResult.Entities.Count).ToString();
            }            

            return Task.FromResult(pagedResult);
        }

        /// <summary>
        /// Inserts an entity to the key value store
        /// </summary>
        /// <param name="entity">The entity for insertion</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The insertion Task</returns>
        /// <exception cref="DuplicateKeyException">Thrown when the partition key and row key already exist</exception>
        public override Task Insert(T entity, CancellationToken cancellationToken)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            string key = this.HashEntityKeys(entity.PartitionKey, entity.RowKey);

            if (this.entities.ContainsKey(key))
            {
                throw new DuplicateKeyException();
            }

            lock (this.entities)
            {
                if (this.entities.ContainsKey(key))
                {
                    throw new DuplicateKeyException();
                }

                entity.ETag = Guid.NewGuid().ToString();
                entity.Timestamp = Clock.UtcNow;

                this.entities.Add(key, JsonConvert.SerializeObject(entity));
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Updates an entity
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The update task</returns>
        public override Task Update(T entity, CancellationToken cancellationToken)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            string key = this.HashEntityKeys(entity.PartitionKey, entity.RowKey);

            if (!this.entities.ContainsKey(key))
            {
                throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, "Entity {0}/{1} does not exist", entity.PartitionKey, entity.RowKey));
            }

            lock (this.entities)
            {
                if (!this.entities.ContainsKey(key))
                {
                    throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, "Entity {0}/{1} does not exist", entity.PartitionKey, entity.RowKey));
                }

                T existingEntity = JsonConvert.DeserializeObject<T>(this.entities[key]);

                if (existingEntity.ETag != entity.ETag)
                {
                    throw new ETagMismatchException();
                }

                entity.ETag = Guid.NewGuid().ToString();
                entity.Timestamp = Clock.UtcNow;

                this.entities[key] = JsonConvert.SerializeObject(entity);
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The delete task</returns>
        public override Task Delete(T entity, CancellationToken cancellationToken)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            string key = this.HashEntityKeys(entity.PartitionKey, entity.RowKey);

            if (!this.entities.ContainsKey(key))
            {
                throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, "Entity {0}/{1} does not exist", entity.PartitionKey, entity.RowKey));
            }

            lock (this.entities)
            {
                if (!this.entities.ContainsKey(key))
                {
                    throw new EntityNotFoundException(string.Format(CultureInfo.InvariantCulture, "Entity {0}/{1} does not exist", entity.PartitionKey, entity.RowKey));
                }

                T existingEntity = JsonConvert.DeserializeObject<T>(this.entities[key]);

                if (existingEntity.ETag != entity.ETag)
                {
                    throw new ETagMismatchException();
                }

                this.entities.Remove(key);
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Initializes the internal storage
        /// </summary>
        public void InitMemory()
        {
            this.entities = new SortedDictionary<string, string>();
            this.sha256 = SHA256CryptoServiceProvider.Create();
        }

        /// <summary>
        /// Clears the entity store
        /// </summary>
        public void Clear()
        {
            this.entities.Clear();
        }

        /// <summary>
        /// Hash the entity keys with the following scheme:
        /// SHA256(partitionKey) + "::" + SHA256(rowKey)
        /// </summary>
        /// <param name="partitionKey">The partition key</param>
        /// <param name="rowKey">The row key</param>
        /// <returns>Hashed version of the keys</returns>
        private string HashEntityKeys(string partitionKey, string rowKey)
        {
            this.ThrowIfNullKeys(partitionKey, rowKey);

            return this.Hash(partitionKey) + "::" + this.Hash(rowKey);
        }

        private string Hash(string str)
        {
            byte[] buffer = UTF8Encoding.UTF8.GetBytes(str);
            byte[] hash = this.sha256.ComputeHash(buffer);
            return Convert.ToBase64String(hash);
        }
    }
}
