using System.Text;
using DevToys.Tools.Tools.Converters.NumberBase;

namespace DevToys.Tools.Models.NumberBase;

internal sealed class Octal : SignedLongNumberBaseDefinition
{
    internal static readonly Octal Instance = new();

    private Octal() : base(NumberBaseConverter.Octal, baseNumber: 8)
    {
    }

    protected override string Format(string number)
    {
        var formattedNumber = new StringBuilder();
        int count = 0;

        for (int i = number.Length - 1; i >= 0; i--)
        {
            if (count > 0 && count % 3 == 0)
            {
                formattedNumber.Insert(0, ' ');
            }

            formattedNumber.Insert(0, number[i]);
            count++;
        }

        return formattedNumber.ToString();
    }
}
