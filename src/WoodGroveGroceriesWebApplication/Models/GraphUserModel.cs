namespace WoodGroveGroceriesWebApplication.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class GraphUserModel
    {
        public GraphUserModel()
        {
        }

        public GraphUserModel(string signInName, string password, string displayName, string givenName, string surname)
        {
            accountEnabled = true;

            signInNames = new List<SignInNames>();
            signInNames.Add(new SignInNames(signInName));

            // always set to 'LocalAccount
            creationType = "LocalAccount";

            this.displayName = displayName;
            this.givenName = givenName;
            this.surname = surname;

            passwordProfile = new PasswordProfile(password);

            passwordPolicies = "DisablePasswordExpiration,DisableStrongPassword";
        }

        public string objectId { get; set; }
        public bool accountEnabled { get; set; }
        public IList<SignInNames> signInNames { get; set; }
        public string creationType { get; set; }
        public string displayName { get; set; }
        public string givenName { get; set; }
        public string surname { get; set; }
        public PasswordProfile passwordProfile { get; set; }
        public string passwordPolicies { get; set; }

        public string[] otherMails { get; set; }

        public string phoneNumber { get; set; }

        /// <summary>
        ///     Serialize the object into Json string
        /// </summary>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static GraphUserModel Parse(string JSON)
        {
            return JsonConvert.DeserializeObject(JSON, typeof(GraphUserModel)) as GraphUserModel;
        }
    }

    public class PasswordProfile
    {
        public PasswordProfile(string password)
        {
            this.password = password;

            // always set to false
            forceChangePasswordNextLogin = false;
        }

        public string password { get; set; }
        public bool forceChangePasswordNextLogin { get; set; }
    }

    public class SignInNames
    {
        public SignInNames(string email)
        {
            // Type must be 'emailAddress' (or 'userName')
            type = "emailAddress";

            // The user email address
            value = email;
        }

        public string type { get; set; }
        public string value { get; set; }
    }

    public class GraphUserSetPasswordModel
    {
        public GraphUserSetPasswordModel(string password)
        {
            passwordProfile = new PasswordProfile(password);
            passwordPolicies = "DisablePasswordExpiration,DisableStrongPassword";
        }

        public PasswordProfile passwordProfile { get; }
        public string passwordPolicies { get; }
    }
}