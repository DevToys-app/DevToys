using DevToys.Tools.Tools.Converters.NumberBase;

namespace DevToys.Tools.Models.NumberBase;

internal sealed class RFC4648Base64 : UnsignedLongNumberBaseDefinition
{
    internal static readonly RFC4648Base64 Instance = new();

    private RFC4648Base64()
        : base(
            displayName: NumberBaseConverter.RFC4648Base64,
            dictionary: "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/",
            isDictionaryCaseSensitive: true)
    {
    }
}
