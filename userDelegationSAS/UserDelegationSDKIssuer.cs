using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace UserDelegationSAS;

internal class UserDelegationSDKIssuer : IUserDelegationSasIssuer
{
    private BlobServiceClient _blobServiceClient { get; set; }

    public UserDelegationSDKIssuer(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<UserDelegationKey> RequestUserDelegationKeyAsync(double hours, CancellationToken cancellationToken)
    {
        var utcNow = DateTimeOffset.UtcNow;

        var userDelegationKey = await _blobServiceClient.GetUserDelegationKeyAsync(
                utcNow,
                utcNow.AddHours(hours), cancellationToken);

        return new UserDelegationKey(userDelegationKey);
    }

    public Uri CreateUserDelegationSASBlob(
        string blobContainerName,
        string blobName,
        double validityInHours,
        UserDelegationKey userDelegationKey)
    {
        var blobClient = _blobServiceClient
        .GetBlobContainerClient(blobContainerName)
        .GetBlobClient(blobName);

        var utcNow = DateTimeOffset.UtcNow;
        var sasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = blobClient.BlobContainerName,
            BlobName = blobClient.Name,
            Resource = "b",
            StartsOn = utcNow,
            ExpiresOn = utcNow.AddHours(validityInHours)
        };

        // Specify the necessary permissions
        sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write);

        // Add the SAS token to the blob URI
        var uriBuilder = new BlobUriBuilder(blobClient.Uri)
        {
            // Specify the user delegation key
            Sas = sasBuilder.ToSasQueryParameters(
                userDelegationKey.OriginalUserDelegationKey,
                _blobServiceClient.AccountName)
        };

        return uriBuilder.ToUri();
    }


}
