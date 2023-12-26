// Forked from https://github.com/alexcpendleton/NLipsum

namespace DevToys.Tools.Helpers.LoremIpsum;

/// <summary>
/// Common formatting strings.
/// </summary>
internal static class FormatStrings
{
    /// <summary>
    /// A string with no formatting.
    /// </summary>
    internal static string Unformatted => "{0}";

    /// <summary>
    /// Formatting strings for Paragraphs.
    /// </summary>
    internal static class Paragraph
    {
        /// <summary>
        /// An Html paragraph.  "&lt;p&gt;Lorem ipsum dolor sit amet&lt;/p&gt;"
        /// </summary>
        public static string Html => "<p>{0}</p>";

        /// <summary>
        /// A block of text ending with a new line and/or carriage return (based on Environment).
        /// </summary>
        public static string LineBreaks => "{0}" + Environment.NewLine + Environment.NewLine;
    }

    /// <summary>
    /// Formatting strings for Sentences.
    /// </summary>
    internal static class Sentence
    {
        /// <summary>
        /// A typical sentence ending with a period/full stop.  "Lorem ipsum dolor sit amet."
        /// </summary>
        public static string Phrase => "{0}.";

        /// <summary>
        /// A sentence ending with a question mark..  "Lorem ipsum dolor sit amet?"
        /// </summary>
        public static string Question => "{0}?";

        /// <summary>
        /// An sentence ending with an exclamation point/mark.  "Lorem ipsum dolor sit amet!"
        /// </summary>
        public static string Exclamation => "{0}!";
    }
}
