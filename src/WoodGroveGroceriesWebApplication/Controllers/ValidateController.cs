namespace WoodGroveGroceriesWebApplication.Controllers
{
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Newtonsoft.Json;

    [Produces("application/json")]
    public class ValidateController : Controller
    {
        [HttpPost]
        [Route("api/ValidateUserAccountId")]
        public IActionResult ValidateUserAccountId()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var body = reader.ReadToEnd();


                if (body == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new B2CResponseContent("Request content is NULL", HttpStatusCode.BadRequest));
                }

                // Check input content value
                if (string.IsNullOrEmpty(body))
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new B2CResponseContent("Request content is empty", HttpStatusCode.BadRequest));
                }

                // Convert the input string into InputClaimsModel object
                var inputClaims = JsonConvert.DeserializeObject(body, typeof(ValidateUserInputModel)) as ValidateUserInputModel;

                if (inputClaims == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest,
                        new B2CResponseContent("Can not deserialize input claims", HttpStatusCode.BadRequest));
                }

                if (!Regex.IsMatch(inputClaims.accountId.ToString(), "^([0-9]){5}5$"))
                {
                    return StatusCode(StatusCodes.Status409Conflict,
                        new B2CResponseContent("The account number should end with 5", HttpStatusCode.Conflict));
                }

                return Ok();
            }
        }
    }
}