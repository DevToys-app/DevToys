namespace DevToys.Blazor.Core.Helpers;

public class ValueBuilder
{
    private string? _stringBuffer;

    public bool HasValue => !string.IsNullOrWhiteSpace(_stringBuffer);

    /// <summary>
    /// Adds a space separated conditional value to a property.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="when"></param>
    /// <returns></returns>
    public ValueBuilder AddValue(string value, bool when = true) => when ? AddRaw($"{value} ") : this;

    public ValueBuilder AddValue(Func<string> value, bool when = true) => when ? AddRaw($"{value()} ") : this;

    private ValueBuilder AddRaw(string style)
    {
        _stringBuffer += style;
        return this;
    }

    public override string ToString() => _stringBuffer != null ? _stringBuffer.Trim() : string.Empty;
}
