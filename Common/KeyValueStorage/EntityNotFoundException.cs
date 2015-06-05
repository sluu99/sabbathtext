namespace KeyValueStorage
{
    using System;

    /// <summary>
    /// This exception should be thrown trying to update or delete an entity that does not exist
    /// </summary>
    public class EntityNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class
        /// </summary>
        public EntityNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class
        /// </summary>
        /// <param name="message">The error message</param>
        public EntityNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">An optional inner exception</param>
        public EntityNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
