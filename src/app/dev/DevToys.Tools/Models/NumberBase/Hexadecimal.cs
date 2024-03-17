using System.Text;
using DevToys.Tools.Tools.Converters.NumberBase;

namespace DevToys.Tools.Models.NumberBase;

internal sealed class Hexadecimal : SignedLongNumberBaseDefinition
{
    internal static readonly Hexadecimal Instance = new();

    private Hexadecimal() : base(NumberBaseConverter.Hexadecimal, baseNumber: 16)
    {
    }

    protected override string Format(string number)
    {
        var formattedNumber = new StringBuilder();
        int count = 0;

        for (int i = number.Length - 1; i >= 0; i--)
        {
            if (count > 0 && count % 4 == 0)
            {
                formattedNumber.Insert(0, ' ');
            }

            formattedNumber.Insert(0, char.ToUpper(number[i]));
            count++;
        }

        return formattedNumber.ToString();
    }

    override protected string AdjustNotFormatted(string number)
    {
        return number.ToUpper();
    }
}
