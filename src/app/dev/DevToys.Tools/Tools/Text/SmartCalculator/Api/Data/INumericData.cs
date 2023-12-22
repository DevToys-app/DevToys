namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;

/// <summary>
/// Indicates that the data is a numeric value support arithmetic operation.
/// </summary>
public interface INumericData : IData
{
    /// <summary>
    /// Gets whether the data's value is negative.
    /// </summary>
    bool IsNegative { get; }

    /// <summary>
    /// Gets the data's value in its current unit of measure.
    /// </summary>
    double NumericValueInCurrentUnit { get; }

    /// <summary>
    /// Gets the data's value in a standard unit of measure.
    /// </summary>
    double NumericValueInStandardUnit { get; }

    /// <summary>
    /// Creates a data from a value in the standard unit of measure.
    /// The returned value should usually be converted to the current unit.
    /// </summary>
    INumericData CreateFromStandardUnit(double valueInStandardUnit);

    /// <summary>
    /// Creates a data from a value in the current unit of measure.
    /// </summary>
    INumericData CreateFromCurrentUnit(double valueInCurrentUnit);

    /// <summary>
    /// Performs an addition.
    /// </summary>
    INumericData Add(INumericData otherData);

    /// <summary>
    /// Performs a substraction.
    /// </summary>
    INumericData Substract(INumericData otherData);

    /// <summary>
    /// Performs a multiplication.
    /// </summary>
    INumericData Multiply(INumericData otherData);

    /// <summary>
    /// Performs a division.
    /// </summary>
    INumericData Divide(INumericData otherData);
}
