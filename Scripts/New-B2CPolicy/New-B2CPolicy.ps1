Param (
    [string] $TenantName,
    [string] $PolicyDirectory
)

$ErrorActionPreference = "Stop";

function New-B2CPolicy {
    Param (
        [string] $tenantId,
        [string] $accessToken,
        [string] $policyDirectory,
        [string] $policyId
    )

    "Uploading $PolicyId policy...";
    $policyFile = "$($PolicyDirectory)\$($PolicyId).xml";
    $policyFileContent = Get-Content -Path $policyFile;
    $uploadPolicyRequestBody = "<string xmlns=`"http://schemas.microsoft.com/2003/10/Serialization/`">" + [System.Security.SecurityElement]::Escape($policyFileContent) + "</string>";
    $uploadPolicyResponse = Invoke-WebRequest "https://main.b2cadmin.ext.azure.com/api/trustFramework?tenantId=$tenantId&overwriteIfExists=true" -Method Post -Headers @{Authorization = "Bearer $accessToken"; "Content-Type" = "application/xml"} -Body $uploadPolicyRequestBody;
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

#---------- Acquire access token for use with Azure REST API resource ----------

$authority = "https://login.microsoftonline.com/common";
$authenticationContext = New-Object Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext($authority);
$resource = "https://management.core.windows.net/";
$clientId = "1950a258-227b-4e31-a9cf-717495945fc2";
$redirectUri = "urn:ietf:wg:oauth:2.0:oob";
$promptBehavior = [Microsoft.IdentityModel.Clients.ActiveDirectory.PromptBehavior]::Always;
$platformParameters = New-Object Microsoft.IdentityModel.Clients.ActiveDirectory.PlatformParameters($promptBehavior);
$authenticationResult = $authenticationContext.AcquireTokenAsync($resource, $clientId, $redirectUri, $platformParameters).Result;
$tenantId = $authenticationResult.TenantId;
$accessToken = $authenticationResult.AccessToken;

#---------- POLICIES ----------

New-B2CPolicy -tenantId $tenantId -accessToken $accessToken -policyDirectory $PolicyDirectory -policyId "B2C_1A_base";
New-B2CPolicy -tenantId $tenantId -accessToken $accessToken -policyDirectory $PolicyDirectory -policyId "B2C_1A_ext";
New-B2CPolicy -tenantId $tenantId -accessToken $accessToken -policyDirectory $PolicyDirectory -policyId "B2C_1A_password_reset";
New-B2CPolicy -tenantId $tenantId -accessToken $accessToken -policyDirectory $PolicyDirectory -policyId "B2C_1A_profile_update_personal";
New-B2CPolicy -tenantId $tenantId -accessToken $accessToken -policyDirectory $PolicyDirectory -policyId "B2C_1A_profile_update_work";
New-B2CPolicy -tenantId $tenantId -accessToken $accessToken -policyDirectory $PolicyDirectory -policyId "B2C_1A_sign_up_sign_in_personal";
New-B2CPolicy -tenantId $tenantId -accessToken $accessToken -policyDirectory $PolicyDirectory -policyId "B2C_1A_sign_up_sign_in_work";
