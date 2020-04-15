namespace WoodGroveGroceriesWebApplication.Models
{
    using System.Net;

    public class UserProfilePercentageB2CResponse : B2CResponseContent
    {
        public UserProfilePercentageB2CResponse(string message, HttpStatusCode status) : base(message, status)
        {
        }

        public int UserProfileCompletePercentage { get; set; }
    }
}