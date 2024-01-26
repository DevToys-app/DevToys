// Forked from https://github.com/alexcpendleton/NLipsum

using System.Text;
using System.Text.RegularExpressions;

namespace DevToys.Tools.Helpers.LoremIpsum;

/// <summary>
/// Represents a utility that generates Lipsum from a source.
/// </summary>
internal sealed class LipsumGenerator
{
    private const string LoremIpsumStartText = "Lorem ipsum dolor sit amet";

    private readonly LipsumsCorpus _lipsum = LipsumsCorpus.LoremIpsum;
    private StringBuilder _lipsumText = new();
    private IReadOnlyList<string> _preparedWords = Array.Empty<string>();

    internal LipsumGenerator(LipsumsCorpus lipsum)
    {
        _lipsum = lipsum;
        LipsumText = new StringBuilder(Lipsums.GetText(lipsum));
    }

    internal LipsumGenerator(string rawData, bool isXml)
    {
        /*
         * If we're receiving an XML string we need to do some
         * parsing to retrieve the lipsum text.
         */
        LipsumText = isXml ?
            LipsumUtilities.GetTextFromRawXml(rawData) : new StringBuilder(rawData);
    }

    /// <summary>
    /// Gets or sets the text used to generate lipsum.
    /// </summary>
    internal StringBuilder LipsumText
    {
        get => _lipsumText;
        set
        {
            _lipsumText = value;
            PreparedWords = PrepareWords();
        }
    }

    /// <summary>
    /// Gets the words prepared from the LipsumText.
    /// </summary>
    internal IReadOnlyList<string> PreparedWords
    {
        get => _preparedWords;
        private set => _preparedWords = value;
    }

    /// <summary>
    /// Generates 'count' features.  The format string will be applied to the feature not the result.
    /// </summary>
    /// <param name="count">How many features are desired.</param>
    /// <param name="feature">The desired feature, such as Paragraph or Sentence.</param>
    /// <returns></returns>
    internal string GenerateLipsum(int count, Features feature)
    {
        var results = new StringBuilder();
        string[] data;
        TextFeature? options = null;
        if (feature == Features.Paragraphs)
        {
            data = GenerateParagraphs(count, FormatStrings.Paragraph.LineBreaks);
        }
        else if (feature == Features.Sentences)
        {
            data = GenerateSentences(count, FormatStrings.Sentence.Phrase);
            options = Sentence.Medium;
        }
        else if (feature == Features.Words)
        {
            data = GenerateWords(count);
            options = Word.Medium;
        }
        else if (feature == Features.Characters)
        {
            data = GenerateCharacters(count);
        }
        else
        {
            throw new NotImplementedException();
        }

        int length = data.Length;
        for (int i = 0; i < length; i++)
        {
            results.Append(data[i]);
            if (options is not null)
            {
                results.Append(options.Delimiter);
            }
        }

        string result = results.ToString();

        // If the user has selected the Lorem Ipsum lipsum, we want to always start the result by "Lorem ipsum dolor sit amet".
        if (_lipsum == LipsumsCorpus.LoremIpsum
            && (feature == Features.Sentences || feature == Features.Paragraphs))
        {
            result = ApplyStartWords(LoremIpsumStartText, result);
        }

        return result.Trim();
    }

    /// <summary>
    /// Generates a single string (in an array with only this as an element) 
    /// by getting the first 'count' characters from LipsumText.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="formatString"></param>
    /// <returns></returns>
    internal string[] GenerateCharacters(int count)
    {
        string[] result = new string[1];

        /* This whole method needs some thought.  
         * Right now it just grabs 'count' amount 
         * of characters from the beginning of the
         * LipsumText.  It'd be nice if it could 
         * generate sentences and then truncate them 
         * but I can't think of an elegant way to 
         * do that at the moment.  TODO. */

        if (count >= LipsumText.Length)
        {
            count = LipsumText.Length - 1;
        }
        char[] chars = LipsumText.ToString().Substring(0, count).ToCharArray();

        result[0] = new string(chars);

        return result;
    }

    /// <summary>
    /// Generates 'count' medium-sized paragraphs of lipsum text.
    /// </summary>
    /// <param name="count">The number of paragraphs desired.</param>
    /// <param name="formatString">The string used to format the paragraphs.  Will not perform formatting if null or empty.</param>
    private string[] GenerateParagraphs(int count, string formatString)
    {
        Paragraph options = Paragraph.Medium;
        options.FormatString = formatString;
        return GenerateParagraphs(count, options);
    }

