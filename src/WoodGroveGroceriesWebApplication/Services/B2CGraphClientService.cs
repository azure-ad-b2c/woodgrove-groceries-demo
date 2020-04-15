namespace WoodGroveGroceriesWebApplication.Services
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Models;
    using Newtonsoft.Json;

    public class B2CGraphClientService
    {
        public readonly string aadGraphEndpoint = "https://graph.windows.net/";
        public readonly string aadGraphResourceId = "https://graph.windows.net/";
        public readonly string aadGraphVersion = "api-version=1.6";

        public readonly string aadInstance = "https://login.microsoftonline.com/";
        private readonly AuthenticationContext authContext;
        private readonly ClientCredential credential;

        public B2CGraphClientService(string tenant, string clientId, string clientSecret, string extensionAppId)
        {
            Tenant = tenant;
            ClientId = clientId;
            ClientSecret = clientSecret;
            ExtensionAppId = extensionAppId;


            // The AuthenticationContext is ADAL's primary class, in which you indicate the direcotry to use.
            authContext = new AuthenticationContext("https://login.microsoftonline.com/" + Tenant);

            // The ClientCredential is where you pass in your client_id and client_secret, which are 
            // provided to Azure AD in order to receive an access_token using the app's identity.
            credential = new ClientCredential(ClientId, ClientSecret);
        }

        public string Tenant { get; }
        public string ClientId { get; }
        public string ClientSecret { get; }

        public string ExtensionAppId { get; }

        /// <summary>
        ///     Create consumer user accounts
        ///     When creating user accounts in a B2C tenant, you can send an HTTP POST request to the /users endpoint
        /// </summary>
        public async Task CreateUser(string signInName, string password, string displayName, string givenName, string surname,
            bool generateRandomPassword)
        {
            if (string.IsNullOrEmpty(signInName))
            {
                throw new Exception("Email address is NULL or empty, you must provide valid email address");
            }

            if (string.IsNullOrEmpty(displayName) || displayName.Length < 1)
            {
                throw new Exception("Dispay name is NULL or empty, you must provide valid dislay name");
            }

            // Use random password for just-in-time migration flow
            if (generateRandomPassword)
            {
                password = GeneratePassword();
            }

            try
            {
                // Create Graph json string from object
                var graphUserModel = new GraphUserModel(signInName, password, displayName, givenName, surname);

                // Send the json to Graph API end point
                await SendGraphRequest("/users/", null, graphUserModel.ToString(), HttpMethod.Post);

                Console.WriteLine($"Azure AD user account '{signInName}' created");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ObjectConflict"))
                {
                    // TBD: Add you error Handling here
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"User with same emaill address '{signInName}' already exists in Azure AD");
                    Console.ResetColor();
                }
            }
        }

        /// <summary>
        ///     Search Azure AD user by signInNames property
        /// </summary>
        public async Task<string> SearcUserBySignInNames(string signInNames)
        {
            return await SendGraphRequest("/users/",
                $"$filter=signInNames/any(x:x/value eq '{signInNames}')",
                null, HttpMethod.Get);
        }


        /// <summary>
        ///     Search Azure AD user by displayName property
        /// </summary>
        public async Task<string> SearchUserByAccountNumber(string accountNumber)
        {
            return await SendGraphRequest("/users/",
                $"$filter=extension_{ExtensionAppId}_AccountNumber eq '{accountNumber}'",
                null, HttpMethod.Get);
        }

        /// <summary>
        ///     Search Azure AD user by displayName property
        /// </summary>
        public async Task<string> SearchUserByDisplayName(string displayName)
        {
            return await SendGraphRequest("/users/",
                $"$filter=displayName eq '{displayName}'",
                null, HttpMethod.Get);
        }

        /// <summary>
        ///     Update consumer user account's password
        /// </summary>
        /// <returns></returns>
        public async Task UpdateUserPassword(string signInName, string password)
        {
            var JSON = await SearcUserBySignInNames(signInName);

            var users = GraphUsersModel.Parse(JSON);

            // If user exists
            if (users != null && users.value != null && users.value.Count == 1)
            {
                // Generate JSON containing the password and password policy
                var graphPasswordModel = new GraphUserSetPasswordModel(password);
                var json = JsonConvert.SerializeObject(graphPasswordModel);

                // Send the request to Graph API
                await SendGraphRequest("/users/" + users.value[0].objectId, null, json, new HttpMethod("PATCH"));
            }
        }

        /// <summary>
        ///     Delete user anccounts from Azure AD by SignInName (email address)
        /// </summary>
        public async Task DeleteAADUserBySignInNames(string signInName)
        {
            // First step, get the user account ID
            var JSON = await SearcUserBySignInNames(signInName);

            var users = GraphUsersModel.Parse(JSON);

            // If the user account Id return successfully, iterate through all accounts
            if (users != null && users.value != null && users.value.Count > 0)
            {
                foreach (var item in users.value)
                {
                    // Send delete request to Graph API
                    await SendGraphRequest("/users/" + item.objectId, null, null, HttpMethod.Delete);
                }
            }
        }

        public async Task DeleteAADUserByObjectId(string objectId)
        {
            // Send delete request to Graph API
            await SendGraphRequest("/users/" + objectId, null, null, HttpMethod.Delete);
        }


        /// <summary>
        ///     Handle Graph user API, support following HTTP methods: GET, POST and PATCH
        /// </summary>
        private async Task<string> SendGraphRequest(string api, string query, string data, HttpMethod method)
        {
            // Get the access toke to Graph API
            var authResult = await AcquireAccessToken();
            var accessToken = authResult.AccessToken;

            // Set the Graph url. Including: Graph-endpoint/tenat/users?api-version&query
            var url = $"{aadGraphEndpoint}{Tenant}{api}?{aadGraphVersion}";

            if (!string.IsNullOrEmpty(query))
            {
                url += "&" + query;
            }

            //Trace.WriteLine($"Graph API call: {url}");
            using (var http = new HttpClient())
            using (var request = new HttpRequestMessage(method, url))
            {
                // Set the authorization header
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // For POST and PATCH set the request content 
                if (!string.IsNullOrEmpty(data))
                {
                    //Trace.WriteLine($"Graph API data: {data}");
                    request.Content = new StringContent(data, Encoding.UTF8, "application/json");
                }

                // Send the request to Graph API endpoint
                using (var response = await http.SendAsync(request))
                {
                    var error = await response.Content.ReadAsStringAsync();

                    // Check the result for error
                    if (!response.IsSuccessStatusCode)
                    {
                        // Throw server busy error message
                        if (response.StatusCode == (HttpStatusCode) 429)
                        {
                            // TBD: Add you error handling here
                        }

                        throw new Exception(error);
                    }

                    // Return the response body, usually in JSON format
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        private async Task<AuthenticationResult> AcquireAccessToken()
        {
            return await authContext.AcquireTokenAsync(aadGraphResourceId, credential);
        }

        /// <summary>
        ///     Generate temporary password
        /// </summary>
        private static string GeneratePassword()
        {
            const string A = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string a = "abcdefghijklmnopqrstuvwxyz";
            const string num = "1234567890";
            const string spe = "!@#$!&";

            var rv = GenerateLetters(4, A) + GenerateLetters(4, a) + GenerateLetters(4, num) + GenerateLetters(1, spe);
            return rv;
        }

        /// <summary>
        ///     Generate random letters from string of letters
        /// </summary>
        private static string GenerateLetters(int length, string baseString)
        {
            var res = new StringBuilder();
            var rnd = new Random();
            while (0 < length--)
            {
                res.Append(baseString[rnd.Next(baseString.Length)]);
            }

            return res.ToString();
        }
    }
}