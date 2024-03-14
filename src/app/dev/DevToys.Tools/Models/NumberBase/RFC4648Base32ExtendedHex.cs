using DevToys.Tools.Tools.Converters.NumberBase;

namespace DevToys.Tools.Models.NumberBase;

internal sealed class RFC4648Base32ExtendedHex : UnsignedLongNumberBaseDefinition
{
    internal static readonly RFC4648Base32ExtendedHex Instance = new();

    private RFC4648Base32ExtendedHex()
        : base(
            displayName: NumberBaseConverter.RFC4648Base32ExtendedHex,
            dictionary: "0123456789ABCDEFGHIJKLMNOPQRSTUV",
            isDictionaryCaseSensitive: true)
    {
    }
}
