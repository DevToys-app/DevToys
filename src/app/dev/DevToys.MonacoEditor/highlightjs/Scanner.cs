#if __MAC__

namespace Foundation;

/// <summary>
/// Reimplementation of https://developer.apple.com/documentation/foundation/scanner
/// </summary>
internal sealed class Scanner
{
    public Scanner(string input)
    {
        String = input;
    }

    private ReadOnlySpan<char> Span => String;

    internal bool IsAtEnd => ScanLocation >= String.Length;

    internal int ScanLocation { get; set; }

    internal string String { get; }

    /// <summary>
    /// Scans the string until a given string is encountered, accumulating characters into a string that’s returned by reference.
    /// </summary>
    /// <param name="stopString">The string to scan up to.</param>
    /// <param name="stringValue">The string to scan up to.</param>
    /// <returns>
    /// true if the receiver scans any characters, otherwise false.
    /// If the only scanned characters are in the charactersToBeSkipped character set (which by default is the whitespace and newline character set),
    /// then this method returns false.
    /// </returns>
    /// <remarks>
    /// If <paramref name="stopString"/> is present in the receiver, then on return the scan location is set to the beginning of that string.
    /// If <paramref name="stopString"/> is the first string in the receiver, then the method returns false and <paramref name="stringValue"/> is not changed.
    /// If the search string (<paramref name="stopString"/>) isn't present in the scanner's source string, the remainder of the source string is put into <paramref name="stringValue"/>,
    /// the receiver’s scanLocation is advanced to the end of the source string, and the method returns true.
    /// Invoke this method with <code>null</code> as <paramref name="stringValue"/> to simply scan up to a given string.
    /// </remarks>
    internal bool ScanUpTo(string stopString, out ReadOnlySpan<char> stringValue)
    {
        int index = String.IndexOf(stopString, ScanLocation, StringComparison.Ordinal);

        if (index > -1)
        {
            stringValue = Span.Slice(ScanLocation, index - ScanLocation);
            ScanLocation = index;
            return true;
        }

        if (index == ScanLocation)
        {
            stringValue = ReadOnlySpan<char>.Empty;
            return false;
        }

        stringValue = Span.Slice(ScanLocation, String.Length - ScanLocation);
        ScanLocation = String.Length;
        return true;
    }
}

#endif
