$ErrorActionPreference = "Stop";

function New-B2CExtensionProperty {
    Param (
        [string] $tenantId,
        [string] $applicationId,
        [string] $accessToken,
        [string] $extensionPropertyName,
        [string] $extensionPropertyDataType
    )

    "Creating $extensionPropertyName extension property...";
    $createExtensionPropertyRequestBody = "{`"name`":`"$extensionPropertyName`",`"dataType`":`"$extensionPropertyDataType`",`"targetObjects`":[`"User`"]}";
    $createExtensionPropertyResponse = Invoke-WebRequest "https://graph.windows.net/$tenantId/applications/$applicationId/extensionProperties/?api-version=1.6" -Method Post -Headers @{Authorization="Bearer $accessToken";"Content-Type"="application/json"} -Body $createExtensionPropertyRequestBody;
}

#---------- AUTHENTICATION ----------

#---------- Load Azure Active Directory Authentication Library (ADAL) assemblies ----------

$adalPath = "$PSScriptRoot\Microsoft.IdentityModel.Clients.ActiveDirectory.dll";

if (!(Test-Path $adalPath)) {
    "The assembly file $adal could not be found.";
    return;
}

[System.Reflection.Assembly]::LoadFile($adalPath);

$adalPlatformPath = "$PSScriptRoot\Microsoft.IdentityModel.Clients.ActiveDirectory.Platform.dll";

if (!(Test-Path $adalPlatformPath)) {
    "The assembly file $adalPlatformPath could not be found.";
    return;
}

[System.Reflection.Assembly]::LoadFile($adalPlatformPath);

#---------- Acquire access token for use with Azure Active Directory Graph API resource ----------

$authority = "https://login.microsoftonline.com/common";
$authenticationContext = New-Object Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext($authority);
$resource = "https://graph.windows.net";
$clientId = "1950a258-227b-4e31-a9cf-717495945fc2";
$redirectUri = "urn:ietf:wg:oauth:2.0:oob";
$promptBehavior = [Microsoft.IdentityModel.Clients.ActiveDirectory.PromptBehavior]::Always;
$platformParameters = New-Object Microsoft.IdentityModel.Clients.ActiveDirectory.PlatformParameters($promptBehavior);
$authenticationResult = $authenticationContext.AcquireTokenAsync($resource, $clientId, $redirectUri, $platformParameters).Result;
$tenantId = $authenticationResult.TenantId;
$accessToken = $authenticationResult.AccessToken;

#---------- TENANT ----------

#---------- Get details of Azure Active Directory B2C tenant ----------

"Getting tenant details...";
$getTenantDetailsResponse = Invoke-WebRequest "https://graph.windows.net/$tenantId/tenantDetails/?api-version=1.6" -Method Get -Headers @{Authorization="Bearer $accessToken"};
$tenantDetails = (ConvertFrom-Json $getTenantDetailsResponse.Content).value;
$tenantName = $tenantDetails.VerifiedDomains | Where-Object Initial -eq "True" | Select-Object -First 1 -ExpandProperty name;
"Tenant name: $tenantName";

#---------- Get details of Azure Active Directory resource ----------

$directoryApplicationClientId = "00000002-0000-0000-c000-000000000000";
$directoryReadWriteRoleName = "Directory.ReadWrite.All";
$userReadPermissionName = "User.Read";

"Getting Azure Active Directory service principal details...";
$getDirectoryServicePrincipalResponse = Invoke-WebRequest "https://graph.windows.net/$tenantId/servicePrincipals?`$filter=appId+eq+'$directoryApplicationClientId'&api-version=1.6" -Method Get -Headers @{Authorization="Bearer $accessToken"};
$directoryServicePrincipal = (ConvertFrom-Json $getDirectoryServicePrincipalResponse.Content).value[0];
$directoryServicePrincipalId = $directoryServicePrincipal.objectId;
$directoryServicePrincipalDirectoryReadWriteRoleId = $directoryServicePrincipal.appRoles | Where-Object value -eq "$directoryReadWriteRoleName" | Select-Object -First 1 -ExpandProperty id;
$directoryServicePrincipalUserReadPermissionId = $directoryServicePrincipal.oauth2Permissions | Where-Object value -eq "$userReadPermissionName" | Select-Object -First 1 -ExpandProperty id;

#---------- APPLICATIONS ----------

#---------- Create Identity Experience Framework Authentication application and service principal ----------

"Creating Identity Experience Framework Authentication application...";
$createIefAuthenticationApplicationRequestBody = "{`"appRoles`":[],`"availableToOtherTenants`":false,`"displayName`":`"IdentityExperienceFramework. Do not modify. Used by IEF for authentication.`",`"errorUrl`":null,`"groupMembershipClaims`":null,`"homepage`":`"https://login.microsoftonline.com/$tenantName`",`"identifierUris`":[`"https://$tenantName/$(New-Guid)`"],`"keyCredentials`":[],`"knownClientApplications`":[],`"logoutUrl`":null,`"oauth2AllowImplicitFlow`":false,`"oauth2AllowUrlPathMatching`":false,`"oauth2Permissions`":[],`"oauth2RequirePostResponse`":false,`"passwordCredentials`":[],`"publicClient`":false,`"recordConsentConditions`":null,`"replyUrls`":[`"https://login.microsoftonline.com/$tenantName`"],`"requiredResourceAccess`":[{`"resourceAppId`":`"$directoryApplicationClientId`",`"resourceAccess`":[{`"id`":`"$directoryServicePrincipalUserReadPermissionId`",`"type`":`"Scope`"}]}],`"samlMetadataUrl`":null}";
$createIefAuthenticationApplicationResponse = Invoke-WebRequest "https://graph.windows.net/$tenantId/applications/?api-version=1.6" -Method Post -Headers @{Authorization="Bearer $accessToken";"Content-Type"="application/json"} -Body $createIefAuthenticationApplicationRequestBody;
$iefAuthenticationApplication = ConvertFrom-Json $createIefAuthenticationApplicationResponse.Content;
$iefAuthenticationApplicationClientId = $iefAuthenticationApplication.appId;
$iefAuthenticationApplicationUserImpersonationPermissionId = $iefAuthenticationApplication.oauth2Permissions[0].id;
"Application Client ID: $iefAuthenticationApplicationClientId";

"Creating Identity Experience Framework Authentication service principal...";
$createIefAuthenticationServicePrincipalRequestBody = "{`"appId`":`"$iefAuthenticationApplicationClientId`"}";
$createIefAuthenticationServicePrincipalResponse = Invoke-WebRequest "https://graph.windows.net/$tenantId/servicePrincipals/?api-version=1.6" -Method Post -Headers @{Authorization="Bearer $accessToken";"Content-Type"="application/json"} -Body $createIefAuthenticationServicePrincipalRequestBody;
$iefAuthenticationServicePrincipal = ConvertFrom-Json $createIefAuthenticationServicePrincipalResponse.Content;
$iefAuthenticationServicePrincipalId = $iefAuthenticationServicePrincipal.objectId;

