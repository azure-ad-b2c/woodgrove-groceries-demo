namespace WoodGroveGroceriesWebApplication.Models
{
    public class EmailInvitationInputModel : CultureBase
    {
        public string FromAddress { get; set; }

        public string FromName { get; set; }

        public string ToAddress { get; set; }

        public string GroupName { get; set; }

        public string AccountNumber { get; set; }

        public string AppLogo { get; set; }

        public string AppBackground { get; set; }
    }
}