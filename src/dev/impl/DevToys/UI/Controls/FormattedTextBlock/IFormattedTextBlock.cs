#nullable enable

using ColorCode;

namespace DevToys.UI.Controls.FormattedTextBlock
{
    public interface IFormattedTextBlock
    {
        void SetText(string? text, ILanguage? language = null);

        void SetInlineTextDiff(string? oldText, string? newText, bool lineDiff);
    }
}
