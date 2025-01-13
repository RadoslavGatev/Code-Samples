﻿using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace UserDelegationSAS;

internal class AzureSdkUserDelegationSasService : IUserDelegationSasService
{
    private BlobServiceClient _blobServiceClient { get; set; }

    public AzureSdkUserDelegationSasService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<UserDelegationKey> GetUserDelegationKeyAsync(double hours, CancellationToken cancellationToken)
    {
        var utcNow = DateTimeOffset.UtcNow;

        var userDelegationKey = await _blobServiceClient.GetUserDelegationKeyAsync(
                utcNow,
                utcNow.AddHours(hours), cancellationToken);

        return new UserDelegationKey(userDelegationKey);
    }

    public Uri CreateUserDelegationSASForBlob(
        string blobContainerName,
        string blobName,
        double validityInHours,
        UserDelegationKey userDelegationKey)
    {
        var blobClient = _blobServiceClient
        .GetBlobContainerClient(blobContainerName)
        .GetBlobClient(blobName);

        var utcNow = DateTimeOffset.UtcNow;
        var sasPermissions = BlobSasPermissions.Read | BlobSasPermissions.Write;
        var sasBuilder = new BlobSasBuilder(sasPermissions, utcNow.AddHours(validityInHours))
        {
            BlobContainerName = blobClient.BlobContainerName,
            BlobName = blobClient.Name,
            Resource = "b",
            StartsOn = utcNow,
        };

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
