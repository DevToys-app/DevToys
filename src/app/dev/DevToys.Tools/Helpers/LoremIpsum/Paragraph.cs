// Forked from https://github.com/alexcpendleton/NLipsum

namespace DevToys.Tools.Helpers.LoremIpsum;

/// <summary>
/// A Paragraph that can be formatted..
/// </summary>
internal sealed class Paragraph : TextFeature
{
    private Sentence _sentenceOptions = Sentence.Medium;

    /// <summary>
    /// Instantiates a paragraph with no formatting.
    /// </summary>
    /// <param name="minSentences">The minimum amount of sentences to be included in this paragraph.</param>
    /// <param name="maxSentences">The maximum amount of sentences to be included in this paragraph.</param>
    internal Paragraph(uint minSentences, uint maxSentences)
        : this(minSentences, maxSentences, FormatStrings.Unformatted) { }

    internal Paragraph(uint minSentences, uint maxSentences, string formatString)
    {
        MinimumSentences = minSentences;
        MaximumSentences = maxSentences;
        FormatString = formatString;
    }

    /// <summary>
    /// Gets or sets the minimum amount of sentences in this paragraph.
    /// </summary>
    internal uint MinimumSentences
    {
        get => MinimumValue;
        set => MinimumValue = value;
    }

    /// <summary>
    /// Gets or sets the maximum amount of words in this paragraph.
    /// </summary>
    internal uint MaximumSentences
    {
        get => MaximumValue;
        set => MaximumValue = value;
    }

    /// <summary>
    /// Gets a Short Paragraph.  (MinimumSentences = 2, MaximumSentences=8)
    /// </summary>
    internal static Paragraph Short => new(2, 8);

    /// <summary>
    /// Gets a Medium length Paragraph.  (MinimumSentences = 3, MaximumSentences=20)
    /// </summary>
    internal static Paragraph Medium => new(3, 20);

    /// <summary>
    /// Gets a Long Paragraph.  (MinimumSentences = 6, MaximumSentences=40)
    /// </summary>
    internal static Paragraph Long => new(6, 40);

    /// <summary>
    /// Gets or sets the options for the sentences in this paragraph.  Default is Sentence.Medium
    /// </summary>
    internal Sentence SentenceOptions
    {
        get => _sentenceOptions;
        set => _sentenceOptions = value;
    }
}
