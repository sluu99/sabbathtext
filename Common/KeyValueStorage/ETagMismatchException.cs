namespace KeyValueStorage
{
    using System;

    /// <summary>
    /// This exception will be thrown when the e-tag of the entity does not match what's in the storage
    /// </summary>
    public class ETagMismatchException : Exception
    {
         /// <summary>
        /// Initializes a new instance of the <see cref="ETagMismatchException"/> class
        /// </summary>
        public ETagMismatchException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ETagMismatchException"/> class
        /// </summary>
        /// <param name="message">The error message</param>
        public ETagMismatchException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ETagMismatchException"/> class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">An optional inner exception</param>
        public ETagMismatchException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