#---------- Create Identity Experience Framework Authentication Proxy application and service principal ----------

"Creating Identity Experience Framework Authentication Proxy application...";
$createIefAuthenticationProxyApplicationRequestBody = "{`"appRoles`":[],`"availableToOtherTenants`":false,`"displayName`":`"ProxyIdentityExperienceFramework. Do not modify. Used by IEF for authentication.`",`"errorUrl`":null,`"groupMembershipClaims`":null,`"homepage`":null,`"identifierUris`":[],`"keyCredentials`":[],`"knownClientApplications`":[],`"logoutUrl`":null,`"oauth2AllowImplicitFlow`":false,`"oauth2AllowUrlPathMatching`":false,`"oauth2Permissions`":[],`"oauth2RequirePostResponse`":false,`"passwordCredentials`":[],`"publicClient`":true,`"recordConsentConditions`":null,`"replyUrls`":[`"https://login.microsoftonline.com/$tenantName`"],`"requiredResourceAccess`":[{`"resourceAppId`":`"$iefAuthenticationApplicationClientId`",`"resourceAccess`":[{`"id`":`"$iefAuthenticationApplicationUserImpersonationPermissionId`",`"type`":`"Scope`"}]},{`"resourceAppId`":`"$directoryApplicationClientId`",`"resourceAccess`":[{`"id`":`"$directoryServicePrincipalUserReadPermissionId`",`"type`":`"Scope`"}]}],`"samlMetadataUrl`":null}";
$createIefAuthenticationProxyApplicationResponse = Invoke-WebRequest "https://graph.windows.net/$tenantId/applications/?api-version=1.6" -Method Post -Headers @{Authorization="Bearer $accessToken";"Content-Type"="application/json"} -Body $createIefAuthenticationProxyApplicationRequestBody;
$iefAuthenticationProxyApplication = ConvertFrom-Json $createIefAuthenticationProxyApplicationResponse.Content;
$iefAuthenticationProxyApplicationClientId = $iefAuthenticationProxyApplication.appId;
"Application Client ID: $iefAuthenticationProxyApplicationClientId";

