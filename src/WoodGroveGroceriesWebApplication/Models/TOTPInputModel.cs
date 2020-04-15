namespace WoodGroveGroceriesWebApplication.Models
{
    using Newtonsoft.Json;

    public class TOTPInputModel
    {
        public string userName { get; set; }
        public string secretKey { get; set; }
        public string totpCode { get; set; }
        public string timeStepMatched { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static TOTPInputModel Parse(string JSON)
        {
            return JsonConvert.DeserializeObject(JSON, typeof(TOTPInputModel)) as TOTPInputModel;
        }
    }
}