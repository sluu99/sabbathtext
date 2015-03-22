namespace SabbathText.Operations
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Operation status code, pretty much mirrors the HTTP status codes
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "Common HTTP status codes")]
    public enum OperationStatusCode
    {
        Ok = 200,
        Created = 201,
        Accepted = 202,
        BadRequest = 400,
        Conflict = 409,
    }

    /// <summary>
    /// This class represents an operation response
    /// </summary>
    /// <typeparam name="T">Type of the response data</typeparam>
    public class OperationResponse<T>
    {
        /// <summary>
        /// Gets or sets the operation status code
        /// </summary>
        public OperationStatusCode StatusCode { get; set; }

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
