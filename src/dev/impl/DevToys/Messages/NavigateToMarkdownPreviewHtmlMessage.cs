#nullable enable

namespace DevToys.Messages
{
    public sealed class NavigateToMarkdownPreviewHtmlMessage
    {
        internal string Html { get; }

        public NavigateToMarkdownPreviewHtmlMessage(string html)
        {
            Html = html;
        }
    }
}
