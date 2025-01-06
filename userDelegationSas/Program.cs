using Azure.Identity;
using Azure.Storage.Blobs;
using UserDelegationSAS;

var tenantId = "<your-tenant-id>";
var clientId = "<your-client-id>";
var clientSecret = "<your-client-secret>";

var accountName = "<yourstorageaccount>";
var container = "mycontainer";
var blob = "mydocument.pdf";

var blobEndpoint = $"https://{accountName}.blob.core.windows.net";

//var service = CreateAzureSdkSasService();
var service = CreateCustomSasService();
var delegationSas = await service.GetUserDelegationKeyAsync(24);
var sasUri = service.CreateUserDelegationSASForBlob(container, blob, 1, delegationSas);

Console.WriteLine(sasUri);

IUserDelegationSasService CreateAzureSdkSasService()
{
    var azCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);

    // Create a blob service client object using DefaultAzureCredential
    BlobServiceClient blobServiceClient = new BlobServiceClient(
        new Uri(blobEndpoint),
        azCredential);
    var service = new AzureSdkUserDelegationSasService(blobServiceClient);

    return service;
}

IUserDelegationSasService CreateCustomSasService()
{
    return new CustomUserDelegationSasService(tenantId, clientId, clientSecret, blobEndpoint);
}