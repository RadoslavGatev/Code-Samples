using Azure.Identity;
using Azure.Storage.Blobs;
using UserDelegationSAS;

var tenantId = "<your-tenant-id>";
var clientId = "<your-client-id>";
var clientSecret = "<your-client-secret>";

var accountName = "<yourstorageaccount>";
var container = "mycontainer";
var blob = "mydocument.pdf";


string blobEndpoint = $"https://{accountName}.blob.core.windows.net";

//var issuer = CreateSdkSasIssuer();
var issuer = CreateSasIssuerFromScratch();
var delegationSas = await issuer.RequestUserDelegationKeyAsync(24);

var sasUri = issuer.CreateUserDelegationSASBlob(container, blob, 1, delegationSas);
Console.WriteLine(sasUri);


IUserDelegationSasIssuer CreateSdkSasIssuer()
{
    var azCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);

    // Create a blob service client object using DefaultAzureCredential
    BlobServiceClient blobServiceClient = new BlobServiceClient(
        new Uri(blobEndpoint),
        azCredential);
    var issuer = new UserDelegationSDKIssuer(blobServiceClient);

    return issuer;
}

IUserDelegationSasIssuer CreateSasIssuerFromScratch()
{
    return new UserDelegationFromScratch(tenantId, clientId, clientSecret, blobEndpoint);
}