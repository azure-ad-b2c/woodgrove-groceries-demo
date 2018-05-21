using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WoodGroveAdministrationWebApplication.Entities;
using WoodGroveAdministrationWebApplication.IdentityModel.Clients.ActiveDirectory;

namespace WoodGroveAdministrationWebApplication.Repositories
{
    public class GraphUserRepository : IUserRepository
    {
        private const string GraphResource = "https://graph.windows.net";

        private readonly IAuthenticationContext _authenticationContext;
        private readonly ClientCredential _clientCredential;
        private readonly string _extensionId;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly string _tenantId;

        public GraphUserRepository(IAuthenticationContext authenticationContext, IOptions<ActiveDirectoryGraphOptions> optionsAccessor, HttpClient httpClient)
        {
            _authenticationContext = authenticationContext ?? throw new ArgumentNullException(nameof(authenticationContext));

            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            var options = optionsAccessor.Value;

            _tenantId = options.TenantId;
            _clientCredential = new ClientCredential(options.ClientId, options.ClientSecret);

            if (options.ExtensionId == null)
            {
                throw new ArgumentNullException(nameof(options.ExtensionId));
            }

            _extensionId = options.ExtensionId.Replace("-", string.Empty);

            _serializerSettings = new JsonSerializerSettings
            {
                Context = new StreamingContext(StreamingContextStates.Other, _extensionId)
            };

            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<User> GetAsync(string id)
        {
            var responseContentAsString = await SendGraphGetRequestAsync(
                AcquireTokenForApplicationAsync,
                $"users/{id}",
                "1.6",
                null);

            return JsonConvert.DeserializeObject<User>(responseContentAsString, _serializerSettings);
        }

        public async Task<IEnumerable<User>> ListAsync()
        {
            var responseContentAsString = await SendGraphGetRequestAsync(
                AcquireTokenForApplicationAsync,
                "users",
                "1.6",
                $"$filter=extension_{_extensionId}_CustomerType eq 'Work'");

            var responseContextAsJObject = JObject.Parse(responseContentAsString);

            if (responseContextAsJObject["value"] != null)
            {
                return JsonConvert.DeserializeObject<List<User>>(responseContextAsJObject["value"].ToString(), _serializerSettings);
            }

            return null;
        }

        public async Task UpdateAsync(User user)
        {
            await SendGraphPatchRequestAsync(
                AcquireTokenForApplicationAsync,
                $"users/{user.ObjectId}",
                "1.6",
                null,
                JsonConvert.SerializeObject(user, _serializerSettings));
        }

        private Task<AuthenticationResult> AcquireTokenForApplicationAsync()
        {
            return _authenticationContext.AcquireTokenAsync(GraphResource, _clientCredential);
        }

        private Task<string> SendGraphGetRequestAsync(
            Func<Task<AuthenticationResult>> acquireTokenAsync,
            string requestPath,
            string apiVersion,
            string requestQuery)
        {
            return SendGraphRequestAsync(
                acquireTokenAsync,
                requestUrl => new HttpRequestMessage(HttpMethod.Get, requestUrl),
                requestPath,
                apiVersion,
                requestQuery);
        }

        private Task<string> SendGraphPatchRequestAsync(
            Func<Task<AuthenticationResult>> acquireTokenAsync,
            string requestPath,
            string apiVersion,
            string requestQuery,
            string requestContentAsString)
        {
            return SendGraphRequestAsync(
                acquireTokenAsync,
                requestUrl =>
                {
                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUrl)
                    {
                        Content = new StringContent(requestContentAsString, Encoding.UTF8, "application/json")
                    };

                    return request;
                },
                requestPath,
                apiVersion,
                requestQuery);
        }

        private async Task<string> SendGraphRequestAsync(
            Func<Task<AuthenticationResult>> acquireTokenAsync,
            Func<string, HttpRequestMessage> createRequest,
            string requestPath,
            string apiVersion,
            string requestQuery)
        {
            var requestUrl = $"https://graph.windows.net/{_tenantId}/{requestPath}?api-version={apiVersion}";

            if (!string.IsNullOrEmpty(requestQuery))
            {
                requestUrl = requestUrl + $"&{requestQuery}";
            }

            var request = createRequest(requestUrl);
            var authenticationResult = await acquireTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            throw new WebException();
        }
    }
}
