namespace SabbathText.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class UpdateZipCodeOperation : BaseOperation<bool>
    {
        private static readonly Regex ZipMessageRegex = new Regex(@"^Zip(?:Code)?\s*(?<ZipCode>\d+)$", RegexOptions.IgnoreCase);

        private UpdateZipCodeOperationCheckpointData checkpointData;

        public UpdateZipCodeOperation(OperationContext context)
            : base(context, "UpdateZipCodeOperation.V1")
        {
        }

        public Task<OperationResponse<bool>> Run(Message incomingMessage)
        {
            this.checkpointData = new UpdateZipCodeOperationCheckpointData(this.Context.Account.AccountId);
            return this.TransitionToProcessMessage(incomingMessage);
        }
        
        protected override Task<OperationResponse<bool>> Resume(string serializedCheckpointData)
        {
            this.checkpointData = JsonConvert.DeserializeObject<UpdateZipCodeOperationCheckpointData>(serializedCheckpointData);
            throw new NotImplementedException();
        }

        private Task<OperationResponse<bool>> TransitionToProcessMessage(Message incomingMessage)
        {
            throw new NotImplementedException();
        }
    }
}
