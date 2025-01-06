using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace UserDelegationSAS;

internal static class StringExtensions
{
    public static T? FromXml<T>(this string value)
    {
        using TextReader reader = new StringReader(value);
        var xmlSerializer = new XmlSerializer(typeof(T));
        return (T?)xmlSerializer.Deserialize(reader);
    }

    public static string ComputeHMACSHA256(this string message, string key)
    {
        var keyAsBinary = Convert.FromBase64String(key);
        using var hmacSha256 = new HMACSHA256(keyAsBinary);
        var signatureHash = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(message));
        var signatureAsBase64 = Convert.ToBase64String(signatureHash);
        return signatureAsBase64;
    }
}
