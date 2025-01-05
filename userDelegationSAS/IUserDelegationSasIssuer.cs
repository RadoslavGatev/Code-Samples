namespace UserDelegationSAS;

public interface IUserDelegationSasIssuer
{
    Uri CreateUserDelegationSASBlob(string blobContainerName, string blobName, double validityInHours, UserDelegationKey userDelegationKey);
    Task<UserDelegationKey> RequestUserDelegationKeyAsync(double hours, CancellationToken cancellationToken = default);
}