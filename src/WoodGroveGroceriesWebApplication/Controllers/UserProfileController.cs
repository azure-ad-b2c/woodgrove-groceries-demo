namespace WoodGroveGroceriesWebApplication.Controllers
{
    using System.IO;
    using System.Net;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Newtonsoft.Json;

    [Produces("application/json")]
    public class UserProfileController : Controller
    {
        [HttpPost]
        [Route("api/UserProfileCalcluateProfileCompletePercentage")]
        public IActionResult CalculateUserProfileCompletePercentage()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var body = reader.ReadToEnd();


                if (body == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest,
                        new UserProfilePercentageB2CResponse("Request content is NULL", HttpStatusCode.BadRequest));
                }

                // Check input content value
                if (string.IsNullOrEmpty(body))
                {
                    return StatusCode(StatusCodes.Status400BadRequest,
                        new UserProfilePercentageB2CResponse("Request content is empty", HttpStatusCode.BadRequest));
                }

                // Convert the input string into InputClaimsModel object
                var inputClaims =
                    JsonConvert.DeserializeObject(body, typeof(UserProfileCompletePercentageInputModel)) as UserProfileCompletePercentageInputModel;

                if (inputClaims == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest,
                        new UserProfilePercentageB2CResponse("Can not deserialize input claims", HttpStatusCode.BadRequest));
                }

                return Ok(new UserProfilePercentageB2CResponse("sucess", HttpStatusCode.OK)
                {
                    UserProfileCompletePercentage = GetUserProfilePercentage(inputClaims)
                });
            }
        }

        public int GetUserProfilePercentage(UserProfileCompletePercentageInputModel input)
        {
            var percentage = 0;

            if (string.IsNullOrEmpty(input.IdentityProvider))
            {
                if (!string.IsNullOrEmpty(input.MfaType))
                {
                    percentage += 30;
                }

                if (input.IsAllergensAdded ?? false)
                {
                    percentage += 30;
                }

                if (!string.IsNullOrEmpty(input.ConsentToShareAllegyInfo))
                {
                    percentage += 20;
                }

                if (input.IsSocialAccountLinked ?? false)
                {
                    percentage += 20;
                }
            }

            else
            {
                if (!string.IsNullOrEmpty(input.MfaType))
                {
                    percentage += 50;
                }

                if (input.IsAllergensAdded ?? false)
                {
                    percentage += 30;
                }

                if (!string.IsNullOrEmpty(input.ConsentToShareAllegyInfo))
                {
                    percentage += 20;
                }
            }

            return percentage;
        }
    }
}