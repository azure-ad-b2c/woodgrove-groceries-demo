namespace WoodGroveGroceriesWebApplication.Models
{
    public class SendEmailCodeInputModel : CultureBase
    {
        public string FromAddress { get; set; }

        public string ToAddress { get; set; }

        public string EmailCode { get; set; }
    }
}