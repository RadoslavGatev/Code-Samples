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

    public static string ComputeHMACSHA256(this string message, string userDelegationKeyValue)
    {
        return Convert.ToBase64String(
                    new HMACSHA256(
                        Convert.FromBase64String(userDelegationKeyValue))
                    .ComputeHash(Encoding.UTF8.GetBytes(message)));
    }
}
