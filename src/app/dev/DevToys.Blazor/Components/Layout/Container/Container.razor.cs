namespace DevToys.Blazor.Components;

public partial class Container : StyledComponentBase
{
    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    public string FinalStyles { get; private set; } = string.Empty;

    public string FinalInnerStyles { get; private set; } = string.Empty;

    protected override void OnParametersSet()
    {
        BuildStyleAttribute();
        base.OnParametersSet();
    }

    private void BuildStyleAttribute()
    {
        var styleBuilder = new StyleBuilder();
        var innerStyleBuilder = new StyleBuilder();
        if (AdditionalAttributes is not null)
        {
            styleBuilder.AddStyleFromAttributes(AdditionalAttributes);
        }

        if (!string.IsNullOrWhiteSpace(Style))
        {
            styleBuilder.AddStyle(Style);
        }

        styleBuilder.AddImportantStyle("display", () => IsVisible ? "flex" : "none");
        styleBuilder.AddImportantStyle("flex-direction", "row");
        styleBuilder.AddImportantStyle("flex-wrap", "nowrap");

        styleBuilder.AddStyle("margin-left", MarginLeft.ToPx(), MarginLeft.HasValue);
        styleBuilder.AddStyle("margin-right", MarginRight.ToPx(), MarginRight.HasValue);
        styleBuilder.AddStyle("margin-top", MarginTop.ToPx(), MarginTop.HasValue);
        styleBuilder.AddStyle("margin-bottom", MarginBottom.ToPx(), MarginBottom.HasValue);

        switch (HorizontalAlignment)
        {
            case UIHorizontalAlignment.Stretch:
                innerStyleBuilder.AddImportantStyle("width", "100%", !Width.HasValue);
                styleBuilder.AddImportantStyle("justify-content", "center", Width.HasValue);
                break;

            case UIHorizontalAlignment.Left:
                styleBuilder.AddImportantStyle("justify-content", "flex-start");
                break;

            case UIHorizontalAlignment.Right:
                styleBuilder.AddImportantStyle("justify-content", "flex-end");
                break;

            case UIHorizontalAlignment.Center:
                styleBuilder.AddImportantStyle("justify-content", "center");
                break;
        }

        styleBuilder.AddImportantStyle("min-height", "min-content", !Height.HasValue);
        styleBuilder.AddImportantStyle("max-height", "100%", !Height.HasValue);
        switch (VerticalAlignment)
        {
            case UIVerticalAlignment.Stretch:
                styleBuilder.AddImportantStyle("align-items", "stretch");
                styleBuilder.AddStyle("height", "100%", !Height.HasValue);
                innerStyleBuilder.AddImportantStyle("max-height", "inherit", !Height.HasValue);
                innerStyleBuilder.AddImportantStyle("height", "100%", !Height.HasValue);
                innerStyleBuilder.AddImportantStyle("min-height", "min-content", !Height.HasValue);
                break;

            case UIVerticalAlignment.Top:
                styleBuilder.AddImportantStyle("align-items", "flex-start");
                break;

            case UIVerticalAlignment.Bottom:
                styleBuilder.AddImportantStyle("height", "100%", !Height.HasValue);
                styleBuilder.AddImportantStyle("align-items", "flex-end");
                break;

            case UIVerticalAlignment.Center:
                styleBuilder.AddImportantStyle("height", "100%", !Height.HasValue);
                styleBuilder.AddImportantStyle("align-items", "center");
                break;
        }

        innerStyleBuilder.AddStyle("pointer-events", "none");
        innerStyleBuilder.AddImportantStyle("width", Width.ToPx(), Width.HasValue);
        innerStyleBuilder.AddImportantStyle("height", Height.ToPx(), Height.HasValue);
        innerStyleBuilder.AddImportantStyle("padding-left", PaddingLeft.ToPx(), PaddingLeft.HasValue);
        innerStyleBuilder.AddImportantStyle("padding-right", PaddingRight.ToPx(), PaddingRight.HasValue);
        innerStyleBuilder.AddImportantStyle("padding-top", PaddingTop.ToPx(), PaddingTop.HasValue);
        innerStyleBuilder.AddImportantStyle("padding-bottom", PaddingBottom.ToPx(), PaddingBottom.HasValue);
        innerStyleBuilder.AddImportantStyle("position", "relative");

        FinalStyles = styleBuilder.ToString();
        FinalInnerStyles = innerStyleBuilder.ToString();
    }
}