"Creating Identity Experience Framework Authentication Proxy service principal...";
$createIefAuthenticationProxyServicePrincipalRequestBody = "{`"appId`":`"$iefAuthenticationProxyApplicationClientId`"}";
$createIefAuthenticationProxyServicePrincipalResponse = Invoke-WebRequest "https://graph.windows.net/$tenantId/servicePrincipals/?api-version=1.6" -Method Post -Headers @{Authorization="Bearer $accessToken";"Content-Type"="application/json"} -Body $createIefAuthenticationProxyServicePrincipalRequestBody;
$iefAuthenticationProxyServicePrincipal = ConvertFrom-Json $createIefAuthenticationProxyServicePrincipalResponse.Content;
$iefAuthenticationProxyServicePrincipalId = $iefAuthenticationProxyServicePrincipal.objectId;

#---------- Create Identity Experience Framework Extensions application and service principal ----------

"Creating Identity Experience Framework Extensions application...";
$createIefGraphApplicationRequestBody = "{`"appRoles`":[],`"availableToOtherTenants`":false,`"displayName`":`"ief-extensions-app. Do not modify. Used by IEF for management of users.`",`"errorUrl`":null,`"groupMembershipClaims`":null,`"homepage`":`"https://login.microsoftonline.com/$tenantName`",`"identifierUris`":[`"https://$tenantName/$(New-Guid)`"],`"keyCredentials`":[],`"knownClientApplications`":[],`"logoutUrl`":null,`"oauth2AllowImplicitFlow`":false,`"oauth2AllowUrlPathMatching`":false,`"oauth2Permissions`":[],`"oauth2RequirePostResponse`":false,`"passwordCredentials`":[],`"publicClient`":false,`"recordConsentConditions`":null,`"replyUrls`":[`"https://login.microsoftonline.com/$tenantName`"],`"requiredResourceAccess`":[{`"resourceAppId`":`"$directoryApplicationClientId`",`"resourceAccess`":[{`"id`":`"$directoryServicePrincipalDirectoryReadWriteRoleId`",`"type`":`"Role`"},{`"id`":`"$directoryServicePrincipalUserReadPermissionId`",`"type`":`"Scope`"}]}],`"samlMetadataUrl`":null}";
$createIefGraphApplicationResponse = Invoke-WebRequest "https://graph.windows.net/$tenantId/applications/?api-version=1.6" -Method Post -Headers @{Authorization="Bearer $accessToken";"Content-Type"="application/json"} -Body $createIefGraphApplicationRequestBody;
$iefGraphApplication = ConvertFrom-Json $createIefGraphApplicationResponse.Content;
$iefGraphApplicationId = $iefGraphApplication.objectId;
$iefGraphApplicationClientId = $iefGraphApplication.appId;
"Application ID: $iefGraphApplicationId";
"Application Client ID: $iefGraphApplicationClientId";

"Creating Identity Experience Framework Extensions service principal...";
$createIefGraphServicePrincipalRequestBody = "{`"appId`":`"$iefGraphApplicationClientId`"}";
$createIefGraphServicePrincipalResponse = Invoke-WebRequest "https://graph.windows.net/$tenantId/servicePrincipals/?api-version=1.6" -Method Post -Headers @{Authorization="Bearer $accessToken";"Content-Type"="application/json"} -Body $createIefGraphServicePrincipalRequestBody;
$iefGraphServicePrincipal = ConvertFrom-Json $createIefGraphServicePrincipalResponse.Content;
$iefGraphServicePrincipalId = $iefGraphServicePrincipal.objectId;

#---------- AUTHORIZATION ----------

$expiryTime = (Get-Date).ToString("yyyy-MM-dd");

#---------- Grant permission by Identity Experience Framework Authentication client to Azure Active Directory resource ----------

"Granting permission by Identity Experience Framework Authentication client to Azure Active Directory resource...";
$createPermissionGrantFromIefAuthenticationToDirectoryRequestBody = "{`"clientId`":`"$iefAuthenticationServicePrincipalId`",`"consentType`":`"AllPrincipals`",`"resourceId`":`"$directoryServicePrincipalId`",`"scope`":`"$userReadPermissionName`", `"expiryTime`":`"$expiryTime`"}";
$createPermissionGrantFromIefAuthenticationToDirectoryResponse = Invoke-WebRequest "https://graph.windows.net/$tenantId/oauth2PermissionGrants/?api-version=1.6" -Method Post -Headers @{Authorization="Bearer $accessToken";"Content-Type"="application/json"} -Body $createPermissionGrantFromIefAuthenticationToDirectoryRequestBody;

#---------- Grant permission by Identity Experience Framework Authentication Proxy client to Azure Active Directory resource ----------

"Granting permission by Identity Experience Framework Authentication Proxy client to Azure Active Directory resource...";
$createPermissionGrantFromIefAuthenticationProxyToDirectoryRequestBody = "{`"clientId`":`"$iefAuthenticationProxyServicePrincipalId`",`"consentType`":`"AllPrincipals`",`"resourceId`":`"$directoryServicePrincipalId`",`"scope`":`"$userReadPermissionName`", `"expiryTime`":`"$expiryTime`"}";
$createPermissionGrantFromIefAuthenticationProxyToDirectoryResponse = Invoke-WebRequest "https://graph.windows.net/$tenantId/oauth2PermissionGrants/?api-version=1.6" -Method Post -Headers @{Authorization="Bearer $accessToken";"Content-Type"="application/json"} -Body $createPermissionGrantFromIefAuthenticationProxyToDirectoryRequestBody;

