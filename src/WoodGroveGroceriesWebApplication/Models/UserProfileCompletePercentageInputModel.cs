namespace WoodGroveGroceriesWebApplication.Models
{
    public class UserProfileCompletePercentageInputModel
    {
        public string MfaType { get; set; }

        public bool? IsAllergensAdded { get; set; }

        public string ConsentToShareAllegyInfo { get; set; }

        public bool? IsSocialAccountLinked { get; set; }

        public string IdentityProvider { get; set; }
    }
}