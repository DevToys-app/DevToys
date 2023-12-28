namespace DevToys.Tools.Helpers.LoremIpsum;

internal sealed class Word : TextFeature
{
    /// <summary>
    /// Instantiates a medium sized sentence with Phrase formatting.
    /// </summary>
    internal Word() : this(Medium.MinimumValue, Medium.MaximumValue) { }

    /// <summary>
    /// Instantiates a sentence with Phrase formatting.
    /// </summary>
    /// <param name="minWords">The minimum amount of words to be included in this sentence.</param>
    /// <param name="maxWords">The maximum amount of words to be included in this sentence.</param>
    internal Word(uint minWords, uint maxWords)
        : this(minWords, maxWords, FormatStrings.Unformatted) { }

    /// <summary>
    /// Instantiates a sentence based on the passed criteria.
    /// </summary>
    /// <param name="minWords">The minimum amount of words to be included in this sentence.</param>
    /// <param name="maxWords">The maximum amount of words to be included in this sentence.</param>
    /// <param name="formatString">The string used to format this sentence.</param>
    internal Word(uint minWords, uint maxWords, string formatString)
    {
        FormatString = formatString;
        MinimumValue = minWords;
        MaximumValue = maxWords;
    }

    /// <summary>
    /// Gets or sets the minimum amount of characters in this word.
    /// </summary>
    internal uint MinimumCharacters
    {
        get => MinimumValue;
        set => MinimumValue = value;
    }

    /// <summary>
    /// Gets or sets the maximum amount of characters in this word.
    /// </summary>
    internal uint MaximumCharacters
    {
        get => MaximumValue;
        set => MaximumValue = value;
    }

    /// <summary>
    /// Gets a Short Sentence.  (MinimumWords = 2, MaximumWords=8)
    /// </summary>
    internal static Word Short => new(1, 3);

    /// <summary>
    /// Gets a Medium length Sentence.  (MinimumWords = 3, MaximumWords=20)
    /// </summary>
    internal static Word Medium => new(4, 8);

    /// <summary>
    /// Gets a Long Sentence.  (MinimumWords = 6, MaximumWords=40)
    /// </summary>
    internal static Word Long => new(10, 25);
}
