namespace QueueStorage
{
    using System;

    /// <summary>
    /// This exception is thrown when deleting a non-existing message, or deleting a message checked out by others
    /// </summary>
    public class DeleteMessageException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteMessageException"/> class
        /// </summary>
        public DeleteMessageException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteMessageException"/> class
        /// </summary>
        /// <param name="message">The error message</param>
        public DeleteMessageException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteMessageException"/> class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">An optional inner exception</param>
        public DeleteMessageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
