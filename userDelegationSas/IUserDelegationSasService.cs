namespace UserDelegationSAS;

public interface IUserDelegationSasService
{
    Uri CreateUserDelegationSASForBlob(string blobContainerName, string blobName, double validityInHours, UserDelegationKey userDelegationKey);
    Task<UserDelegationKey> GetUserDelegationKeyAsync(double hours, CancellationToken cancellationToken = default);
}