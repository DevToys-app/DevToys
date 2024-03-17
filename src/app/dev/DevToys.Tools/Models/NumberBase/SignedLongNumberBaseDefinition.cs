namespace DevToys.Tools.Models.NumberBase;

internal abstract class SignedLongNumberBaseDefinition : INumberBaseDefinition<long>
{
    private readonly int _base;

    protected SignedLongNumberBaseDefinition(string displayName, int baseNumber)
    {
        Guard.IsNotNullOrWhiteSpace(displayName);
        Guard.IsGreaterThan(baseNumber, 0);
        DisplayName = displayName;
        _base = baseNumber;
    }

    public string DisplayName { get; }

    public long MaxValue => long.MaxValue / _base;

    public string ToFormattedString(long number, bool format)
    {
        string stringNumber = Convert.ToString(number, toBase: _base);

        if (format)
        {
            stringNumber = Format(stringNumber);
        }
        else
        {
            stringNumber = AdjustNotFormatted(stringNumber);
        }

        return stringNumber;
    }

    public long Parse(string number)
    {
        return Convert.ToInt64(number, fromBase: _base);
    }

    protected abstract string Format(string number);

    protected virtual string AdjustNotFormatted(string number)
    {
        return number;
    }
}
