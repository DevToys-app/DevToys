using System.Globalization;
using DevToys.Tools.Tools.Converters.NumberBase;

namespace DevToys.Tools.Models.NumberBase;

internal sealed class Decimal : SignedLongNumberBaseDefinition
{
    internal static readonly Decimal Instance = new();

    private Decimal() : base(NumberBaseConverter.Decimal, baseNumber: 10)
    {
    }

    protected override string Format(string number)
    {
        if (decimal.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedNumber))
        {
            // Convert to string using current culture.
            return parsedNumber.ToString("N0", CultureInfo.CurrentCulture);
        }

        return number;
    }
}
