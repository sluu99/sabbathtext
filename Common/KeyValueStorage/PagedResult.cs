namespace KeyValueStorage
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a page of result
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    public class PagedResult<TEntity>
        where TEntity : KeyValueEntity
    {
        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="pageSize">The expected page size.</param>
        internal PagedResult(int pageSize)
        {
            this.Entities = new List<TEntity>(pageSize);
        }

        /// <summary>
        /// Gets the entities for this page
        /// </summary>
        public List<TEntity> Entities { get; private set; }

        /// <summary>
        /// Gets the continuation token
        /// </summary>
        public string ContinuationToken { get; internal set; }
    }
}
