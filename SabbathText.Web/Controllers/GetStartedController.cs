using SabbathText.Web.Models;
using System.Threading.Tasks;
using System.Web.Mvc;
using SabbathText.V1;
using System.Net;

namespace SabbathText.Web.Controllers
{
    public class GetStartedController : BaseController
    {
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