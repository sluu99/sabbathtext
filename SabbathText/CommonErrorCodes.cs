namespace SabbathText
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Common error codes
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "The field names should be self explanatory")]
    public static class CommonErrorCodes
    {
        public const string InvalidInput = "InvalidInput";
        public const string OperationInProgress = "OperationInProgress";
        public const string UnrecognizedIncomingMessage = "UnrecognizedIncomingMessage";
    }
}