#---------- Grant permission by Identity Experience Framework Authentication Proxy client to Identity Experience Framework Authentication resource ----------

"Granting permission by Identity Experience Framework Authentication Proxy client to Identity Experience Framework Authentication resource...";
$createPermissionGrantFromIefAuthenticationProxyToIefAuthenticationRequestBody = "{`"clientId`":`"$iefAuthenticationProxyServicePrincipalId`",`"consentType`":`"AllPrincipals`",`"resourceId`":`"$iefAuthenticationServicePrincipalId`",`"scope`":`"user_impersonation`", `"expiryTime`":`"$expiryTime`"}";
$createPermissionGrantFromIefAuthenticationProxyToIefAuthenticationResponse = Invoke-WebRequest "https://graph.windows.net/$tenantId/oauth2PermissionGrants/?api-version=1.6" -Method Post -Headers @{Authorization="Bearer $accessToken";"Content-Type"="application/json"} -Body $createPermissionGrantFromIefAuthenticationProxyToIefAuthenticationRequestBody;

#---------- Grant permission by Identity Experience Framework Extensions client to Azure Active Directory resource ----------

"Granting permission by Identity Experience Framework Extensions client to Azure Active Directory resource...";
$createAppRoleAssignmentFromIefGraphToDirectoryRequestBody = "{`"id`":`"$directoryServicePrincipalDirectoryReadWriteRoleId`",`"principalId`":`"$iefGraphServicePrincipalId`",`"principalType`":`"ServicePrincipal`",`"resourceId`":`"$directoryServicePrincipalId`"}";
$createAppRoleAssignmentFromIefGraphToDirectoryResponse = Invoke-WebRequest "https://graph.windows.net/$tenantId/servicePrincipals/$iefGraphServicePrincipalId/appRoleAssignments/?api-version=1.6" -Method Post -Headers @{Authorization="Bearer $accessToken";"Content-Type"="application/json"} -Body $createAppRoleAssignmentFromIefGraphToDirectoryRequestBody;
$createPermissionGrantFromIefGraphToDirectoryRequestBody = "{`"clientId`":`"$iefGraphServicePrincipalId`",`"consentType`":`"AllPrincipals`",`"expiryTime`":`"$expiryTime`",`"resourceId`":`"$directoryServicePrincipalId`",`"scope`":`"$userReadPermissionName`"}";
$createPermissionGrantFromIefGraphToDirectoryResponse = Invoke-WebRequest "https://graph.windows.net/$tenantId/oauth2PermissionGrants/?api-version=1.6" -Method Post -Headers @{Authorization="Bearer $accessToken";"Content-Type"="application/json"} -Body $createPermissionGrantFromIefGraphToDirectoryRequestBody;

# #---------- EXTENSION PROPERTIES ----------

"Creating extension properties...";
New-B2CExtensionProperty -tenantId $tenantId -accessToken $accessToken -applicationId $iefGraphApplicationId -extensionPropertyName "AgreedToTermsOfService" -extensionPropertyDataType "String";
New-B2CExtensionProperty -tenantId $tenantId -accessToken $accessToken -applicationId $iefGraphApplicationId -extensionPropertyName "BusinessCustomerRole" -extensionPropertyDataType "String";
New-B2CExtensionProperty -tenantId $tenantId -accessToken $accessToken -applicationId $iefGraphApplicationId -extensionPropertyName "CustomerType" -extensionPropertyDataType "String";
New-B2CExtensionProperty -tenantId $tenantId -accessToken $accessToken -applicationId $iefGraphApplicationId -extensionPropertyName "OrganizationDisplayName" -extensionPropertyDataType "String";
New-B2CExtensionProperty -tenantId $tenantId -accessToken $accessToken -applicationId $iefGraphApplicationId -extensionPropertyName "TermsOfServiceConsentDateTime" -extensionPropertyDataType "DateTime";
