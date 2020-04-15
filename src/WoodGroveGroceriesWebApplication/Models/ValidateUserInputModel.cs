namespace WoodGroveGroceriesWebApplication.Models
{
    using Newtonsoft.Json;

    public class ValidateUserInputModel
    {
        public int accountId { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}