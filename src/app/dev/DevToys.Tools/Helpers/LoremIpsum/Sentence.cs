// Forked from https://github.com/alexcpendleton/NLipsum

namespace DevToys.Tools.Helpers.LoremIpsum;

/// <summary>
/// A sentence that can be formatted.
/// </summary>
internal sealed class Sentence : TextFeature
{
    /// <summary>
    /// Instantiates a sentence with Phrase formatting.
    /// </summary>
    /// <param name="minWords">The minimum amount of words to be included in this sentence.</param>
    /// <param name="maxWords">The maximum amount of words to be included in this sentence.</param>
    internal Sentence(uint minWords, uint maxWords)
        : this(minWords, maxWords, FormatStrings.Sentence.Phrase) { }

    /// <summary>
    /// Instantiates a sentence based on the passed criteria.
    /// </summary>
    /// <param name="minWords">The minimum amount of words to be included in this sentence.</param>
    /// <param name="maxWords">The maximum amount of words to be included in this sentence.</param>
    /// <param name="formatString">The string used to format this sentence.</param>
    internal Sentence(uint minWords, uint maxWords, string formatString)
    {
        FormatString = formatString;
        MinimumWords = minWords;
        MaximumWords = maxWords;
    }

    /// <summary>
    /// Gets or sets the minimum amount of words in this sentence.
    /// </summary>
    internal uint MinimumWords
    {
        get => MinimumValue;
        set => MinimumValue = value;
    }

    /// <summary>
    /// Gets or sets the maximum amount of words in this sentence.
    /// </summary>
    internal uint MaximumWords
    {
        get => MaximumValue;
        set => MaximumValue = value;
    }

    /// <summary>
    /// Gets a Short Sentence.  (MinimumWords = 2, MaximumWords=8)
    /// </summary>
    internal static Sentence Short => new(2, 8);

    /// <summary>
    /// Gets a Medium length Sentence.  (MinimumWords = 3, MaximumWords=20)
    /// </summary>
    internal static Sentence Medium => new(3, 20);

    /// <summary>
    /// Gets a Long Sentence.  (MinimumWords = 6, MaximumWords=40)
    /// </summary>
    internal static Sentence Long => new(6, 40);

    internal override string Format(string text)
    {
        string result = base.Format(text);
        if (result.Length > 1)
        {
            result = string.Concat(result.Substring(0, 1).ToUpper(), result.AsSpan(1));
        }
        return result;
    }
}
