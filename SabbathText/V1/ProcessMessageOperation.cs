namespace SabbathText.V1
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// This operation processes an incoming message
    /// </summary>
    public class ProcessMessageOperation : BaseOperation<bool>
    {
        /// <summary>
        /// Creates a new instance of this operation
        /// </summary>
        /// <param name="context">The operation context</param>
        public ProcessMessageOperation(OperationContext context)
            : base(context, "ProcessMessageOperation.V1")
        {
        }

        /// <summary>
        /// Resumes the operation
        /// </summary>
        /// <param name="serializedCheckpointData">The serialized checkpoint data</param>
        /// <returns>The operation result</returns>
        protected override Task<OperationResponse<bool>> Resume(string serializedCheckpointData)
        {
            throw new NotImplementedException();
        }
    }
}
