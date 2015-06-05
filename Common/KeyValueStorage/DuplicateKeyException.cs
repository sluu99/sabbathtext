namespace KeyValueStorage
{
    using System;

    /// <summary>
    /// This exception should be thrown when there's a duplicate in key during insertion
    /// </summary>
    public class DuplicateKeyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateKeyException"/> class
        /// </summary>
        public DuplicateKeyException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateKeyException"/> class
        /// </summary>
        /// <param name="message">The error message</param>
        public DuplicateKeyException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateKeyException"/> class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">An optional inner exception</param>
        public DuplicateKeyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
