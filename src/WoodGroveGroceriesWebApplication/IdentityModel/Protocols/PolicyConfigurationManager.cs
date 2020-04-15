namespace WoodGroveGroceriesWebApplication.IdentityModel.Protocols
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;

    public class PolicyConfigurationManager : IConfigurationManager<OpenIdConnectConfiguration>
    {
        private readonly IDictionary<string, IConfigurationManager<OpenIdConnectConfiguration>> _configurationManagers = 
            new Dictionary<string, IConfigurationManager<OpenIdConnectConfiguration>>();

        public PolicyConfigurationManager(string authority, IEnumerable<string> policies)
        {
            if (string.IsNullOrEmpty(authority))
            {
                throw new ArgumentNullException(nameof(authority));
            }

            if (policies == null)
            {
                throw new ArgumentNullException(nameof(policies));
            }

            foreach (var policy in policies)
            {
                var metadataAddress = $"{authority}/{policy}/v2.0/.well-known/openid-configuration";
                var configurationRetriever = new OpenIdConnectConfigurationRetriever();
                var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(metadataAddress, configurationRetriever);

                if (!_configurationManagers.ContainsKey(policy.ToLowerInvariant()))
                {
                    _configurationManagers.Add(policy.ToLowerInvariant(), configurationManager);
                }
            }
        }

        public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancel)
        {
            OpenIdConnectConfiguration mergedConfiguration = null;

            foreach (var configurationManager in _configurationManagers)
            {
                var configuration = await configurationManager.Value.GetConfigurationAsync(cancel);

                if (mergedConfiguration != null)
                {
                    MergeConfiguration(mergedConfiguration, configuration);
                }
                else
                {
                    mergedConfiguration = CloneConfiguration(configuration);
                }
            }

            return mergedConfiguration;
        }

        public void RequestRefresh()
        {
            foreach (var configurationManager in _configurationManagers)
            {
                configurationManager.Value.RequestRefresh();
            }
        }

        public Task<OpenIdConnectConfiguration> GetConfigurationForPolicyAsync(string policy, CancellationToken cancel)
        {
            if (string.IsNullOrEmpty(policy))
            {
                throw new ArgumentNullException(nameof(policy));
            }

            policy = policy.ToLowerInvariant();

            if (!_configurationManagers.ContainsKey(policy))
            {
                throw new InvalidOperationException($"The configuration manager for policy '{policy}' was not found.");
            }

            var configurationManager = _configurationManagers[policy];
            return configurationManager.GetConfigurationAsync(cancel);
        }

        private static OpenIdConnectConfiguration CloneConfiguration(OpenIdConnectConfiguration configuration)
        {
            var keySet = configuration.JsonWebKeySet;
            configuration.JsonWebKeySet = null;
            var signingKeys = new List<SecurityKey>(configuration.SigningKeys);
            configuration.SigningKeys.Clear();
            var serializedConfiguration = OpenIdConnectConfiguration.Write(configuration);
            var clonedConfiguration = OpenIdConnectConfiguration.Create(serializedConfiguration);
            configuration.JsonWebKeySet = keySet;
            clonedConfiguration.JsonWebKeySet = keySet;

            foreach (var signingKey in signingKeys)
            {
                configuration.SigningKeys.Add(signingKey);
                clonedConfiguration.SigningKeys.Add(signingKey);
            }

            return clonedConfiguration;
        }

        private static void MergeConfiguration(OpenIdConnectConfiguration mergedConfiguration, OpenIdConnectConfiguration configurationToBeMerged)
        {
            foreach (var idTokenSigningAlgValueSupported in configurationToBeMerged.IdTokenSigningAlgValuesSupported)
            {
                if (!mergedConfiguration.IdTokenSigningAlgValuesSupported.Contains(idTokenSigningAlgValueSupported))
                {
                    mergedConfiguration.IdTokenSigningAlgValuesSupported.Add(idTokenSigningAlgValueSupported);
                }
            }

            foreach (var responseTypeSupported in configurationToBeMerged.ResponseTypesSupported)
            {
                if (!mergedConfiguration.ResponseTypesSupported.Contains(responseTypeSupported))
                {
                    mergedConfiguration.ResponseTypesSupported.Add(responseTypeSupported);
                }
            }

            foreach (var subjectTypeSupported in configurationToBeMerged.SubjectTypesSupported)
            {
                if (!mergedConfiguration.ResponseTypesSupported.Contains(subjectTypeSupported))
                {
                    mergedConfiguration.SubjectTypesSupported.Add(subjectTypeSupported);
                }
            }

            foreach (var signingKeyToBeMerged in configurationToBeMerged.SigningKeys)
            {
                if (mergedConfiguration.SigningKeys.All(mergedSigningKey => mergedSigningKey.KeyId != signingKeyToBeMerged.KeyId))
                {
                    mergedConfiguration.SigningKeys.Add(signingKeyToBeMerged);
                }
            }
        }
    }
}