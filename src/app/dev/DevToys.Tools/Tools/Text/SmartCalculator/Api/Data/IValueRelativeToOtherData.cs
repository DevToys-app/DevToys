namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;

/// <summary>
/// Indicates that a data is always relative to another one.
/// </summary>
public interface IValueRelativeToOtherData : INumericData
{
    /// <summary>
    /// Gets the value, in standard unit, relative to the given <paramref name="other"/> data.
    /// </summary>
    double GetStandardUnitValueRelativeTo(INumericData other);
}
