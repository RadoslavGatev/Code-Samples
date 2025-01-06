using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;

namespace UserDelegationSAS;

internal class CustomUserDelegationSasService(string tenantId, string clientId, string clientSecret, string storageServiceUri) : IUserDelegationSasService
{
    private string _tenantId { get; } = tenantId;
    private string _clientId { get; } = clientId;
    private string _clientSecret { get; } = clientSecret;
    private string _storageServiceUri { get; } = storageServiceUri;

    public async Task<UserDelegationKey> GetUserDelegationKeyAsync(double hours, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();
        var authToken = await GetAuthToken(httpClient, cancellationToken);
        var userDelegationKey = await GetUserDelegationKeyInternal(httpClient, authToken, hours, cancellationToken);
        return userDelegationKey;
    }

    public Uri CreateUserDelegationSASForBlob(string blobContainerName, string blobName, double validityInHours,
        UserDelegationKey userDelegationKey)
    {
        var storageAccountEndpointUri = new Uri(_storageServiceUri);
        var storageAccountName = storageAccountEndpointUri.Host.Split('.').FirstOrDefault();

        var startTime = DateTimeOffset.UtcNow;
        var endTime = startTime.AddHours(validityInHours);
        var start = startTime.ToString("s") + 'Z';
        var end = endTime.ToString("s") + 'Z';

        var pathCleansed = $"{blobContainerName}/{blobName}".Replace("//", "/");
        var stringToSign = "r" + "\n" +
            start + "\n" +
            end + "\n" +
            $"/blob/{storageAccountName}/{pathCleansed}".Replace("//", "/") + "\n" +
            userDelegationKey.SignedObjectId + "\n" +
            userDelegationKey.SignedTenantId + "\n" +
            userDelegationKey.SignedStartsOn.ToString("s") + 'Z' + "\n" +
            userDelegationKey.SignedExpiresOn.ToString("s") + 'Z' + "\n" +
            userDelegationKey.SignedService + "\n" +
            userDelegationKey.SignedVersion + "\n" +
            "\n\n\n\n" +
            "https" + "\n" +
            "2023-01-03" + "\n" +
            "b" + "\n" +
            "\n\n\n\n\n\n";

        string signature = stringToSign.ComputeHMACSHA256(userDelegationKey.Value);
        string signatureUrlEncoded = HttpUtility.UrlEncode(signature);
        var query = string.Join("&",
            "sp=r",
            $"st={start}",
            $"se={end}",
            $"skoid={userDelegationKey.SignedObjectId}",
            $"sktid={userDelegationKey.SignedTenantId}",
            $"skt={userDelegationKey.SignedStartsOn.ToString("s") + 'Z'}",
            $"ske={userDelegationKey.SignedExpiresOn.ToString("s") + 'Z'}",
            "sks=b",
            $"skv={userDelegationKey.SignedVersion}",
            "spr=https",
            $"sv={userDelegationKey.SignedVersion}",
            "sr=b",
            $"sig={signatureUrlEncoded}"
        );

        var sasBuilder = new UriBuilder(_storageServiceUri)
        {
            Scheme = Uri.UriSchemeHttps,
            Port = -1, // default port for scheme
            Path = pathCleansed,
            Query = query
        };

        return sasBuilder.Uri;
    }

    private async Task<UserDelegationKey> GetUserDelegationKeyInternal(HttpClient httpClient,
        string authToken, double hours, CancellationToken cancellationToken)
    {
        var delegationUriBuilder = new UriBuilder(_storageServiceUri)
        {
            Scheme = Uri.UriSchemeHttps,
            Port = -1, // default port for scheme
            Query = "restype=service&comp=userdelegationkey&timeout=30"
        };
        using var delegationKeyRequest = new HttpRequestMessage(HttpMethod.Post, delegationUriBuilder.Uri.ToString());
        delegationKeyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        delegationKeyRequest.Headers.Add("x-ms-version", "2023-01-03");

        var startTime = DateTimeOffset.UtcNow;
        var endTime = startTime.AddHours(hours);
        var payload = $"<KeyInfo><Start>{startTime.ToString("s")}Z</Start><Expiry>{endTime.ToString("s")}Z</Expiry></KeyInfo>";
        var payloadContent = new StringContent(payload, Encoding.UTF8, "application/xml");
        delegationKeyRequest.Content = payloadContent;

        using var delegationKeyResponse = await httpClient.SendAsync(delegationKeyRequest, cancellationToken);
        var xmlResponse = await delegationKeyResponse.Content.ReadAsStringAsync(cancellationToken);

        var result = xmlResponse.FromXml<UserDelegationKey>();
        if (result is null)
        {
            throw new InvalidOperationException("Failed to deserialize the UserDelegationKey. It is null.");
        }
        return result;
    }

    private async Task<string> GetAuthToken(HttpClient httpClient, CancellationToken cancellationToken)
    {
        var tokenEndpoint = $"https://login.microsoftonline.com/{_tenantId}/oauth2/v2.0/token";
        using var aadTokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
        var form = new Dictionary<string, string>
        {
            {"grant_type", "client_credentials"},
            {"client_id", _clientId},
            {"client_secret", _clientSecret},
            {"scope", "https://storage.azure.com/.default"}
        };
        aadTokenRequest.Content = new FormUrlEncodedContent(form);

        var response = await httpClient.SendAsync(aadTokenRequest, cancellationToken);
        using var jsonContent = await response.Content.ReadAsStreamAsync(cancellationToken);

        var parsedJson = await JsonDocument.ParseAsync(jsonContent, cancellationToken: cancellationToken);
        var accessToken = parsedJson.RootElement.GetProperty("access_token").GetString();
        if (accessToken is null)
        {
            throw new InvalidOperationException("Failed to get the access token.");
        }
        return accessToken;
    }
}
