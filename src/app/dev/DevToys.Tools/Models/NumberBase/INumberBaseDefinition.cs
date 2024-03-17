namespace DevToys.Tools.Models.NumberBase;

internal interface INumberBaseDefinition<T> where T : struct
{
    /// <summary>
    /// Gets the display name of the number base.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the maximum value of the number base. This value ensure that the number doesn't overflow and
    /// is used to display to the user when the number is too large.
    /// </summary>
    T MaxValue { get; }

    /// <summary>
    /// Parses a string to a number. Raises an exception if the number is invalid or overflow.
    /// </summary>
    T Parse(string number);

    /// <summary>
    /// Converts a number to a string. If the format is true, the number is formatted.
    /// </summary>
    string ToFormattedString(T number, bool format);
}
