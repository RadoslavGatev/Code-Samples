using System.Xml.Serialization;

namespace UserDelegationSAS;

public class UserDelegationKey
{
    [XmlElement("SignedOid")]
    public string SignedObjectId { get; set; }

    [XmlElement("SignedTid")]
    public string SignedTenantId { get; set; }

    [XmlElement("SignedExpiry")]
    public DateTimeOffset SignedExpiresOn { get; set; }

    [XmlElement("SignedStart")]
    public DateTimeOffset SignedStartsOn { get; set; }

    public string SignedService { get; set; }

    public string SignedVersion { get; set; }

    public string Value { get; set; }

    [XmlIgnore]
    public Azure.Storage.Blobs.Models.UserDelegationKey? OriginalUserDelegationKey { get; private set; }

#pragma warning disable CS8618 // This parameterless constructor is needed because of XmlSerializer
    public UserDelegationKey() { }
#pragma warning restore CS8618 // This parameterless constructor is needed because of XmlSerializer

    public UserDelegationKey(Azure.Storage.Blobs.Models.UserDelegationKey azUserDelegationKey)
    {
        SignedObjectId = azUserDelegationKey.SignedObjectId;
        SignedTenantId = azUserDelegationKey.SignedTenantId;
        SignedExpiresOn = azUserDelegationKey.SignedExpiresOn;
        SignedStartsOn = azUserDelegationKey.SignedStartsOn;
        SignedService = azUserDelegationKey.SignedService;
        SignedVersion = azUserDelegationKey.SignedVersion;
        Value = azUserDelegationKey.Value;
        OriginalUserDelegationKey = azUserDelegationKey;
    }
}
