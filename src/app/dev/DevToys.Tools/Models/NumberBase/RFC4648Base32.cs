using DevToys.Tools.Tools.Converters.NumberBase;

namespace DevToys.Tools.Models.NumberBase;

internal sealed class RFC4648Base32 : UnsignedLongNumberBaseDefinition
{
    internal static readonly RFC4648Base32 Instance = new();

    private RFC4648Base32()
        : base(
            displayName: NumberBaseConverter.RFC4648Base32,
            dictionary: "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567",
            isDictionaryCaseSensitive: true)
    {
    }
}
