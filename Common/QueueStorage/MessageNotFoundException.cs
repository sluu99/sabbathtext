namespace QueueStorage
{
    using System;

    /// <summary>
    /// This exception is thrown when deleting a non-existing message, or deleting a message checked out by others
    /// </summary>
    public class MessageNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNotFoundException"/> class
        /// </summary>
        public MessageNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNotFoundException"/> class
        /// </summary>
        /// <param name="message">The error message</param>
        public MessageNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNotFoundException"/> class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">An optional inner exception</param>
        public MessageNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