    /// <summary>
    /// Generates 'count' paragraphs of lipsum text.
    /// </summary>
    /// <param name="count">The number of paragraphs desired.</param>
    /// <param name="options">Used to determine the minimum and maximum sentences per paragraphs, and format string if applicable.</param>
    /// <returns></returns>
    internal string[] GenerateParagraphs(int count, Paragraph options)
    {
        /*
         * TODO:  These generate methods could probably be 
         * refactored into one method that takes a count 
         * and a TextFeature. */
        string[] paragraphs = new string[count];
        string[] sentences;
        for (int i = 0; i < count; i++)
        {
            /* Get a random amount of sentences based on the
             * min and max from the paragraph options */
            sentences = GenerateSentences(LipsumUtilities.
                RandomInt((int)options.GetMinimum(), (int)options.GetMaximum()));

            // Shove them all together in sentence fashion.
            string joined = string.Join(options.Delimiter, sentences);

            // Format if allowed.
            paragraphs[i] = string.IsNullOrEmpty(options.FormatString) ?
                joined : options.Format(joined);
        }

        return paragraphs;
    }

    /// <summary>
    /// Generates 'count' sentences of lipsum text, using a Medium length sentence.  Will use Phase formatting.
    /// </summary>
    /// <param name="count">The number of sentences desired.</param>
    private string[] GenerateSentences(int count)
    {
        return GenerateSentences(count, FormatStrings.Sentence.Phrase);
    }

    /// <summary>
    /// Generates 'count' sentences of lipsum text, using a Medium length sentence.
    /// </summary>
    /// <param name="count">The number of sentences desired.</param>
    /// <param name="formatString">The string used to format the sentences.  Will not perform formatting if null or empty.</param>
    /// <returns></returns>
    private string[] GenerateSentences(int count, string formatString)
    {
        Sentence options = Sentence.Medium;
        options.FormatString = formatString;
        return GenerateSentences(count, options);
    }

    /// <summary>
    /// Generates 'count' sentences of lipsum text.  
    /// If options.FormatString is not null or empty that string used to format the sentences.
    /// </summary>
    /// <param name="count">The number of sentences desired.</param>
    /// <param name="options">Used to determine the minimum and maximum words per sentence, and format string if applicable.</param>
    /// <returns></returns>
    internal string[] GenerateSentences(int count, Sentence options)
    {
        string[] sentences = new string[count];
        string[] words;

        for (int i = 0; i < count; i++)
        {
            /* Get a random amount of words based on the
             * min and max from the Sentence options */
            words = GenerateWords(LipsumUtilities.
                RandomInt((int)options.MinimumWords, (int)options.MaximumWords));

            // Shove them all together in sentence fashion.
            string joined = string.Join(options.Delimiter, words);

            // Format if allowed.
            sentences[i] = string.IsNullOrEmpty(options.FormatString) ?
                joined : options.Format(joined);
        }

        return sentences;
    }

    /// <summary>
    /// Generates the amount of lipsum words.
    /// </summary>
    /// <param name="count">The amount of words to generate.</param>
    /// <returns></returns>
    internal string[] GenerateWords(int count)
    {
        string[] words = new string[count];

        for (int i = 0; i < count; i++)
        {
            words[i] = LipsumUtilities.RandomElement(PreparedWords);
        }

        return words;
    }

    /// <summary>
    /// Retrieves all of the words in the <see cref="LipsumText"/> as an array.
    /// </summary>
    /// <returns></returns>
    private IReadOnlyList<string> PrepareWords()
    {
        return LipsumUtilities.RemoveEmptyElements(Regex.Split(LipsumText.ToString(), @"\s"));
    }

    private static string ApplyStartWords(string startText, string originalText)
    {
        if (string.IsNullOrWhiteSpace(startText))
        {
            return originalText;
        }

        char space = ' ';
        string[] startTokens = (startText ?? "").Split(space, StringSplitOptions.RemoveEmptyEntries);
        string[] endTokens = (originalText ?? "").Split(space, StringSplitOptions.RemoveEmptyEntries);
        int wordsNeeded = Math.Min(startTokens.Length, endTokens.Length);

        return string.Join(space, startTokens.Take(wordsNeeded).Concat(endTokens.Skip(wordsNeeded)));
    }
}
