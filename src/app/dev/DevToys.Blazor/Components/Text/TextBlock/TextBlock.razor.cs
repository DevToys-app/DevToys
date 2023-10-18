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
    /// Indicates whether the text can be trimmed if there's not enough space to display it fully.
    /// </summary>
    [Parameter]
    public bool CanTrim { get; set; }

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
        string classBuilder
            = new CssBuilder("text-block")
            .AddClass($"type-{Appearance.Class}")
            .AddClass("no-wrap", NoWrap)
            .AddClass("trim", CanTrim)
            .AddClass("hide", !IsVisible)
            .AddClass("disabled", !IsActuallyEnabled)
            .AddClass("horizontal-center", HorizontalAlignment == UIHorizontalAlignment.Center)
            .AddClass("vertical-center", VerticalAlignment == UIVerticalAlignment.Center)
            .AddClass(FinalCssClasses)
            .Build();

        builder.OpenElement(0, Appearance.Tag);
        builder.AddAttribute(1, "id", Id);
        builder.AddAttribute(2, "class", classBuilder.ToString());
        builder.AddAttribute(3, "style", Style);

        int sequence = 4;
        if (AdditionalAttributes is not null)
        {
            foreach (KeyValuePair<string, object> attribute in AdditionalAttributes)
            {
                builder.AddAttribute(sequence, attribute.Key, attribute.Value);
                sequence++;
            }
        }

        if (HighlightedSpans is null || HighlightedSpans.Length == 0)
        {
            builder.AddContent(sequence, Text);
        }
        else if (!string.IsNullOrEmpty(Text!))
        {
            // Create a set of `text <mark> highlighted span </mark> text`
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
