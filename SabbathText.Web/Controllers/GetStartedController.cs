namespace SabbathText.Web.Controllers
{
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using SabbathText.V1;
    using SabbathText.Web.Models;

    /// <summary>
    /// The controller for the get started page
    /// </summary>
    public class GetStartedController : BaseController
    {
        /// <summary>
        /// The index action
        /// </summary>
        /// <param name="model">The get started model</param>
        /// <returns>An action result.</returns>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(GetStartedModel model)
        {
            if (!ModelState.IsValid)
            {
                return this.View("Invalid", model);
            }

            string phoneNumber = model.PhoneNumber.ExtractUSPhoneNumber();

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return this.View("Invalid", model);
            }

            model.PhoneNumber = phoneNumber;

            GreetUserOperation greetUserOp = new GreetUserOperation(await this.CreateContext(model.PhoneNumber));
            OperationResponse<bool> response = await greetUserOp.Run();

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return this.View("TextUs", model);
            }

            return this.View("Success", model);            
        }
    }
}