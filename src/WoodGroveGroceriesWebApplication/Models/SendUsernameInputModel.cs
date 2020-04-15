namespace WoodGroveGroceriesWebApplication.Models
{
    public class SendUsernameInputModel : CultureBase
    {
        public string FromAddress { get; set; }

        public string AccountNumber { get; set; }

        public string PhoneNumber { get; set; }
    }
}