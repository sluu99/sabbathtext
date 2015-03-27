namespace SabbathText.Operations
{
    using System.Diagnostics.CodeAnalysis;
    using System.Net;

    /// <summary>
    /// This class represents an operation response
    /// </summary>
    /// <typeparam name="T">Type of the response data</typeparam>
    public class OperationResponse<T>
    {
        /// <summary>
        /// Gets or sets the operation status code
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the data returned from the operation
        /// </summary>
        public T Data { get; set; }
    }
}
