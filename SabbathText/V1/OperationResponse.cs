namespace SabbathText.V1
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
        /// Gets or sets the error code
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the error description
        /// </summary>
        public string ErrorDescription { get; set; }

        /// <summary>
        /// Gets or sets the data returned from the operation
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Create a new OK response.
        /// </summary>
        /// <param name="responseData">The response data.</param>
        /// <returns>A response with OK status code.</returns>
        public static OperationResponse<T> CreateOK(T responseData)
        {
            return new OperationResponse<T>
            {
                Data = responseData,
                StatusCode = HttpStatusCode.OK,
            };
        }
    }
}
