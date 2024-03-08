using System.Globalization;
using System.Text;
using DevToys.Tools.Models.NumberBase;
using DevToys.Tools.Tools.Converters.NumberBase;
using Decimal = DevToys.Tools.Models.NumberBase.Decimal;

namespace DevToys.Tools.Helpers;

internal static class NumberBaseHelper
{
    internal static bool TryDetectNumberBase(string? potentialNumber, out INumberBaseDefinition<long>? numberBaseDefinition)
    {
        numberBaseDefinition = null;

        if (string.IsNullOrWhiteSpace(potentialNumber)
            || potentialNumber.Length > 100) // unlikely to be a number.
        {
            return false;
        }

        // Remove the potential formatting on the input number.
        string unformattedPotentialNumber = UnformatNumber(potentialNumber);

        // Try to detect the base of the input number.
        try
        {
            long parsedNumber = Decimal.Instance.Parse(unformattedPotentialNumber);
            numberBaseDefinition = Decimal.Instance;
            return true;
        }
        catch { }

        try
        {
            long parsedNumber = Binary.Instance.Parse(unformattedPotentialNumber);
            numberBaseDefinition = Binary.Instance;
            return true;
        }
        catch { }

        try
        {
            long parsedNumber = Hexadecimal.Instance.Parse(unformattedPotentialNumber);
            numberBaseDefinition = Hexadecimal.Instance;
            return true;
        }
        catch { }

        try
        {
            long parsedNumber = Octal.Instance.Parse(unformattedPotentialNumber);
            numberBaseDefinition = Octal.Instance;
            return true;
        }
        catch { }

        return false;
    }

    internal static bool TryConvertNumberBase(
        string? inputNumber,
        INumberBaseDefinition<long> numberBase,
        bool format,
        out string hexadecimal,
        out string @decimal,
        out string octal,
        out string binary,
        out string error)
    {
        Guard.IsNotNull(numberBase);

        hexadecimal = string.Empty;
        @decimal = string.Empty;
        octal = string.Empty;
        binary = string.Empty;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(inputNumber))
        {
            return true;
        }

        if (!TryParseNumber(inputNumber, numberBase, out long signedValue, out error))
        {
            return false;
        }

        // Convert the number to the other bases and format them if needed.
        hexadecimal = Hexadecimal.Instance.ToFormattedString(signedValue, format);
        @decimal = Decimal.Instance.ToFormattedString(signedValue, format);
        octal = Octal.Instance.ToFormattedString(signedValue, format);
        binary = Binary.Instance.ToFormattedString(signedValue, format);
        return true;
    }

    internal static bool TryConvertNumberBase<T>(
        string? inputNumber,
        INumberBaseDefinition<T> inputNumberBase,
        INumberBaseDefinition<T> outputNumberBase,
        bool format,
        out string result,
        out string error)
        where T : struct
    {
        Guard.IsNotNull(inputNumberBase);
        Guard.IsNotNull(outputNumberBase);

        result = string.Empty;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(inputNumber))
        {
            return true;
        }

        if (!TryParseNumber(inputNumber, inputNumberBase, out T unsignedValue, out error))
        {
            return false;
        }

        // Convert the number to the other bases and format them if needed.
        result = outputNumberBase.ToFormattedString(unsignedValue, format);
        return true;
    }

    private static bool TryParseNumber<T, U>(string? inputNumber, T numberBase, out U value, out string error)
        where T : INumberBaseDefinition<U>
        where U : struct
    {
        value = default;
        error = string.Empty;

        // Remove the potential formatting on the input number.
        string unformattedInputNumber = UnformatNumber(inputNumber);

        try
        {
            // Try to parse the number.
            value = numberBase.Parse(unformattedInputNumber);
            return true;
        }
        catch (OverflowException)
        {
            error = string.Format(NumberBaseConverter.ValueOverflow, numberBase.DisplayName, numberBase.MaxValue);
            return false;
        }
        catch (Exception)
        {
            error = string.Format(NumberBaseConverter.ValueInvalid, numberBase.DisplayName);
            return false;
        }
    }

    private static string UnformatNumber(string? formattedNumber)
    {
        if (string.IsNullOrWhiteSpace(formattedNumber!))
        {
            return string.Empty;
        }

        var unformattedNumber = new StringBuilder();

        string currentCulture = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
        for (int i = 0; i < formattedNumber.Length; i++)
        {
            if (!char.IsWhiteSpace(formattedNumber[i]) && formattedNumber[i] != Convert.ToChar(currentCulture))
            {
                unformattedNumber.Append(formattedNumber[i]);
            }
        }

        return unformattedNumber.ToString();
    }
}
