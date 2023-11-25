namespace DevToys.Tools.Models;

internal enum EndOfLineSequence
{
    Unknown = 0,
    LineFeed = 1,
    CarriageReturn = 2,
    CarriageReturnLineFeed = 3,
    Mixed = 4
}

internal static class EndOfLineSequenceExtensions
{
    internal static string ToLineBreakCharacters(this EndOfLineSequence lineBreakType)
    {
        return lineBreakType switch
        {
            EndOfLineSequence.Unknown => Environment.NewLine,
            EndOfLineSequence.LineFeed => "\n",
            EndOfLineSequence.CarriageReturn => "\r",
            EndOfLineSequence.CarriageReturnLineFeed => "\r\n",
            EndOfLineSequence.Mixed => Environment.NewLine,
            _ => throw new ArgumentOutOfRangeException(nameof(lineBreakType)),
        };
    }
}
