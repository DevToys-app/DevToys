using DevToys.Api;
using Microsoft.UI.Xaml.Controls;

namespace DevToys.UI.Framework.Controls;

/// <summary>
/// Represents an instance of the Monaco Editor.
/// </summary>
public interface IMonacoEditor
{
    bool ReadOnly { get; set; }

    string Text { get; set; }

    string? CodeLanguage { get; set; }

    TextSpan SelectedSpan { get; set; }

    Control UIHost { get; }

    event EventHandler TextChanged;

    event EventHandler SelectedSpanChanged;

    Task HighlightSpansAsync(IReadOnlyList<TextSpan> spans);
}
