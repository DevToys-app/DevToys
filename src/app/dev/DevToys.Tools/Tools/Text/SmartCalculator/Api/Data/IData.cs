using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;

/// <summary>
/// Represents a data.
/// </summary>
public interface IData : IToken, IEquatable<IData>, IComparable<IData>
{
    /// <summary>
    /// When multiple data or various type are discovered the text document at a same location, the parser
    /// needs to solve the conflict. This property indicates the priority to of a data over others.
    /// <see cref="int.MinValue"/> is the highest priority.
    /// </summary>
    int ConflictResolutionPriority { get; }

    /// <summary>
    /// Gets an optional internal non-localized name that represents the subtype of token.
    /// </summary>
    string? Subtype { get; }

    /// <summary>
    /// Gets a string representation of the data that will be displayed to the user.
    /// </summary>
    string GetDisplayText(string culture);

    /// <summary>
    /// Gets whether the data has the <paramref name="expectedSubtype"/>.
    /// </summary>
    bool IsOfSubtype(string expectedSubtype);

    /// <summary>
    /// Merged the locations of the current data with the given <paramref name="otherData"/>.
    /// </summary>
    IData MergeDataLocations(IData otherData);
}

/// <summary>
/// Represents a data with a strongly typed value.
/// </summary>
public interface IData<T> : IData
{
    /// <summary>
    /// Gets the value of the data.
    /// </summary>
    T Value { get; }
}
