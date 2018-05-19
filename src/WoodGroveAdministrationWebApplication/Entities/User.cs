using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WoodGroveAdministrationWebApplication.Entities
{
    public class User
    {
        [JsonExtensionData]
        private IDictionary<string, JToken> _extensionData;

        [JsonIgnore]
        public string BusinessCustomerRole { get; set; }

        public string DisplayName { get; set; }

        public string JobTitle { get; set; }

        public string ObjectId { get; set; }

        [JsonIgnore]
        public string OrganizationDisplayName { get; set; }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            var extensionId = context.Context;
            BusinessCustomerRole = (string) _extensionData[$"extension_{extensionId}_BusinessCustomerRole"];
            OrganizationDisplayName = (string) _extensionData[$"extension_{extensionId}_OrganizationDisplayName"];
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            var extensionId = context.Context;
            _extensionData[$"extension_{extensionId}_BusinessCustomerRole"] = BusinessCustomerRole;
            _extensionData[$"extension_{extensionId}_OrganizationDisplayName"] = OrganizationDisplayName;
        }
    }
}
