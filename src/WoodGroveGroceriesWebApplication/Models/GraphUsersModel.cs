namespace WoodGroveGroceriesWebApplication.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class GraphUsersModel
    {
        public string odatametadata { get; set; }
        public List<GraphUserModel> value { get; set; }

        public static GraphUsersModel Parse(string JSON)
        {
            return JsonConvert.DeserializeObject(JSON.Replace("odata.metadata", "odatametadata"), typeof(GraphUsersModel)) as GraphUsersModel;
        }
    }
}