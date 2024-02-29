using System.Text;
using DevToys.Tools.Tools.Converters.NumberBase;

namespace DevToys.Tools.Models.NumberBase;

internal sealed class Binary : SignedLongNumberBaseDefinition
{
    internal static readonly Binary Instance = new();

    private Binary() : base(NumberBaseConverter.Binary, baseNumber: 2)
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

            formattedNumber.Insert(0, number[i]);
            count++;
        }

        // Ensure the first block is always 4 characters
        while (count % 4 != 0)
        {
            formattedNumber.Insert(0, '0');
            count++;
        }

        return formattedNumber.ToString();
    }

    protected override string AdjustNotFormatted(string number)
    {
        var formattedNumber = new StringBuilder(number);
        int count = formattedNumber.Length;

        // Ensure the first block is always 4 characters
        while (count % 4 != 0)
        {
            formattedNumber.Insert(0, '0');
            count++;
        }

        return formattedNumber.ToString();
    }
}
