using DevToys.Tools.Tools.Converters.NumberBase;

namespace DevToys.Tools.Models.NumberBase;

internal sealed class CustomUnsigned : UnsignedLongNumberBaseDefinition
{
    internal CustomUnsigned(string dictionary)
        : base(
            NumberBaseConverter.CustomFormat,
            dictionary,
            isDictionaryCaseSensitive: true)
    {
    }
}
