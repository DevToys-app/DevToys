namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;

/// <summary>
/// Defines identifiers for supported binary operators
/// </summary>
public enum BinaryOperatorType
{
    /// <summary>
    /// Identity equal operator
    /// </summary>
    [Description("==")]
    Equality = 0,

    /// <summary>
    /// Identity no equal operator
    /// </summary>
    [Description("!=")]
    NoEquality = 1,

    /// <summary>
    /// Less than operator
    /// </summary>
    [Description("<")]
    LessThan = 2,

    /// <summary>
    /// Less than or equal operator
    /// </summary>
    [Description("<=")]
    LessThanOrEqualTo = 3,

    /// <summary>
    /// Greater than operator
    /// </summary>
    [Description(">")]
    GreaterThan = 4,

    /// <summary>
    /// Greater than or equal operator
    /// </summary>
    [Description(">=")]
    GreaterThanOrEqualTo = 5,

    /// <summary>
    /// Addition operator
    /// </summary>
    [Description("+")]
    Addition = 6,

    /// <summary>
    /// Subtraction operator
    /// </summary>
    [Description("-")]
    Subtraction = 7,

    /// <summary>
    /// Multiplication operator
    /// </summary>
    [Description("*")]
    Multiply = 8,

    /// <summary>
    /// Division operator
    /// </summary>
    [Description("/")]
    Division = 9,
}
