using Microsoft.AspNetCore.Components.Rendering;

namespace DevToys.Blazor.Components;

public class TextBlock : StyledComponentBase
{
    /// <summary>
    /// Gets or sets the appearance of the text.
    /// </summary>
    [Parameter]
    public TextBlockAppearance Appearance { get; set; } = TextBlockAppearance.Body;

    /// <summary>
    /// Gets or sets whether the text can wrap or not.
    /// </summary>
    [Parameter]
    public bool NoWrap { get; set; }

    /// <summary>
    /// Gets or sets the text to display.
    /// </summary>
    [Parameter]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets spans to highlight.
    /// </summary>
    [Parameter]
    public TextSpan[]? HighlightedSpans { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, Appearance.Tag);
        builder.AddAttribute(1, "id", Id);
        builder.AddAttribute(2, "class", $"text-block type-{Appearance.Class} {(NoWrap ? "no-wrap" : string.Empty)} {FinalCssClasses}");
        builder.AddAttribute(3, "style", Style);

        if (HighlightedSpans is null || HighlightedSpans.Length == 0)
        {
            builder.AddContent(4, Text);
        }
        else if (!string.IsNullOrEmpty(Text!))
        {
            // Create a set of `text <mark> highlighted span </mark> text`
            int sequence = 4;
            int lastPlainTextStartPosition = 0;

            foreach (TextSpan highlightedSpan in HighlightedSpans.OrderBy(span => span.StartPosition))
            {
                string nonHighlightedText
                    = Text.Substring(
                        lastPlainTextStartPosition,
                        Math.Max(0, highlightedSpan.StartPosition - lastPlainTextStartPosition));
                builder.AddContent(
                    ++sequence,
                    nonHighlightedText);

                builder.OpenElement(++sequence, "mark");

                string highlightedText
                    = Text.Substring(
                        highlightedSpan.StartPosition,
                        highlightedSpan.Length);
                builder.AddContent(
                    ++sequence,
                    highlightedText);

                lastPlainTextStartPosition = highlightedSpan.EndPosition;

                builder.CloseElement();
            }

            builder.AddContent(sequence, Text.Substring(lastPlainTextStartPosition));
        }

        builder.CloseElement();
    }
}
