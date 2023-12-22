using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;

public abstract record Data<T> : Token, IData<T>
{
    public string? Subtype { get; }

    public T Value { get; }

    public virtual int ConflictResolutionPriority => 0;

    public abstract string GetDisplayText(string culture);

    protected Data(
        string lineTextIncludingLineBreak,
        int startInLine,
        int endInLine,
        T value,
        string dataType,
        string subtype)
        : base(lineTextIncludingLineBreak, startInLine, endInLine, dataType)
    {
        Guard.IsNotNullOrWhiteSpace(subtype);
        Subtype = subtype;
        Value = value;
    }

    public bool IsOfSubtype(string expectedSubtype)
    {
        return string.Equals(Subtype, expectedSubtype, StringComparison.OrdinalIgnoreCase);
    }

    public abstract IData MergeDataLocations(IData otherData);

    public bool Equals(IData? other)
    {
        if (ReferenceEquals(this, other))
            return true;

        if (other is not null
            && StartInLine == other.StartInLine
            && Length == other.Length)
        {
            return true;
        }

        return false;
    }

    public int CompareTo(IData? other)
    {
        return StartInLine.CompareTo(other?.StartInLine ?? 0);
    }

    public override string ToString()
    {
        return base.ToString();
    }

    protected void ThrowIncompatibleUnitsException()
    {
        throw new IncompatibleUnitsException();
    }

    protected void ThrowUnsupportedArithmeticOperationException()
    {
        throw new UnsupportedArithmeticOperationException();
    }
}
