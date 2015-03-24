namespace KeyValueStorage
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// This class provides an in-memory key value store implementation
    /// </summary>
    /// <typeparam name="T">The key value entity type</typeparam>
    public class KeyValueStore<T>
        where T : KeyValueEntity
    {
        /// <summary>
        /// The internal storage
        /// </summary>
        private Dictionary<string, string> entities;

        /// <summary>
        /// The SHA265 hashing provider
        /// </summary>
        private SHA256 sha256;

        /// <summary>
        /// The lock object used for concurrent operations
        /// </summary>
        private object padLock;

        /// <summary>
        /// Gets an entity
        /// </summary>
        /// <param name="partitionKey">The partition key</param>
        /// <param name="rowKey">The row key</param>
        /// <returns>The entity, or null if not found</returns>
        public Task<T> Get(string partitionKey, string rowKey)
        {
            return this.Get(partitionKey, rowKey, CancellationToken.None);
        }

        /// <summary>
        /// Gets an entity
        /// </summary>
        /// <param name="partitionKey">The partition key</param>
        /// <param name="rowKey">The row key</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The entity, or null if not found</returns>
        public virtual Task<T> Get(string partitionKey, string rowKey, CancellationToken cancellationToken)
        {
            string key = this.HashEntityKeys(partitionKey, rowKey);

            if (!this.entities.ContainsKey(key))
            {
                return Task.FromResult<T>(null);
            }

            lock (this.padLock)
            {
                if (!this.entities.ContainsKey(key))
                {
                    return Task.FromResult<T>(null);
                }

                return Task.FromResult(JsonConvert.DeserializeObject<T>(this.entities[key]));
            }
        }

        /// <summary>
        /// Inserts an entity to the key value store
        /// </summary>
        /// <param name="entity">The entity for insertion</param>
        /// <returns>The insertion Task</returns>
        /// <exception cref="DuplicateKeyException">Thrown when the partition key and row key already exist</exception>
        public Task Insert(T entity)
        {
            return this.Insert(entity, CancellationToken.None);
        }

        /// <summary>
        /// Inserts an entity to the key value store
        /// </summary>
        /// <param name="entity">The entity for insertion</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The insertion Task</returns>
        /// <exception cref="DuplicateKeyException">Thrown when the partition key and row key already exist</exception>
        public virtual Task Insert(T entity, CancellationToken cancellationToken)
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

            lock (this.padLock)
            {
                if (this.entities.ContainsKey(key))
                {
                    throw new DuplicateKeyException();
                }

                entity.ETag = Guid.NewGuid().ToString();
                entity.Timestamp = DateTime.UtcNow;

                this.entities.Add(key, JsonConvert.SerializeObject(entity));
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Updates an entity
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>The update task</returns>
        public Task Update(T entity)
        {
            return this.Update(entity, CancellationToken.None);
        }

        /// <summary>
        /// Updates an entity
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The update task</returns>
        public virtual Task Update(T entity, CancellationToken cancellationToken)
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

            lock (this.padLock)
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
                entity.Timestamp = DateTime.UtcNow;

                this.entities[key] = JsonConvert.SerializeObject(entity);
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>The delete task</returns>
        public Task Delete(T entity)
        {
            return this.Delete(entity, CancellationToken.None);
        }

        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The delete task</returns>
        public virtual Task Delete(T entity, CancellationToken cancellationToken)
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

            lock (this.padLock)
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
        /// Inserts or gets an entity from the store
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>The entity itself</returns>
        public virtual Task<T> InsertOrGet(T entity)
        {
            return this.InsertOrGet(entity, CancellationToken.None);
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
        /// Initializes the internal storage
        /// </summary>
        public void InitMemory()
        {
            this.entities = new Dictionary<string, string>();
            this.sha256 = SHA256CryptoServiceProvider.Create();
            this.padLock = new object();
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

        /// <summary>
        /// Hash the entity keys with the following scheme:
        /// SHA256(SHA256(partitionKey) + "_" + SHA256(rowKey))
        /// </summary>
        /// <param name="partitionKey">The partition key</param>
        /// <param name="rowKey">The row key</param>
        /// <returns>Hashed version of the keys</returns>
        private string HashEntityKeys(string partitionKey, string rowKey)
        {
            this.ThrowIfNullKeys(partitionKey, rowKey);

            byte[] buffer = UTF8Encoding.UTF8.GetBytes(partitionKey);
            byte[] hash = this.sha256.ComputeHash(buffer);
            string partitionKeyHex = Convert.ToBase64String(hash);

            buffer = UTF8Encoding.UTF8.GetBytes(rowKey);
            hash = this.sha256.ComputeHash(buffer);
            string rowKeyHex = Convert.ToBase64String(hash);

            buffer = UTF8Encoding.UTF8.GetBytes(partitionKeyHex + "_" + rowKeyHex);
            hash = this.sha256.ComputeHash(buffer);
            return Convert.ToBase64String(hash);
        }
    }
}
