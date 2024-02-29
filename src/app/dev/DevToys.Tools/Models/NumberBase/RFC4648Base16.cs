using DevToys.Tools.Tools.Converters.NumberBase;

namespace DevToys.Tools.Models.NumberBase;

internal sealed class RFC4648Base16 : UnsignedLongNumberBaseDefinition
{
    internal static readonly RFC4648Base16 Instance = new();

    private RFC4648Base16()
        : base(
            displayName: NumberBaseConverter.RFC4648Base16,
            dictionary: "0123456789ABCDEF",
            isDictionaryCaseSensitive: false)
    {
    }
}
