#nullable enable

namespace DevTools.Common.UI.Controls.FormattedTextBlock
{
    internal sealed class CustomWordChunker : CustomDelimiterChunker
    {
        private static string[] WordSeparaters { get; }
            =
            {
            " ",
            "\t",
            "\r\n",
            "\r",
            "\n",
            ".",
            "(",
            ")",
            "{",
            "}",
            ",",
            "!",
            "?",
            ";"
        };

        /// <summary>
        /// Gets the default singleton instance of the chunker.
        /// </summary>
        public static CustomWordChunker Instance { get; } = new CustomWordChunker();

        public CustomWordChunker() : base(WordSeparaters)
        {
        }
    }
}
