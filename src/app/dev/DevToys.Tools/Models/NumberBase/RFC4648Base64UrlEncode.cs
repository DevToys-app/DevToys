using DevToys.Tools.Tools.Converters.NumberBase;

namespace DevToys.Tools.Models.NumberBase;

internal sealed class RFC4648Base64UrlEncode : UnsignedLongNumberBaseDefinition
{
    internal static readonly RFC4648Base64UrlEncode Instance = new();

    private RFC4648Base64UrlEncode()
        : base(
            displayName: NumberBaseConverter.RFC4648Base64UrlEncode,
            dictionary: "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_",
            isDictionaryCaseSensitive: true)
    {
    }
}
